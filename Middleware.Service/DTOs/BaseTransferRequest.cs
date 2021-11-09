using Middleware.Core.DTO;
using System;
using System.ComponentModel;
using System.Linq;

namespace Middleware.Service.DTOs
{
    public class BaseTransferRequest
    {
        public string SourceAccountId { get; set; }
        public string SourceAccountName { get; set; }
        public string DestinationAccountId { get; set; }
        public string DestinationAccountName { get; set; }
        public string DestinationInstitutionId { get; set; }
        public string Narration { get; set; }
        public decimal Amount { get; set; }
        [DefaultValue(false)]
        public bool IsLimitExceeded { get; set; }

        // public string DestinationBranchCode { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if (string.IsNullOrEmpty(DestinationAccountId))
            {
                problemSource = "destination";
                return false;
            }
            //if (DestinationAccountId.Any(c => char.IsDigit(c) == false))
            //{
            //    problemSource = "destination";
            //    return false;
            //}
            if (string.IsNullOrEmpty(SourceAccountId))
            {
                problemSource = "source";
                return false;
            }
            if (Amount <= 0)
            {
                problemSource = "amount";
                return false;
            }
            if (String.Equals( DestinationAccountId, SourceAccountId))
            {
                problemSource = "destination";
                return false;
            }

            return true;
        }

    }
}
