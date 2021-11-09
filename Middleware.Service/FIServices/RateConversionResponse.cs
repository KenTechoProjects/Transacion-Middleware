using System;
namespace Middleware.Service.FIServices
{
    public class RateConversionResponse : BaseResponse
    {
        public decimal CurrentRate { get; set; }
    }
}
