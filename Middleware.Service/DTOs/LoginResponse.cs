using Middleware.Core.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Middleware.Service.DTOs
{
    public class LoginResponse
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AuthenticationToken { get; set; }
        public DateTime? LastLogin { get; set; }
        public string LocalBankCode { get; set; }
        public string LocalWalletCode { get; set; }
   
        public string PhotoUrl { get; set; }
        public bool HasUploadedKYC { get; set; }
        public string ReferralCode { get;set; }
        public bool IsApproved{ get; set; }
        public bool HasImpersonated{ get; set; }

    }
   
}
