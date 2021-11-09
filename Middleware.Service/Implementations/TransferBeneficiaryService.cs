using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Newtonsoft.Json;

namespace Middleware.Service.Implementations
{
    public class TransferBeneficiaryService : ITransferBeneficiaryService
    {
        readonly IMessageProvider _messageProvider;
        readonly IBeneficiaryService _service;
        readonly IAuthenticator _authenticator;
        readonly ILogger _logger;

        public TransferBeneficiaryService(IMessageProvider messageProvider, IBeneficiaryService service,
                                            IAuthenticator authenticator, ILoggerFactory logger)
        {
            _messageProvider = messageProvider;
            _service = service;
            _authenticator = authenticator;
            _logger = logger.CreateLogger(typeof(TransferBeneficiaryService));
        }
        public async Task<BasicResponse> AddBeneficiary(NewTransferBeneficiaryRequest request, AuthenticatedUser user, string language)
        {



            _logger.LogInformation("Inside the AddBeneficiary of TransferBenefitiaryService");
            var response = new BasicResponse(false);
            if (!request.IsValid(out var source))
            {
                response = new ServiceResponse<TransferResponse>(false)
                {
                    FaultType = FaultMode.CLIENT_INVALID_ARGUMENT,
                    Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                        ResponseDescription = $"{_messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)} - {source}"
                    }
                };
                return response;
            }
            _logger.LogInformation("Calling from calling _authenticator.ValidateAnswer method of th Transfer BenefitiaryService  Request:======>{0}", JsonConvert.SerializeObject(new { user.UserName, request.Answer }));

            response = await _authenticator.ValidateAnswer(user.UserName, request.Answer);
            _logger.LogInformation("Response from calling _authenticator.ValidateAnswer method of the Transfer BenefitiaryService:======>{0}", JsonConvert.SerializeObject(response));
            if (!response.IsSuccessful)
            {
                response.FaultType = FaultMode.INVALID_RESPONSE;
                response.Error = new ErrorResponse { ResponseCode = response.Error.ResponseCode, ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language) };
                //response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                return response;
            }
            if (request.Beneficiary != null && !string.IsNullOrEmpty(request.Beneficiary.AccountName))
            {
                request.Beneficiary.AccountName = Util.EncodeString(request.Beneficiary.AccountName);
            }
            if (request.Beneficiary != null && !string.IsNullOrEmpty(request.Beneficiary.Alias))
            {
                request.Beneficiary.Alias = Util.EncodeString(request.Beneficiary.Alias);
            }
            //var doesBeneficiaryExists=_service.GetTransferBeneficiaries()
            response = await _service.AddTransferBeneficiary(request.Beneficiary, user.UserName);
            _logger.LogInformation("response from calling _service.AddTransferBeneficiary method of th Transfer BenefitiaryService:======>{0}", JsonConvert.SerializeObject(response));

            if (!response.IsSuccessful)
            {
                if (response.Error.ResponseCode == "BMS040")
                {
                    response.Error.ResponseDescription = _messageProvider.GetMessage(ResponseCodes.BENEFICIARY_ALREADY_EXISTS, language);
                    response.Error.ResponseCode = ResponseCodes.BENEFICIARY_ALREADY_EXISTS;
                }
                else
                {
                    response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                }
            }
            return response;
        }

        public async Task<BasicResponse> DeleteBeneficiary(string beneficiaryID, AuthenticatedUser user, Answer answer, string language)
        {
            var response = new BasicResponse(false);
            try
            {
                _logger.LogInformation("Inside the DeleteBeneficiary method of TrasnferBeneficiaryService");
                _logger.LogInformation("Start calling the _authenticator.ValidateAnswer Inside the DeleteBeneficiary method of TrasnferBeneficiaryService");
              var  response_ = await _authenticator.ValidateAnswer(user.UserName, answer);
                if (!response.IsSuccessful)
                {
                    _logger.LogInformation("Finished  calling the _authenticator.ValidateAnswer Inside the DeleteBeneficiary method of TrasnferBeneficiaryService. Response {response}",JsonConvert.SerializeObject(response_));
                    response.FaultType = FaultMode.INVALID_RESPONSE;
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = response_?.Error.ResponseCode,
                        ResponseDescription = _messageProvider.GetMessage(response_?.Error.ResponseCode, language)
                    };
                    
                    _logger.LogInformation("Response from Validate Answer: {answer}", JsonConvert.SerializeObject(response));
                    return response;
                }
                _logger.LogInformation("Start Calling _service.RemoveTransferBeneficiary Inside the DeleteBeneficiary method of TrasnferBeneficiaryService");
                response = await _service.RemoveTransferBeneficiary(beneficiaryID, user.UserName);
                if (!response.IsSuccessful)
                {
                    _logger.LogInformation("Finished  calling the  _service.RemoveTransferBeneficiary Inside the DeleteBeneficiary method of TrasnferBeneficiaryService. Response {response}", JsonConvert.SerializeObject(response_));

                    if (response.Error.ResponseDescription == "")
                    {
                        response.Error.ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INCORRECT_SECURITY_ANSWER, language);
                    }
                    else
                    {
                        response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Error = new ErrorResponse
                {
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language),
                    ResponseCode = ResponseCodes.GENERAL_ERROR

                };
                _logger.LogCritical(ex, "An error Occurred in The Deletebeneficiary method of TransferBeneficiaryService");
                return response;
            }


        }

        public async Task<ServiceResponse<IEnumerable<TransferBeneficiary>>> GetBeneficiaries(AuthenticatedUser user, string language)
        {
            _logger.LogInformation("Inside the GetBeneficiaries method of TrasnferBeneficiaryService");
            var result = await _service.GetTransferBeneficiaries(user.UserName);
            _logger.LogInformation("Response from GetBeneficiaries   method of the Transferbenefitiayservice: =====>>{0}", JsonConvert.SerializeObject(result));
            if (!result.IsSuccessful)
            {
                result.Error.ResponseDescription = _messageProvider.GetMessage(result.Error.ResponseCode, language);
            }
            return result;
        }
    }
}