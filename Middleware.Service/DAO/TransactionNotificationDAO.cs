using Dapper;
using Middleware.Core.DAO;
using Middleware.Core.Model;
using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.DAO
{
    public class TransactionNotificationDAO : BaseDAO, ITransactionNotificationDAO
    {
       // private readonly IDbConnection _connection;

        public TransactionNotificationDAO(IDbConnection connection) : base(connection)
        {
           // _connection = connection;
        }

        #region Public Method
        public async Task<List<Transaction>> GetTransactionNotificationsForCustomer(List<string> accountNumbers)
        {
            string responseCode, sql;
            FetchTransactioNQuery(accountNumbers, out responseCode, out sql);
            var data = await _connection.QueryAsync<Transaction>(sql, new { responseCode });


            return data.ToList();

        } 
        //Testing on UpdateDestinationTransactionNotificationsForCustomer and UpdateSourceTransactionNotificationsForCustomer
        public async Task<ServiceResponse> UpdateDestinationTransactionNotificationsForCustomer(List<string> accountNumbers, long transactionId)
        {
            var response = new ServiceResponse(false);
            string responseCode, sql;
            QueryFeedDestination(accountNumbers,"destinationaccountid","DestinationTransactionTag", out responseCode, out sql);
            var executionCount = await _connection.ExecuteAsync(sql, new { responseCode, transactionId }, UnitOfWorkSession?.GetTransaction());

            response.IsSuccessful = executionCount > 0;
            var mss = executionCount > 1 ? " records affected" : "record affected";
            response.Data = $"{executionCount} {mss}";
            response.FaultType = FaultMode.NONE;
            return response;

        }

     
        public async Task<ServiceResponse> UpdateSourceTransactionNotificationsForCustomer(List<string> accountNumbers, long transactionId, long cusotmerId)
        {
            var response = new ServiceResponse(false);
            string responseCode, sql;
            QueryFeedSource(accountNumbers,"sourceaccountid","SourceTransactionTag", out responseCode, out sql);

            var executionCount = await _connection.ExecuteAsync(sql, new { responseCode, transactionId, cusotmerId }, UnitOfWorkSession?.GetTransaction());

            response.IsSuccessful = executionCount > 0;
            var mss = executionCount > 1 ? " records affected" : "record affected";
            response.Data = $"{executionCount} {mss}";
            response.FaultType = FaultMode.NONE;
            return response;
        }

        #endregion


        #region Private Methods

        private static void FetchTransactioNQuery(List<string> accountNumbers, out string responseCode, out string sql)
        {
            StringBuilder queryT = new StringBuilder();
            int count = 0;
            queryT.Append("(");
            foreach (var accontNumber in accountNumbers)
            {
                count++;
                if (count != accountNumbers.Count)
                {
                    //queryT.Append($"destinationaccountid='{accontNumber}' Or sourceaccountid='{accontNumber}' OR ");
                    queryT.Append($"(destinationaccountid='{accontNumber}' and DestinationTransactionTag=1)  OR ");
                    queryT.Append($"(sourceaccountid='{accontNumber}' and SourceTransactionTag=1) OR ");
                }
                else
                {
                    // queryT.Append($"destinationaccountid='{accontNumber}' Or sourceaccountid='{accontNumber}') and ");
                    queryT.Append($"(destinationaccountid='{accontNumber}' and DestinationTransactionTag=1)  OR ");
                    queryT.Append($"(sourceaccountid='{accontNumber}' and SourceTransactionTag=1) ) and ");
                }

            }
            string sql1 = queryT.ToString();
            responseCode = "00";
            sql = $"SELECT * FROM Transactions WHERE {sql1} responsecode=@responseCode;";
        }

        private static void QueryFeedDestination(List<string> accountNumbers, string accountNumberLeg, string destinationTransactiontype, out string responseCode, out string sql)
        {
            StringBuilder queryT = new StringBuilder();
            int count = 0;
            queryT.Append("Update Transactions set DestinationTransactionTag=2 WHERE (");
            foreach (var accontNumber in accountNumbers)
            {
                count++;
                if (count != accountNumbers.Count)
                {
                    queryT.Append($"({accountNumberLeg}='{accontNumber}' and {destinationTransactiontype}=1) Or ");
                    // queryT.Append($"(destinationaccountid='{accontNumber}' and DestinationTransactionTag=1) Or ");
                }
                else
                {
                    queryT.Append($"({accountNumberLeg}='{accontNumber}' and {destinationTransactiontype}=1) ) and ");
                    // queryT.Append($"(destinationaccountid='{accontNumber}' and DestinationTransactionTag=1) ) and ");
                }

            }
            string sql1 = queryT.ToString();
            responseCode = "00";
            sql = $"{sql1} responsecode=@responseCode and ID=@transactionId;";
        }
        private static void QueryFeedSource(List<string> accountNumbers, string accountNumberLeg, string soureTransactiontype, out string responseCode, out string sql)
        {
            StringBuilder queryT = new StringBuilder();
            int count = 0;
            queryT.Append("Update Transactions set SourceTransactionTag=2 WHERE (");
            foreach (var accontNumber in accountNumbers)
            {
                count++;
                if (count != accountNumbers.Count)
                {
                    queryT.Append($"(({accountNumberLeg}='{accontNumber}' and  {soureTransactiontype}=1) and customerid=@cusotmerId)  Or ");
                    // queryT.Append($"((sourceaccountid='{accontNumber}' and  SourceTransactionTag=1) and customerid=@cusotmerId)  Or ");
                }
                else
                {
                    queryT.Append($"(({accountNumberLeg}='{accontNumber}')  and  {soureTransactiontype}=1) and customerid=@cusotmerId ) and ");
                    //queryT.Append($"((sourceaccountid='{accontNumber}')  and  SourceTransactionTag=1) and customerid=@cusotmerId ) and ");
                }

            }
            string sql1 = queryT.ToString();
            responseCode = "00";
            sql = $"{sql1} responsecode=@responseCode and ID=@transactionId;";
        }
        #endregion
    }
}
