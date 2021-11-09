using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.DTOs
{
    public class OnboardingCompletionResponse
    {
        public string AccountNumber { get; set; }
        public string CifId { get; set; }
        public string CustomerName { get; set; }
        public string AccountTier { get; set; }
        public decimal MaximumTransactionAmount { get; set; }
        public AccountOpeningRecordStatus AccountOpeningRecordStatus { get; set; }
    }
}
