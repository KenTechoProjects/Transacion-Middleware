using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Core.DTO;
using Middleware.Service.DTO;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Routing;
using Middleware.Service.FIServices;

namespace Middleware.Service.WalletServices
{
    public class WalletService : IWalletService, IWalletCreator
    {
        private readonly HttpClient _walletHttpClient;
        private readonly SystemSettings _settings;
        private readonly WalletConfigSettings _walletSettings; 
        private readonly StatementServiceSettings _statementSettings;
        readonly ILogger _logger;
        internal const string _prefix = "W";
        private readonly string _baseUrl = string.Empty;
        public WalletService(IHttpClientFactory factory, ILoggerFactory logger, IOptions<WalletConfigSettings> walletSettings, IOptions<SystemSettings> settingsProvider,IOptions<StatementServiceSettings> statementServiceSettings)
        {

            _walletSettings = walletSettings.Value;
            _walletHttpClient = factory.CreateClient("HttpMessageHandler");
            BuildWalletClient();
            _logger = logger.CreateLogger(typeof(WalletService));
            _settings = settingsProvider.Value;
            _baseUrl = _walletSettings.BaseAddress;
            _statementSettings = statementServiceSettings.Value;
        }

        private void BuildWalletClient()
        {
            _walletHttpClient.BaseAddress = new Uri(_walletSettings.BaseAddress);
            _walletHttpClient.DefaultRequestHeaders.Add("AppId", _walletSettings.AppId);
            _walletHttpClient.DefaultRequestHeaders.Add("Secret", _walletSettings.Secret);
        }
        public async Task<BasicResponse> ChargeWallet(string walletNumber, decimal amount, string narration, string referenceNumber)
        {
            try
            {
                var walletBeforeDebit = await GetWallet(walletNumber);
                _logger.LogInformation("Inside the ChargeWallet of the Wallet Service Class", DateTime.UtcNow);
                var request = new
                {
                    walletId = walletNumber,
                    amount,
                    TransactionReference = referenceNumber,
                    narration
                };
                string url = Path.Combine(_baseUrl, "api/v1/wallet/debit");
                _logger.LogInformation("Sending data to wallte service  debit endpoint {0} in  the ChargeWallet of the Wallet Service Class Request========> {1} ", url, JsonConvert.SerializeObject(request));
                var data = await PostMessage<dynamic>(request, "api/v1/wallet/debit", _walletHttpClient);
                //if (data.IsSuccessful == false)
                //{
                //    var walletAfterCallToWalletDebit = await GetWallet(walletNumber);

                //    if (walletAfterCallToWalletDebit != null && walletAfterCallToWalletDebit.GetPayload() != null && walletBeforeDebit != null && walletBeforeDebit.GetPayload() != null)
                //    {
                //        var waBe = walletBeforeDebit.GetPayload().Balance.AvailableBalance;
                //        var waAf = walletAfterCallToWalletDebit.GetPayload().Balance.AvailableBalance;
                //        if (waAf < waBe)
                //        {
                //            var resp = await FundWallet(walletNumber, amount, "Reversal of an account", referenceNumber);
                //            return resp;
                //        }

                //    }

                //}
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A server error occurred in  the ChargeWallet of the Wallet Service Class at {0}", DateTime.UtcNow);
                return new BasicResponse { FaultType = FaultMode.GATEWAY_ERROR, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };

            }


        }


        public async Task<BasicResponse> FundWallet(string walletNumber, decimal amount, string narration, string referenceNumber)
        {




            try
            {
                _logger.LogInformation("Inside the  FundWallet of the Wallet Service Class at {0}", DateTime.UtcNow);

                var request = new
                {
                    walletId = walletNumber,
                    amount,
                    TransactionReference = referenceNumber,
                    narration
                };

                string url = Path.Combine(_baseUrl, "api/v1/wallet/fund");
                string ser = JsonConvert.SerializeObject(request);
                _logger.LogInformation("Sending data to wallte service  debit endpoint {0} in  the FundWallet   of the Wallet Service Class =================:Request=>  {1}", url, ser);

                var data = await PostMessage<dynamic>(request, "api/v1/wallet/fund", _walletHttpClient);
                _logger.LogInformation("Response  data from  the {0} in  the  PostMessage   of the Wallet Service Class =================:{1}", url, JsonConvert.SerializeObject(data));

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A server error occurred in  the  FundWallet of the Wallet Service Class");
                return new BasicResponse { FaultType = FaultMode.GATEWAY_ERROR, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };

            }
        }

