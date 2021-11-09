using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.Model
{
    public class Tracker
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string TransactionReference { get; set; }
        public DateTime TransactionTime { get; set; }
    }
}
