using Middleware.Core.Model;
using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.DAO
{
    public interface ITransactionNotificationDAO
    {
        Task<List<Transaction>> GetTransactionNotificationsForCustomer(List<string> accountNumbers);
      //  Task<List<Transaction>> GetTransactionNotificationsForCustomer(long customerId, Core.Model.TransactionTag transactionTag);
       // Task<ServiceResponse> GetTransactionNotificationsForCustomerFront(long customerId, Core.Model.TransactionTag transactionTag, string lnguage);
        Task<ServiceResponse> UpdateSourceTransactionNotificationsForCustomer(List<string> accountNumbers,long transactionId, long cusotmerId);
        Task<ServiceResponse> UpdateDestinationTransactionNotificationsForCustomer(List<string> accountNumbers, long transactionId);
    }
}