        public async Task<ServiceResponse<IEnumerable<StatementRecord>>> GetTransactions(string walletNumber, DateTime start, DateTime end)
        {

            try
            {
                _logger.LogInformation("Inside the GetTransactions   of the Wallet Service Class at {0}", DateTime.UtcNow);


                var response = new ServiceResponse<IEnumerable<StatementRecord>>(false);
                var path = $"api/v1/wallet/statement?walletId={walletNumber}&startDate={start.ToString("dd-MM-yyyy")}&endDate={end.ToString("dd-MM-yyyy")}";

                string url = Path.Combine(_baseUrl, path);
                _logger.LogInformation("Fetching data from   {0} in the GetTransactions of the Wallet Service Class", url);
                _logger.LogInformation("Request  data from  the {0} in  the  GetTransactions    of the Wallet Service Class Request: =================:{1}", url, walletNumber);
                var serviceResponse = await GetRequest<WalletStatementResponse>(path, _walletHttpClient);

                _logger.LogInformation("SERVICE RESPONSE  ========  {0}", JsonConvert.SerializeObject(serviceResponse));

                if (!serviceResponse.IsSuccessful)
                {
                    response.Error = new ErrorResponse
                    {
                        ResponseCode = $"{_prefix}{serviceResponse.Error?.ResponseCode}"
                    };
                    return response;
                }
                _logger.LogInformation("|||||||||||||||||||||||||||||||||||||||||||||||||||");
                var body = serviceResponse.GetPayload();
                // _logger.LogInformation($"SEE_BODY: {JsonConvert.SerializeObject(body)}");
                _logger.LogInformation("Response  data from  the {0} in  the  GetTransactions    of the Wallet Service Class response: =================:{1}", url, JsonConvert.SerializeObject(body));

                var result = new List<StatementRecord>(body.statement.Count);

                _logger.LogInformation("iteration in  the GetTransactions   of the Wallet Service Class ");
                var records = body.statement.OrderByDescending(x => x.txnstamp).Take(_statementSettings.NumberOftransactiondisplayedRecords);
                foreach (var item in records)
                {
                    if (item != null)
                    {
                        result.Add(new StatementRecord
                        {
                            Amount = Convert.ToDecimal(item.amount),
                            Description = item.narration,           //$"{Util.DecodeString(item.narration)}",
                            Date = DateTime.ParseExact(item.txnstamp, "dd-MM-yyyy HH:mm:ss", null),
                            //Date = Convert.ToDateTime(item.txnstamp),
                            IsCredit = item.drcrflag == "C",
                            PostedDate = DateTime.ParseExact(item.txnstamp, "dd-MM-yyyy HH:mm:ss", null)
                        });
                    }

                }
                response.IsSuccessful = true;
              
                response.SetPayload(result);
                return response;
            }
            catch (Exception ex)
            {

                _logger.LogCritical(ex, "A server error occurred in  the  GetTransactions of the Wallet Service Class ");
                var data = new ServiceResponse<IEnumerable<StatementRecord>> { FaultType = FaultMode.GATEWAY_ERROR, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };
                return data;
            }

        }


