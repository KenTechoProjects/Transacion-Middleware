using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Core.Model;
using Dapper;
using System.Linq;

namespace Middleware.Core.DAO
{
    public class ReversalLogDAO : BaseDAO, IReversalLogDAO
    {

        public ReversalLogDAO(IDbConnection connection) : base(connection)
        {
        }

        public async Task Add(ReversalLog reversalLog)
        {

            string sql = @"INSERT INTO [dbo].[ReversalLogs]
           ([TransactionId],[ReversalReference],[ReversalType],[ReversalStatus],
            [DateCreated] ,[StatusMessage], DestinationAccountID, SourceAccountID, Amount, Narration)
            VALUES
           (@TransactionId ,@ReversalReference,@ReversalType,@ReversalStatus,
            @DateCreated,@StatusMessage, @DestinationAccountID, @SourceAccountID, @Amount, @Narration)";
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            await _connection.ExecuteAsync(sql, reversalLog, UnitOfWorkSession?.GetTransaction());
        }
    }
}
