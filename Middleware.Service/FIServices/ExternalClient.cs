using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Newtonsoft.Json;

namespace Middleware.Service.FIServices
{
    public class ExternalClient : BaseClient, IExternalTransferService
    {
        //private readonly FISettings _fiSettings;
        private readonly FISettings _fiSettings;
        private readonly HttpClient _client;
        private readonly SystemSettings _settings;
        private readonly ILogger _logger;
        public ExternalClient(IOptions<FISettings> fiSettingProvider, IOptions<SystemSettings> settingsProvider,
            IHttpClientFactory factory, ILoggerFactory logger) : base(logger)
        {
            _fiSettings = fiSettingProvider.Value;
            if (_fiSettings.BranchCodeMappings == null)
            {
                throw new ArgumentException("Branch code mappings cannot be null");
            }
            _settings = settingsProvider.Value;
            _client = factory.CreateClient("HttpMessageHandler");
            _client.BaseAddress = new Uri(_fiSettings.BaseAddress);
            _client.DefaultRequestHeaders.Add("AppId", _fiSettings.AppId);
            _client.DefaultRequestHeaders.Add("AppKey", _fiSettings.AppKey);
            _logger = logger.CreateLogger(typeof(ExternalClient));
        }

        public async Task<ServiceResponse<dynamic>> GetAccountName(string acNumber, string bankCode)
        {
            _logger.LogInformation("Inside the GetAccountName of EternalClient");
            var response = new ServiceResponse<dynamic>(false);
            var serviceResponse = new InquiryResponse();
            try
            {
                string accountNumber = string.Empty;
         
                 
             
               // _logger.LogInformation("Calling Util.GetonyNumbers inside GetAccount method of EternalClient "); 
                //string strippedOffLeadingStrings = Util.GetonyNumbers(acNumber);
                //if (acNumber.Length <= 19)
                //{

                //    accountNumber = acNumber;
                //}
                //else
                //{
                //    accountNumber = Util.RemoveCharacterFromBegining(strippedOffLeadingStrings, 3);
                //}

               ////// var request = new InquiryRequest(_settings.CountryId, accountNumber)
                var request = new InquiryRequest(_settings.CountryId, acNumber)
                {
                    RequestId = GenerateReference(),
                    BankCode = bankCode
                };
                _logger.LogInformation("Request from {0} in the GetAccountName of ExternalClient: Request:===> {1}", _fiSettings.InterBankInquiryPath,JsonConvert.SerializeObject(request));
                serviceResponse = await PostMessage<InquiryResponse, InquiryRequest>(request, _fiSettings.InterBankInquiryPath, _client);
                _logger.LogInformation("Response from {0} in the GetAccountName of ExternalClient. Response;====> {1}", _fiSettings.InterBankInquiryPath, JsonConvert.SerializeObject(request));
                if (serviceResponse.IsSuccessful() == false)
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning(" FI External name lookup error {0}. Response Code = {1}", request.RequestId, serviceResponse.ResponseCode);
                    }
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = $"{_prefix}{serviceResponse.ResponseCode}"
                    };
                    return response;
                }
                response.IsSuccessful = true;
                _logger.LogInformation("response from {0} in the GetAccountName of ExternalClient", _fiSettings.InterBankInquiryPath);
                response.SetPayload(new { serviceResponse.AccountName });
                return response;

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Server error occurred in the GetAccountName method of ExternalClient at {0}", DateTime.UtcNow);
                response.Error.ResponseDescription = "Service invocation failure";
                response.Error.ResponseCode = ResponseCodes.GENERAL_ERROR;
                return response;
            }

        }

        public async Task<ServiceResponse<IEnumerable<Branch>>> GetBranches(string bankCode)
        {
            _logger.LogInformation("Inside the GetBranches  of EternalClient");
            var response = new ServiceResponse<IEnumerable<Branch>>(false);
            var request = new GetBranchesRequest(_settings.CountryId, bankCode)
            {
                BankCode = bankCode,
                RequestId = GenerateReference()
            };
            _logger.LogInformation("Request to {0} in the GetBranches  of EternalClient: Request=={1}", _fiSettings.BranchesPath, JsonConvert.SerializeObject(request));
            var serviceResponse = await PostMessage<GetBranchesResponse, GetBranchesRequest>(request, _fiSettings.BranchesPath, _client);
            _logger.LogInformation("response from {0} in the GetBranches  of EternalClient", JsonConvert.SerializeObject(serviceResponse));
            if (serviceResponse.IsSuccessful() == false)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(" FI error retrieving bank branches {0}. Response Code = {1}", request.RequestId, serviceResponse.ResponseCode);
                }
                response.Error = new ErrorResponse
                {
                    ResponseCode = $"{_prefix}{serviceResponse.ResponseCode}"
                };
                return response;
            }
            response.IsSuccessful = true;
            var branches = new List<Branch>();
            foreach (var item in serviceResponse.Branches)
            {
                branches.Add(new Branch
                {
                    BranchCode = item.BranchCode,
                    BranchName = item.BranchName
                });
            }
            response.SetPayload(branches);
            return response;
        }

        public async Task<BasicResponse> Transfer(BaseTransferRequest request, string reference)
        {
            _logger.LogInformation("Inside the  Transfer of EternalClient");
            var response = new BasicResponse(false);
            if (!_fiSettings.BranchCodeMappings.TryGetValue(request.DestinationInstitutionId, out string branchCode))
            {
                _logger.LogInformation("Invalid account number");
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INVALID_ACCOUNT_NUMBER
                };
                return response;
            }
            _logger.LogInformation("Branch code {0}", branchCode);

            //var destinationAccountNmberRemovedString = string.Empty ;
            //var destinationAccountNmber  = Utilities.Util.GetonyNumbers(request.DestinationAccountId);

            //if (destinationAccountNmberRemovedString.Length > 19)
            //{
            //    destinationAccountNmber = Util.RemoveCharacterFromBegining(destinationAccountNmberRemovedString, 3);
            //}
            
            var payload = new InterBankTransferRequest(_settings.CountryId)
            {
                Amount = request.Amount,
                SourceAccountNumber = request.SourceAccountId,
                //BeneficiaryAccountNumber = destinationAccountNmber,
                BeneficiaryAccountNumber = request.DestinationAccountId,
                BeneficiaryAccountName = request.DestinationAccountName,
                BeneficiaryBankCode = request.DestinationInstitutionId,
                BeneficiaryBranchCode = branchCode,
                ClientReferenceId = reference,
                RequestId = GenerateReference(),
                Narration = request.Narration
            };
            _logger.LogInformation("Request to {0}  Transfer metod of EternalClient Request:===> {1}", JsonConvert.SerializeObject(_fiSettings.InterBankTransferPath), JsonConvert.SerializeObject(payload));
            var serviceResponse = await PostMessage<BaseResponse, InterBankTransferRequest>(payload, _fiSettings.InterBankTransferPath, _client);
            _logger.LogInformation("response to {0} in the Transfer of EternalClient", JsonConvert.SerializeObject(serviceResponse));
            if (serviceResponse.IsSuccessful()==false)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(" FI External transfer error {0}. Response Code = {1}", payload.RequestId + "/" + reference, serviceResponse.ResponseCode);
                }
                response.Error = new ErrorResponse
                {
                    ResponseCode = $"{_prefix}{serviceResponse.ResponseCode}"
                };
                return response;
            }
            response.IsSuccessful = true;
            _logger.LogInformation("Final Response:===>{0}", JsonConvert.SerializeObject(response));
            return response;
        }
    }
}
