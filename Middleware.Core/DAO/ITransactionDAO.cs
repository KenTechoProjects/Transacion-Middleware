using Middleware.Core.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Middleware.Core.DAO
{
    public interface ITransactionDAO
    {
        Task<Transaction> Find(long ID);
        Task<decimal> GetDailySum(long customerId, TransactionType transactionType, DateTime date);
        Task<long> Add(Transaction transaction);
        Task<bool> Update(Transaction transaction);
        Task<IEnumerable<Transaction>> GetTransactions(long customerId);
        Task<IEnumerable<Transaction>> GetTransactions(long customerId, TransactionStatus transactionStatus, string accountNumber, DateTime start, DateTime end);
        //Task<List<>>
    }
}
