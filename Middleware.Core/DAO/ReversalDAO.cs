using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Core.Model;
using Dapper;
using System.Linq;

namespace Middleware.Core.DAO
{
    public class ReversalDAO : BaseDAO, IReversalDAO
    {

        public ReversalDAO(IDbConnection connection) : base(connection)
        {
        }

        public async Task<Reversal> Find(long Id)
        {
            var sql = "Select r.*, t.* from Reversals r, Transactions t where r.TransactionId = t.id order by r.DateCreated ";
            return (await _connection.QueryAsync<Reversal, Transaction, Reversal>(sql,
                (r, t) =>
                {
                    r.Transaction = t;
                    return r;
                }, new { Id },
                splitOn: "ID"
                )).FirstOrDefault();
        }

        public async Task Add(Reversal reversal)
        {

            string sql = @"INSERT INTO [dbo].[Reversals]
                        ([TransactionId] ,[ReversalType],[ReversalStatus],[DateCreated],[LastTryDate],[RetryCount],ReversalReference,[StatusMessage])
                        VALUES
                        (@TransactionId, @ReversalType, @ReversalStatus, @DateCreated, @LastTryDate,@RetryCount,@ReversalReference,@StatusMessage)";
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            await _connection.ExecuteAsync(sql, reversal, UnitOfWorkSession?.GetTransaction());

        }

        public async Task Update(Reversal reversal)
        {
            string sql = @"UPDATE [dbo].[Reversals]
                       SET [ReversalStatus] = @ReversalStatus
                          ,[LastTryDate] = @LastTryDate
                          ,[RetryCount] = @RetryCount
                          ,[StatusMessage] = @StatusMessage
                          ,[ReversalReference] = @ReversalReference
                            WHERE  [ID] = @ID";
            await _connection.ExecuteAsync(sql, reversal, UnitOfWorkSession?.GetTransaction());
        }

        public async Task<IEnumerable<Reversal>> FindByTypeAndStatus(ReversalType reversalType, ReversalStatus reversalStatus)
        {
            var sql = @"Select r.*, t.* from [dbo].[Reversals] r, [dbo].[Transactions] t where r.TransactionId = t.id and ReversalType = @ReversalType 
                        and ReversalStatus = @reversalStatus ";
            return (await _connection.QueryAsync<Reversal, Transaction, Reversal>(sql,
                (r, t) =>
                {
                    r.Transaction = t;
                    return r;
                }, new { ReversalType = reversalType, ReversalStatus = reversalStatus },
                splitOn: "ID"
                )).ToList();
        }
    }
}
