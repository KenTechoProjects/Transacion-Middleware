using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.Model
{
    public class Biller
    {
        public long Id { get; set; }
        public string BillerCode { get; set; }
        public string BillerName { get; set; }
        public BillerType BillerType { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateCreated { get; set; }
        public bool IsUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
   
    }

    public enum BillerType
    {
        TELCO = 1,
        BILL
    }
}
