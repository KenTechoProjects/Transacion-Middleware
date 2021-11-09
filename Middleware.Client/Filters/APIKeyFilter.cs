using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Middleware.Core.DAO;
using Middleware.Service;
using Middleware.Service.DTOs;
using Middleware.Service.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Middleware.Client.Filters
{
    [AttributeUsage(validOn: AttributeTargets.Method)]
    public class APIKeyFilter : Attribute, IAsyncActionFilter
    {
        private const string APIKEYNAME = "appKey";
        private const string APIID = "appId";
    

        private readonly IMessageProvider _messageProvider;

        private readonly ILogger _log;

        private const string ApiKeyHeader = "appKey";
        private readonly ICustomerDAO _customerDAO;
        public APIKeyFilter(ICustomerDAO customerDAO, ILoggerFactory logger, IMessageProvider messageProvider)
        {
            _customerDAO = customerDAO; _log = logger.CreateLogger<APIKeyFilter>();
            _messageProvider = messageProvider;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //
            var hasApiKey = !context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out var potentialAppKey);
            if (hasApiKey == true)
            {
        hasApiKey = !context.HttpContext.Request.Headers.TryGetValue("appKey", out   potentialAppKey);
            }
    
            var hasAppId = !context.HttpContext.Request.Headers.TryGetValue(APIID, out var potentialAppId);
  
           hasAppId = !context.HttpContext.Request.Headers.TryGetValue(APIID, out   potentialAppId);
           context.HttpContext.Request.Headers.TryGetValue("walletNumber", out var walletNumber);

       var iscountry=!    context.HttpContext.Request.Headers.TryGetValue("countryId", out var country);
            // context.ActionArguments["beneficiary"] = user;

      
            var islanguage = !context.HttpContext.Request.Headers.TryGetValue("language", out var language_);
            var language = language_.ToString().ToLower();
            if(string.IsNullOrWhiteSpace(language_))
            {
                language_ = "04";
            }
            if (islanguage)
            {
                context.Result = new ObjectResult(

                   new ServiceResponse<BasicResponse>(false)
                   {
                       Error = new ErrorResponse
                       {
                           ResponseCode = ResponseCodes.REQUEST_NOT_FOUND,
                           ResponseDescription = "Provide the language"
                       },
                       FaultType = FaultMode.INVALID_OBJECT_STATE,
                       IsSuccessful = false
                   }
                 )

                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }

 //if (iscountry)
 //           {
 //               context.Result = new ObjectResult(

 //                  new ServiceResponse<BasicResponse>(false)
 //                  {
 //                      Error = new ErrorResponse
 //                      {
 //                          ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE,
 //                          ResponseDescription = "Invalid country code"
 //                      },
 //                      FaultType = FaultMode.INVALID_OBJECT_STATE,
 //                      IsSuccessful = false
 //                  }
 //                )

 //               {
 //                   StatusCode = (int)HttpStatusCode.BadRequest
 //               };
 //               return;
 //           }
            // context.ActionArguments["beneficiary"] = user;
            if (string.IsNullOrWhiteSpace(walletNumber))
            {

                context.Result = new ObjectResult(

                    new ServiceResponse<BasicResponse>(false)
                    {
                        Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.INVALID_ACCOUNT_NUMBER,
                            ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_ACCOUNT_NUMBER,language)
                        },
                        FaultType = FaultMode.INVALID_OBJECT_STATE,
                        IsSuccessful = false
                    }
                  )

                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }
            var custId =await _customerDAO.FindByWalletNumber(walletNumber);
           
            if (custId == null)
            {
                context.Result = new ObjectResult(

                  new ServiceResponse<BasicResponse>(false)
                  {
                      Error = new ErrorResponse
                      {
                          ResponseCode = ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH,
                          ResponseDescription = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH, language)
                      },
                      FaultType = FaultMode.INVALID_OBJECT_STATE,
                      IsSuccessful = false
                  }
                )

                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }

            if ((hasApiKey || hasAppId) == true)
            {
                //context.Result = new UnauthorizedResult();

                context.Result = new ObjectResult(

                    new ServiceResponse<BasicResponse>(false)
                    {
                        Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.MISSING_APYKEY_TOKEN,
                            ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.MISSING_APYKEY_TOKEN, language)} or {_messageProvider.GetMessage(ResponseCodes.MISSING_AUTH_TOKEN, language)}"
                        },
                        FaultType = FaultMode.UNAUTHORIZED,
                        IsSuccessful = false
                    }
                  )

                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }
            context.HttpContext.Request.Headers.Add("customerId", custId.Id.ToString());
           context.HttpContext.Request.Headers.Add("alias",$"{custId.LastName} {custId.FirstName}"  );

            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = configuration.GetSection("SenegalExternalCallSetting:AppKey");//To read API key from appsettings.json use this line
            var appId = configuration.GetSection("SenegalExternalCallSetting:AppId");//To read API key from appsettings.json use this line

            if (apiKey != null)
            {
                if (!apiKey.Value.Equals(potentialAppKey) || !appId.Value.Equals(potentialAppId))
                {
                    //context.Result = new UnauthorizedResult();
                    //return;

                    context.Result = new ObjectResult(

                        new ServiceResponse<BasicResponse>(false)
                        {
                            Error = new ErrorResponse
                            {
                                ResponseCode = ResponseCodes.MISSING_AUTH_TOKEN,
                                ResponseDescription = "UnAuthorized.................!"
                            },
                            FaultType = FaultMode.UNAUTHORIZED,
                            IsSuccessful = false
                        }
                      )

                    {
                        StatusCode = (int)HttpStatusCode.BadRequest
                    };
                    return;
                }
            }

            await next();
        }
    }


    public class ValdateTokenFilter : Attribute, IAsyncActionFilter
    {
        private const string HEADER_KEY = "authToken";





      
        private readonly ICustomerDAO _customerDAO;
        private readonly ILogger _log;
        private readonly IMessageProvider _messageProvider; 
        readonly IAuthenticationServices _service;
        public ValdateTokenFilter(ICustomerDAO customerDAO, ILoggerFactory logger, IMessageProvider messageProvider, IAuthenticationServices service)
        {
            _customerDAO = customerDAO;
            _log = logger.CreateLogger<ValdateTokenFilter>();
            _messageProvider = messageProvider;
            _service = service;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //
         
        var iswallet=!context.HttpContext.Request.Headers.TryGetValue("walletNumber", out var walletNumber);
        var islanguage=!context.HttpContext.Request.Headers.TryGetValue("language", out var language_); 
            var language = language_.ToString().ToLower();
            if (islanguage)
            {
                context.Result = new ObjectResult(

                   new ServiceResponse<BasicResponse>(false)
                   {
                       Error = new ErrorResponse
                       {
                           ResponseCode = ResponseCodes.REQUEST_NOT_FOUND,
                           ResponseDescription = "Provide the language"
                       },
                       FaultType = FaultMode.INVALID_OBJECT_STATE,
                       IsSuccessful = false
                   }
                 )

                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }
           // context.ActionArguments["beneficiary"] = user;
           if(string.IsNullOrWhiteSpace(walletNumber))
            {

                context.Result = new ObjectResult(

                    new ServiceResponse<BasicResponse>(false)
                    {
                        Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.INVALID_ACCOUNT_NUMBER,
                            ResponseDescription =_messageProvider.GetMessage(ResponseCodes.INVALID_ACCOUNT_NUMBER, language)
                        },
                        FaultType = FaultMode.INVALID_OBJECT_STATE,
                        IsSuccessful = false
                    }
                  )

                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }
            var custId =await _customerDAO.FindByWalletNumber(walletNumber);
           
            if (iswallet)
            {
                context.Result = new ObjectResult(

                  new ServiceResponse<BasicResponse>(false)
                  {
                      Error = new ErrorResponse
                      {
                          ResponseCode = ResponseCodes.INVALID_ACCOUNT_NUMBER,
                          ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_ACCOUNT_NUMBER, language)
                      },
                      FaultType = FaultMode.INVALID_OBJECT_STATE,
                      IsSuccessful = false
                  }
                )

                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }
            string token;
            var rr = context.HttpContext.Request.Headers.TryGetValue(HEADER_KEY, out var headers);
            token = headers.FirstOrDefault();

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new ObjectResult(new ErrorResponse { ResponseCode = ResponseCodes.MISSING_AUTH_TOKEN, ResponseDescription = _messageProvider.GetMessage(ResponseCodes.MISSING_AUTH_TOKEN , language) })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }
            try
            {
                var response = await _service.ValidateSession(token);
                if (response.IsSuccessful==false)
                {

                    context.Result = new ObjectResult(new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_SESSION,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.MISSING_AUTH_TOKEN, language)
                    })
                    {
                        StatusCode = (int)HttpStatusCode.Unauthorized
                    };
                }
               

            }
            catch (Exception e)
            {
                _log.LogCritical(e, "System error");
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

            await next();
        }
    }
}
