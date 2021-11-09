using System;
using System.Collections.Generic;
using Middleware.Core.DTO;

namespace Middleware.Service.DTOs
{
    public class BasePaymentRequest
    {
        public string BillerCode { get; set; }
        public string ProductCode { get; set; }
        public decimal Amount { get; set; }
        public string SourceAccountId { get; set; }
        public string CustomerReference { get; set; }
        public AccountType SourceAccountType { get; set; }
        public IDictionary<string, string> PaymentParameters { get; set; }
    }

    public class AirTimeBeneficiaryRequest
    {
        public string reference { get; set; }  
        public string customerId { get; set; }
        public string billerCode { get; set; }  
        public string billerName { get; set; }
        public string referenceNumber { get; set; }
        public string customerName { get; set; }
        public string alias { get; set; }
        public string countryId { get; set; }
      
    }
}
