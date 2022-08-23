using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace domain.Ultils
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {


        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {

            var controllerName = (context.ApiDescription.ActionDescriptor as ControllerActionDescriptor)?.ControllerName;

            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();
            if (!string.IsNullOrWhiteSpace(controllerName) && !controllerName.StartsWith("Report") )
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Token",
                    In = ParameterLocation.Header,
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string"
                    }
                });
            }
            
        }
    }
}
