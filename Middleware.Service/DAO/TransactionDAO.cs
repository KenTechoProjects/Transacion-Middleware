using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Middleware.Service.Model;

namespace Middleware.Service.DAO
{
    public class TransactionDAO : ITransactionDAO
    {
        private readonly IDbConnection _connection;

        public TransactionDAO(IDbConnection connection)
        {
            _connection = connection;
        }


        public async Task<Transaction> Find(long ID)
        {
            return await _connection.QueryFirstOrDefaultAsync<Transaction>("SELECT * FROM Transactions c WHERE c.ID = @ID",
                                                                         new Transaction { ID = ID });
        }

        public async Task<decimal> GetDailySum(long customerId, TransactionType transactionType, DateTime date)
        {
            var result = await _connection.QueryFirstOrDefaultAsync<decimal>(@"Select isnull(sum(amount),0) Total from Transactions c
                                                where c.customerId = @customerId 
                                                and c.TransactionType = @TransactionType 
                                                and c.DateCreated >= @DateCreated and c.DateCreated < dateadd(day, 1, @DateCreated)
                                                and c.TransactionStatus = @TransactionStatus", 
                                                new Transaction { CustomerId = customerId,
                                                                TransactionStatus = TransactionStatus.Successful,
                                                                TransactionType = transactionType,
                                                                DateCreated = date.Date });
                                                return result;
        }

        public async Task<bool> Add(Transaction transaction)
        {
            string sql = @"INSERT INTO [dbo].[Transactions]
                               ([DestinationCountryId]
                               ,[CustomerId]
                               ,[SourceAccountId]
                               ,[DestinationAccountID]
                               ,[BillerID]
                               ,[Narration]
                               ,[TransactionReference]
                               ,[DestinationInstitution]
                               ,[Amount]
                               ,[TransactionType]
                               ,[DateCreated],
                                [TransactionStatus])
                         VALUES
                               (@DestinationCountryId, 
                               @CustomerId, 
                               @SourceAccountId, 
                               @DestinationAccountID, 
                               @BillerID, 
                               @Narration,
                               @TransactionReference,
                               @DestinationInstitution, 
                               @Amount,
                               @TransactionType, 
                               @DateCreated,
                               @TransactionStatus)";
            var result = await _connection.ExecuteAsync(sql, transaction);
            return (int)result == 1;
        }

        public async Task<bool> Update(Transaction transaction)
        {
            string sql = @"UPDATE[dbo].[Transactions]
                         SET  [ResponseCode] = @ResponseCode
                             ,[ResponseTime] = @ResponseTime
                             ,[ProviderReference] = @ProviderReference
                             ,[TransactionStatus] = @TransactionStatus
                        WHERE [TransactionReference] = @transactionReference";
            var result = await _connection.ExecuteAsync(sql, transaction);
            return (int)result == 1;
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(long customerId)
        {
            return await _connection.QueryAsync<Transaction>("SELECT TOP 5 * FROM Transactions c WHERE c.CustomerId = @CustomerId ORDER BY DateCreated DESC",
                                                                         new Transaction { CustomerId = customerId });
        }
    }
}