using Dapper;
using Microsoft.Extensions.Logging;
using Middleware.Core.DAO;
using Middleware.Core.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.DAO
{
    public class AccountOpeningRequestDAO : BaseDAO, IAccountOpeningRequestDAO

    {

        private readonly ILogger _logger;

        public AccountOpeningRequestDAO(IDbConnection connection, ILoggerFactory logger) : base(connection)
        {
            _logger = logger.CreateLogger(typeof(AccountOpeningRequestDAO ));

        }

        public async Task<Model.  AccountOpeningRequest> Add(Model. AccountOpeningRequest entity)
        {

            var query = @"INSERT INTO AccountOpeningRequests
                            (FirstName,
                            MiddleName,
                            LastName,
                            BirthDate,
                            Gender,
                            EmailAddress,
                            PhoneNumber,
                            BiometricTag,
                            Status,
                            Nationality,
                            Ndtype,
                            Ndnumber,
                            Nalutation,
DateCreated, Country, State,City, StreetName, HouseNumber, Address, PostalCode,MaritalStatus, AccountType,BranchCode,Occupation,Signature,IdImage,PasswortPhoto,IdType,AccountOpeningRecordStatus,AccountOpeningStatus
) OUTPUT INSERTED.*
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
                            @Salutation,@DateCreated,@Country,@State,@City,@StreetName,@HouseNumber,@Address,@PostalCode,@MaritalStatus,@AccountType,@BranchCode,@Occupation,@Signature,@IdImage,@PassportPhoto,@IdType,@AccountOpeningRecordStatus,@AccountOpeningStatus)";

            var transaction = UnitOfWorkSession?.GetTransaction();
            if (transaction == null)
            {
                transaction = _connection.BeginTransaction();
            }
            //  var results= await _connection.QueryFirstOrDefaultAsync<WalletOpeningRequest>(query, entity, transaction);
            var result = await _connection.QueryFirstOrDefaultAsync<Model. AccountOpeningRequest>(query, entity, transaction);
            transaction.Commit();
            return result;

        }

        public async Task<Model.AccountOpeningRequest> Find(string walletNumber)
        {

            var query = @"SELECT *
                           FROM WalletOpeningRequests
                           WHERE phoneNumber = @walletNumber";
            var result = await _connection.QueryFirstOrDefaultAsync<Model.AccountOpeningRequest>(query, new { walletNumber });

            return result;




        }

        public async Task<long> Update(Model.AccountOpeningRequest entity)
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
                              salutation = @Salutation,
, Country=@Country, State=@State,City=@City,StreetName=@StreetName, HouseNumber=@HouseNumber, Address=@Address, PostalCode=@PostalCode,=@MaritalStatus, AccountType=@AccountType,BrachCode=@BrachCode,
Occupation=@Occupation,Signature=@Signature,=@IdImage,PasswortPhoto=@PassportPhoto,IdType=@IdType,AccountOpeningRecordStatus=@AccountOpeningRecordStatus,AccountOpeningStatus=@AccountOpeningStatus  
                        WHERE id = @Id";
            var transaction = UnitOfWorkSession?.GetTransaction();
          //  _ = await _connection.ExecuteAsync(query, entity, transaction);
           var result= await _connection.ExecuteAsync(query, entity, transaction);

            return result;

        }
    }
}
