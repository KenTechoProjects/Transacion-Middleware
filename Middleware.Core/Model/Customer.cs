using Middleware.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Middleware.Core.Model
{
    public class Customer
    {
        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public string BankId { get; set; }
        [JsonIgnore]
        public string CifId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public bool IsActive { get; set; }
        [JsonIgnore]
        public DateTime? LastLogin { get; set; }
        public DateTime? DateCreated { get; set; }
        public string EmailAddress { get; set; }
        [JsonIgnore]
        public string AccountNumber { get; set; }
        public string WalletNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string ReferralCode { get; set; }
        public string ReferredBy { get; set; }
        public bool HasWallet { get; set; }
        public bool HasAccount { get; set; }
        public OnboardingStatus OnboardingStatus { get; set; }
        public OnboardingType OnboardingType { get; set; }
        //From Ghana Replicate

        //public string ReferralCode { get; set; }
        //public string CustomerReferralCode { get; set; }
        //public string CustomerReference { get; set; }
        //public bool IsExistingCustomer { get; set; }
        //public int InvalidLoginCount { get; set; }

    }

    public enum OnboardingStatus
    {
        STARTED = 1,
        COMPLETED
    }
}
