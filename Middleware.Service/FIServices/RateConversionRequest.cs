using System;
namespace Middleware.Service.FIServices
{
    public class RateConversionRequest : BaseRequest
    {
        public string SourceCurrency { get; set; }
        public string DestinationCurrency { get; set; }

        public RateConversionRequest(string countryId, string sourceCurrency, string targetCurrency) :
            base(countryId)
        {
            SourceCurrency = sourceCurrency;
            DestinationCurrency = targetCurrency;
        }
    }
}
