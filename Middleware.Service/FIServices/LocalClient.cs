using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using System.Linq;
using Middleware.Service.Utilities;
using Newtonsoft.Json;
using System.IO;
using Middleware.Core.Model;

namespace Middleware.Service.FIServices
{
    public class LocalClient : BaseClient, ILocalAccountService
    {
        private readonly HttpClient _fiClient;
        private readonly HttpClient _statmentServiceClient;
        private readonly FISettings _fiSettings;
        private readonly StatementServiceSettings _statementSettings;
        private readonly SystemSettings _settings;
        private const string DATE_FORMAT = "yyyy-MM-dd";
        private readonly ILogger _logger;
        private readonly SchemeCodeForAccountChanges _schemeCodeForAccountChanges;

        public LocalClient(IOptions<FISettings> fiSettingProvider, IOptions<StatementServiceSettings> statementSettingsProvider,
            IHttpClientFactory factory, ILoggerFactory logger, IOptions<SystemSettings> settingsProvider, IOptions<SchemeCodeForAccountChanges> schemeCodeForAccountChanges)
            : base(logger)
        {
            _fiSettings = fiSettingProvider.Value;
            _statementSettings = statementSettingsProvider.Value;
            _fiClient = factory.CreateClient("HttpMessageHandler");
            BuildFiClient();
            _statmentServiceClient = factory.CreateClient("HttpMessageHandler");
            BuildStatementClient();
            _logger = logger.CreateLogger(typeof(LocalClient));
            _settings = settingsProvider.Value;
            this._schemeCodeForAccountChanges = schemeCodeForAccountChanges.Value;
        }

        private void BuildFiClient()
        {
            _fiClient.BaseAddress = new Uri(_fiSettings.BaseAddress);
            _fiClient.DefaultRequestHeaders.Add("AppId", _fiSettings.AppId);
            _fiClient.DefaultRequestHeaders.Add("AppKey", _fiSettings.AppKey);
        }

        private void BuildStatementClient()
        {
            _statmentServiceClient.BaseAddress = new Uri(_statementSettings.BaseAddress);
            _statmentServiceClient.DefaultRequestHeaders.Add("AppId", _statementSettings.AppId);
            _statmentServiceClient.DefaultRequestHeaders.Add("AppKey", _statementSettings.AppKey);
        }

