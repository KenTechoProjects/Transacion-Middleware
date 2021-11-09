using Microsoft.AspNetCore.Mvc;
using Middleware.Core.DTO;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Client.Controllers
{
    [Route("api/document")]
    public class CustomerDocumentServiceController : RootController
    {
        public readonly ICustomerDocumentService _customerDocService;
        private readonly IUserActivityService _userActivityService;
        static string successActivityResult = "Successful";
        static string failedActivityResult = "Failed";
        public CustomerDocumentServiceController(ICustomerDocumentService customerDocService, IUserActivityService activityService)
        {
            _customerDocService = customerDocService;
            _userActivityService = activityService;
        }

        [HttpGet, Route("customers")]
        public async Task<IActionResult> Customers([FromHeader]string language)
        {
            var response = await _customerDocService.FetchCustomers(language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }

        [HttpGet, Route("{phoneNumber}")]
        public async Task<IActionResult> SendCustomerDocuments([FromRoute]string phoneNumber, string language)
        {
            var response = await _customerDocService.FetchCustomerDocuments(phoneNumber, language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }

        [HttpGet, Route("{phoneNumber}/{docType}")]
        public async Task<IActionResult> SendCustomerDocument([FromRoute]string phoneNumber, [FromRoute]DocumentType docType, string language)
        {
            var response = await _customerDocService.FetchCustomerDocument(phoneNumber, docType, language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }
        //[HttpGet("all-customer-documents")]
        //public async Task<IActionResult>CustomersDocuments(string phoneNumber)
        //{

        //}

        [HttpGet, Route("{phoneNumber}/{docType}/{status}")]
        public async Task<IActionResult> UpdateCustomerDocumentsOld([FromRoute]string phoneNumber, [FromRoute]DocumentType docType, [FromRoute]bool status,[FromHeader] string language)
        {
            //update customer documents and remove the debit freeze from account by calling 
            //PnD endpoint if approvals are successful for all docs.

            var response = await _customerDocService.UpdateCustomerDocument(phoneNumber, docType, status, language);
            await _userActivityService.AddByUsername(phoneNumber, "Update Customer Document", response.IsSuccessful ? successActivityResult : failedActivityResult, response.Error?.ResponseDescription);


            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response);
        }
    [HttpPost, Route("update-document-status")]
        public async Task<IActionResult> UpdateCustomerDocuments([FromBody] UpdateCustomerdocumentStatus customerdocumentStatus,[FromHeader] string language)
        {
            //update customer documents and remove the debit freeze from account by calling 
            //PnD endpoint if approvals are successful for all docs.
            if(customerdocumentStatus.IsValid(out string message)==false)
            {
                return CreateResponse(new Service.DTOs.ErrorResponse {  ResponseCode=ResponseCodes.INVALID_INPUT_PARAMETER,ResponseDescription =message},  FaultMode.CLIENT_INVALID_ARGUMENT);
            }
            var response = await _customerDocService.UpdateCustomerDocument(customerdocumentStatus.PhoneNuumber, customerdocumentStatus.DocumentType, customerdocumentStatus.DocumentStatus, language);
            await _userActivityService.AddByUsername(customerdocumentStatus.PhoneNuumber, "Update Customer Document", response.IsSuccessful ? successActivityResult : failedActivityResult, response.Error?.ResponseDescription);


            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response);
        }
   
    
    
    }
}
