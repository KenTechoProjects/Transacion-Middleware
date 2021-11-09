using System;
using System.Collections.Generic;
using Middleware.Core.DTO;

namespace Middleware.Core.Model
{
    public class Case
    {
        public long Id { get; set; }
        public IEnumerable<Document> Documents { get; set; }
        public Customer Customer { get; set; }
        public string RequestReference { get; set; }
        public string ServerReference { get; set; }
        public string Comments { get; set; }
        public CaseState State { get; set; }
        public long Customer_Id { get; set; }
        public DateTime DateCreated { get; set; }
        public string AccountId { get; set; }
        public DateTime DateOfBirth { get; set; }
        public AccountType AccountType { get; set; }
        public Case()
        {
            DateCreated = DateTime.UtcNow;
        }
    }

}
