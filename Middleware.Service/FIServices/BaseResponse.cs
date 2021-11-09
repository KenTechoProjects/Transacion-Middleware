using System;

namespace Middleware.Service.FIServices
{
    public class BaseResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }

        public bool IsSuccessful()
        {
            return ResponseCode == "00";
        }
    }
}