using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Client.BaseServces
{
    public static class SwaggerServices
    {
        public static IServiceCollection AddSwaggerSerice(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "FBNMobile Senegal API",
                    Description = "This API provides all the endpoints needed to extablish the FBNMobile Senegal calls",
                    // TermsOfService = new Uri(""),
                    Contact = new OpenApiContact
                    {
                        Name = "First Bank  Senegal",
                        Email = string.Empty,
                        //  Url = new Uri(" "),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under Firstbank Senegal",
                        //  Url = new Uri(""),
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(System.AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);
            });
            return services;
        }
    }
}
