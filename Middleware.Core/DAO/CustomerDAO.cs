using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Middleware.Core.Model;

namespace Middleware.Core.DAO
{

    public class CustomerDAO : BaseDAO, ICustomerDAO
    {
        private readonly ILogger _log;

        public CustomerDAO(IDbConnection connection, ILoggerFactory log) : base(connection)
        {
            _log = log.CreateLogger(typeof(CustomerDAO));
        }
        public async Task<bool> FindByReferralCode(string referrelaCode)
        {



            _log.LogInformation("Inside the FindByReferralCode  of CustomerDAO");
            var data = await _connection.QueryFirstOrDefaultAsync<Customer>("SELECT * FROM Customers  WHERE  ReferralCode =@referrelaCode",
            new { referrelaCode });
            _log.LogInformation("Finished the customer check the FindByReferralCode  of CustomerDAO");
            return data != null;

        }
        public async Task<Customer> FindByEmail(string emailAddress)
        {
            return await _connection.QueryFirstOrDefaultAsync<Customer>("SELECT * FROM Customers c WHERE lower(c.emailAddress) = lower(@emailAddress)",
                                                                       new Customer { EmailAddress = emailAddress });
        }

        public async Task<Customer> FindByCustomerId(string customerId)
        {
            return await _connection.QueryFirstOrDefaultAsync<Customer>("SELECT * FROM Customers c WHERE c.BankId = @BankId",
                                                                       new Customer { BankId = customerId });
        }

        public async Task<Customer> Find(long Id)
        {
            return await _connection.QueryFirstOrDefaultAsync<Customer>("SELECT * FROM Customers c WHERE c.Id = @Id",
                                                                       new Customer { Id = Id });

        }

        public async Task<Customer> Add(Customer customer)
        {



            string sql = @"INSERT INTO Customers 
                            (
                            FirstName,
                            LastName,
                            Middlename,
                            EmailAddress,
                            IsActive,
                            AccountNumber,
                            OnboardingStatus,
                            DateCreated,
                            WalletNumber,
                            BankId,
                            HasWallet,
                            HasAccount,Gender,
                            Title,ReferralCode,ReferredBy
                             ) OUTPUT INSERTED.*
                        VALUES 
                            (
                            @FirstName,
                            @LastName,
                            @Middlename,
                            @EmailAddress,
                            @IsActive,
                            @AccountNumber,
                            @OnboardingStatus,
                            @DateCreated,
                            @WalletNumber,
                            @BankId,
                            @HasWallet,
                            @HasAccount,@Gender,
                            @Title,@ReferralCode,@ReferredBy);";


            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
                var trans = UnitOfWorkSession?.GetTransaction();
                if (trans == null)
                {
                    trans = _connection.BeginTransaction();
                }



                var result = await _connection.QueryFirstOrDefaultAsync<Customer>(sql, customer, trans);

                return result;
            }
            else
            {

                var result = await _connection.QueryFirstOrDefaultAsync<Customer>(sql, customer, UnitOfWorkSession?.GetTransaction());

                return result;
            }

            
        }

        public async Task<Customer> FindByAccountNumber(string AccountNumber)
        {
            var result = await _connection.QueryFirstOrDefaultAsync<Customer>("SELECT * FROM Customers c WHERE c.AccountNumber = @AccountNumber",
                                                                                       new Customer { AccountNumber = AccountNumber });
            return result;
        }


        public async Task<Customer> FindByWalletNumber(string WalletNumber)
        {
            var result = await _connection.QueryFirstOrDefaultAsync<Customer>("SELECT * FROM Customers c WHERE c.WalletNumber =@WalletNumber",
                                                                      new { WalletNumber });

            return result;
        }
        public async Task Update(Customer customer)
        {

            _log.LogInformation("Inside the Update method of the Customer DAO at {0}", DateTime.UtcNow);




            string sql = @"UPDATE Customers 
                        SET FirstName = @FirstName,
                            LastName = @LastName,
                            EmailAddress = @EmailAddress,
                            IsActive = @IsActive,
                            AccountNumber = @AccountNumber, 
                            OnboardingStatus = @OnboardingStatus,                            
                            MiddleName = @MiddleName, 
                            LastLogin = @LastLogin,
                            HasAccount = @HasAccount,
                            BankId = @BankId, ReferralCode=@ReferralCode,
                        ReferredBy=@ReferredBy
                        WHERE Id = @Id";

            var trans = UnitOfWorkSession?.GetTransaction();

            var updated = await _connection.ExecuteAsync(sql, customer, UnitOfWorkSession?.GetTransaction());

        }

        public async Task UpdateCustomerStatus(Customer customer)
        {
            string sql = @"UPDATE Customers 
                        SET IsActive = @IsActive WHERE Id = @Id";
            await _connection.ExecuteAsync(sql, customer, UnitOfWorkSession?.GetTransaction());
        }

        public async Task<bool> Exists(string walletNumber)
        {
            var query = @"SELECT COUNT(id) FROM Customers
                           WHERE  WalletNumber = @walletNumber";
            var count = await _connection.ExecuteScalarAsync<int>(query, new { walletNumber });
            return count > 0;
        }

        public async Task<bool> HasReachedLoginFailCount(string walletNumber)
        {

          

                var data = DateTime.Now.Date;
                var result = await _connection.QueryFirstOrDefaultAsync<long>("SELECT Count(*) FROM UserActivityLogs WHERE walletNumber =@walletNumber and   CONVERT(date, activitydate)=  CONVERT(date, @data) and  activityresult ='Failed'",
                                                                    new { walletNumber, data }); ;
                var loginFailMaximumCount = await GetMaxLoginCount();
                
                    return result >= loginFailMaximumCount;
            
               
          

            //DailyLoginFailCount
        }
        public async Task<long> GetMaxLoginCount()
        {
            var result = await _connection.QueryFirstOrDefaultAsync("SELECT * from LoginSetting");

            return result.MaxLoginAttempt;
            //DailyLoginFailCount
        }
        public async Task<string> ReferralCodeExists(string referralCode)
        {
            var result= await _connection.QueryFirstOrDefaultAsync<string>("SELECT ReferralCode FROM Customers WHERE ReferralCode=@referralCode;",
                                                                       new   {referralCode});
            return result;
        }


        
    }
}
