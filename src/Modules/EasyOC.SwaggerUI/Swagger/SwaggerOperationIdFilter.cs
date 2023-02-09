using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Globalization;
using System.Text;

namespace EasyOC.SwaggerUI.Swagger
{
    public class SwaggerOperationIdFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.OperationId = FriendlyId(context.ApiDescription);
        }

        private static string FriendlyId(ApiDescription apiDescription)
        {
            var parts = (RelativePathSansQueryString(apiDescription) + "/" + apiDescription.HttpMethod.ToLower())
                .Split('/');

            var builder = new StringBuilder();
            foreach (var part in parts)
            {
                var trimmed = part.Trim('{', '}');
                builder.AppendFormat("{0}{1}",
                    part.StartsWith("{") ? "By" : string.Empty,
                    CultureInfo.InvariantCulture.TextInfo.ToTitleCase(trimmed)
                );
            }

            return builder.ToString();
        }

        private static string RelativePathSansQueryString(ApiDescription apiDescription)
        {
            if (apiDescription != null)
            {
                return apiDescription.RelativePath.Split('?').First();
            }
            else
                return string.Empty;
            {
            }
        }
    }
}
