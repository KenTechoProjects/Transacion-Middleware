using Middleware.Core.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class OnboardingInitiationResponse
    {
        public string AccountNumber { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CifId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public AccountOpeningStatus AccountOpeningStatus { get; set; }
        public AccountOpeningRecordStatus  AccountOpeningRecordStatus { get; set; }
    }


}
