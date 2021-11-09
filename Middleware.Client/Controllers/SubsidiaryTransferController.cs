using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Middleware.Client.Filters;
using Middleware.Core.DTO;
using Middleware.Service;
using Middleware.Service.DTOs;

namespace Middleware.Client.Controllers
{
    [Route("api/transfer/subsidiaries")]
    public class SubsidiaryTransferController : RootController
    {
        private ISubsidiaryTransferService _service;

        public SubsidiaryTransferController(ISubsidiaryTransferService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetSubsidiaries()
        {
            var subsidiaries = await _service.GetSubsidiaries();
            if(subsidiaries == null || !subsidiaries.Any())
            {
                return NotFound();
            }
            return Ok(subsidiaries);
        }

        [HttpGet]
        [Route("name/{accountNumber}/{countryID}")]
        [ServiceFilter(typeof(SessionFilter))]
        public async Task<IActionResult> GetAccountName([FromRoute]string accountNumber, [FromRoute]string countryID, [FromHeader]string language)
        {
            var result = await _service.GetAccountName(accountNumber, countryID, language);
            if(!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        [HttpGet]
        [Route("rate/{sourceCurrency}/{targetCurrency}")]
        [ServiceFilter(typeof(SessionFilter))]
        public async Task<IActionResult> GetRate([FromRoute]string sourceCurrency, [FromRoute]string targetCurrency, [FromHeader]string language)
        {
            var result = await _service.GetForexRate(sourceCurrency, targetCurrency, language);
            if(!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        [HttpGet]
        [Route("fees/{amount}")]
        [ServiceFilter(typeof(SessionFilter))]
        public async Task<IActionResult> GetTransactionCharge([FromRoute]decimal amount, [FromHeader]string language)
        {
            var result = await _service.GetCharges(amount, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        [HttpPost]
        [ServiceFilter(typeof(SessionFilter))]
        [TypeFilter(typeof(DecryptRequestDataFilter<CrossBorderTransferRequest>))]
        //
        public async Task<IActionResult> Transfer([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery]CrossBorderTransferRequest request,
            string customerID, [FromHeader] string language, [FromQuery] bool saveAsBeneficiary = false)
        {
            var result = await _service.Transfer(request, customerID, language, saveAsBeneficiary);
            if(!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

    }
}
