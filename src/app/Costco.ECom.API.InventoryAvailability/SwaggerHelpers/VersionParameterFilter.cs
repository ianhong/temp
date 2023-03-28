namespace Costco.ECom.API.InventoryAvailability.SwaggerHelpers
{
    /// <summary>
    /// Swagger OperationFilter to add default version to swagger OpenApi definition
    /// </summary>
    public class VersionParameterFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var versionParameter = operation.Parameters.SingleOrDefault(p => p.Name == "version");
            if (versionParameter != null)
            {
                versionParameter.Schema.Default = new OpenApiString($"{InventoryServiceHelpers.DefaultApiVersionMajor}.{InventoryServiceHelpers.DefaultApiVersionMinor}");
            }
        }
    }
}
