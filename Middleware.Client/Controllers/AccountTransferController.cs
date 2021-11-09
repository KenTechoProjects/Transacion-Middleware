using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Middleware.Client.Filters;
using Middleware.Service;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Newtonsoft.Json;
using Middleware.Service.Extensions;
using Middleware.Core.DTO;
using Microsoft.AspNetCore.Http;

namespace Middleware.Client.Controllers
{
    [Route("api/transfer/account")]
    public class AccountTransferController : RootController
    {
        private readonly IUserActivityService _userActivityService;
        readonly INationalTransferService _service;
        static string successActivityResult = "Successful";
        static string failedActivityResult = "Failed";
        private readonly ILogger _log;
        public AccountTransferController(INationalTransferService service, IUserActivityService userActivity, ILoggerFactory log)
        {
            _service = service;
            _userActivityService = userActivity;
            _log = log.CreateLogger(typeof(AccountTransferController));
        }

        [HttpGet]
        [Route("institutions")]
        [ProducesResponseType(typeof(InstitutionResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetFinancialInstitutions([FromHeader] string language)
        {
            var institutions = await _service.GetInstitutions();
            if (!institutions.Banks.Any() && !institutions.MobileMoneyOperators.Any())
            {
                return NotFound();

            }
            return Ok(institutions);
        }
        //[FromBody] BaseEncryptedRequestDTO encryptedRequestData, [FromQuery] TransactionDetails request
        [HttpGet]
        [Route("name/{accountNumber}/{institutionCode}")]
        [ServiceFilter(typeof(SessionFilter))]

        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAccountName([FromRoute] string accountNumber, [FromRoute] string institutionCode, [FromHeader] string language, [FromHeader] string authToken)
        {
            var result = await _service.GetAccountName(accountNumber, institutionCode, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }
        [HttpPost, Route("toaccount")]
        [ServiceFilter(typeof(SessionFilter))]
        [ServiceFilter(typeof(DuplicateFilter))]

        [TypeFilter(typeof(DecryptRequestDataFilter<AuthenticatedTransferRequest>))]
        [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]

        public async Task<IActionResult> TransferToBank([FromHeader] string authToken, [FromHeader] string transRef, [FromQuery] AuthenticatedTransferRequest request,
            [FromBody] BaseEncryptedRequestDTO encryptedRequestData, [FromQuery] AuthenticatedUser user, [FromHeader] string language,  [FromQuery] bool saveAsBeneficiary = false)
        {
            //var checkedRequest = request.IsPropertyNull("Narration");
            //if (checkedRequest != null)
            //{
            //    request = (AuthenticatedTransferRequest)checkedRequest;
            //}

            var result = await _service.Transfer(request, user, language, saveAsBeneficiary);

            await _userActivityService.AddByUsername(user.UserName, "Transfer Account to Account", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }


        [HttpPost, Route("towallet")]
        [ServiceFilter(typeof(SessionFilter))]
        [ServiceFilter(typeof(DuplicateFilter))]
        //[ProducesResponseType(typeof(TransferResponse), Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ErrorResponse), Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]

        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [TypeFilter(typeof(DecryptRequestDataFilter<AuthenticatedTransferRequest>))]
        public async Task<IActionResult> TransferToWallet([FromHeader] string authToken,
            [FromHeader] string transRef, [FromBody] BaseEncryptedRequestDTO encryptedRequestData, [FromQuery] AuthenticatedTransferRequest request, 
            [FromQuery] AuthenticatedUser user, [FromHeader] string language, [FromQuery] bool saveAsBeneficiary = false)
        {
            _log.LogInformation("Inside the TransferToWallet action mehod of the Accounttranfercontroler");
            var result = await _service.AccountToWallet(request, user, language, saveAsBeneficiary);
            _log.LogInformation("Result from AccountToWallet inside the TransferToWallet action mehod of the Accounttranfercontroler: {0}", JsonConvert.SerializeObject(result));
            await _userActivityService.AddByUsername(user.UserName, "Transfer Account to Wallet", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


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
        [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]

        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TransferToSelf([FromHeader] string authToken, [FromHeader] string transRef, [FromBody] BaseEncryptedRequestDTO encryptedRequestData,
            [FromQuery] SelfTransferRequest request, [FromQuery] AuthenticatedUser user, [FromHeader] string language)
        {
            var result = await _service.TransferToSelf(request, user, language);
            await _userActivityService.AddByUsername(user.UserName, "Transfer Account to Account Self", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        [HttpGet]
        [Route("{bankCode}/branches")]
        [ProducesResponseType(typeof(IEnumerable<Branch>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBankBranches([FromHeader] string authToken, [FromRoute] string bankCode, [FromHeader] string language)
        {
            var result = await _service.GetBankBranches(bankCode, language);
            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        #region Staning In struction
        [HttpPost, Route("toaccount-standignstruction")]
        //  [ServiceFilter(typeof(SessionFilter))]
        //[ServiceFilter(typeof(DuplicateFilter))]

        [TypeFilter(typeof(DecryptRequestDataFilterS<AuthenticatedTransferRequest>))]
        [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> TransferToBanks([FromQuery] AuthenticatedTransferRequest request,
            [FromBody] BaseEncryptedRequestDTOStandingInstruction encryptedRequestData,
            [FromHeader] string language, [FromQuery] bool saveAsBeneficiary = false)
        {

            string transRef = Guid.NewGuid().ToString();
            var user = encryptedRequestData.User;
            var result = await _service.Transfer(request, user, language, saveAsBeneficiary);

            await _userActivityService.AddByUsername(user.UserName, "Transfer Account to Account", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        [HttpPost, Route("towallet-standignstruction")]

        //[ServiceFilter(typeof(DuplicateFilter))]
        //[ProducesResponseType(typeof(TransferResponse), Microsoft.AspNetCore.Http.StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ErrorResponse), Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]

        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [TypeFilter(typeof(DecryptRequestDataFilterS<AuthenticatedTransferRequest>))]
        public async Task<IActionResult> TransferToWallets([FromBody] BaseEncryptedRequestDTOStandingInstruction encryptedRequestData,
            [FromQuery] AuthenticatedTransferRequest request, [FromHeader] string language,
            [FromQuery] bool saveAsBeneficiary = false)
        {
            string transRef = Guid.NewGuid().ToString();
            var user = encryptedRequestData.User;


            _log.LogInformation("Inside the TransferToWallet action mehod of the Accounttranfercontroler");
            var result = await _service.AccountToWallet(request, user, language, saveAsBeneficiary);
            _log.LogInformation("Result from AccountToWallet inside the TransferToWallet action mehod of the Accounttranfercontroler: {0}", JsonConvert.SerializeObject(result));
            await _userActivityService.AddByUsername(user.UserName, "Transfer Account to Wallet", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }

        [HttpPost, Route("self-standignstruction")]

        //[ServiceFilter(typeof(DuplicateFilter))]
        [TypeFilter(typeof(DecryptRequestDataFilterS<SelfTransferRequest>))]
        [ProducesResponseType(typeof(TransferResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TransferToSelfs(
            [FromBody] BaseEncryptedRequestDTOStandingInstruction encryptedRequestData,
            [FromQuery] SelfTransferRequest request, [FromHeader] string language)
        {
            string transRef = Guid.NewGuid().ToString();
            var user = encryptedRequestData.User;


            var result = await _service.TransferToSelf(request, user, language);
            await _userActivityService.AddByUsername(user.UserName, "Transfer Account to Account Self", result.IsSuccessful ? successActivityResult : failedActivityResult, result.Error?.ResponseDescription);


            if (!result.IsSuccessful)
            {
                return CreateResponse(result.Error, result.FaultType);
            }
            return Ok(result.GetPayload());
        }


        #endregion
    }
}