        public async Task<ServiceResponse<Wallet>> GetWallet(string walletNumber)
        {
            try
            {


                _logger.LogInformation("Inside   the  GetWallet  of the Wallet Service Class at {0}", DateTime.UtcNow);

                var serviceResponse = new ServiceResponse<Wallet>(false);
                var path = $"api/v1/wallet/balance?walletId={walletNumber}";
                _logger.LogInformation("Fetching GetWalletResponse in   the  GetWallet  of the Wallet Service Class at {0}", DateTime.UtcNow);
                string url = Path.Combine(_baseUrl, path);
                _logger.LogInformation("Request  data from  the {0} in  the  GetWallet   of the Wallet Service Class =================:{1}", url, walletNumber);
                var result = await GetRequest<GetWalletResponse>(path, _walletHttpClient);
                if (result.IsSuccessful)
                {
                    var walletDetails = result.GetPayload();
                    serviceResponse.IsSuccessful = true;
                    serviceResponse.SetPayload(new Wallet()
                    {
                        Balance = new AccountBalance()
                        {
                            AvailableBalance = walletDetails.Balance,
                            BookBalance = walletDetails.Balance
                        },
                        Currency = walletDetails.Currency,
                        WalletNumber = walletDetails.WalletNumber,
                        WalletType = walletDetails.WalletType,
                        WalletName = walletDetails.AccountName
                    });

                }
                else
                {
                    serviceResponse.Error = result.Error;

                }
                _logger.LogInformation("Response  data from  the {0} in  the  PostMessage   of the Wallet Service Class =================:{1}", url, JsonConvert.SerializeObject(serviceResponse));

                return serviceResponse;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A server error occurred in  the  GetWallet  of the Wallet Service Class ");
                var data = new ServiceResponse<Wallet> { FaultType = FaultMode.GATEWAY_ERROR, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };
                return data;

            }
        }

        public async Task<BasicResponse> WalletTransfer(BaseTransferRequest walletTransferRequest)
        {
            try
            {
                _logger.LogInformation("Inside  the   WalletTransfer  of the Wallet Service Class ");

                if (string.IsNullOrWhiteSpace(walletTransferRequest.Narration))
                {
                    walletTransferRequest.Narration = null;
                }


                var request = new
                {
                    SourceWalletId = walletTransferRequest.SourceAccountId,
                    DestinationWalletId = walletTransferRequest.DestinationAccountId,
                    walletTransferRequest.Amount,
                    TransactionReference = DateTime.Now.Ticks.ToString()
                    ,
                    walletTransferRequest.Narration
                };
                _logger.LogInformation("Making wallet transfering  in   the WalletTransfer    of the Wallet Service Class ");

                string url = Path.Combine(_baseUrl, "api/v1/wallet/transfer");

                _logger.LogInformation("Request  data from  the {0} in  the  WalletTransfer   of the Wallet Service Class =================:{1}", url, JsonConvert.SerializeObject(request));
                var data = await PostMessage<dynamic>(request, "api/v1/wallet/transfer", _walletHttpClient);
                _logger.LogInformation("Response  data from  the {0} in  the  PostMessage   of the Wallet Service Class =================:{1}", url, JsonConvert.SerializeObject(data));
                return data;
            }
            catch (Exception ex)
            {
                {
                    _logger.LogCritical(ex, "A server error occurred in  the  WalletTransfer   of the Wallet Service Class at {0}", DateTime.UtcNow);
                    var data = new BasicResponse { FaultType = FaultMode.GATEWAY_ERROR, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };
                    return data;
                }
            }
        }

        protected async Task<BasicResponse> PostMessage<U>(U request, string path, HttpClient client)
        {
            try
            {

                //var checkedRequest = request.IsPropertyNull("Narration");
                //if (checkedRequest != null)
                //{
                //    request = (U)checkedRequest;
                //}

                _logger.LogInformation("Inside the  PostMessage   of the Wallet Service Class at {0}", DateTime.UtcNow);
                string url = Path.Combine(_baseUrl, path);

                _logger.LogInformation("Wallet Request =================== {0} ", JsonConvert.SerializeObject(new { request, url }));

                var response = new BasicResponse(false);
                var input = Util.SerializeAsJson<U>(request);
                //_logger.LogInformation($"REQUEST: {input}");
                var message = new StringContent(input, Encoding.UTF8, "application/json");

                var rawResponse = await client.PostAsync(path, message);
                _logger.LogInformation("Response  data from  the {0} in  the  PostMessage", url, DateTime.UtcNow);

                var body = await rawResponse.Content.ReadAsStringAsync();
                _logger.LogInformation("Fetching data from  the {0} in  the  PostMessage   of the Wallet Service Class at {1}", url, DateTime.UtcNow);


                //_logger.LogInformation($"Wallet Api Response : {body}");
                if (!rawResponse.IsSuccessStatusCode)
                {
                    var error = Util.DeserializeFromJson<WalletServiceError>(body);
                    response.Error = new ErrorResponse { ResponseCode = $"{_prefix}{error?.Code}", ResponseDescription = error.Message };
                }
                else
                {
                    response.IsSuccessful = true;
                }
                _logger.LogInformation("Request  data from  the {0} in  the  PostMessage   of the Wallet Service Class =================:{1}", url, response);

                return response;
            }
            catch (Exception ex)
            {

                _logger.LogCritical(ex, "A server error occurred in  the  PostMessage of the Wallet Service Class at {0}", DateTime.UtcNow);
                var data = new BasicResponse { FaultType = FaultMode.GATEWAY_ERROR, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };
                return data;
            }
        }

