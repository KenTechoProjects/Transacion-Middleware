using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class ProductInfo
    {
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public decimal Surcharge { get; set; }
        public decimal Vat { get; set; }
        public bool IsFixedAmount { get; set; }
        public decimal? Price { get; set; }
        public bool ValidationSupported { get; set; }
        public string ReferenceName { get; set; }
        public IDictionary<string,string> RequestParams { get; set; }
    } 
}

