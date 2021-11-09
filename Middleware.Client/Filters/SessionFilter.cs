using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Middleware.Service;
using Middleware.Service.DTOs;

namespace Middleware.Client.Filters
{
    public class SessionFilter : IAsyncActionFilter
    {
        private const string HEADER_KEY = "authToken";
        readonly IAuthenticationServices _service;
        private readonly ILogger _logger;
         
        public SessionFilter(IAuthenticationServices Service, ILoggerFactory logger)
        {
            _service = Service;
            _logger = logger.CreateLogger(typeof(SessionFilter));
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string token;
            var rr = context.HttpContext.Request.Headers.TryGetValue(HEADER_KEY, out var headers);
            if (!rr)
            {
                context.Result = new ObjectResult(
                    new ErrorResponse 
                    { 
                        ResponseCode = ResponseCodes.MISSING_AUTH_TOKEN, 
                        ResponseDescription = "Missing Auth Token" 
                    })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;

            }

            token = headers.FirstOrDefault();

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new ObjectResult(new ErrorResponse { ResponseCode = ResponseCodes.MISSING_AUTH_TOKEN, ResponseDescription = "Missing Auth Token" })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }
            try
            {
                var response = await _service.ValidateSession(token);
                if (response.IsSuccessful)
                {
                    var user = response.GetPayload();
                    context.ActionArguments["user"] = user;
                    await next();
                }
                else
                {

                    context.Result = new ObjectResult(new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_SESSION,
                        ResponseDescription = "Invalid Session 2"
                    })
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized
                    };
                }

            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "System error");
                //TODO: Send language dependent error message
                var error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR
                };
                context.Result = new ObjectResult(error)
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError
                };

            }



        }
    }


}