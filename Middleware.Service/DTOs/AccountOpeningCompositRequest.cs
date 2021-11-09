using Middleware.Core.DTO;
using Middleware.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Middleware.Service.DTOs
{
    public class AccountOpeningCompositRequest
    {
        public AccountOpeningOnboardingInitiationRequest AccountOpeningRequest { get; set; }
      //  public AccountOpeningRequest AccountOpeningRequest { get; set; }
       // public DocumentAccountOpening Document { get; set; }
        public DocumentDTO Document { get; set; }
        public DocumentDTO DocumSignatureent { get; set; }
        public DocumentDTO IdentificationDTO { get; set; }
        public DocumentDTO Selfie { get; set; }
         

        [JsonIgnore]
        public AccountType AccountType { get; set; }
        public string CountryCode { get; set; }
        [JsonIgnore]
        public Customer Customer { get; set; }
        //public bool IsValid(out string problem)
        //{
        //    if( Document.is)
        //}
    }
}
