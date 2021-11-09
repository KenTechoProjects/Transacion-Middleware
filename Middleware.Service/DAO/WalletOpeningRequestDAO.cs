using System;
using System.Data;
using System.Threading.Tasks;
using Middleware.Service.Model;
using Dapper;
using Middleware.Core.DAO;
using Middleware.Service.Utilities;
using Microsoft.Extensions.Logging;

namespace Middleware.Service.DAO
{
    public class WalletOpeningRequestDAO : BaseDAO, IWalletOpeningRequestDAO
    {

        private readonly ILogger _logger;

        public WalletOpeningRequestDAO(IDbConnection connection, ILoggerFactory logger) : base(connection)
        {
            _logger = logger.CreateLogger(typeof(WalletOpeningRequestDAO));

        }

        public async Task<long> Add(WalletOpeningRequest entity)
        {

            var query = @"INSERT INTO WalletOpeningRequests
                            (FirstName,
                            MiddleName,
                            LastName,
                            BirthDate,
                            Gender,
                            EmailAddress,
                            PhoneNumber,
                            BiometricTag,
                            Status,
                            nationality,
                            idtype,
                            idnumber,
                            salutation,DateCreated) OUTPUT INSERTED.*
                        VALUES
                            (@FirstName,
                            @MiddleName,
                            @LastName,
                            @Birthdate,
                            @Gender,
                            @EmailAddress,
                            @PhoneNumber,
                            @BiometricTag,
                            @Status,
                            @Nationality,
                            @IdType,
                            @IdNumber,
                            @Salutation,@DateCreated)";

            var transaction = UnitOfWorkSession?.GetTransaction();
            if (transaction == null)
            {
                transaction = _connection.BeginTransaction();
            }
            //  var results= await _connection.QueryFirstOrDefaultAsync<WalletOpeningRequest>(query, entity, transaction);
            var result = await _connection.ExecuteAsync(query, entity, transaction);
            transaction.Commit();
            return result;

        }

        public async Task<WalletOpeningRequest> Find(string walletNumber)
        {

            var query = @"SELECT *
                           FROM WalletOpeningRequests
                           WHERE phoneNumber = @walletNumber";
            var result = await _connection.QueryFirstOrDefaultAsync<WalletOpeningRequest>(query, new { walletNumber });

            return result;




        }

        public async Task Update(WalletOpeningRequest entity)
        {


            var query = @"UPDATE WalletOpeningRequests
                          SET firstName = @FirstName,
                              middleName = @MiddleName,
                              lastName = @LastName,
                              birthDate = @BirthDate,
                              gender = @Gender,
                              emailAddress = @EmailAddress,
                              phoneNumber = @PhoneNumber,
                              biometricTag = @BiometricTag,
                              status = @Status,
                              nationality = @Nationality,
                              idtype = @IdType,
                              idnumber = @IdNumber,
                              photolocation = @PhotoLocation,
                              salutation = @Salutation
                        WHERE id = @Id";
            var transaction = UnitOfWorkSession?.GetTransaction();
            _ = await _connection.ExecuteAsync(query, entity, transaction);



        }
    }
}
