using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Middleware.Core.DAO;
using Middleware.Service.Model;

namespace Middleware.Service.DAO
{
    public class TransactionTrackerDAO : BaseDAO, ITransactionTrackerDAO
    {

        public TransactionTrackerDAO(IDbConnection connection) : base(connection)
        {

        }

        public async Task<long> Add(Tracker trackingInfo)
        {
            var query = @"INSERT INTO Trackers (CustomerId, TransactionReference, TransactionTime) 
                                                VALUES (@CustomerId, @TransactionReference, @TransactionTime); SELECT SCOPE_IDENTITY()";

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            var token = (UnitOfWorkSession)base.UnitOfWorkSession;
            return await _connection.QueryFirstOrDefaultAsync<long>(query, trackingInfo, token?.GetTransaction());
        }

        public async Task<bool> Find(string transactionReference)
        {
            var result = false;
            var response = await _connection.QueryFirstOrDefaultAsync<Tracker>(@"SELECT * FROM Trackers WHERE TransactionReference = @transactionReference", 
                                                                                                 new Tracker { TransactionReference = transactionReference});
            if (response != null)
            {
                result = true;
            }

            return result;
        }
    }
}
