using Middleware.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Middleware.Service.DTOs
{
    public class AccountOpeningOnboardingInitiationRequest
    {

        public string Title { get; set; }
        public string Gender { get; set; }
      
        [JsonIgnore]
        public string AccountNumber { get; set; }
        public string FirstName { get; set; }
        public string Salutation { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }

        public string StateCode { get; set; }
        public string MaritalStatus { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        [JsonIgnore]
        public string PostalCode { get; set; }
      
        [JsonIgnore]
        public string Country { get; set; }
        [JsonIgnore]
        public string PassportPhoto { get; set; }
        [JsonIgnore]
        public string Occupation { get; set; }
        [JsonIgnore]
        public string HouseNumber { get; set; }
        [JsonIgnore]
        public string BranchCode { get; set; }
        [JsonIgnore]
        public string AccountType { get; set; }
        [JsonIgnore]
        public string ReferralCode { get; set; }
        public string ReferralBy { get; set; }
        public string Nationality { get; set; }
    
        [JsonIgnore]
        public string Address { get; set; }
      
        public string IdType { get; set; }
        public string IdNumber { get; set; }

      //  [JsonIgnore]
        public string StreetName { get; set; }
        //public string DeviceId { get; set; }  

        //public string DeviceModel { get; set; } 

        public DocumentDTO Selfie { get; set; }
        public DocumentDTO Sgnature { get; set; }
        public  DocumentDTO Identification { get; set; }
        public DocumentDTO Signature { get; set; }
        [DefaultValue(false)]
        public bool HasWallet { get; set; }
        public Device Device { get; set; }
        public bool IsValid(out string problemSource)
        { problemSource = string.Empty;
            if (DateOfBirth==default(DateTime))
            {
                problemSource = "DateOfBirth";
                return false;
            }
            problemSource = string.Empty;
            if (FirstName?.Length < 2 || FirstName?.Length > 50)
            {
                problemSource = "First Name";
                return false;
            }
            if (LastName?.Length < 2 || LastName?.Length > 50)
            {
                problemSource = "Last Name";
                return false;
            }
            if (string.IsNullOrEmpty(PhoneNumber))
            {
                problemSource = "Phone Number";
                return false;
            }
            if (string.IsNullOrEmpty(Email) || !IsEmailPatternMatched(Email))
            {
                problemSource = "Email Address";
                return false;
            }

            if (HasWallet == false)
            {
                if (Selfie.IsValid(out problemSource) == false)
                {
                    return false;
                }
                if (!Identification.IsValid(out problemSource))
                {
                    return false;
                }
                if (!Signature.IsValid(out problemSource))
                {
                    return false;
                }

            }
            else
            {
                if (!Signature.IsValid(out problemSource))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsEmailPatternMatched(string email)
        {
            var pattern = @"^((\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*)\s*[,]{0,1}\s*)+$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }
    }

    public class AccountOpeningRequestForCustomerWithWallet
    {
        [JsonIgnore]
        public long Id { get; set; }
        public string Salutation { get; set; }//
        [JsonIgnore]
        public string FirstName { get; set; }
        [JsonIgnore]
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        [JsonIgnore]
        public string PhoneNumber { get; set; }
        [JsonIgnore]
        public string HouseNumber { get; set; }
    
        public string CountryCode { get; set; }
        [JsonIgnore]
        public string Email { get; set; }
        [JsonIgnore]
        public DateTime DateOfBirth { get; set; }
        [JsonIgnore]
        public string Gender { get; set; }
        //[JsonIgnore]
        public string StreetName { get; set; }
       
        public string Address { get; set; }
      
        public string PostalCode { get; set; }
      
        public string State { get; set; }
     
        public string City { get; set; }
        [JsonIgnore]
        public string Country { get; set; }
   
        public string Occupation { get; set; }

  [JsonIgnore]
        public string Signature { get; set; }

   [JsonIgnore]
        public string IdImage { get; set; }
        public string MaritalStatus { get; set; }
        [JsonIgnore]
        public string AccountType { get; set; }
        [JsonIgnore]
        public string ReferralCode { get; set; }
        [JsonIgnore]
        public string Nationality { get; set; }

        public string IdNumber { get; set; }//      
        [JsonIgnore]
        public string IdType { get; set; }
        [JsonIgnore]
        public string PassportPhoto { get; set; }
        [JsonIgnore]
        public string RequestId { get; set; }
        [JsonIgnore]
        public string BiometricTag { get; set; }
  

        //[JsonIgnore]
        //public string ReferralCode { get; set; }
        public string ReferralBy { get; set; }
        [JsonIgnore]
        public DateTime DateCreated { get; set; }
      
        public string BranchCode { get; set; }
        public DocumentDTO SignatureDto { get; set; }
        public DocumentDTO Identification { get; set; }
        public DocumentDTO Selfie { get; set; }
        public Device Device { get; set; }
        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            //if (string.IsNullOrWhiteSpace(this.FirstName))
            //{
            //    problemSource = "FirstName";
            //    return false;
            //}
            //if (string.IsNullOrWhiteSpace(this.LastName))
            //{
            //    problemSource = "LastName";
            //    return false;
            //}

            if (string.IsNullOrWhiteSpace(this.Salutation))
            {
                problemSource = "Salutation";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(this.IdNumber))
            {
                problemSource = "Id Number";
                return false;
            }
            //if (string.IsNullOrWhiteSpace(this.Nationality))
            //{
            //    problemSource = "Nationality";
            //    return false;
            //}
            if (string.IsNullOrWhiteSpace(this.IdImage))
            {
                problemSource = "Id Image";
                return false;
            }

            //if (string.IsNullOrWhiteSpace(this.Gender))
            //{
            //    problemSource = "Gender";
            //    return false;
            //}
            if (string.IsNullOrWhiteSpace(this.MaritalStatus))
            {
                problemSource = "Marital Status";
                return false;
            }

            //if (DateOfBirth == default(DateTime))
            //{
            //    problemSource = "Date Of Birth";
            //    return false;
            //}

            //if (string.IsNullOrWhiteSpace(this.Email))
            //{
            //    problemSource = "Email";
            //    return false;
            //}
            //if (string.IsNullOrWhiteSpace(this.CountryCode))
            //{
            //    problemSource = "Country Code";
            //    return false;
            //}

            if (string.IsNullOrWhiteSpace(this.PhoneNumber))
            {
                problemSource = "Wallet Number";
                return false;
            }

            return true;
        }

    }
}
