using Middleware.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.Model
{
    public class Limit
    {
        public int ID { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal SingleLimit { get; set; }
        public decimal DailyLimit { get; set; }
    }
}
