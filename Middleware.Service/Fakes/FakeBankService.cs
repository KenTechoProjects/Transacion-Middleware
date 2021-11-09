using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;

namespace Middleware.Service.Fakes
{
    public class FakeBankService : ILocalAccountService
    {
        readonly IDictionary<string, IEnumerable<StatementRecord>> _transactions;
        private readonly IDictionary<string, IEnumerable<BankAccount>> _accounts;
        public FakeBankService()
        {
            _accounts = new Dictionary<string, IEnumerable<BankAccount>>
            {
                {
                    "B001",
                    new[]
                    {
                            new BankAccount
                            {
                                Number = "0013456789",
                                Description = "First Savings",
                                Currency = "NGN",
                                Balance = new AccountBalance
                                {
                                    BookBalance = 125000,
                                    AvailableBalance = 125000
                                },
                                IsDebitable = true
                            },
                            new BankAccount
                            {
                                Number = "0016543210",
                                Description = "First Current",
                                Currency = "NGN",
                                Balance = new AccountBalance
                                {
                                    BookBalance = 4000,
                                    AvailableBalance = 3950
                                },
                                IsDebitable = true
                            },
                            new BankAccount
                            {
                                Number = "0016547899",
                                Description = "First Current Dom",
                                Currency = "USD",
                                Balance = new AccountBalance
                                {
                                    BookBalance = 300,
                                    AvailableBalance = 200
                                },
                                IsDebitable = true
                            }
                    }
                },

                {
                    "B002",
                    new[]
                    {
                            new BankAccount
                            {
                                Number = "0123456789",
                                Description = "First Savings",
                                Currency = "GHS",
                                Balance = new AccountBalance
                                {
                                    BookBalance = 125000,
                                    AvailableBalance = 125000
                                },
                                IsDebitable = true
                            },
                            new BankAccount
                            {
                                Number = "9876543210",
                                Description = "First Current",
                                Currency = "GHS",
                                Balance = new AccountBalance
                                {
                                    BookBalance = 4000,
                                    AvailableBalance = 3950
                                },
                                IsDebitable = true
                            }
                    }
                },

                {
                    "B003",
                    new[]
                    {
                            new BankAccount
                            {
                                Number = "0123456789",
                                Description = "First Savings",
                                Currency = "GHS",
                                Balance = new AccountBalance
                                {
                                    BookBalance = 125000,
                                    AvailableBalance = 125000
                                },
                                IsDebitable = true
                            },
                            new BankAccount
                            {
                                Number = "9876543210",
                                Description = "First Current",
                                Currency = "GHS",
                                Balance = new AccountBalance
                                {
                                    BookBalance = 4000,
                                    AvailableBalance = 3950
                                },
                                IsDebitable = true
                            }
                    }
                }
            };


            _transactions = new Dictionary<string, IEnumerable<StatementRecord>>();
            var date = DateTime.Today;
            _transactions.Add("0123456789",
            new List<StatementRecord>
            {
                new StatementRecord
                {
                    Amount = 12.5M,
                    Date = date.AddDays(-4),
                    Description = "Transfer to Kwesi John"
                },
                new StatementRecord
                {
                    Amount = 1.34M,
                    Date = date.AddDays(-3),
                    Description = "Interest earned"
                }

             });


            _transactions.Add("9876543210",
           new List<StatementRecord>
           {
                new StatementRecord
                {
                    Amount = 0.94M,
                    Date = date.AddDays(-6),
                    Description = "Account maintenance charges"
                },
                new StatementRecord
                {
                    Amount = 1.5M,
                    Date = date.AddDays(-6),
                    Description = "Airtime recharge"
                },
                new StatementRecord
                {
                    Amount = 5M,
                    Date = date.AddDays(-1),
                    Description = "Bill payment- Shell Filling station"
                }
            });

        }

        public Task<BasicResponse> DoAccountsBelongtoCustomer(string customerID, string sourceAccount, string destinationAccount)
        {
            return Task.FromResult(new BasicResponse(true));
        }

        public Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber)
        {
            var response = new ServiceResponse<dynamic>(true);
            response.SetPayload(new { AccountName = "FBNBank Customer" });
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber, string bankCode)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<IEnumerable<BankAccount>>> GetAccounts(string customerID)
        {
            var response = new ServiceResponse<IEnumerable<BankAccount>>(false);
            var test = _accounts[customerID.Trim()];
            if (_accounts.TryGetValue(customerID, out var accounts))
            {
                response.IsSuccessful = true;
                response.SetPayload(accounts);
            }
            else
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = string.Empty
                };
            }

            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<StatementRecord>>> GetTransactions(string accountNumber, DateTime start, DateTime end)
        {
            IEnumerable<StatementRecord> result;
            var response = new ServiceResponse<IEnumerable<StatementRecord>>(false);
            if (!_transactions.TryGetValue(accountNumber, out result))
            {
                result = new List<StatementRecord>(0);
            }
            response.IsSuccessful = true;

            response.SetPayload(result);
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<StatementRecord>>> GetTransactionsTestawa(string accountNumber, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public Task<BasicResponse> IsCustomerAccount(string accountNumber, string customerID)
        {
            return Task.FromResult(new BasicResponse(true));
        }

        public Task<BasicResponse> Transfer(BaseTransferRequest request, string reference)
        {
            var response = new BasicResponse(true);
           
            return Task.FromResult(response);
        }
        public Task<BasicResponse> TransferToSelf(SelfTransferRequest request, string reference)
        {
            var response = new BasicResponse(true);

            return Task.FromResult(response);
        }

    }
}