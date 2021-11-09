using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware.Core.DTO;
using Middleware.Core.Model;
using Middleware.Service;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Client.Controllers
{
    [Route("api/adminIntegration")]
    public class IntegrationController : RootController
    {
        private readonly IDashboardIntegrationService _dashboardIntegrationService;
        private readonly ICustomerDocumentService _customerDocumentService;
        public IntegrationController(IDashboardIntegrationService dashboardIntegrationService, ICustomerDocumentService customerDocumentService)
        {
            _dashboardIntegrationService = dashboardIntegrationService;
            _customerDocumentService = customerDocumentService;
        }

        [HttpGet("customer-information")]
        [ProducesResponseType(typeof(CustomerDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCustomerDetails(string walletNo, [FromHeader] string langCode)
        {
            var result = await _dashboardIntegrationService.GetCustomerInformaton(walletNo, langCode);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        [HttpPost, Route("device/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ActivateDevice(string deviceId, [FromHeader] string langCode)
        {
            var response = await _dashboardIntegrationService.ActivateDevice(deviceId, langCode);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok();
        }

        [HttpPost, Route("device/de-activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeActivateDevice(string deviceId, [FromHeader] string langCode)
        {
            var response = await _dashboardIntegrationService.DeactivateDevice(deviceId, langCode);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok();
        }

        [HttpGet("customer-devices")]
        [ProducesResponseType(typeof(IEnumerable<CustomerDevice>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCustomerDevices(string walletNo, [FromHeader] string langCode)
        {
            var result = await _dashboardIntegrationService.GetCustomerDevices(walletNo, langCode);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        [HttpPost, Route("profile/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LockProfile(string walletNo, [FromHeader] string langCode)
        {
            var response = await _dashboardIntegrationService.LockProfile(walletNo, langCode);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok();
        }

        [HttpPost, Route("device/release")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeviceRelease(string deviceId, [FromHeader] string langCode)
        {
            var response = await _dashboardIntegrationService.ReleaseDevice(deviceId, langCode);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok();
        }

        [HttpPost, Route("profile/unlock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UnLockProfile(string walletNo, [FromHeader] string langCode)
        {
            var response = await _dashboardIntegrationService.UnlockProfile(walletNo, langCode);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok();
        }

        [HttpGet, Route("customers")]
        [ProducesResponseType(typeof(IEnumerable<UnapprovedDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Customers([FromHeader] string langCode)
        {
            var response = await _customerDocumentService.FetchCustomers(langCode);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }

        [HttpGet, Route("{phoneNumber}")]
        [ProducesResponseType(typeof(IEnumerable<SendDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendCustomerDocuments([FromRoute] string phoneNumber, [FromHeader] string langCode)
        {
            var response = await _customerDocumentService.FetchCustomerDocuments(phoneNumber, langCode);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }

        [HttpPost, Route("{phoneNumber}/{docType}/{status}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCustomerDocuments([FromRoute] string phoneNumber, [FromRoute] DocumentType docType, [FromRoute] bool status, [FromHeader] string langCode)
        {
            //update customer documents and remove the debit freeze from account by calling 
            //PnD endpoint if approvals are successful for all docs.

            var response = await _customerDocumentService.UpdateCustomerDocument(phoneNumber, docType, status, langCode);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok();
        }
    }
}
