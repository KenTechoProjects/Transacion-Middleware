
using Middleware.Core.Enums;
using Middleware.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Middleware.Core.DTO
{
    public class AccountOpeningRequest
    {
        [JsonIgnore]
        public long Id { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string WalletNumber { get; set; }
        [JsonIgnore]
        public string HouseNumber { get; set; }
        public string CountryCode { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        [JsonIgnore]
        public string StreetName { get; set; }
        public string Address { get; set; }
      
        public string MaritalStatus { get; set; }
        [JsonIgnore]
        public string AccountType { get; set; }
        [JsonIgnore]
        public string Nationality { get; set; }
        public string IdNumber { get; set; }
        public string SelfieImage { get; set; }
        public string IdImage { get; set; }
        
        public string IdType { get; set; }
        [JsonIgnore]
        public string PassportPhoto { get; set; }
        [JsonIgnore]
        public string RequestId { get; set; }
        [JsonIgnore]
        public string BiometricTag { get; set; }

   
        [JsonIgnore]
        public string ReferralCode { get; set; }
        public string ReferralBy { get; set; }
        [JsonIgnore]
        public DateTime DateCreated { get; set; }
     
        public string BranchCode { get; set; }
        //public Document Document { get; set; }
        //public Customer Customer { get; set; }
        public string Signature { get; set; }
        public AccountOpeningStatus AccountOpening { get; set; }
      //  public AccountOpeningRecordStatus AccountOpeningRecordStatus { get; set; }
        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if (string.IsNullOrWhiteSpace(this.FirstName))
            {
                problemSource = "FirstName";
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.LastName))
            {
                problemSource = "LastName";
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.Salutation))
            {
                problemSource = "Salutation";
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.Signature))
            {
                problemSource = "Signature";
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.IdNumber))
            {
                problemSource = "Id Number";
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.Nationality))
            {
                problemSource = "Nationality";
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.IdImage))
            {
                problemSource = "Id Image";
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.Gender))
            {
                problemSource = "Gender";
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.MaritalStatus))
            {
                problemSource = "Marital Status";
                return false;
            }

            if (DateOfBirth == default(DateTime))
            {
                problemSource = "Date Of Birth";
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.Email))
            {
                problemSource = "Email";
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.CountryCode))
            {
                problemSource = "Country Code";
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.WalletNumber))
            {
                problemSource = "Walle tNumber";
                return false;
            }






            return true;
        }

    }

 


    public class DocumentAccountOpening
    {
        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public string Note { get; set; }
        [JsonIgnore]
        public string Path { get; set; }

        [JsonIgnore]
        public string DocumentName { get; set; }

     
        public string IdNumber { get; set; }
        [JsonIgnore]
        public DocumentState State { get; set; }
        [JsonIgnore]
        public DocumentStatus Status { get; set; }
 
        public string Type { get; set; }
        [JsonIgnore]
        public string Reference { get; set; }
        [JsonIgnore]
        public string ServerReference { get; set; }
   
        [JsonIgnore]
        public string PhoneNumber { get; set; }
        [JsonIgnore]
        public string CustomerReference { get; set; }
        
        public string Comment { get; set; }
        [JsonIgnore]
        public long CustomerId { get; set; }
        [JsonIgnore]
        public DateTime StatusDate { get; set; }
        [JsonIgnore]
        public DateTime DateCreated { get; set; }
        [JsonIgnore]
        public DateTime LastUpdatedDate { get; set; }
 
        public DocumentType DocumentType { get; set; }
        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            
            

            if (string.IsNullOrWhiteSpace(this.Type))
            {
                problemSource = "Type";
                return false;
            }
            if (string.IsNullOrWhiteSpace(this.IdNumber))
            {
                problemSource = "Id Number";
                return false;
            }

            //if (string.IsNullOrWhiteSpace(this.WalletNumber))
            //{
            //    problemSource = "Walle tNumber";
            //    return false;
            //}






            return true;
        }
    }

    public class AccountOpeningPayload
    {
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string MobileNumber { get; set; }
        public string Email { get; set; }
        // public DateTime DateOfBirth { get; set; }
        public string DateOfBirth { get; set; }
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
        public string ReferralCode { get; set; }
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
        public string Region { get; set; }
        public string CountryId { get; set; }

    }
}
