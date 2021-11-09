using System;
using Middleware.Core.DTO;

namespace Middleware.Service.DTOs
{
    public class BaseAirtimePurchaseRequest
    {
        public string TelcoCode { get; set; }
        public string PhoneNumber { get; set; }
        public decimal Amount { get; set; }
        public string SourceAccountId { get; set; }
        public AccountType SourceAccountType { get; set; }
    }
}
