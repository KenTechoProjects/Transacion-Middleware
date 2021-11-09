using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Middleware.Service.DTOs;

namespace Middleware.Client.Controllers
{
    [ApiController]
    public class RootController : ControllerBase
    {
        protected IActionResult CreateResponse(ErrorResponse error, FaultMode category)
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
                    code =  (int)HttpStatusCode.BadGateway;
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
        protected IActionResult CreateResponse(ErrorResponseT error, FaultMode category)
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
