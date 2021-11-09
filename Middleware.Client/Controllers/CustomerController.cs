    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Middleware.Service;
using Middleware.Service.DTOs;
using Middleware.Service.Utilities;
using Middleware.Client.Filters;
using Middleware.Service.Onboarding;
using Middleware.Service.Processors;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Middleware.Core.DTO;

namespace Middleware.Client.Controllers
{
    [Route("api/customer")]
    public class CustomerController : RootController
    {
        readonly ICustomerService _service;
        private readonly IUserActivityService _userActivityService;
        private readonly IAuthenticator  _authenticator;
        static string successActivityResult = "Successful";
        static string failedActivityResult = "Failed";
        private readonly ILogger _logger;

        public CustomerController(ICustomerService service, IUserActivityService userActivityService, ILoggerFactory logger, IAuthenticator authenticator)
        {
            _service = service;
            _userActivityService = userActivityService;
            _logger = logger.CreateLogger<CustomerController>();
            _authenticator = authenticator;
        }

        [HttpPost,Route("change-password")]
        [ServiceFilter(typeof(SessionFilter))]
       // [TypeFilter(typeof(UserActivityFilter), Arguments = new object[] { "Change Password" })]
        [TypeFilter(typeof(DecryptRequestDataFilter<ChangePasswordRequest>))]
        public async Task<IActionResult> ChangePassword([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]ChangePasswordRequest request, [FromQuery]AuthenticatedUser user, [FromHeader]string language, [FromHeader] string authToken)
        {
            var result = await _service.ChangePassword(request, user.UserName, language);

            await _userActivityService.AddByUsername(user.UserName, "Change Password", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result);
        }

        [HttpPost,Route("pin")]
        [ServiceFilter(typeof(SessionFilter))]
        //[TypeFilter(typeof(UserActivityFilter), Arguments = new object[] { "Change Pin" })]
        [TypeFilter(typeof(DecryptRequestDataFilter<ChangePinRequest>))]
 
        public async Task<IActionResult> ChangePin([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]ChangePinRequest request, [FromQuery]AuthenticatedUser user, [FromHeader]string language, [FromHeader] string authToken)
        {
            _logger.LogInformation("inside ChangePin cotroller Request==>", JsonConvert.SerializeObject(new {request=request, user=user }));
            var result = await _service.ChangePin(request, user.UserName, language);
           await _userActivityService.AddByUsername(user.UserName, "Change PIN", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result);
        }
       
        [HttpPost, Route("reset-password")]
        [TypeFilter(typeof(DecryptRequestDataFilter<ResetPasswordRequest>))]
    
        public async Task<IActionResult> ResetPassword([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]ResetPasswordRequest request, [FromHeader] string language, [FromHeader] string authToken)
        {
            var result = await _service.ResetPassword(request, language);
            _logger.LogInformation("inside ChangePin cotroller Request==>", JsonConvert.SerializeObject(request));
            await _userActivityService.AddByUsername(request.UserName, "Reset Password", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result);
        }

        [HttpPost, Route("validate-question")]
        [ServiceFilter(typeof(SessionFilter))]
        [TypeFilter(typeof(DecryptRequestDataFilter<Answer>))]       
        public async Task<IActionResult> ValidateAnwser([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery] Answer request, [FromQuery] AuthenticatedUser user, [FromHeader] string language, [FromHeader] string authToken)
        {
            var result = await _authenticator.ValidateAnswer(user.Id.ToString(), request);
          
             
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result);
        }


        [HttpPost, Route("reset-questions")]
        [TypeFilter(typeof(DecryptRequestDataFilter<ResetQuestionsRequest>))]
      
        public async Task<IActionResult> ResetQuestions([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]ResetQuestionsRequest request, [FromHeader]string language, [FromHeader] string authToken)
        {
            var result = await _service.ResetQuestion(request, language);
            _logger.LogInformation("inside ChangePin cotroller Request==>", JsonConvert.SerializeObject(   request ));
            await _userActivityService.AddByUsername(request.Username, "Reset Questions", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);

            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result);
        }

        [HttpPost, Route("reset-pin")]
        [ServiceFilter(typeof(SessionFilter))]
        // [TypeFilter(typeof(UserActivityFilter), Arguments = new object[] { "Reset Pin" })]
        [TypeFilter(typeof(DecryptRequestDataFilter<ResetPinRequest>))]
       
        public async Task<IActionResult> ResetPin([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]ResetPinRequest request, [FromQuery]AuthenticatedUser user, [FromHeader]string language, [FromHeader] string authToken)
        {
            _logger.LogInformation("inside ChangePin cotroller Request==>", JsonConvert.SerializeObject(new { request = request, user = user }));
            var result = await _service.ResetPin(request, user.UserName, language);
            await _userActivityService.AddByUsername(user.UserName, "Reset Pin", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result);
        }


        [HttpGet, Route("selfie")]
        [ServiceFilter(typeof(SessionFilter))]
        public async Task<IActionResult> GetSelfie([FromQuery]AuthenticatedUser user, [FromHeader] string language, [FromHeader] string authToken)
        {
            var result = await _service.GetSelfie(user.WalletNumber, language);
  await _userActivityService.AddByUsername(user.UserName, "Take Selfie", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);

            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
           
            return Ok(result.GetPayload());
        }

        
        [HttpPost, Route("send-otp")]
        [TypeFilter(typeof(DecryptRequestDataFilter<OtpRequest>))]
      
        public async Task<IActionResult> SendOtp( [FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]OtpRequest request, [FromHeader] string language, [FromHeader] string authToken)
        {
            var result = await _service.SendOtp(request, language);

             await _userActivityService.AddByUsername(request.UserName, "Send OTP", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result);
        }

        [HttpPost, Route("send-otp-question")]
        [TypeFilter(typeof(DecryptRequestDataFilter<OtpRequest>))]
  
        public async Task<IActionResult> SendOtpNew( [FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]OtpRequest request, [FromHeader] string language,[FromHeader] string authToken)
        {
            var result = await _service.SendOtpNew(request, language);

          await _userActivityService.AddByUsername(request.UserName, "Send OTP", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result);
        }

        [HttpPost, Route("initiate-password-reset")]
        [TypeFilter(typeof(DecryptRequestDataFilter<InitiatePasswordResetRequest>))]
        // 
        public async Task<IActionResult> InitiatePasswordReset([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]InitiatePasswordResetRequest request, [FromHeader]string language, [FromHeader] string authToken)
        {
            var result = await _service.InitiatePasswordReset(request, language);

          await _userActivityService.AddByUsername(request.Username, "Initiate Reset Password", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result);
        }
    }

}
