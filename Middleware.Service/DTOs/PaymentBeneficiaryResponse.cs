using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class PaymentBeneficiaryResponse
    {
        public string Reference { get; set; }
        public string BillerCode { get; set; }
        public string BillerName { get; set; }
        public string ReferenceNumber { get; set; }
        public string CustomerName { get; set; }
        public string Alias { get; set; }
        public string PaymentType { get; set; }
    }
}
