using System;
using Middleware.Core.DTO;

namespace Middleware.Service.DTOs
{
    public class CaseInfo
    {
        public string AccountId { get; set; }
        public AccountType AccountType { get; set; }
        public CaseState State { get; set; }
        public DocumentInfo[] Documents { get; set; }
    }
}
