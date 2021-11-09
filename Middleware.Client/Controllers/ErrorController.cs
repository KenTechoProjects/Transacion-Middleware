using System;
using Microsoft.AspNetCore.Mvc;
using Middleware.Service.DTOs;
using Middleware.Service.Utilities;

namespace Middleware.Client.Controllers
{
    [ApiController]
    public class ErrorController : Controller
    {
        readonly IMessageProvider _provider;

        public ErrorController(IMessageProvider provider)
        {
            _provider = provider;
        }

        [Route("error/{code}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult HandleError(int code, string language)
        {
            var response = new ErrorResponse
            {
                ResponseCode = ResponseCodes.GENERAL_ERROR,
                ResponseDescription = _provider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
            };
            return new ObjectResult(response)
            {
                StatusCode = code
            };
        }
    }
}