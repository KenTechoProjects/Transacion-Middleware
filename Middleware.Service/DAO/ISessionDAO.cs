using System;
using System.Threading.Tasks;
using Middleware.Core.DAO;
using Middleware.Service.Model;

namespace Middleware.Service.DAO
{
    public interface ISessionDAO : ITransactionCoordinator
    {
        Task Add(Session session);
        Task<Session> Find(string token);
        Task Delete(string token);
        Task DeleteCustomerSessions(long customerId);
        Task DeleteCustomerSessions(string username);
    }
}
