using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.Processors
{
    public interface ITransactionNotificationService
    {
        //To return TransactionNotificationDTO
        Task<ServiceResponse> GetTransactionNotificationsForCustomer(string walleNumber, string language);
        Task<ServiceResponse> GetTransactionNotificationsForCustomerFront(string walletNumber,  string language);
        Task<ServiceResponse> UpdateTransactionNotificationsForCustomer(AuthenticatedUser user, string language, long transationId, bool source = false);
        Task<ServiceResponse> UpdateTransactionNotificationsForCustomer_(string walletNumber, string language, long transationId, bool source = false);
    }
}
