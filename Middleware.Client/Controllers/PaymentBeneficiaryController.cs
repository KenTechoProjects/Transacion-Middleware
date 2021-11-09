using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware.Client.Filters;
using Middleware.Core.DTO;
using Middleware.Service;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Middleware.Client.Controllers
{
    [Route("api/beneficiary/payment")]
    [ServiceFilter(typeof(SessionFilter))]
   
    public class PaymentBeneficiaryController : RootController
    {
        readonly IPaymentBeneficiaryService _service;
        private readonly IUserActivityService _userActivityService;
        static string successActivityResult = "Successful";
        static string failedActivityResult = "Failed";
        private readonly IBillsService _billService;

        public PaymentBeneficiaryController(IPaymentBeneficiaryService service, IUserActivityService activityService, IBillsService billService)
        {
            _service = service;
            _userActivityService = activityService;
            _billService = billService;
        }

        [HttpGet("fetch-payment-beneficiaries-for-customer")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetBeneficiaries([FromQuery]AuthenticatedUser user, [FromHeader] string countryId, [FromHeader]string language)
        {

         var response=await   _billService.GetPaymentBeneficiariesAsyc(user.WalletNumber, countryId, language);
          //var result = await _service.GetBeneficiaries(user.UserName, language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response);

        }


    
        //[TypeFilter(typeof(UserActivityFilter), Arguments = new object[] { "Add Beneficiary" })]
   
        [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<BasicResponse>), StatusCodes.Status200OK)]

        [Consumes("application/json")]
        [Produces("application/json")]
        [Route("payment-add-beneficiary/{countryId}")]
        [TypeFilter(typeof(DecryptRequestDataFilter<NewPaymentBeneficiaryPaymentRequest>))]
        //
        public async Task<IActionResult> AddBeneficiary([FromBody] BaseEncryptedRequestDTO encryptedRequestData, [FromQuery] NewPaymentBeneficiaryPaymentRequest request,   [FromQuery]AuthenticatedUser user, [FromHeader]string language,  string countryId, [FromHeader] string authToken)
        {
            var response = await _service.AddBeneficiary(request, user, language, countryId);
            await _userActivityService.AddByUsername(user.UserName, "Add Beneficiary", response.IsSuccessful ? successActivityResult : failedActivityResult, response.Error?.ResponseDescription);


            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            
            return Ok(response);
        }

        [HttpPost("{beneficiaryID}")]
       // [TypeFilter(typeof(UserActivityFilter), Arguments = new object[] { "Remove Beneficiary" })]
       [ApiExplorerSettings(IgnoreApi =true)]
        [TypeFilter(typeof(DecryptRequestDataFilter<Answer>))]
        //
        public async Task<IActionResult> RemoveBeneficiary([FromBody] BaseEncryptedRequestDTO encryptedRequestData,string beneficiaryID, [FromQuery]Answer answer, [FromQuery]AuthenticatedUser user, [FromHeader] string language)
        {
            
            
             var result = await _service.DeleteBeneficiary(beneficiaryID, user.UserName, answer, language);
             
            await _userActivityService.AddByUsername(user.UserName, "Remove Beneficiary", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result);
        }

       //[ServiceFilter(typeof(ValdateTokenFilter))]
        [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<BasicResponse>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Route("payment-remove-beneficiary")]
        [TypeFilter(typeof(DecryptRequestDataFilter<>))]
        //
        public async Task<IActionResult> DeleteBeneficiary([FromBody] BaseEncryptedRequestDTO encryptedRequestData, [FromQuery] AuthenticatedUser user,[FromQuery] RemovePaymentBeneficiaryPaymentRequest request, [FromHeader] string language, [FromHeader] string authToken)
        {
            //RemovePaymentBeneficiaryPaymentRequest request,AuthenticatedUser user,
            var response = await _billService.DeletePaymentBeneficiariesAsyc(request, user, language);
            await _userActivityService.AddByUsername(user.UserName, "Remove Beneficiary", response.IsSuccessful ? successActivityResult : failedActivityResult, response.Error?.ResponseDescription);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
               
            }

            return Ok(response);
        }


       // [ServiceFilter(typeof(ValdateTokenFilter))]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BasicResponse>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Route("fetch-payment-beneficiaries-for-customer/{countryId}")]
        public async Task<IActionResult> GetBeneficiary([FromQuery] AuthenticatedUser user, string countryId,  [FromHeader] string language, [FromHeader] string authToken)
        {

            var response = await _billService.GetPaymentBeneficiariesAsyc(user.WalletNumber, countryId, language);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }

            return Ok(response);


        }

    }
}
