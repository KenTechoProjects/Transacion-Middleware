using Middleware.Core.Model;
using Middleware.Service.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class CustomerDetail
    {
        public string AccountNumber { get; set; }
        public string CustomerId { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool HasWallet { get; set; }
        public string BankId { get; set; }
        public bool HasAccount { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public OnboardingStatus Status { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public IEnumerable<TransactionsDTO> Transactions { get; set; }
        public IEnumerable<UserActivity> UserActivities { get; set; }
       // public IEnumerable<Device> Devices { get; set; }
    }
}
