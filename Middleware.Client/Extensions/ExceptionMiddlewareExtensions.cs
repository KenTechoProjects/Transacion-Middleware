using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Middleware.Core.DTO;
using Middleware.Service.DTOs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Middleware.Client.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {

        public static void UseConfigureExceptionHandler(this IApplicationBuilder app, ILoggerFactory logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        

                        logger.CreateLogger($"Something went wrong: {contextFeature.Error}");
                        await context.Response.WriteAsync(

                          
                            new ErrorDetails()
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = "Internal Server Error."
                        }.ToString());

   //await context.Response.WriteAsync(new ErrorDetails()
   //                     {
   //                         StatusCode = context.Response.StatusCode,
   //                         Message = "Internal Server Error."
   //                     }.ToString());
                    }
                });
            });
        }

        public static IActionResult CreateResponse(ErrorResponseT error, FaultMode category)
        {
            int code;
            switch (category)
            {
                case FaultMode.UNAUTHORIZED:
                    code = (int)HttpStatusCode.Unauthorized;
                    break;
                case FaultMode.CLIENT_INVALID_ARGUMENT:
                    code = (int)HttpStatusCode.BadRequest;
                    break;
                case FaultMode.INVALID_OBJECT_STATE:
                    code = (int)HttpStatusCode.Conflict;
                    break;
                case FaultMode.GATEWAY_ERROR:
                    code = (int)HttpStatusCode.BadGateway;
                    break;
                case FaultMode.REQUESTED_ENTITY_NOT_FOUND:
                    code = (int)HttpStatusCode.NotFound;
                    break;
                case FaultMode.LIMIT_EXCEEDED:
                    code = Microsoft.AspNetCore.Http.StatusCodes.Status429TooManyRequests;
                    break;
                default:
                    code = (int)HttpStatusCode.InternalServerError;
                    break;
            }
            return new ObjectResult(error)
            {
                StatusCode = code
            };
        }
    }


}
