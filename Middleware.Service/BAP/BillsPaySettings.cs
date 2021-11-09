using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.BAP
{
    public class BillsPaySettings
    {
        public string BaseAddress { get; set; }
        public string AppKey { get; set; }
        public string TransactionPath { get; set; }
        public decimal Surcharge { get; set; }
        public decimal Vat { get; set; }
        public BillsPayEndpoints Endpoints { get; set; }
        public int RequestTimeout { get; set; }
        public string WildcardEndpoint { get; set; }
        //public decimal AirtimeSingleLimit { get; set; }
        //public decimal BillSingleLimit { get; set; }
        //public decimal AirtimeDailyLimit { get; set; }
        //public decimal BillDailyLimit { get; set; }
    }

    public class BillsPayEndpoints
    {
        public string Billers { get; set; }
        public string Telcos { get; set; }
        public string Products { get; set; }
        public string CustomerLookUp { get; set; }
    }
}
