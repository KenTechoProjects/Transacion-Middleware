using System;

namespace Middleware.Service.DTOs
{
    public class BeneficiaryRemovalRequest
    {
        public long BeneficiaryID { get; set; }
        public string Pin { get; set; }
    }    
}