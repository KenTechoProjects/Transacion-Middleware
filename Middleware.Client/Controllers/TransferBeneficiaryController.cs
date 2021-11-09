using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Middleware.Service;
using Middleware.Client.Filters;
using Middleware.Service.DTOs;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Middleware.Service.Processors;
using Middleware.Core.DTO;

namespace Middleware.Client.Controllers
{
    [Route("api/beneficiary/transfer")]
  [ServiceFilter(typeof(SessionFilter))]
    public class TransferBeneficiaryController : RootController
    {
        static string successActivityResult = "Successful";
        static string failedActivityResult = "Failed";
        readonly ITransferBeneficiaryService _service;
        private readonly IUserActivityService _userActivityService;
       
        public TransferBeneficiaryController(ITransferBeneficiaryService service, IUserActivityService userActivityService)
        {
            _service = service;
            _userActivityService = userActivityService;
             
        }

        [HttpGet][ProducesResponseType(typeof(TransferBeneficiary), StatusCodes.Status200OK)]
        [HttpGet][ProducesErrorResponseType(typeof(ErrorResponse))]
        public async Task<IActionResult> GetBeneficiaries([FromQuery]AuthenticatedUser user, [FromHeader] string language, [FromHeader] string authToken)
        {
            var result = await _service.GetBeneficiaries(user, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());

        }


        [HttpPost]
         //[TypeFilter(typeof(UserActivityFilter), Arguments = new object[] { "Add Beneficiary" })]
        [TypeFilter(typeof(DecryptRequestDataFilter<NewTransferBeneficiaryRequest>))]
        //
        public async Task<IActionResult> AddBeneficiary([FromBody] BaseEncryptedRequestDTO encryptedRequestData, [FromQuery] NewTransferBeneficiaryRequest request, [FromQuery]AuthenticatedUser user, [FromHeader] string language, [FromHeader] string authToken)
        {
           
            var result = await _service.AddBeneficiary(request, user, language);
          await _userActivityService.AddByUsername(user.UserName, "Add Beneficiary", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result);
        }

        [HttpPost("{beneficiaryID}")]
        //  [TypeFilter(typeof(UserActivityFilter), Arguments = new object[] { "Remove Beneficiary" })]
        [TypeFilter(typeof(DecryptRequestDataFilter<Answer>))]
        //
        public async Task<IActionResult> RemoveBeneficiary([FromBody] BaseEncryptedRequestDTO encryptedRequestData,string beneficiaryID, [FromQuery]Answer request, [FromQuery]AuthenticatedUser user,[FromHeader] string language, [FromHeader] string authToken)
        {
            var result = await _service.DeleteBeneficiary(beneficiaryID, user, request, language);
           await   _userActivityService.AddByUsername(user.UserName, "Remove Beneficiary", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result);
        }
    }
}
