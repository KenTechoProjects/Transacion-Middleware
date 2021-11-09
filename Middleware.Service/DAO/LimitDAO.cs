using System;
using System.Threading.Tasks;
using Middleware.Service.Model;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using Middleware.Core.Model;

namespace Middleware.Service.DAO
{
    public class LimitDAO : ILimitDAO
    {
        private readonly IDbConnection _connection;

        public LimitDAO(IDbConnection connection)
        {
            _connection = connection;
            if (_connection.State == ConnectionState.Closed) { _connection.Open(); }
        }

        public async Task<Limit> Find(TransactionType transactionType)
        {
            var limit = await _connection.QueryFirstOrDefaultAsync<Limit>("SELECT * FROM Limits c WHERE c.TransactionType = @transactionType",
                                                                 new Limit {  TransactionType = transactionType });
            return limit;
        }
    }
}