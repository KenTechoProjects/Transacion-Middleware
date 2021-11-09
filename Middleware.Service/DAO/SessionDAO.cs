using System;
using System.Threading.Tasks;
using Middleware.Service.Model;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using Middleware.Core.DAO;

namespace Middleware.Service.DAO
{
    public class SessionDAO : BaseDAO, ISessionDAO
    {

        public SessionDAO(IDbConnection connection) : base(connection)
        {
        }
        public async Task Delete(string token)  
        {
            await _connection.ExecuteAsync("DELETE FROM Sessions WHERE token = @token",
                                                               new { token });
        }

        public async Task DeleteCustomerSessions(long customerId)
        {
            var transaction = UnitOfWorkSession?.GetTransaction();
            await _connection.ExecuteAsync("DELETE FROM Sessions WHERE customer_Id = @customerId",
                                                                   new { customerId }, transaction);
        }

        public async Task DeleteCustomerSessions(string username)
        {
            var transaction = UnitOfWorkSession?.GetTransaction();
            await _connection.ExecuteAsync("DELETE FROM Sessions WHERE UserName = @UserName",
                                                                   new Session {  UserName = username }, transaction);
        }


        public async Task<Session> Find(string token)
        {
            return await _connection.QuerySingleOrDefaultAsync<Session>("SELECT * FROM Sessions c WHERE c.token = @token",
                                                                    new { token });
        }

        public async Task Add(Session session)
        {
            string sql = @"INSERT INTO Sessions( 
                                BankId,
                                WalletNumber,
                                Username,
                                StartDate,
                                Token,
                                Customer_Id) 
                            VALUES (
                                @BankId,
                                @WalletNumber,
                                @UserName,
                                @StartDate,
                                @Token,
                                @Customer_Id)";

            var transaction = UnitOfWorkSession?.GetTransaction();
            await _connection.ExecuteAsync(sql, session, transaction);

        }


    }
}