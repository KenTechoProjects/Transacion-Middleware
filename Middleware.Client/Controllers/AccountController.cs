using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Middleware.Service;
using Middleware.Client.Filters;
using Middleware.Service.DTOs;
using Microsoft.AspNetCore.Http;
using Middleware.Core.DTO;
using Microsoft.AspNetCore.Authorization;

namespace Middleware.Client.Controllers
{
    [Route("api/account")]
    [ServiceFilter(typeof(SessionFilter))]
    public class AccountController : RootController
    {
        readonly IAccountService _service;

        public AccountController(IAccountService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(CompositeAccountData), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccounts([FromQuery]AuthenticatedUser user, [FromHeader]string language, [FromHeader] string authToken)
        {
            var response = await _service.GetCustomerAccounts(user, language);
            if(!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }

        [HttpGet]
        [Route("bank")]
        public async Task<IActionResult> GetBankAccounts([FromQuery]AuthenticatedUser user, [FromHeader]string language, [FromHeader] string authToken)
        {
            var response = await _service.GetAccounts(user.BankId, language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }

        [HttpGet]
        [Route("wallet")]
        public async Task<IActionResult> GetWallet([FromQuery]AuthenticatedUser user, [FromHeader]string language, [FromHeader] string authToken)
        {
            var response = await _service.GetWallet(user.WalletNumber, language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            return Ok(response.GetPayload());
        }

        [HttpGet]
        [Route("{accountIdentifier}/transactions")]
        [ProducesResponseType(typeof(IEnumerable<StatementRecord>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecentTransactions(string accountIdentifier, [FromQuery]AccountType accountType, [FromQuery]AuthenticatedUser user, [FromHeader]string language, [FromHeader] string authToken)
        {
            var response = await _service.GetRecentTransactions(accountIdentifier, accountType, user, language);
            if(!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            var transactions = response.GetPayload();
            transactions = transactions.OrderByDescending(s => s.Date);
            return Ok(transactions);
        }

 

        [HttpPost]
        [Route("transactions-with-date")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<StatementRecord>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<StatementRecord>), StatusCodes.Status400BadRequest)]
        [TypeFilter(typeof(DecryptRequestDataFilter<GetTranactionByDateRrange>))]
        public async Task<IActionResult> TranssactionHistoryWithateRage([FromQuery] AccountType accountType, [FromQuery] AuthenticatedUser user, [FromHeader] string authToken,
            [FromHeader] string language,[FromQuery] GetTranactionByDateRrange request ,[FromBody] BaseEncryptedRequestDTO encryptedRequestData )
        {
            
            //string accountIdentifier, AccountType accountType, AuthenticatedUser user,DateTime startDate,DateTime endDate, string language
            var response = await _service.GetRecentTransactionsWithDaterange(request.TransactionIdentifier, accountType,
                user, request.StartDate, request.EndDate, language);
            if (!response.IsSuccessful)
            {
                return CreateResponse(response.Error, response.FaultType);
            }
            var transactions = response.GetPayload();
            transactions = transactions.OrderByDescending(s => s.Date);
            return Ok(transactions);
        }
    }
}
