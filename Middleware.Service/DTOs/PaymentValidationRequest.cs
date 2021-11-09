using System;
using System.Collections.Generic;
namespace Middleware.Service.DTOs
{
    public class PaymentValidationRequest
    {
        public string BillerCode { get; set; }
        public string ProductCode { get; set; }
        public string CustomerReference { get; set; }
        public string ValidationPath { get; set; }
        public IDictionary<string, string> CustomerDetails { get; set; }
    }
}
