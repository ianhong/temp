﻿using AlwaysOn.Shared.TelemetryExtensions;
using Azure.Core;
using Azure.Identity;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.FeatureManagement;

namespace Costco.ECom.API.InventoryAvailability
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<SysConfiguration>();

            services.AddSingleton(typeof(ITelemetryChannel),
                                new ServerTelemetryChannel() { StorageFolder = "/tmp" });
            services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions()
            {
                ConnectionString = Configuration[SysConfiguration.ApplicationInsightsConnStringKeyName],
                EnableAdaptiveSampling = bool.TryParse(Configuration[SysConfiguration.ApplicationInsightsAdaptiveSamplingName], out bool result) ? result : true
            });

            //load default featuremanagement scope "FeatureManagement"
            services.AddFeatureManagement();

            services.AddSingleton<TokenCredential>(builder =>
            {
                var managedIdentityClientId = Configuration["AZURE_CLIENT_ID"];
                if (!string.IsNullOrEmpty(managedIdentityClientId))
                {
                    return new ManagedIdentityCredential(managedIdentityClientId);
                }
                else
                {
                    return new DefaultAzureCredential();
                }
            });

            services.AddSingleton<AppInsightsCosmosRequestHandler>();

            services.AddHealthChecks();// Adds a simple liveness probe HTTP endpoint, path mapping happens further below

            services.AddSingleton<IDatabaseService, CosmosDbService>();

            if (Convert.ToBoolean(Configuration["UseStubRepository"]))
            {
                services.AddSingleton<IInventoryService>(new InventoryMockService());
            }
            else
            {
                services.AddSingleton<IInventoryService, InventoryDbService>();
            }

            services.AddSingleton<ConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { $"{Configuration.GetValue<string>("REDIS_HOST_NAME")}:{Configuration.GetValue<int>("REDIS_PORT_NUMBER")}" },
                Ssl = true,
                AbortOnConnectFail = false,
                Password = Configuration.GetValue<string>("REDIS_KEY")
            }));

            services.AddSingleton<IRedisCacheService<InventoryItem>, RedisCacheService<InventoryItem>>();

            services.AddSingleton<IMessageProducerService, ServiceBusProducerService>();

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = Globals.JsonSerializerOptions.DefaultIgnoreCondition;
                options.JsonSerializerOptions.PropertyNamingPolicy = Globals.JsonSerializerOptions.PropertyNamingPolicy;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AlwaysOn InventoryService API", Version = "v1" });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.OperationFilter<ApiKeyFilter>(); // Custom parameter in Swagger for API Key-protected operations
                c.OperationFilter<VersionParameterFilter>(); // Custom add default value for version parameter in Swagger
            });

            services.AddCors();

            services.AddSingleton<ITelemetryInitializer>(sp =>
            {
                var sysConfig = sp.GetService<SysConfiguration>();
                return new AlwaysOnCustomTelemetryInitializer($"{nameof(InventoryAvailability)}-{sysConfig.AzureRegionShort}", sp.GetService<IHttpContextAccessor>());
            });

            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true; // enable the "api-supported-versions" header with each response
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(InventoryServiceHelpers.DefaultApiVersionMajor, InventoryServiceHelpers.DefaultApiVersionMinor);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UsePathBase("/inventoryservice");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // Enable middleware to serve generated Swagger as a JSON endpoint.

                // Production CatalogService API will run on the same domain as the UI, but for local development, CORS needs to be enabled.
                app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
            }

            var sysConfig = app.ApplicationServices.GetService<SysConfiguration>();

            if (sysConfig.EnableSwagger)
            {
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "AlwaysOn Inventoryervice");
                });
            }

            // Handler for uncaught exceptions. Otherweise AppInsights will report a 200 for failed requests instead of 500
            // Source: https://stackoverflow.com/a/40543603/1537195
            app.UseExceptionHandler(options =>
            {
                options.Run(
                async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "text/html";
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex != null)
                    {
                        //var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace}";
                        var err = $"Error: {ex.Error.Message}";
                        await context.Response.WriteAsync(err).ConfigureAwait(false);
                    }
                });
            });

            app.Use(async (context, next) =>
            {
                context.Response.OnStarting(o =>
                {
                    if (o is HttpContext ctx)
                    {
                        // In order to get relative location headers (without the host part), we modify any location header here
                        // This is to simplify the reverse-proxy setup in front of the application
                        try
                        {
                            if (!string.IsNullOrEmpty(context.Response.Headers.Location))
                            {
                                var locationUrl = new Uri(context.Response.Headers.Location);
                                context.Response.Headers.Location = locationUrl.PathAndQuery;
                            }
                        }
                        catch (Exception) { }

                        // Add tracing headers to each response
                        // Source: https://khalidabuhakmeh.com/add-headers-to-a-response-in-aspnet-5
                        context.Response.Headers.Add("X-Server-Name", Environment.MachineName);
                        context.Response.Headers.Add("X-Server-Location", sysConfig.AzureRegion);
                        context.Response.Headers.Add("X-Correlation-ID", Activity.Current?.RootId);
                        context.Response.Headers.Add("X-Requested-Api-Version", ctx.GetRequestedApiVersion()?.ToString());
                    }
                    return Task.CompletedTask;
                }, context);
                await next();
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health/liveness");
            });
        }
    }
}