using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Middleware.Service;
using Middleware.Service.DTOs;
using Middleware.Service.Utilities;
using Middleware.Client.Filters;
using Middleware.Service.Model;
using Middleware.Service.Processors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Middleware.Core.DTO;

namespace Middleware.Client.Controllers
{
    [Route("api/auth")]
    public class AuthenticationController : RootController
    {
        private readonly IAuthenticationServices _service;
        private readonly IUserActivityService _userActivityService;
        static string successActivityResult = "Successful";
        static string failedActivityResult = "Failed";
        private readonly IAuthenticator _authenticator;
        private readonly IImageManager _imageManager;

        public AuthenticationController(IAuthenticationServices service, IUserActivityService userActivityService, IAuthenticator authenticator, IImageManager imageManager)
        {
            _service = service;
            _userActivityService = userActivityService;
            _authenticator = authenticator;
            _imageManager = imageManager;
        }
        [HttpPost, Route("Test-Image-Overwrite")]
 
        //[AllowAnonymous][ApiExplorerSettings(IgnoreApi =true)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
 
        [AllowAnonymous][ApiExplorerSettings(IgnoreApi =true)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
 
        public async Task<IActionResult> Test([FromHeader]string identifier, [FromHeader] string fileName, [FromHeader]  DocumentType documentType
            ,[FromBody] PhotoUpdateRequest request, [FromHeader] string language)
        {

             var resu = await _imageManager.SaveImage(identifier, fileName, documentType, request.Picture);


            return Ok(resu);
        }

        [HttpPost, Route("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [TypeFilter(typeof(DecryptRequestDataFilter<LoginRequest>))]
        public async Task<IActionResult> Authenticate([FromQuery] LoginRequest request, [FromBody] BaseEncryptedRequestDTO encryptedRequestData, [FromHeader] string language)
        {
            var result = await _service.Authenticate(request, language);

            await _userActivityService.AddByUsername(request.UserName, "Login", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);



            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }


        [HttpPost, Route("logout")]
        public async Task<IActionResult> Logout([FromHeader] string authToken, [FromHeader] string phoneNumber)
        {
            await _userActivityService.AddByUsername(phoneNumber, "Logout", successActivityResult, "");
            await _service.EndSession(authToken);



            return Ok();
        }


        [HttpGet, Route("security-question")]
        [ProducesResponseType(typeof(Service.Onboarding.SecurityQuestion), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSecurityQuestion(string username, [FromHeader] string language)
        {
            var result = await _service.GetSecurityQuestion(username, language);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }
      
        [HttpPost("validate-user-pin")]
        [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status400BadRequest)]
        [TypeFilter(typeof(DecryptRequestDataFilter<VeriFyPin>))]
       [ServiceFilter(typeof(SessionFilter))]
        public async Task<IActionResult> ValidatePin([FromBody] BaseEncryptedRequestDTO encryptedRequestData, [FromQuery] VeriFyPin request,[FromQuery] AuthenticatedUser user, [FromHeader] string authToken
           ,
            [FromHeader] string language)
        {
            var response = await _authenticator.ValidatePin(request.WalletNumber, request.Pin);
            if (response.IsSuccessful == true)
            {
                response.FaultType = FaultMode.NONE;
                return Ok(response);
            }
            return CreateResponse(response.Error, response.FaultType);
        }

    }

}
