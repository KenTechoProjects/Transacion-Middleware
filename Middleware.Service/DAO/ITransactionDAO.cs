using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Core.Model;
using Middleware.Service.Model;

namespace Middleware.Service.DAO
{
    public interface ITransactionDAO
    {
        Task<Transaction> Find(long ID);
        Task<decimal> GetDailySum(long customerId, TransactionType transactionType, DateTime date);
        Task<bool> Add(Transaction transaction);
        Task<bool> Update(Transaction transaction);
        Task<IEnumerable<Transaction>> GetTransactions(long customerId);
    }
}
