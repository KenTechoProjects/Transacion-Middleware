using System;
using System.Threading.Tasks;
using Middleware.Core.Model;

namespace Middleware.Core.DAO
{
    public interface ICustomerDAO : ITransactionCoordinator
    {
        Task<Customer> FindByWalletNumber(string walletNumber);
        Task<bool> FindByReferralCode(string referrelaCode);
        Task<Customer> FindByAccountNumber(string accountNumber);
        Task<Customer> Find(long Id);
        Task<Customer> FindByCustomerId(string customerId);
        Task<Customer> FindByEmail(string emailAddress);
        Task<Customer> Add(Customer customer);
        Task<bool> HasReachedLoginFailCount(string walletNumber);
        Task<long> GetMaxLoginCount();
        Task Update(Customer customer);
        Task UpdateCustomerStatus(Customer customer);
        Task<bool> Exists(string walletNumber);
        Task<string> ReferralCodeExists(string referralCode);
    }
}
