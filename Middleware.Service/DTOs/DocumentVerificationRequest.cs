using System;
using Middleware.Core.DTO;

namespace Middleware.Service.DTOs
{
    public class DocumentVerificationRequest
    {
        public string AccountId { get; set; }
        public AccountType AccountType { get; set; }
        public DocumentData[] Documents { get; set; }
    }
}
