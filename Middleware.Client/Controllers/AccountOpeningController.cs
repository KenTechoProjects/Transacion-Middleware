using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Client.Controllers
{
    [Route("api/accountopening")]
   
    public class AccountOpeningController : RootController
    {
 private readonly IUserActivityService _userActivityService;
        private readonly IAccountOpeningService _accountOpening;
        public AccountOpeningController(IAccountOpeningService accountOpening, IUserActivityService userActivityService)
        {
            _accountOpening = accountOpening;
            _userActivityService = userActivityService;
        }

        [HttpPost("haswallet")]
        public async Task<IActionResult> AccountOnboardingForCustomerThatHasWallet([FromBody] AccountOpeningRequestForCustomerWithWallet request, [FromHeader] string language)
        {
            var response = await _accountOpening.AccountOnboarding(request, language);
            if (response.IsSuccessful == true)
            {
                return Ok(response.GetPayload());
            }
            else
            {
                return CreateResponse(response.Error, response.FaultType);
            }
        }
        [HttpPost("hasnowallet")]
        public async Task<IActionResult> AccountOnboardingForCustomerThatHasNWallet([FromBody] AccountOpeningCompositRequest request, [FromHeader] string language)
        {
            var response = await _accountOpening.AccountOnboarding(request, language, true);
            if (response.IsSuccessful == true)
            {
                return Ok(response.GetPayload());
            }
            else
            {
                return CreateResponse(response.Error, response.FaultType);
            }
        }

    }
}
