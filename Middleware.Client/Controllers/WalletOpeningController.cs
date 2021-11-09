using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware.Client.Filters;
using Middleware.Core.DTO;
using Middleware.Service;
using Middleware.Service.DTOs;

namespace Middleware.Client.Controllers
{
    [Route("api/walletopening")]
    public class WalletOpeningController : RootController
    {
        private readonly IWalletOpeningService _service;

        public WalletOpeningController(IWalletOpeningService service)
        {
            _service = service;
        }


        [HttpPost]
        [TypeFilter(typeof(DecryptRequestDataFilter<WalletInitialisationRequest>))]
        public async Task<IActionResult> Initialise([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]WalletInitialisationRequest request, [FromHeader]string language)
        {
            
            var result = await _service.InitialiseWallet(request, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            //
            return Ok(result.GetPayload());

        }
        [HttpGet]
        [Route("check-gen-pin")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> ChcekcGetUniqueReferralCode([FromHeader] string language)
        {
            var data = await _service.GetUniqueReferralCode();
            return Ok(data);
        }


        [HttpPost, Route("idDocument")]
        [TypeFilter(typeof(DecryptRequestDataFilter<IdUpdateRequest>))]
        //
        public async Task<IActionResult> AddIdentification([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]IdUpdateRequest request, [FromHeader]string language)
        {
        
            var result = await _service.AddIdentificationDocument(request, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result);
        }

        [HttpPost, Route("photo")]
        [ProducesResponseType(typeof(ServiceResponse<PhotoUploadResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<PhotoUploadResponse>), StatusCodes.Status400BadRequest)]
        [TypeFilter(typeof(DecryptRequestDataFilter<PhotoUpdateRequest>))]
        //
        public async Task<IActionResult> AddPhotograph([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery] PhotoUpdateRequest request, [FromHeader] string language)
        {
            var result = await _service.AddPhoto(request, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result.GetPayload());
        }

        [HttpPost, Route("validationCode")]
        public async Task<IActionResult> ValidatePhone([FromHeader]string phoneNumber, [FromHeader]string language)
        {
            var result = await _service.SendValidationCode(phoneNumber, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result);
        }

        [HttpPost]
        [Route("complete-wallet-opening")]
        [ProducesResponseType(typeof(WalletCompletionRequest), StatusCodes.Status201Created)]
        [ProducesErrorResponseType(typeof(WalletCompletionRequest))]
        [TypeFilter(typeof(DecryptRequestDataFilter<WalletCompletionRequest>))]
        //
        public async Task<IActionResult> Complete([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery] WalletCompletionRequest request, [FromHeader]string language)
        {

            var result = await _service.Complete(request, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Created("api/walletopening/complete-wallet-opening", result.GetPayload());
        }

        [HttpGet, Route("status")]
        public async Task<IActionResult> GetStatus([FromHeader]string phoneNumber, [FromHeader]string language)
        {
            var result = await _service.GetWalletOpeningStatus(phoneNumber, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }

            return Ok(result.GetPayload());
        }

        [HttpPost, Route("biodata")]
        [TypeFilter(typeof(DecryptRequestDataFilter<Biodata>))]
        //
        public async Task<IActionResult> UpdatePersonalDetails([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromHeader]string phoneNumber,[FromQuery] Biodata request, [FromHeader]string language)
        {
            var result = await _service.UpdatePersonalInformation(phoneNumber, request, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result);
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost, Route("createotpsample")]
        public async Task<IActionResult> CreateOTP([FromHeader]string phoneNumber, [FromHeader]string language)
        {
            var result = await _service.CreateOTP(phoneNumber, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            result.FaultType = FaultMode.NONE;
            return Ok(result);
        }
        
        
        [HttpGet][Route("validate-referral-code")]
        [ProducesResponseType(typeof(ReferralCodeResponse),StatusCodes.Status200OK )]
        [ProducesResponseType(typeof(ReferralCodeResponse),StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateReferralCode([FromHeader] string referralCode, [FromHeader] string language)
        {
            var response = await _service.ReferralCodeExists(referralCode, language);
            if (response.Exists==true)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        [Route("senegal-code")][HttpGet]
        public async Task<IActionResult> SenegalCode()
        {
          var bankCode=await Task.FromResult( _service.SenegalCode());
          return Ok(bankCode);
        }
        

    }
}
