using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class BillsPayResponse
    {
        public bool Status { get; set; }
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public ResponseMessage ResponseMessage { get; set; }
        public BeneficiaryStatus BeneficiaryStatus { get; set; }
    }


    public class ResponseMessage
    {
        public int Status { get; set; }
        public string StatusMessage { get; set; }
        public string ReportedStatusMessage { get; set; }
        public double AmountPaid { get; set; }
        public string TransactionReference { get; set; }
        public string PaymentChannelReference { get; set; }
    }
}
