// <copyright file="Program.cs" company="Costco Wholesale">
// Copyright (c) Costco Wholesale. All rights reserved.
// </copyright>
// ----------------------------------------------------------------------
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Serilog;

namespace Costco.ECom.API.InventoryAvailability
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                Log.Information($"Starting web host for {nameof(InventoryAvailability)}");

                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Load values from k8s CSI Key Vault driver mount point
                config.AddKeyPerFile(directoryPath: "/mnt/secrets-store/", optional: true, reloadOnChange: true);

                var builtConfig = config.Build();

                // TODO: Transition Serilog AppInsights sink to use the connection string instead of Instrumentation Key, once that is fully supported
                Log.Logger = new LoggerConfiguration()
                                    .ReadFrom.Configuration(builtConfig)
                                    .Enrich.FromLogContext()
                                    .WriteTo.Console(
                                            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                                    .WriteTo.ApplicationInsights(builtConfig[SysConfiguration.ApplicationInsightsConnStringKeyName], TelemetryConverter.Traces)
                                    .CreateLogger();
            })
            /*.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var settings = config.Build();
                    config.AddAzureAppConfiguration(options =>
                    options
                        .Connect(settings.GetConnectionString("AppConfig"))
                        // Load configuration values with no label
                        .Select("InventoryService:*", LabelFilter.Null)
                        // Override with any configuration values specific to current hosting env
                        .Select("InventoryService:*", hostingContext.HostingEnvironment.EnvironmentName)
                        // Configure to reload configuration if the registered sentinel key is modified
                        .ConfigureRefresh(refreshOptions =>
                            refreshOptions.Register("InventoryService:Settings:Sentinel", refreshAll: true))
                    );

                    // TODO: Transition Serilog AppInsights sink to use the connection string instead of Instrumentation Key, once that is fully supported
                    Log.Logger = new LoggerConfiguration()
                                        .ReadFrom.Configuration(settings)
                                        .Enrich.FromLogContext()
                                        .WriteTo.Console(
                                                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
                                        .WriteTo.ApplicationInsights(settings[SysConfiguration.ApplicationInsightsConnStringKeyName], TelemetryConverter.Traces)
                                        .CreateLogger();
                })
                .UseStartup<Startup>();
            }) */
            .UseSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
             {
                webBuilder.UseStartup<Startup>();
             });
    }
}