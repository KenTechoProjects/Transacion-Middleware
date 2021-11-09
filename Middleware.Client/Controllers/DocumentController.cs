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
using Middleware.Service.Model;
using Middleware.Service.Processors;

namespace Middleware.Client.Controllers
{
    [Route("api/documents")]
   // [ServiceFilter(typeof(SessionFilter))]
    public class DocumentController : RootController
    {
        private readonly IDocumentService _service;
        private readonly IUserActivityService _userActivityService;
        static string successActivityResult = "Successful";
        static string failedActivityResult = "Failed";
        public DocumentController(IDocumentService service, IUserActivityService activityService)
        {
            _service = service;
            _userActivityService = activityService;
        }

        [HttpPost]
        [TypeFilter(typeof(DecryptRequestDataFilter<DocumentVerificationRequest>))]
        //
        public async Task<IActionResult> Submit([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery] DocumentVerificationRequest request,
            [FromQuery]AuthenticatedUser user, string language)
        {
            var response = await _service.CreateRequest(user.Id, request, language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response);//TODO: Switch to 201
        }

        [HttpGet]
        [ProducesResponseType(typeof(CaseInfo), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetStatus([FromQuery]AccountType type,
            [FromQuery]AuthenticatedUser user, [FromHeader]string language,[FromHeader] string authToken)
        {
            var response = await _service.GetCaseDetails(user.Id, type, language);
            if(!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }

        [HttpPost, Route("{reference}")]
        [ApiExplorerSettings(IgnoreApi =true)]
        [TypeFilter(typeof(DecryptRequestDataFilter<DocumentData>))]
        //
        public async Task<IActionResult> UpdateDocument([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromRoute] string reference,
            [FromQuery]AuthenticatedUser user, [FromQuery]DocumentData documentData,[FromHeader] string language,[FromHeader] string authToken)
        {
            var response = await _service.UpdateDocument(user.Id, documentData, reference, language);
            await _userActivityService.AddByUsername(user.UserName, "Update Document", response.IsSuccessful ? successActivityResult : failedActivityResult, response.Error?.ResponseDescription);


            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response);
        }
        

        [HttpGet, Route("bio-tag")]
        [ApiExplorerSettings(IgnoreApi = true)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBiometricTag( [FromHeader] string authToken)
        {
            var result = await _service.GetBiometricTag();
            if (string.IsNullOrEmpty(result))
            {
                return BadRequest();
            }
            return Ok(result);
        }

        [HttpPost, Route("kyc-single-upload/{dockType}")]
        [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicResponse), StatusCodes.Status400BadRequest)]
        [TypeFilter(typeof(DecryptRequestDataFilter<KYCUploadRequest_>))]
       
        public async Task<IActionResult> UploadKYCDocs([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]KYCUploadRequest_ request, DocumentType dockType,  [FromHeader]string language, [FromHeader] string authToken)
        {
            var response = await _service.UploadWalletOpeninggDocs(request, dockType, language);
            await _userActivityService.AddByUsername(request.WalletNumber, "Upload KYC", response.IsSuccessful ? successActivityResult : failedActivityResult, response.Error?.ResponseDescription);


            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
       
            return Ok(response);
        }

        [HttpGet, Route("kyc-document-types")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(typeof(List<DocumentType>), StatusCodes.Status200OK)]
        public async Task<IActionResult> KYCDoumentTypes([FromHeader]string language, [FromHeader] string authToken)
        {
            var response = await _service.DocumentTypes(language);            
            return Ok(response);
        }

        [HttpGet, Route("{walletNumber}/get-uploads")]
        [ProducesResponseType(typeof(IEnumerable<DocumentStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<DocumentStatusResponse>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DocumentStatus([FromRoute]string walletNumber, string language, [FromHeader] string authToken)
        {
            var response = await _service.GetCustomerDocumentsStatus(walletNumber, language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
           var data = new ServiceSuccessResponse {  PayLoad=response.GetPayload(), IsSuccessfull=response.IsSuccessful   };
            return Ok(data);
        }
    }
}
