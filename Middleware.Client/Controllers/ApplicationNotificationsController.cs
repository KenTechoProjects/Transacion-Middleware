using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware.Service.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Client.Controllers
{
    [Route("api/appnotification")]
    public class ApplicationNotificationsController : RootController
    {
        private readonly ITransactionNotificationService _transactionNotificationService;
        public ApplicationNotificationsController(ITransactionNotificationService transactionNotificationService)
        {
            _transactionNotificationService = transactionNotificationService;
        }

        [HttpGet]
        [Route("front-notification")]
        public async Task<IActionResult> GetCustomerNotificationFront([FromQuery] string walletNumber, string language)
        {
            var response = await _transactionNotificationService.GetTransactionNotificationsForCustomerFront(walletNumber, language);

            if (response.IsSuccessful == true)
            {
                return Ok(response);

            }
            return BadRequest(response);
        }


        [HttpGet]
        [Route("transaction-notifications")]
        public async Task<IActionResult> GetTransactionNotificationsForCustomer([FromQuery] string walletNumber, string language)
        {
            var response = await _transactionNotificationService.GetTransactionNotificationsForCustomer(walletNumber, language);

            if (response.IsSuccessful == true)
            {
                return Ok(response);

            }
            return BadRequest(response);
        }


        [HttpGet]
        [Route("view-transaction")]
        public async Task<IActionResult> UpdateTransactionNotificationsForCustomer([FromQuery] string walletNumber, [FromQuery] long trsantionId, [FromHeader] string language, [FromQuery] bool source = false)
        {
            var response = await _transactionNotificationService.UpdateTransactionNotificationsForCustomer_(walletNumber, language, trsantionId, source);

            if (response.IsSuccessful == true)
            {
                return Ok(response);

            }
            return BadRequest(response);
        }


    }
}
