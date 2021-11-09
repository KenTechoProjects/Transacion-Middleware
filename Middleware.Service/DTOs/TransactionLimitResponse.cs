using Middleware.Service.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.DTOs
{
    public class TransactionLimitResponse
    {
        [DefaultValue(false)]
        public bool IsLimitExceeded { get; set; } 
        public   string LimitType { get; set; }
        [DefaultValue(false)]
        public bool NotFound { get; set; }
        public Limit Limit { get; set; }

    }


}
