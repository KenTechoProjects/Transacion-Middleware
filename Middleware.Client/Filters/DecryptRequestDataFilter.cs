using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Core.DTO;
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
    public class DecryptRequestDataFilter<T> : IAsyncActionFilter
    {
        public T RequestDataType { get; set; }
        private const string REQUEST = "EncryptedRequestData";
        private const string LANGUAGE = "language";
        private readonly IMessageProvider _messageProvider;
        private readonly ILogger _logger;
        private readonly SystemSettings _settings;
        public DecryptRequestDataFilter(IMessageProvider messageProvider, ILoggerFactory logger,
                IOptions<SystemSettings> settings)
        {
            _messageProvider = messageProvider;
            _logger = logger.CreateLogger<DecryptRequestDataFilter<T>>();
            _settings = settings.Value;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var hasLanguage = context.ActionArguments.TryGetValue(LANGUAGE, out var language);
           
            if (hasLanguage == false)
            {
                language = "en";
            }

            if (!context.ActionArguments.TryGetValue(REQUEST, out var output))
            {
                context.Result = new ObjectResult(
                    new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, (string)language)
                    })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }

            var request = output as BaseEncryptedRequestDTO;
            _logger.LogInformation($"{typeof(T).Name} ENCRYPTED REQUEST ==> {Util.SerializeAsJson(request)}");
            if (!request.IsValid(out string problemSource))
            {
                context.Result = new ObjectResult(
                    new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, (string)language)} - {problemSource}"
                    })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }

            if (!Util.IsBase64String(request.EncryptedData))
            {
                 context.Result = new ObjectResult(
                    new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_ENCRYPTION_DATA,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_ENCRYPTION_DATA, (string)language)
                    })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }

            try
            {
                var deserializedData = Util.DecryptRequest<T>(request.EncryptedData, _settings.EncryptionKey);
                if (deserializedData != null)
                {
                    context.ActionArguments["request"] = deserializedData;
                }
                await next();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Decryption error");
                var error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.UNABLE_TO_DECRYPT,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.UNABLE_TO_DECRYPT,(string)language)
                };
                context.Result = new ObjectResult(error)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
    }


    public class DecryptRequestDataFilterS<T> : IAsyncActionFilter
    {
        public T RequestDataType { get; set; }
        private const string REQUEST = "EncryptedRequestData";
        private const string LANGUAGE = "language";
        private readonly IMessageProvider _messageProvider;
        private readonly ILogger _logger;
        private readonly SystemSettings _settings;
        public DecryptRequestDataFilterS(IMessageProvider messageProvider, ILoggerFactory logger,
                IOptions<SystemSettings> settings)
        {
            _messageProvider = messageProvider;
            _logger = logger.CreateLogger<DecryptRequestDataFilter<T>>();
            _settings = settings.Value;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var hasLanguage = context.ActionArguments.TryGetValue(LANGUAGE, out var language);

            if (hasLanguage == false)
            {
                language = "en";
            }

            if (!context.ActionArguments.TryGetValue(REQUEST, out var output))
            {
                context.Result = new ObjectResult(
                    new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                        ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, (string)language)
                    })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }

            var request = output as BaseEncryptedRequestDTOStandingInstruction;
            
            _logger.LogInformation($"{typeof(T).Name} ENCRYPTED REQUEST ==> {Util.SerializeAsJson(request)}");
            if (!request.IsValid(out string problemSource))
            {
                context.Result = new ObjectResult(
                    new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, (string)language)} - {problemSource}"
                    })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }

            if (!Util.IsBase64String(request.EncryptedData))
            {
                context.Result = new ObjectResult(
                   new ErrorResponse
                   {
                       ResponseCode = ResponseCodes.INVALID_ENCRYPTION_DATA,
                       ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_ENCRYPTION_DATA, (string)language)
                   })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
                return;
            }

            try
            {
                var deserializedData = Util.DecryptRequest<T>(request.EncryptedData, _settings.EncryptionKey);
                if (deserializedData != null)
                {
                    context.ActionArguments["request"] = deserializedData;
                }
                await next();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Decryption error");
                var error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.UNABLE_TO_DECRYPT,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.UNABLE_TO_DECRYPT, (string)language)
                };
                context.Result = new ObjectResult(error)
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
        }
    }

}
