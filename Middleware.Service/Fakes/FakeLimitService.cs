using Middleware.Core.DTO;
using Middleware.Core.Model;
using Middleware.Service.DTOs;
using Middleware.Service.Model;
using Middleware.Service.Processors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.Fakes
{
    public class FakeLimitService : ILimitService
    {
        public Task<LimitResponse> ValidatePaymentLimit(long customerId, TransactionType transactionType, decimal amount)
        {
            return Task.FromResult(new LimitResponse()
            {
                SingleLimitExceeded = false,
                DailyLimitExceeded = false
            });
        }

        public Task<bool> ValidateLimit(long customerId, TransactionType transactionType, decimal amount)
        {
            return Task.FromResult(true);
        }

        Task<TransactionLimitResponse> ILimitService.ValidateLimit(long customerId, TransactionType transactionType, decimal amount)
        {
            return null;
        }

        public Task<TransactionLimitResponse> ValidateLimitAfterToken(long customerId, TransactionType transactionType, decimal amount, bool lmitExceeded = false)
        {
            return null;
        }
    }
}