        protected async Task<ServiceResponse<T>> PostMessage<U, T>(U request, string path, HttpClient client)
        {
            try
            {

                //var checkedRequest = request.IsPropertyNull("Narration");
                //if (checkedRequest != null)
                //{
                //    request = (U)checkedRequest;
                //}var 

                _logger.LogInformation("Inside the U generic  PostMessage   of the Wallet Service Class at {0}", DateTime.UtcNow);



                var response = new ServiceResponse<T>(false);
                var input = Util.SerializeAsJson<U>(request);
                string url = Path.Combine(_baseUrl, path);
                //_logger.LogInformation($"REQUEST: {input}");
                var message = new StringContent(input, Encoding.UTF8, "application/json");
                _logger.LogInformation("Posting to {0} in the U generic  PostMessage   of the Wallet Service Class at {1}", path, DateTime.UtcNow);


                var rawResponse = await client.PostAsync(path, message);
                var body = await rawResponse.Content.ReadAsStringAsync();
                //_logger.LogInformation($"Wallet Api Response : {body}");
                if (!rawResponse.IsSuccessStatusCode)
                {
                    var error = Util.DeserializeFromJson<WalletServiceError>(body);
                    response.Error = new ErrorResponse { ResponseCode = $"{_prefix}{error.Code}", ResponseDescription = error.Message };
                }
                else
                {
                    response.IsSuccessful = true;
                    var result = Util.DeserializeFromJson<T>(body);
                    response.SetPayload(result);
                }
                _logger.LogInformation("Response Error data from  the {0} in  the  PostMessage   of the Wallet Service Class Response: =================:{1}", url, JsonConvert.SerializeObject(response));


                return response;
            }
            catch (Exception ex)
            {

                _logger.LogCritical(ex, "A server error occurred in  the  U generic  PostMessage   of the Wallet Service Class ");
                var data = new ServiceResponse<T> { FaultType = FaultMode.GATEWAY_ERROR, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };
                return data;
            }
        }


