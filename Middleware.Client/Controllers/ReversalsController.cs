using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Middleware.Service.Processors;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Middleware.Client.Controllers
{
    [Route("api/reversals")]
    public class ReversalsController : RootController
    {
        
        readonly IReversalService _service;

        public ReversalsController(IReversalService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("account")]
        public async Task<IActionResult> AccountTransactionReversals()
        {  await _service.ReverseAccountTransactions();            
            return Ok();
        }

        [HttpPost]
        [Route("wallet")]
        public async Task<IActionResult> WalletTransactionReversals()
        {
             await _service.ReverseWalletTransactions();
            
            return Ok();
        }

    }
}
