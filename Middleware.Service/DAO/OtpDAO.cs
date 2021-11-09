using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Middleware.Core.DAO;
using Middleware.Service.Model;

namespace Middleware.Service.DAO
{
    public class OtpDAO : BaseDAO, IOtpDAO
    {
        private readonly ILogger _logger;
        public OtpDAO(IDbConnection connection, ILoggerFactory logger) : base(connection)
        {
            _logger = logger.CreateLogger(typeof(OtpDAO));
        }

        public async Task Add(Otp otp)
        {
            

                string query = @"INSERT INTO OTPs
                             (userid,
                              salt,
                              code,
                              datecreated,
                              purpose)
                            VALUES
                            (
                              @UserId,
                              @Salt,
                              @Code,
                              @DateCreated,
                              @Purpose)";

                await _connection.ExecuteAsync(query, otp, UnitOfWorkSession?.GetTransaction());
            

        }

        public async Task Delete(long id)
        {
            
                string query = @"DELETE FROM OTPs WHERE id = @id";
                await _connection.ExecuteAsync(query, new { id }, UnitOfWorkSession?.GetTransaction());

         
        }

        public async Task<Otp> Find(string userId, OtpPurpose purpose)
        {
           
                _logger.LogInformation("Inside the Find method of the OtpDAO   at {0}", DateTime.UtcNow);
                var query = @"SELECT * FROM OTPs WHERE userId = @userId AND purpose = @purpose";
                var result=  await _connection.QuerySingleOrDefaultAsync<Otp>(query, new { userId, purpose });
            _logger.LogInformation("Leaving Otp find method of otpDAO");
                return result;
            
        }

        public async Task Update(Otp otp)
        {
            
                _logger.LogInformation("Inside the Update method of the Otp Service  at {0}", DateTime.UtcNow);
                string query = @"UPDATE OTPs
                             SET code = @Code,
                                 salt = @Salt,
                                 datecreated = @DateCreated
                             WHERE id = @Id";
                await _connection.ExecuteAsync(query, new { otp.Code, otp.Salt, otp.Id, otp.DateCreated },
                    UnitOfWorkSession?.GetTransaction());
            _logger.LogInformation("Updated OTP");
           

        }
    }
}
