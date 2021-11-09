using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class AccountOpeningRequest
    {
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string HouseNumber { get; set; }
        public string StreetName { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Signature { get; set; }
        public string MaritalStatus { get; set; }
        public string BranchCode { get; set; }
        public string AccountType { get; set; }
        public string Occupation { get; set; }
        public string Nationality { get; set; }
        public string IdNumber { get; set; }
        public string IdType { get; set; }
        public string IdImage { get; set; }
        public string PassportPhoto { get; set; }
        public string CurrenyCode { get; set; }
        public string RequestId { get; set; }
        public string CountryId { get; set; }
    }
}


