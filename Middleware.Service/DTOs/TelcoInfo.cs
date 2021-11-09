using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class TelcoInfo
    {
        public int TelcoId { get; set; }
        public string TelcoName { get; set; }
        public string TelcoCode { get; set; }
    }
    



    //public class ServiceGetTelcoResponse
    //{
    //    public bool status { get; set; }
    //    public string message { get; set; }
    //    public TelcoDatum[] data { get; set; }
    //    public object validation { get; set; }
    //    public string status_code { get; set; }
    //}
    
    //public class TelcoDatum
    //{
    //    public string type { get; set; }
    //    public string url { get; set; }
    //    public string slug { get; set; }
    //    public string name { get; set; }
    //    public string description { get; set; }
    //    public Currency currency { get; set; }
    //    public object[] surcharge { get; set; }
    //    public float amount { get; set; }
    //    public string biller_name { get; set; }
    //    public int biller_id { get; set; }
    //    public string biller_logo { get; set; }
    //    public object subscription_enabled { get; set; }
    //    public string customer_id_label { get; set; }
    //    public string validation_type { get; set; }
    //    public int cycle_days { get; set; }
    //    public object amount_type { get; set; }
    //    public bool pay_again { get; set; }
    //    public float vat_rate { get; set; }
    //}


}
