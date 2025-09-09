using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LibrarySystem.Presentation.Swagger
{
    public sealed class AddRequiredHeadersOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var isApiController = context.MethodInfo
                .DeclaringType?
                .GetCustomAttributes(true)
                .OfType<ApiControllerAttribute>()
                .Any() == true;

            if (!isApiController)
                return;

            var path = context.ApiDescription.RelativePath ?? string.Empty;
            if (!path.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
                return;

            operation.Parameters ??= new List<OpenApiParameter>();

            void AddHeaderOnce(string name, bool required, string? type = "string", string? format = null, string? description = null)
            {
                if (operation.Parameters.Any(p =>
                        p.In == ParameterLocation.Header &&
                        p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = name,
                    In = ParameterLocation.Header,
                    Required = required,
                    Description = description,
                    Schema = new OpenApiSchema { Type = type, Format = format }
                });
            }

            AddHeaderOnce(
                name: "X-Client-Name",
                required: true,
                description: "Nazwa klienta (wymagane przez middleware)."
            );

            AddHeaderOnce(
                name: "X-Request-Id",
                required: false,
                format: "uuid",
                description: "Opcjonalny identyfikator żądania."
            );
        }
    }
}
