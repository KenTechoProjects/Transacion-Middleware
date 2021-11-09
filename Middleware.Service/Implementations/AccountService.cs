using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Core.DTO;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using Middleware.Core.DAO;
using Middleware.Core.Model;

namespace Middleware.Service.Implementations
{
    public class AccountService : IAccountService
    {
        readonly ILocalAccountService _service;
        private readonly IWalletService _walletService;
        readonly IMessageProvider _messageProvider;
        private readonly SystemSettings _settings;
        readonly ILogger _logger;
        readonly ITransactionDAO _transactionDAO;
        public AccountService(ILocalAccountService service, IMessageProvider messageProvider,
            IOptions<SystemSettings> settingsProvider, ILoggerFactory logger, IWalletService walletService, ITransactionDAO transactionDAO)
        {
            _service = service;
            _messageProvider = messageProvider;
            _settings = settingsProvider.Value;
            _logger = logger.CreateLogger(typeof(AccountService));
            _walletService = walletService;
            _transactionDAO = transactionDAO;
        }

        public async Task<ServiceResponse<IEnumerable<BankAccount>>> GetAccounts(string customerID, string language)
        {
            var response = new ServiceResponse<IEnumerable<BankAccount>>(false);
            try
            {
                _logger.LogInformation("Inside the GetAccounts method of the Account Service Class at {0}", DateTime.UtcNow);
                  response = await _service.GetAccounts(customerID);

                if (!response.IsSuccessful)
                {
                    response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "A server erro have occurred  int GetAccounts method of the Account Service Class {0} ", DateTime.UtcNow);

                if (!string.IsNullOrWhiteSpace(ex.Message) && ex.Message.Contains("A connection attempt failed"))
                {
                    response.Error = new ErrorResponse()
                    {
                        ResponseCode = ResponseCodes.THIRD_PARTY_NETWORK_ERROR,
                        ResponseDescription =
                            _messageProvider.GetMessage(ResponseCodes.THIRD_PARTY_NETWORK_ERROR, language)
                    };
                    return response;
                }


                return new ServiceResponse<IEnumerable<BankAccount>>
                {
                    FaultType = FaultMode.GATEWAY_ERROR,
                    IsSuccessful = false,
                    Error = new ErrorResponse
                    {
                        ResponseCode = ResponseCodes.GENERAL_ERROR,
                        ResponseDescription = "Server Error"
                    }
                };
            }

        }

        public async Task<ServiceResponse<Wallet>> GetWallet(string walletNumber, string language)
        {
            var response = await _walletService.GetWallet(walletNumber);

            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
            }
            return response;
        }

        public async Task<ServiceResponse<CompositeAccountData>> GetCustomerAccounts(AuthenticatedUser user, string language)
        {
            //The assumption is that the user will always have a wallet
            var response = new ServiceResponse<CompositeAccountData>(false);
            var walletTask = _walletService.GetWallet(user.WalletNumber);
            Task<ServiceResponse<IEnumerable<BankAccount>>> accountTask = null;
            var hasAccount = false;
            if (!string.IsNullOrEmpty(user.BankId))
            {
                hasAccount = true;
                accountTask = _service.GetAccounts(user.BankId);
                await Task.WhenAll(new List<Task> { walletTask, accountTask });
            }
            else
            {
                await walletTask;
            }

            var payload = new CompositeAccountData { HasWallet = true, HasAccounts = hasAccount };
            var walletResponse = walletTask.Result;
            //_logger.LogInformation($"walletResponse : { Newtonsoft.Json.JsonConvert.SerializeObject(walletResponse)}");
            payload.WalletData = new WalletResult
            {
                IsSuccessful = walletResponse.IsSuccessful,
                Wallet = walletResponse.IsSuccessful ? walletResponse.GetPayload() : null,
                Message = walletResponse.IsSuccessful ? null : _messageProvider.GetMessage(walletResponse.Error.ResponseCode, language)
            };

            if (hasAccount)
            {
                var accountResponse = accountTask.Result;
                payload.AccountData = new AccountResult
                {
                    IsSuccessful = accountResponse.IsSuccessful,
                    Accounts = accountResponse.IsSuccessful ? accountResponse.GetPayload() : null,
                    Message = accountResponse.IsSuccessful ? null : _messageProvider.GetMessage(accountResponse.Error.ResponseCode, language)
                };
            }
            response.IsSuccessful = true;
            response.SetPayload(payload);
            return response;
        }



        public async Task<ServiceResponse<IEnumerable<StatementRecord>>> GetRecentTransactionsWithDaterange(string accountIdentifier, AccountType accountType, AuthenticatedUser user,DateTime startDate,DateTime endDate, string language)
        {
            var response = new ServiceResponse<IEnumerable<StatementRecord>>(false);
            var end = endDate;
            var start = startDate;
            _logger.LogInformation("Inside the GetRecentTransactions of accountService");
            switch (accountType)
            {
                case AccountType.BANK:
                    _logger.LogInformation("User selected {0}. Payload to _service.IsCustomerAccount:=====>{1}", accountType.ToString(), JsonConvert.SerializeObject(new { Account_Identifier = accountIdentifier, User_Bank = user.BankId }));
                    if ((await _service.IsCustomerAccount(accountIdentifier, user.BankId)).IsSuccessful == false)
                    {
                        response.Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH,
                            ResponseDescription = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH, language)
                        };
                        _logger.LogInformation("Customer account dos not match .  Error Response:=====> {0}", JsonConvert.SerializeObject(response));
                        return response;
                    }
                    _logger.LogInformation("Calling _service.GetTransactions for Bank in the GetRecentTransactionsWithDaterange method of Accountservice. Payload :=====>{0}"
                        ,
                        JsonConvert.SerializeObject(new { Account_Identifier = accountIdentifier, StartDate = start, EndDate = end }));

