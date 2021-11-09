using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Middleware.Client.Filters;
using Middleware.Core.DTO;
using Middleware.Service;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
namespace Middleware.Client.Controllers
{
    [Route("api/transfer/wallet")]
    public class WalletTransferController : RootController
    {
        static string successActivityResult = "Successful";
        static string failedActivityResult = "Failed";
        private readonly INationalTransferService _service;
        private readonly IUserActivityService _userActivityService;
        public WalletTransferController(INationalTransferService service, IUserActivityService userActivity)
        {
            _service = service;
            _userActivityService = userActivity;
        }




        [HttpPost, Route("toaccount")]
        [ServiceFilter(typeof(SessionFilter))]
        [ServiceFilter(typeof(DuplicateFilter))]
        [TypeFilter(typeof(DecryptRequestDataFilter<AuthenticatedTransferRequest>))]
        //
        public async Task<IActionResult> WalletToAccount([FromBody] BaseEncryptedRequestDTO encryptedRequestData, 
            [FromQuery] AuthenticatedTransferRequest request, [FromQuery] AuthenticatedUser user, [FromHeader] string language, [FromHeader] string authToken,
            [FromHeader] string transRef,[FromQuery] bool saveAsBeneficiary = false)
        {


            var result = await _service.WalletToAccount(request, user, language, saveAsBeneficiary);
            await _userActivityService.AddByUsername(user.UserName, "Wallet to Account", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }


        [HttpPost, Route("towallet")]
        [ServiceFilter(typeof(SessionFilter))]
        [ServiceFilter(typeof(DuplicateFilter))]
        [TypeFilter(typeof(DecryptRequestDataFilter<AuthenticatedTransferRequest>))]
        public async Task<IActionResult> WalletToWallet([FromBody] BaseEncryptedRequestDTO encryptedRequestData, [FromQuery] AuthenticatedTransferRequest request,
            [FromHeader] string transRef,[FromQuery] AuthenticatedUser user, [FromHeader] string language,  [FromHeader] string authToken, [FromQuery] bool saveAsBeneficiary = false)
        {

            var result = await _service.WalletToWallet(request, user, language, saveAsBeneficiary);
            await _userActivityService.AddByUsername(user.UserName, "Wallet to Wallet", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }

            return Ok(result.GetPayload());
        }

        [HttpPost, Route("self")]
        [ServiceFilter(typeof(SessionFilter))]
        [ServiceFilter(typeof(DuplicateFilter))]
        [TypeFilter(typeof(DecryptRequestDataFilter<SelfTransferRequest>))]
        public async Task<IActionResult> WalletToSelf([FromBody] BaseEncryptedRequestDTO encryptedRequestData, [FromQuery] SelfTransferRequest request,
            [FromHeader] string transRef,[FromQuery] AuthenticatedUser user, [FromHeader] string language, [FromHeader] string authToken)
        {

            var result = await _service.WalletToSelf(request, user, language);
            await _userActivityService.AddByUsername(user.UserName, "Wallet to Self", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);

            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        [HttpGet]
        [Route("name/{walletId}/{walletScheme}")]
        [ServiceFilter(typeof(SessionFilter))]
        public async Task<IActionResult> GetExternalWalletName([FromRoute] string walletId, [FromRoute] string walletScheme, [FromHeader] string language, [FromHeader] string authToken)
        {
            var result = await _service.GetWalletName(walletId, walletScheme, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }


        [HttpGet]
        [Route("name/{walletId}")]
        [ServiceFilter(typeof(SessionFilter))]
        public async Task<IActionResult> GetLocalWalletName([FromRoute] string walletId, [FromHeader] string language, [FromHeader] string authToken)
        {
            var result = await _service.GetWalletName(walletId, null, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        [HttpGet]
        [Route("schemes")]
        [ServiceFilter(typeof(SessionFilter))]
        public async Task<IActionResult> GetWalletSchemes([FromHeader] string language, [FromHeader] string authToken)
        {
            var result = await _service.GetWalletSchemes(language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        [HttpGet]
        [Route("charges")]
        [ServiceFilter(typeof(SessionFilter))]
        public async Task<IActionResult> GetTransferCharge(decimal amount, decimal balance, [FromHeader] string language, [FromHeader] string authToken)
        {
            var result = await _service.GetTransferCharge(amount, balance, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }


        #region Standing Instruction
        [HttpPost, Route("toaccount-standignstruction")]
        // [ServiceFilter(typeof(SessionFilter))]
        //[ServiceFilter(typeof(DuplicateFilter))]
        [TypeFilter(typeof(DecryptRequestDataFilterS<AuthenticatedTransferRequest>))]
        //
        public async Task<IActionResult> WalletToAccounts([FromBody]  BaseEncryptedRequestDTOStandingInstruction encryptedRequestData, [FromQuery] AuthenticatedTransferRequest request,
           [FromHeader] string language,   [FromQuery] bool saveAsBeneficiary = false)
        {

            
            string transRef = Guid.NewGuid().ToString();
            var user = encryptedRequestData.User;
            var result = await _service.WalletToAccount(request, user, language, saveAsBeneficiary);
            await _userActivityService.AddByUsername(user.UserName, "Wallet to Account", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }


        [HttpPost, Route("towallet-standignstruction")]
        // [ServiceFilter(typeof(SessionFilter))]
       // [ServiceFilter(typeof(DuplicateFilter))]
        [TypeFilter(typeof(DecryptRequestDataFilterS<AuthenticatedTransferRequest>))]
        public async Task<IActionResult> WalletToWallets([FromBody] BaseEncryptedRequestDTOStandingInstruction  encryptedRequestData,
            [FromQuery] AuthenticatedTransferRequest request,
            [FromHeader] string language,
            [FromQuery] bool saveAsBeneficiary = false)
        {
            
            string transRef = Guid.NewGuid().ToString();
            var user = encryptedRequestData.User;
            var result = await _service.WalletToWallet(request, user, language, saveAsBeneficiary);
            await _userActivityService.AddByUsername(user.UserName, "Wallet to Wallet", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }

            return Ok(result.GetPayload());
        }

        [HttpPost, Route("self-standignstruction")]
        //  [ServiceFilter(typeof(SessionFilter))]
        //[ServiceFilter(typeof(DuplicateFilter))]

        [TypeFilter(typeof(DecryptRequestDataFilterS<SelfTransferRequest>))]
        public async Task<IActionResult> WalletToSelfs([FromBody] BaseEncryptedRequestDTOStandingInstruction  encryptedRequestData, [FromQuery] SelfTransferRequest request,
           
            [FromHeader] string language)
        {
            
            string transRef = Guid.NewGuid().ToString();
            var user = encryptedRequestData.User;
            var result = await _service.WalletToSelf(request, user, language);
            await _userActivityService.AddByUsername(user.UserName, "Wallet to Self", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);

            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        #endregion
    }
}
