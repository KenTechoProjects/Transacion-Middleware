using Middleware.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class PaymentBeneficiary
    {
        public string Reference { get; set; }
        public string BillerCode { get; set; }
        public string BillerName { get; set; }
        public string ReferenceNumber { get; set; }
        public string CustomerName { get; set; }
        public string Alias { get; set; }
    
        public PaymentType PaymentType { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if (string.IsNullOrEmpty(ReferenceNumber))
            {
                problemSource = "Reference";
                return false;
            }
            if (ReferenceNumber.Any(c => char.IsDigit(c) == false))
            {
                problemSource = "Reference";
                return false;
            }
            return true;
        }
    }


}
