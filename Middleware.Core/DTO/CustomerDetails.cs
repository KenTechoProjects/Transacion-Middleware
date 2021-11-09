using Middleware.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Core.DTO
{
    public class CustomerDetails
    {
        public string BankId { get; set; }
 public string FirstName { get; set; }
 public string LastName { get; set; }
 public string MiddleName { get; set; }
 public bool IsActive { get; set; }
 public DateTime? LastLogin { get; set; }
 public DateTime? DateCreated { get; set; }
 public string EmailAddress { get; set; }
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
 public IEnumerable<Transaction> Transactions { get; set; }
 public IEnumerable<UserActivity> UserActivities { get; set; }
    }
}
