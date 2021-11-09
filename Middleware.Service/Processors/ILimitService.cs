using Middleware.Core.DTO;
using Middleware.Core.Model;
using Middleware.Service.DTOs;
using Middleware.Service.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.Processors
{
   public interface ILimitService
    {
        Task<TransactionLimitResponse> ValidateLimit(long customerId, TransactionType transactionType, decimal amount);
        Task<TransactionLimitResponse> ValidateLimitAfterToken(long customerId, TransactionType transactionType, decimal amount, bool lmitExceeded=false);
        //Task<LimitResponse> ValidatePaymentLimit(long customerId, TransactionType transactionType, decimal amount);
    }
}
 