                    response = await _service.GetTransactions(accountIdentifier, start, end);
                    _logger.LogInformation("Response from _service.GetTransactions for Bank in the  GetRecentTransactionsWithDaterange method  inside the AccountService. Response:====> {0}",
                        JsonConvert.SerializeObject(response));
                    break;
                case AccountType.WALLET:
                    _logger.LogInformation("User selected {0}", accountType.ToString());
                    if (accountIdentifier != user.WalletNumber)
                    {
                        response.Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH,
                            ResponseDescription = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH, language)
                        };
                        _logger.LogInformation("from GetRecentTransactionsWithDaterange for Wallet inside the AccountService: User accountidentifier is different from the walletnumber {0}", JsonConvert.SerializeObject(response));
                        return response;
                    }
                    _logger.LogInformation("Calling _service.GetTransactions. Payload :=====>{0}"
                        ,
                        JsonConvert.SerializeObject(new { Account_Identifier = accountIdentifier, StartDate = start, EndDate = end }));

                    response = await _walletService.GetTransactions(accountIdentifier, start, end);
                    _logger.LogInformation("Response from _walletService.GetTransactions in the GetRecentTransactionsWithDaterange method  inside the AccountService. Response:====> {0}",
                        JsonConvert.SerializeObject(response));
                    break;
            }

            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);

            }

            _logger.LogInformation("Response from GetRecentTransactionsWithDaterange inside the AccountService. Response:====> {0}",
                JsonConvert.SerializeObject(response));



            return response;
        }

        private async Task<IEnumerable<Transaction>> LocalDbTransactionrecords(long customerId, TransactionStatus transactionStatus, string accountNumber, DateTime start, DateTime end)
        {


            var transactions = await  _transactionDAO.GetTransactions(customerId,transactionStatus,accountNumber,start,end);

            return transactions;
        }

        public async Task<ServiceResponse<IEnumerable<StatementRecord>>> GetRecentTransactions(string accountIdentifier, AccountType accountType, AuthenticatedUser user, string language)
        {
            var response = new ServiceResponse<IEnumerable<StatementRecord>>(false);
            var end = DateTime.UtcNow;
            var start = end.AddDays(_settings.StatementOffset);
            _logger.LogInformation("Inside the GetRecentTransactions of accountService");
            switch (accountType)
            {
                case AccountType.BANK:
                    _logger.LogInformation("User selected {0}. Payload to _service.IsCustomerAccount:=====>{1}",  accountType.ToString(), JsonConvert.SerializeObject(new{Account_Identifier= accountIdentifier,User_Bank= user.BankId }));
                    if ((await _service.IsCustomerAccount(accountIdentifier, user.BankId)).IsSuccessful==false)
                    {
                        response.Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH,
                            ResponseDescription = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH, language)
                        };
                        _logger.LogInformation("Customer account dos not match .  Error Response:=====> {0}", JsonConvert.SerializeObject(response));
                        return response;
                    }
                    _logger.LogInformation("Calling _service.GetTransactions for Bank in the GetRecentTransactions method of Accountservice. Payload :=====>{0}"
                        , 
                        JsonConvert.SerializeObject(new { Account_Identifier =   accountIdentifier,StartDate= start,EndDate= end }));

                    response = await _service.GetTransactions(accountIdentifier, start, end);
                    var data = response.GetPayload().ToList();
                    for(int i = 0; i < data.Count; i++)
                    {
                        var d = data[i];

                    }
                    _logger.LogInformation("Response from _service.GetTransactions for Bank in the  GetRecentTransactions method  inside the AccountService. Response:====> {0}",
                        JsonConvert.SerializeObject(response));
                    break;
                case AccountType.WALLET:
                    _logger.LogInformation("User selected {0}", accountType.ToString());
                    if (accountIdentifier != user.WalletNumber)
                    {
                        response.Error = new ErrorResponse
                        {
                            ResponseCode = ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH,
                            ResponseDescription = _messageProvider.GetMessage(ResponseCodes.ACCOUNT_CUSTOMER_MISMATCH, language)
                        };
                        _logger.LogInformation("from GetRecentTransactions for Wallet inside the AccountService: User accountidentifier is different from the walletnumber {0}", JsonConvert.SerializeObject(response) );
                        return response;
                    }
                    _logger.LogInformation("Calling _service.GetTransactions. Payload :=====>{0}"
                        ,
                        JsonConvert.SerializeObject(new { Account_Identifier = accountIdentifier, StartDate = start, EndDate = end }));

                    response = await _walletService.GetTransactions(accountIdentifier, start, end);
                    _logger.LogInformation("Response from _walletService.GetTransactions in the GetRecentTransactions method  inside the AccountService. Response:====> {0}",
                        JsonConvert.SerializeObject(response));
                    break;
            }

            if (!response.IsSuccessful)
            {
                response.Error.ResponseDescription = _messageProvider.GetMessage(response.Error.ResponseCode, language);
               
            }

            _logger.LogInformation("Response from GetRecentTransactions inside the AccountService. Response:====> {0}",
                JsonConvert.SerializeObject(response));



            return response;
        }

 


    }
}
