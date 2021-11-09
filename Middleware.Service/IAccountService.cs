using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Middleware.Service.DTOs;
using Middleware.Core.DTO;

namespace Middleware.Service
{
    public interface IAccountService
    {
        Task<ServiceResponse<IEnumerable<BankAccount>>> GetAccounts(string customerID, string language);
        Task<ServiceResponse<Wallet>> GetWallet(string walletNumber, string language);
        Task<ServiceResponse<IEnumerable<StatementRecord>>> GetRecentTransactions(string accountIdentifier, AccountType accountType, AuthenticatedUser user, string language);
        Task<ServiceResponse<CompositeAccountData>> GetCustomerAccounts(AuthenticatedUser user, string language);
        Task<ServiceResponse<IEnumerable<StatementRecord>>> GetRecentTransactionsWithDaterange(string accountIdentifier, AccountType accountType, AuthenticatedUser user, DateTime startDate, DateTime endDate, string language);

 
    }
}
