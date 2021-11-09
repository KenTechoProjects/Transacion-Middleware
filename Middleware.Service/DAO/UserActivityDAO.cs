using System;
using System.Threading.Tasks;
using Middleware.Service.Model;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Middleware.Core.DAO;

namespace Middleware.Service.DAO
{
    public class UserActivityDAO : BaseDAO, IUserActivityDAO
    {
        public UserActivityDAO(IDbConnection connection) : base(connection)
        {

        }

        public async Task Insert(UserActivity userActivity)
        {

            string sql = @"INSERT INTO UserActivityLogs (CustomerId,Activity,ActivityDate,ActivityResult,ResultDescription,walletNumber) 
                            VALUES 
                          (@CustomerId,@Activity,@ActivityDate,@ActivityResult,@ResultDescription,@walletNumber)";

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            await _connection.ExecuteAsync(sql, userActivity, UnitOfWorkSession?.GetTransaction());
         //var data=   await _connection.QueryFirstOrDefaultAsync<UserActivity>(sql, userActivity, UnitOfWorkSession?.GetTransaction());
           // return data;
        }

        public async Task<IEnumerable<UserActivity>> GetUserActivities(long CustomerID)
        {
            return await _connection.QueryAsync<UserActivity>("SELECT TOP 5 * FROM UserActivityLogs c WHERE c.CustomerId = @CustomerID ORDER BY ActivityDate DESC",
                                                                          new UserActivity { CustomerId = CustomerID });
        }
    }
}