        protected async Task<ServiceResponse<T>> PUTMessage<U, T>(U request, string path, HttpClient client)
        {
            try
            {

                //var checkedRequest = request.IsPropertyNull("Narration");
                //if (checkedRequest != null)
                //{
                //    request = (U)checkedRequest;
                //}var 

                _logger.LogInformation("Inside the U generic  PostMessage   of the Wallet Service Class at {0}", DateTime.UtcNow);


                string url = Path.Combine(_baseUrl, path);
                var response = new ServiceResponse<T>(false);
                var input = Util.SerializeAsJson<U>(request);
                _logger.LogInformation("Request  data from  the {0} in  the  PostMessage   of the Wallet Service Class =================:{1}", url, input);
                //_logger.LogInformation($"REQUEST: {input}");
                var message = new StringContent(input, Encoding.UTF8, "application/json");
                _logger.LogInformation("Posting to {0} in the U generic  PostMessage   of the Wallet Service Class at {1}", url, input);


                var rawResponse = await client.PutAsync(path, message);
                var body = await rawResponse.Content.ReadAsStringAsync();
                //_logger.LogInformation($"Wallet Api Response : {body}");
                if (!rawResponse.IsSuccessStatusCode)
                {
                    var error = Util.DeserializeFromJson<WalletServiceError>(body);
                    response.Error = new ErrorResponse { ResponseCode = $"{_prefix}{error.Code}", ResponseDescription = error.Message };
                }
                else
                {


                    response.IsSuccessful = true;
                    var result = Util.DeserializeFromJson<T>(body);
                    response.SetPayload(result);
                }
                _logger.LogInformation("Response Error data from  the {0} in  the  PostMessage   of the Wallet Service Class =================:{1}", url, JsonConvert.SerializeObject(response));

                return response;
            }
            catch (Exception ex)
            {

                _logger.LogCritical(ex, "A server error occurred in  the  U generic  PostMessage   of the Wallet Service Class");
                var data = new ServiceResponse<T> { FaultType = FaultMode.GATEWAY_ERROR, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };
                return data;
            }
        }
        private protected async Task<ServiceResponse<T>> GetRequest<T>(string path, HttpClient client)
        {
            try
            {
                string url = Path.Combine(_baseUrl, path);
                _logger.LogInformation("Inside the  generic  GetRequest of the Wallet Service Class ");

                var response = new ServiceResponse<T>(false);

                _logger.LogInformation("Sending data to wallte service    {0} in  the GetRequest of the Wallet Service Class ", url);

                var rawResponse = await client.GetAsync(path);

                var body = await rawResponse.Content.ReadAsStringAsync();
                if (rawResponse.IsSuccessStatusCode)
                {
                    var data = Util.DeserializeFromJson<T>(body);

                    response.IsSuccessful = true;
                    response.SetPayload(data);
                }
                else
                {
                    var error = Util.DeserializeFromJson<WalletServiceError>(body);
                    response.Error = new ErrorResponse { ResponseCode = $"{_prefix}{error.Code}", ResponseDescription = error.Message };
                }
                _logger.LogInformation("Response from {0} .Response Data: ====>{1}", url, JsonConvert.SerializeObject(response));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A server error occurred in  the  GetRequest   of the Wallet Service Class ");
                var data = new ServiceResponse<T> { FaultType = FaultMode.GATEWAY_ERROR, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };
                return data;
            }
        }

        public async Task<ServiceResponse<string>> WalletEnquiry(string walletNumber)
        {
            try
            {

                _logger.LogInformation("Inside the WalletEnquiry of the Wallet Service Class at {0}", DateTime.UtcNow);

                var serviceResponse = new ServiceResponse<string>(false);
                var path = $"api/v1/wallet/enquire?walletId={walletNumber}";
                string url = Path.Combine(_baseUrl, path);
                _logger.LogInformation("Fetching data from {0} the  generic   WalletEnquiry of the Wallet Service Class. Request===>{1}", url, walletNumber);

                var response = await GetRequest<GetWalletEnquiryResponse>(path, _walletHttpClient);
                if (response.IsSuccessful)
                {
                    serviceResponse.IsSuccessful = true;
                    serviceResponse.SetPayload(response.GetPayload()?.Name);
                }
                else
                {
                    serviceResponse.Error = new ErrorResponse() { ResponseCode = response.Error?.ResponseCode, ResponseDescription = response.Error?.ResponseDescription };
                }
                _logger.LogInformation("Response data  {0} in  the WalletEnquiry  of the Wallet Service Class . Response====>{1}", url, JsonConvert.SerializeObject(serviceResponse));

                return serviceResponse;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A server error occurred in  the  WalletEnquiry   of the Wallet Service Class");
                var data = new ServiceResponse<string> { FaultType = FaultMode.GATEWAY_ERROR, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };
                return data;
            }

        }