        public async Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber, string bankCode)
        {
            _logger.LogInformation("Inside the GetAccountName  method with two parameters of the LocalClient");
            var data = await GetFBNAccountName(accountNumber, _settings.CountryId, _fiClient, _fiSettings.InquiryPath, bankCode);
            return data;
        }
        public async Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber)
        {
            _logger.LogInformation("Inside the GetAccountName  method with one parameter of the LocalClient");
            var data = await GetFBNAccountName(accountNumber, _settings.CountryId, _fiClient, _fiSettings.InquiryPath, "");
            return data;

        }
        public async Task<ServiceResponse<IEnumerable<BankAccount>>> GetAccounts(string customerID)
        {
            _logger.LogInformation("Inside the GetAccounts  method of the LocalClient");
            var request = new GetAccountsRequest(_settings.CountryId, customerID)
            {
                RequestId = GenerateReference()
            };
            var response = new ServiceResponse<IEnumerable<BankAccount>>(false);
            var serviceResponse = await PostMessage<GetAccountsResponse, GetAccountsRequest>(request, _fiSettings.AccountsPath, _fiClient);

            if (!serviceResponse.IsSuccessful())
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning($" Error retrieving customer accounts for request {request.RequestId}. Response Code = {serviceResponse.ResponseCode}");
                }
                response.Error = new ErrorResponse
                {
                    ResponseCode = $"{_prefix}{serviceResponse.ResponseCode}"
                };
                return response;
            }
            var result = new List<BankAccount>();
            foreach (var item in serviceResponse.Accounts)
            {
                var getCodeType = _schemeCodeForAccountChanges.Scheme_Code_Charged.Trim().Split(',').Contains(item.ProductCode.Trim());
                string accoutCharge = getCodeType == true ? _schemeCodeForAccountChanges.Charges.ToString() : "nil";
                if (_fiSettings.PermittedSchemeTypes.Contains(item.ProductCode))
                {
                    result.Add(new BankAccount
                    {
                        Number = item.AccountNumber,
                        Currency = item.CurrencyCode,
                        Description = item.Product,
                        Name = item.AccountName,
                        Balance = new AccountBalance
                        {
                            AvailableBalance = item.AvailableBalance,
                            BookBalance = item.BookBalance
                        },
                        IsDebitable = !_fiSettings.SchemeTypeFilters.DebitRestricted.Contains(item.ProductCode) && _fiSettings.DebitableCurrencyCodes.Contains(item.CurrencyCode),
                        TransactionCharge = accoutCharge
                    });
                }

            }
            response.IsSuccessful = true;
            response.SetPayload(result);
            return response;
        }

        public async Task<ServiceResponse<IEnumerable<StatementRecord>>> GetTransactions(string accountNumber, DateTime start, DateTime end)
        {
            _logger.LogInformation("Inside the GetTransactions  method of the LocalClient");
            var request = new StatementRequest(_settings.CountryId)
            {
                AccountNumber = accountNumber,
                StartDate = start.ToString(DATE_FORMAT),
                EndDate = end.ToString(DATE_FORMAT),
                RequestId = GenerateReference()
            };
            var response = new ServiceResponse<IEnumerable<StatementRecord>>(false);
          // var ress=await PostMessageMTest<StatementResponse, StatementRequest>(request, _statementSettings.StatementPath, _statmentServiceClient);

            
            var serviceResponse = await PostMessage<StatementResponse, StatementRequest>(request, _statementSettings.StatementPath, _statmentServiceClient);


            var path = Path.Combine(_statmentServiceClient.BaseAddress.AbsoluteUri, _statementSettings.StatementPath);
            _logger.LogInformation("Response from {0} Inside the GetTransactions  method of the LocalClient Reponse:====>{1}", JsonConvert.SerializeObject(path), JsonConvert.SerializeObject(serviceResponse));
            if (!serviceResponse.IsSuccessful())
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(" Error retrieving statement for request {0}. Response Code = {1}", request.RequestId, serviceResponse.ResponseCode);
                }
                response.Error = new ErrorResponse
                {
                    ResponseCode = $"{_prefix}{serviceResponse.ResponseCode}"
                };
                return response;
            }


            var result = new List<StatementRecord>(serviceResponse.Transactions.Count);
            var records = serviceResponse.Transactions.OrderByDescending(x => x.Ordinal).Take(_statementSettings.NumberOftransactiondisplayedRecords);
            foreach (var item in records)
            {
                var extraNarration = string.IsNullOrEmpty(item.AdditonalNarration) ? string.Empty : item.AdditonalNarration;
                if (item != null)
                {
                    result.Add(new StatementRecord
                    {
                        Amount = item.Amount,
                        Description = $"{Util.DecodeString(item.Narration)}{extraNarration}",
                        Date = DateTime.Parse(item.PostedDate),
                        IsCredit = item.DrCr == "C",
                        PostedDate = DateTime.Parse(item.PostedDate)
                    });
                }


            }


            _logger.LogInformation(" Result before taking {0} the first the GetTransactions  method of the LocalClient for Bank Reponse:====>{1} ", _statementSettings.NumberOftransactiondisplayedRecords, JsonConvert.SerializeObject(result));

            response.IsSuccessful = true;

            response.SetPayload(result);
            _logger.LogInformation("Final response Inside the GetTransactions  method of the LocalClient for Bank Reponse:====>{0} ", JsonConvert.SerializeObject(result));
            return response;
        }

        public async Task<BasicResponse> Transfer(BaseTransferRequest request, string reference)
        {
            _logger.LogInformation("Inside the Transfer method of the LocalClient");
            var data = await TransferFund(request.SourceAccountId, request.DestinationAccountId, request.Amount,
                request.Narration, reference);
            return data;
        }

        public async Task<BasicResponse> IsCustomerAccount(string accountNumber, string customerID)
        {
            _logger.LogInformation("Inside the IsCustomerAccount method of the LocalClient");
            var response = new BasicResponse(false);
            var request = new AccountDetaisRequest(accountNumber, _settings.CountryId)
            {
                RequestId = GenerateReference()
            };
            var serviceResponse = await PostMessage<AccountDetailsResponse, AccountDetaisRequest>(request, _fiSettings.AccountDetailsPath, _fiClient);

            _logger.LogInformation("Response from PostMessage in the IsCustomerAccount of the LocalClient service {0}"
                , JsonConvert.SerializeObject(serviceResponse));
            if (serviceResponse.IsSuccessful() == false)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(" Error retrieving customer account details for request {0}. Response Code = {1}", request.RequestId, serviceResponse.ResponseCode);
                }
                response.Error = new ErrorResponse
                {
                    ResponseCode = $"{_prefix}{serviceResponse.ResponseCode}"
                };
                return response;
            }
            response.IsSuccessful = serviceResponse.CustomerID == customerID;

            return response;
        }

        public async Task<BasicResponse> TransferToSelf(SelfTransferRequest request, string reference)
        {
            _logger.LogInformation("Inside the TransferToSelf  method of the LocalClient");
            _logger.LogInformation("TransferToSelf Request {0}", JsonConvert.SerializeObject(request));
            var data = await TransferFund(request.SourceAccountNumber, request.DestinationAccountNumber, request.Amount,
                request.Narration, reference);
            _logger.LogInformation("TransferToSelf Response {0}", JsonConvert.SerializeObject(data));
            return data;
        }

        public async Task<BasicResponse> DoAccountsBelongtoCustomer(string customerID, string sourceAccount, string destinationAccount)
        {
            _logger.LogInformation("Inside the DoAccountsBelongtoCustomer  method of the LocalClient");
            _logger.LogInformation(" DoAccountsBelongtoCustomer Request {0}", JsonConvert.SerializeObject(new { customerID, sourceAccount, destinationAccount }));
            var request = new GetAccountsRequest(_settings.CountryId, customerID)
            {
                RequestId = GenerateReference()
            };
            var response = new BasicResponse(false);
            var serviceResponse = await PostMessage<GetAccountsResponse, GetAccountsRequest>(request, _fiSettings.AccountsPath, _fiClient);
            if (!serviceResponse.IsSuccessful())
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(" Error retrieving customer accounts for request {0}. Response Code = {1}", request.RequestId, serviceResponse.ResponseCode);
                }
                response.Error = new ErrorResponse
                {
                    ResponseCode = $"{_prefix}{serviceResponse.ResponseCode}"
                };
                return response;
            }
            var exists = serviceResponse.Accounts.Any(a => a.AccountNumber == sourceAccount) &&
                serviceResponse.Accounts.Any(a => a.AccountNumber == destinationAccount);
            response.IsSuccessful = exists;
            _logger.LogInformation(" DoAccountsBelongtoCustomer Response {0}", JsonConvert.SerializeObject(response));
            return response;
        }

        private async Task<BasicResponse> TransferFund(string sourceAccount, string destinationAccount, decimal amount, string narration, string reference)
        {
            _logger.LogInformation("Inside the TransferFund  method of the LocalClient");
            _logger.LogInformation("transferFund Request {0}", JsonConvert.SerializeObject(new { sourceAccount, destinationAccount, amount, narration, reference }));
            var response = new BasicResponse(false);
            var payload = new IntraBankTransferRequest(_settings.CountryId)
            {
                Amount = amount,
                SourceAccountNumber = sourceAccount,
                DestinationAccountNumber = destinationAccount,
                ClientReferenceId = reference,
                RequestId = GenerateReference(),
                Narration = narration
            };
            try
            {
                var serviceResponse = await PostMessage<BaseResponse, IntraBankTransferRequest>(payload, _fiSettings.LocalTransferPath, _fiClient);
                _logger.LogInformation(" TransferFund response from FI  {0}", JsonConvert.SerializeObject(serviceResponse));
                if (!serviceResponse.IsSuccessful())
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning($"FI local transfer error. RequestId = {payload.RequestId}/{reference}. Response Code = {serviceResponse.ResponseCode}");
                    }
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = $"{_prefix}{serviceResponse.ResponseCode}"
                    };
                    return response;
                }
                response.IsSuccessful = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FI CLIENT FAILURE");
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.TRANSACTION_FAILED
                };
                return response;
            }
        }


    }
}