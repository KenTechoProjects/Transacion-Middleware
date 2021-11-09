using Middleware.Core.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class BuyAirtimeRequest
    {
        public int BillerId { get; set; }
        public string TelcoCode { get; set; }
        public string PhoneNumber { get; set; }
        public decimal Amount { get; set; }
        public string SourceAccountId { get; set; }
        public AccountType SourceAccountType { get; set; }
        public string Pin { get; set; }
    }
}
