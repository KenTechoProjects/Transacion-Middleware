using System;
using System.Collections.Generic;
using System.Text;
using Middleware.Core.Model;

namespace Middleware.Service.DTOs
{
    public class PaymentResponse
    {
        public BeneficiaryStatus BeneficiaryStatus { get; set; }
        public string Reference { get; set; }
        public TransactionStatus Status { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }

    }
}