        public async Task<ServiceResponse<WalletCompletionResponse>> OpenWallet(WalletCreationRequest request)
        {

            try
            {
                _logger.LogInformation("Inside the OpenWallet of the Wallet Service Class at {0}", DateTime.UtcNow);

                //var formatdate = new DateTime();


                var result = new ServiceResponse<WalletCompletionResponse>(false);
                var payload = new
                {
                    FirstName = request.FirstName,
                    MobileNumber = request.PhoneNumber,
                    DateOfBirth = request.BirthDate,
                    Email = request.Email,
                    Gender = request.Gender,
                    LastName = request.LastName,
                    MiddleName = request.MiddleName,
                    Nationality = request.Nationality,
                    State = request.State,
                    MotherMaidenName = request.MotherMaidenName,
                    Address = request.Address,
                    LGA = request.LGA

                };

                string url = Path.Combine(_baseUrl, "api/v1/wallet/register");
                _logger.LogInformation("Posting  to the {0} external service inside  OpenWallet of the Wallet Service Class Request:==========>{1}",
                    url, JsonConvert.SerializeObject(payload));

                var response = await PostMessage<dynamic, CreateWalletResponse>(payload, "api/v1/wallet/register", _walletHttpClient);

                _logger.LogInformation("Response data  from  {0} in  the GetRequest of the Wallet Service Class   Response:======>{1}", url, JsonConvert.SerializeObject(response));
                if (response.IsSuccessful)
                {
                    var createWalletResponse = response.GetPayload();
                    result.SetPayload(new WalletCompletionResponse()
                    {
                        CustomerName = createWalletResponse.CustomerName,
                        HasCeiling = createWalletResponse.IsRestricted,
                        MaximumTransactionAmount = createWalletResponse.CurrentLimit,
                        WalletNumber = createWalletResponse.WalletId,
                        WalletType = createWalletResponse.WalletType
                    });
                    result.IsSuccessful = true;
                }
                else
                {
                    result.Error = new ErrorResponse() { ResponseCode = response.Error?.ResponseCode, ResponseDescription = response.Error?.ResponseDescription };
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A server error occurred in  the  OpenWallet of the Wallet Service Class ");
                var data = new ServiceResponse<WalletCompletionResponse> { FaultType = FaultMode.GATEWAY_ERROR, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };
                return data;
            }
        }

        public async Task<BasicResponse> ReverseTransaction(string reference, string walletNumber)
        {
            try
            {
                _logger.LogInformation("Inside the ReverseTransaction of the Wallet Service Class at {0}", DateTime.UtcNow);
                string url = Path.Combine(_baseUrl, "api/v1/wallet/reverse");

                var request = new
                {
                    walletId = walletNumber,
                    TransactionReference = reference
                };
                _logger.LogInformation("Posting  to   {0}   inside ReverseTransaction   of the Wallet Service  ", url);

                var data = await PostMessage<dynamic>(request, "api/v1/wallet/reverse", _walletHttpClient);
                _logger.LogInformation("Response data from  {0} in  the ReverseTransaction  of the Wallet Service Class . response ====>{1}", url, JsonConvert.SerializeObject(data));
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A server error occurred in  the ReverseTransaction   of the Wallet Service Class");
                var data = new BasicResponse { FaultType = FaultMode.GATEWAY_ERROR, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.INPUT_VALIDATION_FAILURE, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };


                return data;
            }
        }
        public async Task<ServiceResponse<UpgrateWalletResponse>> UpgradeWallet(UpgradeWalletDTO request)
        {
            var response = new ServiceResponse<UpgrateWalletResponse>(false);

            try
            {
                _logger.LogInformation("Inside the UpgradeWallet of WalletServices.");
                var resp = await PUTMessage<UpgradeWalletDTO, UpgrateWalletResponse>(request, "api/v1/wallet/upgrade", _walletHttpClient);

                string url = Path.Combine(_baseUrl, "api/v1/wallet/upgrade");
                _logger.LogInformation("Response data  of {0} in the UpgradeWallet of the Wallet Service Class   Response:======>{1}", url, JsonConvert.SerializeObject(response));

                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Server Error occurred in the UpgradeWallet wallet service ");
                var data = new ServiceResponse<UpgrateWalletResponse> { FaultType = FaultMode.SERVER, IsSuccessful = false, Error = new ErrorResponse { ResponseCode = ResponseCodes.GENERAL_ERROR, ResponseDescription = ex.InnerException != null ? ex.InnerException.Message : ex.Message } };
                return data;
            }


        }
    }
}
