using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Middleware.Client.Filters;
using Middleware.Service.DTOs;
using Middleware.Service;
using Microsoft.AspNetCore.Http;
using Middleware.Core.DTO;

namespace Middleware.Client.Controllers
{
    [Route("api/payment")]
    public class PaymentController : RootController
    {
        private readonly IBillsService _billService;

        public PaymentController(IBillsService billService)
        {
            _billService = billService;
        }

        [HttpGet]
        [Route("telcos")]
        [ProducesResponseType(typeof(IEnumerable<BillerInfo>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> GetTelcos(string language)
        {
            var response = await _billService.GetTelcos(language);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<BillerInfo>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Route("billers")]
        public async Task<IActionResult> GetBillers([FromHeader] string language)
        {
            var response = await _billService.GetBillers(language);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }

            return Ok(response.GetPayload());
        }

        [HttpGet]
        [Route("products/{billerCode}")]
        [ProducesResponseType(typeof(IEnumerable<ProductInfo>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<IActionResult> GetProducts(string billerCode, [FromHeader] string language)
        {
            var response = await _billService.GetProducts(billerCode, language);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }

        [HttpPost]
        [Route("validate")]
        [ServiceFilter(typeof(SessionFilter))]
        [ProducesResponseType(typeof(IEnumerable<PaymentValidationResponse>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [TypeFilter(typeof(DecryptRequestDataFilter<PaymentValidationRequest>))]
 
        public async Task<IActionResult> ValidateCustomer([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery] PaymentValidationRequest request, [FromHeader] string language)
        {
            var response = await _billService.Validate(request, language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }


        [HttpPost]
        [Route("airtime")]
        [ServiceFilter(typeof(SessionFilter))]
        //[ServiceFilter(typeof(DuplicateFilter))]
        [ProducesResponseType(typeof(IEnumerable<PaymentResponse>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [TypeFilter(typeof(DecryptRequestDataFilter<AirtimePurchaseRequest>))]
        //
        public async Task<IActionResult> BuyAirtime([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery] AirtimePurchaseRequest request, [FromQuery] AuthenticatedUser user,
            [FromQuery] bool saveBeneficiary, [FromHeader] string language)
        {
            var response = await _billService.BuyAirtime(request, user, saveBeneficiary, language);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }

        [HttpPost]
        [Route("bill")]
        [ServiceFilter(typeof(SessionFilter))]
        //[ServiceFilter(typeof(DuplicateFilter))]
        [ProducesResponseType(typeof(IEnumerable<PaymentResponse>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [TypeFilter(typeof(DecryptRequestDataFilter<BillPaymentRequest>))]
    
        public async Task<IActionResult> PayBill([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery] BillPaymentRequest request, [FromQuery] AuthenticatedUser user,
            bool saveBeneficiary, [FromHeader] string language)
        {
            var response = await _billService.PayBill(request, user, saveBeneficiary, language);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PaymentResponse>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Route("channel_billers")]
        public async Task<IActionResult> GetChannelBillers()
        {
            var response = await _billService.GetChannelBillers();

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }

            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PaymentResponse>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Route("bap_billers")]
        public async Task<IActionResult> GetBapBillers()
        {
            var response = await _billService.GetBapBillers();

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }

            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PaymentResponse>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Route("bap_products")]
        public async Task<IActionResult> GetBapProducts(string slug)
        {
            var response = await _billService.GetBapProducts(slug);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }

            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PaymentResponse>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Route("bap_i_products")]
        public async Task<IActionResult> GetBapIProducts(string slug)
        {
            var response = await _billService.GetBapIProducts(slug);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }

            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PaymentResponse>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Route("airtime_billers")]
        public async Task<IActionResult> GetAirtimeBillers()
        {
            var response = await _billService.GetAirtimeBillers();

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }

            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PaymentResponse>), StatusCodes.Status200OK)]
        [Consumes("application/json")]
        [Produces("application/json")]
        [Route("wildcard")]
        public async Task<IActionResult> Wildcard()
        {
            var response = await _billService.Wildcard();

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }

            return Ok();
        }


       
        [ServiceFilter(typeof(APIKeyFilter))]
        [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<BasicResponse>), StatusCodes.Status200OK)]

        [Consumes("application/json")]
        [Produces("application/json")]
        [Route("payment-add-beneficiary")]
        [TypeFilter(typeof(DecryptRequestDataFilter<AirTimeBenefitiary>))]
        //
        public async Task<IActionResult> AddBeneficiary([FromBody] BaseEncryptedRequestDTO encryptedRequestData,[FromQuery] AirTimeBenefitiary beneficiary, [FromHeader] PaymentType paymentType, [FromHeader] string walletNumber, [FromHeader] string countryId, [FromHeader] string language, [FromHeader] string appKey, [FromHeader] string appId)
        {

            var response = await _billService.SaveAirtimeBeneficiary(beneficiary, paymentType, walletNumber, language, countryId);

            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }

            return Ok(response);


        }



 
    }
}
