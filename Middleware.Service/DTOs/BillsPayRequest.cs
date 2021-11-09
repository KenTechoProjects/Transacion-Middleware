using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class WalletBillsPayRequest
    {
        public PaymentData data { get; set; }
    }


    public class AccountBillsPayRequest
    {
        public PaymentData data { get; set; }
    }



    public class PaymentData
    {
        public int biller_id { get; set; }
        public string slug { get; set; }
        public string wallet_id { get; set; }
        public string account_number { get; set; }
        public string transaction_reference { get; set; }
        public string customer_id { get; set; }
        public double amount { get; set; }
        public string payment_method_code { get; set; }
        public string type { get; set; }
        public string customer_email { get; set; }
        public string customer_phone { get; set; }
    }
}
