using System;
using System.Text.Json.Serialization;
using Middleware.Core.DTO;
using Middleware.Core.Enums;
using Middleware.Service.DTOs;

namespace Middleware.Service.Model
{
    public class WalletOpeningRequest
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string BiometricTag { get; set; }
        public WalletOpeningStatus Status { get; set; }
        public string Nationality { get; set; }
        public string IdNumber { get; set; }
        public IdentificationType IdType { get; set; }
        public string PhotoLocation { get; set; }
        public string Salutation { get; set; }
        public DateTime DateCreated { get; set; }
    }

    //public class AccountOpeningRequest
    //{
    //    public long Id { get; set; }
    //    public string FirstName { get; set; }
    //    public string MiddleName { get; set; }
    //    public string LastName { get; set; }
    //    public DateTime BirthDate { get; set; }
    //    public string Gender { get; set; }
    //    public string EmailAddress { get; set; }
    //    public string PhoneNumber { get; set; }
    //    public string BiometricTag { get; set; }
    //    public AccountOpeningStatus  AccountOpening { get; set; }
    //    public AccountOpeningRecordStatus  AccountOpeningRecordStatus { get; set; }
    //    public string Nationality { get; set; }
    //    public string IdNumber { get; set; }
    //    public IdentificationType IdType { get; set; }
    //    public string PhotoLocation { get; set; }
    //    public string Salutation { get; set; }
    //    public DateTime DateCreated { get; set; }
    //}

    public class AccountOpeningRequest
    {
      
        public long Id { get; set; }
        public string Salutation { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string WalletNumber { get; set; }
       
        public string HouseNumber { get; set; }
        public string CountryCode { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
   
        public string StreetName { get; set; }
        public string Address { get; set; }
      
        public string PostalCode { get; set; }
        public string State { get; set; }
        public string City { get; set; }
     
        public string Country { get; set; }
     
        public string Occupation { get; set; }
        public string MaritalStatus { get; set; }
      
        public string AccountType { get; set; }
     
        public string Nationality { get; set; }
        public string IdNumber { get; set; }
        public string SelfieImage { get; set; }
        public string IdImage { get; set; }

        public string IdType { get; set; }
     
        public string PassportPhoto { get; set; }
  
        public string RequestId { get; set; }
     
        public string BiometricTag { get; set; }


   
        public string ReferralCode { get; set; }
        public string ReferralBy { get; set; }
      
        public DateTime DateCreated { get; set; }

        public string BranchCode { get; set; }
        //public Document Document { get; set; }
        //public Customer Customer { get; set; }
        public string Signature { get; set; }
        public AccountOpeningStatus AccountOpeningStatus { get; set; }
        public AccountOpeningRecordStatus AccountOpeningRecordStatus { get; set; }
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

}
