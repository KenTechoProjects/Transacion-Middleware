using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Middleware.Core.Enums;
using Middleware.Service.BAP;

namespace Middleware.Service.DTOs
{
    public class BasicResponse
    {
        [DefaultValue(true)]
        public bool IsSuccessful { get; set; }
        [DefaultValue(null)]
        public ErrorResponse Error { get; set; }

        public BasicResponse()
        {
            IsSuccessful = false;
        }
        public BasicResponse(bool isSuccessful)
        {
            IsSuccessful = isSuccessful;
        }
      [DefaultValue(FaultMode.NONE)]
        public FaultMode FaultType { get; set; }


    }

    public class ReferralCodeResponse
    {
        [DefaultValue(true)]
        public bool Exists { get; set; }
        public string Message { get; set; }
        
        public ReferralCodeResponse(bool exists=false)
        {
            Exists = exists;
        }
         
    }
    public class ErrorResponse
    {

        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        public AccountOpeningStatus AccountOpeningStatus { get; set; }



        public static T Create<T>(FaultMode fault, string errorCode, string errorMessage) where T : BasicResponse, new()
        {
            var response = new T
            {
                IsSuccessful = false,
                FaultType = fault,
                Error = new ErrorResponse
                {
                    ResponseCode = errorCode,
                    ResponseDescription = errorMessage
                }
            };
            return response;
        }

        public override string ToString()
        {
            return $"{ResponseCode} - {ResponseDescription}";
        }

    }


    public class ErrorResponseT
    {

        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
        [DefaultValue(true)]
        public bool IsLimitExceeded { get; set; }
        [DefaultValue(true)]
        public bool IsTransactionNeedOTP { get; set; }


        public static T Create<T>(FaultMode fault, string errorCode, string errorMessage) where T : BasicResponseT, new()
        {
            var response = new T
            {
                IsSuccessful = false,
                FaultType = fault,
                Error = new ErrorResponseT
                {
                    ResponseCode = errorCode,
                    ResponseDescription = errorMessage
                }
            };
            return response;
        }

        public override string ToString()
        {
            return $"{ResponseCode} - {ResponseDescription}";
        }

    }

    public class BasicResponseT
    {
        [DefaultValue(true)]
        public bool IsSuccessful { get; set; }
        [DefaultValue(null)]
        public ErrorResponseT Error { get; set; }

        public BasicResponseT()
        {
            IsSuccessful = false;
        }
        public BasicResponseT(bool isSuccessful)
        {
            IsSuccessful = isSuccessful;
        }
        [DefaultValue(FaultMode.NONE)]
        public FaultMode FaultType { get; set; }


    }






    //Still Thinking
    // public class TransactionLimitMessage
    // {
    //public bool IsLimitExceeded { get; set; }

    // }

    public class BAPResponse : BasicResponse
    {
        public BAPStatus Status;
    }

    public enum FaultMode
    {
        CLIENT_INVALID_ARGUMENT,
        SERVER,
        TIMEOUT,
        REQUESTED_ENTITY_NOT_FOUND,
        INVALID_OBJECT_STATE,
        UNAUTHORIZED,
        GATEWAY_ERROR,
        LIMIT_EXCEEDED,
        EXISTS,
        PHOTO_NOT_YET_PROVIDED,
        INVALID_RESPONSE,
        RESET_PASSWORD,
        DEACTIVATED,
        AUTHTENTIATION_FAILURE,
        THRID_PARTY_CONNECTION_FAILURE,
        LIMIT_EXCEED_FOR_TOKEN,
        LIMIT_EXCEED_FOR_OTP,
        NONE
    }
}
