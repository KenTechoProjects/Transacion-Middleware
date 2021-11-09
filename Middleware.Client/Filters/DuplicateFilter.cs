using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Middleware.Client.Filters
{
    public class DuplicateFilter : IAsyncActionFilter
    {
        private const string HEADER_KEY = "transRef";
        private const string LANGUAGE = "language";
        readonly ITransactionTracker _transactionTracker;
        readonly IMessageProvider _messageProvider;

        public DuplicateFilter(ITransactionTracker transactionTracker, IMessageProvider messageProvider)
        {
            _transactionTracker = transactionTracker;
            _messageProvider = messageProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string transReference;
            // language_ = context.ActionArguments[LANGUAGE].ToString();

            context.HttpContext.Request.Headers.TryGetValue(LANGUAGE, out var language_);
            string language = language_.ToString()?.ToLower();
            language =language== "" ? "en" : language;
          //  context.ActionArguments[LANGUAGE] = language;
            if (!context.HttpContext.Request.Headers.TryGetValue(HEADER_KEY, out var headers))
            {
                context.Result = new ObjectResult(
                    new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.TRANSACTION_REFERENCE_MISSING,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.TRANSACTION_REFERENCE_MISSING, language)
                    })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }

            transReference = headers.FirstOrDefault();

            if (string.IsNullOrEmpty(transReference))
            {
                context.Result = new ObjectResult(new ErrorResponse
                {
                    ResponseCode = ResponseCodes.TRANSACTION_REFERENCE_MISSING,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.TRANSACTION_REFERENCE_MISSING, language)
                })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }

            var exists = await _transactionTracker.Exists(transReference);

            if (exists)
            {
                context.Result = new ObjectResult(new ErrorResponse
                {
                    ResponseCode = ResponseCodes.TRANSACTION_REFERENCE_ALREADY_EXISTS,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.TRANSACTION_REFERENCE_ALREADY_EXISTS, language)
                })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }
            //if (!context.ActionArguments.ContainsKey("user"))
            //{
            //    context.Result = new ObjectResult(new ErrorResponse
            //    {
            //        ResponseCode = ResponseCodes.CUSTOMER_NOT_FOUND,
            //        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language)
            //    })
            //    {
            //        StatusCode = (int)HttpStatusCode.BadRequest
            //    };
            //    return;
            //}
            // if (!context.ActionArguments.ContainsKey("CustomerId"))
            //{
            //    context.Result = new ObjectResult(new ErrorResponse
            //    {
            //        ResponseCode = ResponseCodes.CUSTOMER_NOT_FOUND,
            //        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language)
            //    })
            //    {
            //        StatusCode = (int)HttpStatusCode.BadRequest
            //    };
            //    return;
            //}


            var result = await next();

            if (result.Result is OkResult || result.Result is OkObjectResult)
            {
                var user = context.ActionArguments["user"] as AuthenticatedUser;
                var response = await _transactionTracker.AddTransactionReference(user.Id, transReference);
                if (!response.IsSuccessful)
                {
                    context.Result = new ObjectResult(new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.TRANSACTION_REFERENCE_ALREADY_EXISTS,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.TRANSACTION_REFERENCE_ALREADY_EXISTS, language)
                    })
                    {
                        StatusCode = (int)HttpStatusCode.Conflict
                    };
                    return;
                }

            }


        }
    }
}
