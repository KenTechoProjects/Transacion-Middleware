using System;
namespace Middleware.Service.DTOs
{
    public class WalletInitialisationRequest
    {
        public string Salutation { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string DeviceId { get; set; }
        public string ReferredBy { get; set; }
        public bool ByPassReferral { get; set; }
    }
}
