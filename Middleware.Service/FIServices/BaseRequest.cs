using System;

namespace Middleware.Service.FIServices
{
    public class BaseRequest
    {
        public string CountryId { get; set; }    
        public string RequestId { get; set; }   

        public BaseRequest(string countryId)
        {
            CountryId = countryId;
        }
    }
}