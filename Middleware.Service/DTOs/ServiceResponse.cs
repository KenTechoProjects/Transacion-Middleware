using System;
namespace Middleware.Service.DTOs
{
    public class ServiceResponse<T> : BasicResponse
    {

        private T _payload;

        public ServiceResponse() : base(false)
        {

        }
        public ServiceResponse(bool isSuccessful) : base(isSuccessful)
        {

        }

        public T GetPayload()
        {
            return _payload;
        }
        public void SetPayload(T payload)
        {
            _payload = payload;
        }


    }
    public class ServiceResponse : BasicResponse
    {
        public ServiceResponse() : base(false)
        {

        }
        public ServiceResponse(bool isSuccessful) : base(isSuccessful)
        {

        }
        public dynamic Data { get; set; }
    }
    public class ServiceSuccessResponse 
    {

        public bool IsSuccessfull { get; set; }               
        public object PayLoad { get; set; }
    }

    public class ServiceErrorResponse:BasicResponse
    {
       
 

        public ServiceErrorResponse() : base(false)
        {

        }
        public ServiceErrorResponse(bool isSuccessful) : base(isSuccessful)
        {

        }
      
    }



    public class ServiceResponseT<T> : BasicResponseT
    {

        private T _payload;

        public ServiceResponseT() : base(false)
        {

        }
        public ServiceResponseT(bool isSuccessful) : base(isSuccessful)
        {

        }

        public T GetPayload()
        {
            return _payload;
        }
        public void SetPayload(T payload)
        {
            _payload = payload;
        }


    }
}
