using Microsoft.Extensions.Logging;
using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Middleware.Core.DAO;
using System.Data;
using Middleware.Service.Utilities;

namespace Middleware.Service.DAO
{
    public class BenefitiariesDAO : BaseDAO, IBenefitiariesDAO
    {
        readonly ILogger _log;

        readonly IMessageProvider _messageProvider;
        private readonly ICustomerDAO _customerDAO;
        public BenefitiariesDAO(ILoggerFactory logger, IDbConnection connection, IMessageProvider messageProvider, ICustomerDAO customerDAO) : base(connection)
        {
            _log = logger.CreateLogger<BenefitiariesDAO>();
            if (_connection.State == ConnectionState.Closed) { _connection.Open(); }
            _messageProvider = messageProvider;
            _customerDAO = customerDAO;
        }

        public async Task<Dictionary<bool, string>> BeneficiaryExistssAsyc(string walletNumber, string beneficiaryNumber, string countryId, string referenceNumber, PaymentType paymentType)
        {
            var response = new BasicResponse(false);
            var sql = "select * from AirTimeBeneiciary  where  WalletNumber=@WalletNumber and BeneficiaryNumber=@BeneficiaryNumber and CountryId=@CountryId  and PaymentType=@PaymentType";

            var exists = await _connection.QueryFirstOrDefaultAsync<AirTimeBenefitiaryResponse>(sql, new { BeneficiaryNumber = beneficiaryNumber, WalletNumber = walletNumber, CountryId = countryId, PaymentType = paymentType });
            if (exists != null)
            {
                return new Dictionary<bool, string>()
                  {
            { true, $"pour {paymentType.ToString()}:for {paymentType.ToString()}" },

                    };
            }
            sql = "select * from AirTimeBeneiciary  where  ReferenceNumber=@ReferenceNumber;";

            exists = await _connection.QueryFirstOrDefaultAsync<AirTimeBenefitiaryResponse>(sql, new { ReferenceNumber = referenceNumber });
            if (exists != null)
            {
                return new Dictionary<bool, string>()
                  {
            { true, "pour le numéro de référence:for reference number" },

                    };
            }
            return new Dictionary<bool, string>()
                  {
            { false, "" },

                    };
        }

        public async Task<BasicResponse> DeletePaymentBeneficiariesAsyc(long id, string walletNumber,string customerId, string language)
        {
            var response = new BasicResponse(false);
            var sql = "Update AirTimeBeneiciary SET IsDeleted=1, IsActive=0 where Id=@Id and WalletNumber=@WalletNumber and CustomerId=@CustomerId;";

            var deleted = await _connection.ExecuteAsync(sql, new { Id = id, WalletNumber = walletNumber, CustomerId=customerId});
            if (deleted > 0)
            {
                response.IsSuccessful = true;
                response.FaultType = FaultMode.NONE;
            }
            else
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INVALID_INPUT_PARAMETER,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_INPUT_PARAMETER, language)
                };
                response.FaultType = FaultMode.CLIENT_INVALID_ARGUMENT;
                return response;
            }
            return response;
        }

        public async Task<IEnumerable<AirTimeBenefitiaryResponse>> GetPaymentBeneficiariesAsyc(string walletNumber, string countryId)
        {


            var sql = "SELECT * FROM AirTimeBeneiciary where WalletNumber=@WalletNumber and CountryId=@CountryId and isDeleted=0;";

            var responses = await _connection.QueryAsync<AirTimeBenefitiaryResponse>(sql, new { WalletNumber = walletNumber, CountryId = countryId });

            return responses;
        }

        async Task<BasicResponse> IBenefitiariesDAO.SaveAirtimeBeneficiary(AirTimeBenefitiary request, string language)
        {
            var response = new BasicResponse(false);
            var custId = !string.IsNullOrWhiteSpace(request.CustomerId) ? Convert.ToInt64(request.CustomerId) : 0;
            var customerexists = await _customerDAO.Find(custId);

            if (customerexists == null)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.CUSTOMER_NOT_FOUND,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.CUSTOMER_NOT_FOUND, language)
                };
                return response;
            }
            if (customerexists.WalletNumber != request.WalletNumber)
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.INVALID_ACCOUNT_NUMBER,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.INVALID_ACCOUNT_NUMBER, language)
                };
                return response;

            }
            
            var existsDictionary = await BeneficiaryExistssAsyc(request.WalletNumber, request.BeneficiaryNumber, request.CountryId, request.ReferenceNumber, request.PaymentType);
           //Key is Boolean and Value is String
            var exists = existsDictionary.FirstOrDefault();
            var  existsSplit = exists.Value.Split(":");

            if (exists.Key)
            {
                string messageDisplay = existsSplit[1];
                if (language.ToLower() == "fr")
                {
                    messageDisplay = existsSplit[0]; ;
                }

                var errrMessage = $"{ _messageProvider.GetMessage(ResponseCodes.BENEFICIARY_ALREADY_EXISTS, language)}";
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.BENEFICIARY_ALREADY_EXISTS,
                    ResponseDescription = errrMessage.Contains("An Error") ? errrMessage : $"{errrMessage} {messageDisplay}"
                };
                return response;
            }


            string sql = "INSERT INTO AirTimeBeneiciary (ReferenceNumber,CustomerId,BillerCode,Alias,PaymentType,CountryId,WalletNumber,IsDeleted,IsActive,BeneficiaryName,BeneficiaryNumber,BillerName)" +
              " Values (@ReferenceNumber,@CustomerId,@BillerCode,@Alias,@PaymentType,@CountryId,@WalletNumber,@IsDeleted,@IsActive,@BeneficiaryName,@BeneficiaryNumber,@BillerName)";
            var save = await _connection.ExecuteAsync(sql, request);
            if (save > 0)
            {
                response.IsSuccessful = true;
                response.FaultType = FaultMode.NONE;
            }
            else
            {
                response.Error = new ErrorResponse
                {
                    ResponseCode = ResponseCodes.REQUEST_NOT_COMPLETED,
                    ResponseDescription = _messageProvider.GetMessage(ResponseCodes.REQUEST_NOT_COMPLETED, language)
                };
            }
            return response;


        }
    }
}
