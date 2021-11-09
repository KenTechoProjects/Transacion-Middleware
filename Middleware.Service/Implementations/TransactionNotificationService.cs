using Dapper;
using Microsoft.Extensions.Logging;
using Middleware.Core.DAO;
using Middleware.Core.Model;
using Middleware.Service.DAO;
using Middleware.Service.DTOs;
using Middleware.Service.FIServices;
using Middleware.Service.Processors;
using Middleware.Service.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.Implementations
{
    //LocalClient
    public class TransactionNotificationService : ITransactionNotificationService
    {
        private IAccountService _accountService;
        private ITransactionNotificationDAO _tranactioNotificationDAO;
        private readonly IDbConnection _connection;
        private readonly ICustomerDAO _customerDAO;
        private readonly ILogger _log;
        public readonly IMessageProvider _messageProvider;
        public TransactionNotificationService(IDbConnection connection, ILoggerFactory log, IMessageProvider messageProvider,
            ITransactionNotificationDAO tranactioNotificationDAO, IAccountService accountService, ICustomerDAO customerDAO) : base()
        {
            _connection = connection;
            _log = log.CreateLogger<TransactionNotificationService>();
            _messageProvider = messageProvider;

            _tranactioNotificationDAO = tranactioNotificationDAO;
            _accountService = accountService;
            _customerDAO = customerDAO;
        }
        #region Public Methods
        public async Task<ServiceResponse> GetTransactionNotificationsForCustomer(string walletNumber, string language)
        {
            var response = new ServiceResponse(false);
            try
            {
                var customer = await _customerDAO.FindByWalletNumber(walletNumber);
                if (customer != null)
                {
                    var accounts = await CustomerAccounts(customer.BankId, walletNumber, language);
                    var getTrasactions = await _tranactioNotificationDAO.GetTransactionNotificationsForCustomer(accounts);
                    if (getTrasactions != null && getTrasactions.Count() > 0)
                    {


                        var resultDebit = CustomerDebit(customer.Id, getTrasactions, accounts).ToList();

                        var resultCredited = CustomerCredited(customer.Id, getTrasactions, accounts).ToList();

                        // var resultOfUnioData = resultDebit.Union(resultCredited);

                        var TransactionsMade = new TransactionNotificationDTO
                        {
                            Credited = new TransactionsNotificationsDataFinal
                            {
                                RecordCount = resultCredited.Count,
                                Transactioncategory = Transactioncategory.Credit,
                                TransactionsDTOs = resultCredited
                            },
                            Debited = new TransactionsNotificationsDataFinal
                            {

                                RecordCount = resultDebit.Count,
                                Transactioncategory = Transactioncategory.Debit,
                                TransactionsDTOs = resultDebit

                            }
                        };
                        response.Data = TransactionsMade;
                        response.IsSuccessful = true;
                        return response;

                    }
                }



            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "Error occurred in the GetTransactionNotificationsForCustomer method of TransactionNotificationService");
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };

            }
            return response;


        }
        public async Task<ServiceResponse> GetTransactionNotificationsForCustomerFront(string walletNumber, string lnguage)
        {
            var response = new ServiceResponse(false);
            try
            {
                var customer = await _customerDAO.FindByWalletNumber(walletNumber);
                if (customer != null)
                {
                    var accounts = await CustomerAccounts(customer.BankId, walletNumber, lnguage);


                    var result = await _tranactioNotificationDAO.GetTransactionNotificationsForCustomer(accounts);

                    var resultDebit = CustomerDebit(customer.Id, result, accounts).ToList();

                    var resultCredited = CustomerCredited(customer.Id, result, accounts).ToList();

                    var data = new TransactionNotificationDTO
                    {
                        Credited = new TransactionsNotificationsDataFinal
                        {
                            RecordCount = resultCredited.Count

                        },
                        Debited = new TransactionsNotificationsDataFinal
                        {
                            RecordCount = resultDebit.Count

                        }
                    };
                    if (result != null && result.Count > 0)
                    {



                        response.IsSuccessful = true;
                        response.Data = result.Count;
                        response.FaultType = FaultMode.NONE;
                    }

                }
            }
            catch (Exception ex)
            {
                response.FaultType = FaultMode.SERVER;
                _log.LogCritical(ex, "Error occurred in the GetTransactionNotificationsForCustomerFront method of TransactionNotificationService");
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, lnguage)
                };
            }

            return response;


        }

        public async Task<ServiceResponse> UpdateTransactionNotificationsForCustomer(AuthenticatedUser user, string language, long transationId, bool source = false)
        {
            var response = new ServiceResponse(false);
            try
            {


                var accounts = await CustomerAccounts(user.BankId, user.WalletNumber, language);
                if (source == true)
                {

                    response = await _tranactioNotificationDAO.UpdateSourceTransactionNotificationsForCustomer(accounts, transationId,user.Id);
                    if (response.IsSuccessful == true)
                    {
                        response.FaultType = FaultMode.NONE;
                    }
                }
                else
                {

                    response = await _tranactioNotificationDAO.UpdateDestinationTransactionNotificationsForCustomer(accounts, transationId);
                    if (response.IsSuccessful == true)
                    {
                        response.FaultType = FaultMode.NONE;
                    }

                }
                //After a sucessful transaction update, ask Dayo if he wants to reload  notification status of the updated record or all of the new transaction records?
                //for now I discssed with Dayo on te 13-1--2021 by 1:19 pm he agreed that he will only update the status of the viewed record 
            }
            catch (Exception ex)
            {

                _log.LogCritical(ex, "Error occurred in the UpdateTransactionNotificationsForCustomer method of TransactionNotificationService");
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };
            }
            return response;
        }
        public async Task<ServiceResponse> UpdateTransactionNotificationsForCustomer_(string walletNumber, string language, long transationId, bool source = false)
        {
            var response = new ServiceResponse(false);
            try
            {

                var customer = await _customerDAO.FindByWalletNumber(walletNumber);
                if (customer != null)
                {
                    AuthenticatedUser user = new AuthenticatedUser
                    {
                        BankId = customer.BankId,
                        UserName = walletNumber,
                        WalletNumber = walletNumber, Id=customer.Id
                    };


                var accounts = await CustomerAccounts(user.BankId, user.UserName, language);
                    if (source == true)
                    {

                        response = await _tranactioNotificationDAO.UpdateSourceTransactionNotificationsForCustomer(accounts, transationId,user.Id);
                        if (response.IsSuccessful == true)
                        {
                            response.FaultType = FaultMode.NONE;
                        }
                    }
                    else
                    {

                        response = await _tranactioNotificationDAO.UpdateDestinationTransactionNotificationsForCustomer(accounts, transationId);
                        if (response.IsSuccessful == true)
                        {
                            response.FaultType = FaultMode.NONE;
                        }
                    }
                    //After a sucessful transaction update, ask Dayo if he wants to reload  notification status of the updated record or all of the new transaction records?
                    //for now I discssed with Dayo on te 13-1--2021 by 1:19 pm he agreed that he will only update the status of the viewed record 
                }

            }
            catch (Exception ex)
            {

                _log.LogCritical(ex, "Error occurred in the UpdateTransactionNotificationsForCustomer method of TransactionNotificationService");
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.GENERAL_ERROR,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.GENERAL_ERROR, language)
                };
            }
            return response;
        }

        #endregion

        #region private ethods

        private static List<TransactionsNotificationsData> CustomerCredited(long customerId, List<Transaction> getTrasactions, List<string> accounts)
        {
            var resultCredit = getTrasactions.Where(transaction =>
                                     accounts
                                     .Select(arrayElement => arrayElement.Trim())
                                     .Any(value => accounts.Contains(value) && (transaction.CustomerId != customerId)
                                     || (transaction.CustomerId == customerId &&
                                     accounts.Contains(value)==value.Contains(transaction.DestinationAccountID)) 
                                     && transaction.DestinationTransactionTag != TransactionTag.Old)).ToList();

            var data = resultCredit.Select(p =>
                new TransactionsNotificationsData
                {
                    Id = p.ID,
                    Amount = p.Amount,
                    CustomerId = p.CustomerId,
                    DateTransacted = p.DateCreated.ToLongDateString(),
                    DestinationAccount = p.DestinationAccountID,
                    SourceAcount = p.SourceAccountId,
                    TimeTransacted = p.DateCreated.ToLongTimeString(),
                    DestinationTransactionTag = p.DestinationTransactionTag,
                    SourceTransactionTag = p.SourceTransactionTag,
                    TransactionType = p.TransactionType,
                    IsCredited = true
                });
            return data.ToList();
        }

        private static List<TransactionsNotificationsData> CustomerDebit(long customerId, List<Transaction> getTrasactions, List<string> accounts)
        {
            var resultDebit = getTrasactions.Where(transaction =>
                  accounts
                  .Select(arrayElement => arrayElement.Trim())
                  .Any(value => accounts.Contains(value) && transaction.CustomerId == customerId)
                  &&transaction.SourceTransactionTag!= TransactionTag.Old).ToList();


            var data = resultDebit.Select(p =>
                new TransactionsNotificationsData
                {
                    Id = p.ID,
                    Amount = p.Amount,
                    CustomerId = p.CustomerId,
                    DateTransacted = p.DateCreated.ToLongDateString(),
                    DestinationAccount = p.DestinationAccountID,
                    SourceAcount = p.SourceAccountId,
                    TimeTransacted = p.DateCreated.ToLongTimeString(),
                    DestinationTransactionTag = p.DestinationTransactionTag,
                    SourceTransactionTag = p.SourceTransactionTag,
                    TransactionType = p.TransactionType,
                    IsCredited = false
                });
            return data.ToList();
        }

        private async Task<List<string>> CustomerAccounts(string bankId, string walletnumber, string lnguage)
        {


            List<string> accounts = new List<string>();




            var custmerAccounts = await _accountService.GetAccounts(bankId, lnguage);
            var custmerWallets = await _accountService.GetWallet(walletnumber, lnguage);


            if (custmerAccounts.GetPayload() != null && custmerAccounts.GetPayload().Count() > 0)
            {
                accounts.AddRange(custmerAccounts.GetPayload().Select(p => p.Number));
            }
            if (custmerWallets.GetPayload() != null)
            {
                accounts.Add(custmerWallets.GetPayload().WalletNumber);
            }
            return accounts;

        }


        #endregion
    }
}
