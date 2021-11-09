namespace Middleware.Service.DTOs
{
    public class ResponseCodes
    {
        public const string SUCCESS = "FBN000";
        public const string INVALID_LOGIN_DETAILS = "FBN001";
        public const string INVALID_TRANSACTION_PIN = "FBN002";
        public const string PROFILE_DEACTIVATED = "FBN003";
        public const string INVALID_SESSION = "FBN004";
        public const string MISSING_AUTH_TOKEN = "FBN005";
        public const string DEVICE_MISMATCH = "FBN006";
        public const string DEVICE_DISABLED = "FBN007";
        public const string INPUT_VALIDATION_FAILURE = "FBN008";
        public const string INCORRECT_SECURITY_ANSWER = "FBN009";
        public const string GENERAL_ERROR = "FBN010";
        public const string INVALID_INPUT_PARAMETER = "FBN011";
        public const string CUSTOMER_NOT_FOUND = "FBN012";
        public const string ACCOUNT_CUSTOMER_MISMATCH = "FBN013";
        public const string CUSTOMER_ALREADY_ONBOARDED = "FBN014";
        public const string ACCOUNT_MISMATCH = "FBN015";
        public const string LIMIT_EXCEEDED = "FBN016";
        public const string REQUEST_NOT_FOUND = "FBN017";
        public const string INVALID_WALLET_REGISTRATION_STATE = "FBN018";
        public const string WALLET_ALREADY_OPENED = "FBN019";
        public const string BILLERS_NOT_FOUND = "FBN020";
        public const string PRODUCTS_NOT_FOUND = "FBN021";
        public const string PHONENUMBER_ALREADY_EXISTS = "FBN022";
       
        public const string REQUEST_MISMATCH = "FBN023";
        public const string REQUEST_NOT_COMPLETED = "FBN024";
        public const string DOCUMENT_NOT_UPDATABLE = "FBN025";
        public const string TRANSACTION_REFERENCE_MISSING = "FBN026";
        public const string TRANSACTION_REFERENCE_ALREADY_EXISTS = "FBN027";
        public const string DEVICE_NOT_AVAILABLE= "FBN028";
        public const string NEW_DEVICE_DETECTED = "FBN029";
        public const string DEVICE_LIMIT_REACHED = "FBN030";
        public const string BENEFICIARY_SAVED_SUCCESSFULLY = "FBN031";
        public const string UNABLE_TO_SAVE_BENEFICIARY = "FBN032";
        public const string CODE_VALIDATION_ERROR = "FBN033";
        public const string INVALID_ENTITY_STATE = "FBN034";
        public const string DEVICE_NOT_FOUND = "FBN035";
        public const string TRANSACTION_FAILED = "FBN036";
        public const string IMAGE_UNPROCCESSABLE = "FBN037";
        public const string INVALID_ACCOUNT_LENGTH = "FBN038";
        public const string INVALID_ACCOUNT_NUMBER = "FBN039";
        public const string PAYMENT_SINGLE_LIMIT_EXCEEDED = "FBN041";
        public const string PAYMENT_DAILY_LIMIT_EXCEEDED = "FBN042";
        public const string INSUFFICIENT_RESPONSE_CODE = "IW09";
        public const string PRODUCT_NOT_FOUND = "FBN040";
        public const string UPPERLINK_VALIDATION_ERROR = "UPP001";
        public const string INVALID_DOCUMENT_TYPE = "FBN046";
        public const string PROFILE_NOT_FOUND = "FBN047";
        public const string CUSTOMER_NOT_REGISTERED = "FBN048";
        public const string CASE_NOT_FOUND = "FBN049";
        public const string IMAGE_PROFILE_NOT_CREATED = "FBN050";
        public const string PHOTO_NOT_YET_PROVIDED = "FBN051";
        public const string PLEASE_TRY_RESUMING_ONBORADING = "FBN052";
        public const string BENEFICIARY_ALREADY_EXISTS = "FBN053"; 
        public const string RESET_PASSWORD = "FBN054"; 
        public const string AUTHTENTIATION_FAILURE = "FBN055";

        public const string MIDDLEWARE_ERROR = "FBN056";
        public const string MISSING_APYKEY_TOKEN = "FBN057";
        public const string MISSING_COUNTRY_CODE = "FBN058";
        public const string WALLET_OPENING_FAILURE = "FBN059";
        public const string DEVICE_REGISTRATION_FAILURE = "FBN060";
        public const string CUSTOMER_REGISTRATION_FAILURE = "FBN061";
        public const string CASE_REGISTRATION_FAILURE = "FBN062";
        public const string DOCUMENT_FILING_FAILURE = "FBN063";
        public const string ACCOUNT_OPENING_FAILURE = "FBN064";
        public const string UPDATE_FAILURE = "FBN065";
        public const string WALLET_CREATED_BUT_NOT_REGISTERED = "FBN066";
        public const string DEVICE_AREADY_EXISTS = "FBN067"; 
        public const string SMS_FAILURE = "FBN068"; 
        public const string REFERRALCODE_DOES_NOT_EXISTS = "FBN069"; 
        public const string INVALID_TARGET_TYPE = "FBN070";
        public const string TARGET_ALREADY_EXIST = "FBN071";
        public const string TRANSACTION_LIMIT_NOT_YET_SET = "FBN072";
        public const string SQL_DATABASE_NETWORK_ERROR = "FBN073";
        public const string THIRD_PARTY_NETWORK_ERROR = "FBN074";
        public const string USE_THIS_DEVICE = "FBN075";
        public const string INVALID_ENCRYPTION_DATA = "FBN091";
        public const string UNABLE_TO_DECRYPT = "FBN092";
        public const string DEVICE_ALREADY_ATACHED = "FBN093";
 

        public const string LIMIT_EXCEED_FOR_OTP = "FBN096";
        public const string LIMIT_EXCEED_FOR_TOKEN = "FBN097";
        public const string PHONE_NUMBER_MISMATCH = "FBN098";
    
        public const string ACCOUNT_OPENING_INITILIZATON_FAILURE = "FBN100";
        public const string ACCOUNT_OPENING_SELFIE_FAILURE = "FBN101";
        public const string ACCOUNT_OPENING_SIGNATURE_FAILURE = "FBN102";
        public const string ACCOUNT_OPENING_ID_FAILURE = "FBN103";
        public const string OTP_SUCCESS_MESSAGE = "FBN105";
         public const string OTP_FAILURE_MESSAGE = "FBN106";






 








        //Please try resuming onboarding to complete your onboarding process

    }
}