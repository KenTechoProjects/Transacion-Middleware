using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Middleware.Core.Model;

namespace Middleware.Core.DAO
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
        //var result = await _connection.QueryFirstOrDefaultAsync<decimal>(@"Select isnull(sum(amount),0) Total from Transactions c
        //                                        where c.customerId = @customerId 
        //                                        and c.TransactionType = @TransactionType 
        //                                        and c.DateCreated >= @DateCreated
        //                                        and c.TransactionStatus = @TransactionStatus", 
        //                                        new Transaction { CustomerId = customerId,
        //                                                        TransactionStatus = TransactionStatus.Successful,
        //                                                        TransactionType = transactionType,
        //                                                        DateCreated = date.Date });
            var result = await _connection.QueryFirstOrDefaultAsync<decimal>(@"Select isnull(sum(amount),0) Total from Transactions c
                                                where c.customerId = @customerId 
                                                and c.TransactionType = @TransactionType 
                                                and CONVERT(date, c.datecreated) = CONVERT(date,@DateCreated)
                                                and c.TransactionStatus = @TransactionStatus", 
                                                new Transaction { CustomerId = customerId,
                                                                TransactionStatus = TransactionStatus.Successful,
                                                                TransactionType = transactionType,
                                                                DateCreated = date.Date });
                                                return result;
        }

        public async Task<long> Add(Transaction transaction)
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
                                [TransactionStatus],
                                [ResponseCode])
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
                               @TransactionStatus, 
                               @ResponseCode)
                               SELECT SCOPE_IDENTITY()";
            var data= await _connection.QueryFirstOrDefaultAsync<long>(sql, transaction);
            return data;
          //  return (int)result == 1;
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
            return await _connection.QueryAsync<Transaction>("SELECT * FROM Transactions c WHERE c.CustomerId = @CustomerId  ORDER BY DateCreated DESC",
                                                                         new Transaction { CustomerId = customerId });
        }

        public async Task<IEnumerable<Transaction>> GetTransactions(long customerId, TransactionStatus transactionStatus, string accountNumber, DateTime start, DateTime end)
        {
            return await _connection.QueryAsync<Transaction>("SELECT TOP 5 * FROM Transactions c WHERE c.CustomerId = @CustomerId  and transactionstatus=@TransactionStatus ORDER BY DateCreated DESC",
                                                                         new Transaction { CustomerId = customerId, TransactionStatus= transactionStatus });
        }
    }
}