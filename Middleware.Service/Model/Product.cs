using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.Model
{
    public class Product
    {
        public long Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public bool IsFixedAmount { get; set; }
        public decimal Price { get; set; }
        public long Biller_Id { get; set; }
        public string ReferenceName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateCreated { get; set; }
        public bool IsUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string GatewayMetadata { get; set; }
        public string AdditionalParameters { get; set; }
        public bool ValidationSupported { get; set; }
        public Biller Biller { get; set; }

    }
}
