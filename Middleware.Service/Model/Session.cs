using System;
using Middleware.Core.Model;

namespace Middleware.Service.Model
{
    public class Session
    {
        public Customer Customer { get; set; }
        public string BankId { get; set; }
        public string WalletNumber { get; set; }
        public string UserName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? LastActiveTime { get; set; }
        public string Token { get; set; }
        public long ID { get; set; }
        public long Customer_Id { get; set; }
    }
}
