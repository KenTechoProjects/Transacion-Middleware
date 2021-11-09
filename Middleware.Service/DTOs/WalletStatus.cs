using Middleware.Core.DTO;
using System;
namespace Middleware.Service.DTOs
{
    public class WalletStatus
    {
        public string PhoneNumber { get; set; }
        public WalletOpeningStatus Status { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string EmailAddress { get; set; }
        public string BiometricTag { get; set; }
        public string Nationality { get; set; }
        public string IdNumber { get; set; }
        public string Salutation { get; set; }
       
        public IdentificationType IdType { get; set; }  

    }
}
