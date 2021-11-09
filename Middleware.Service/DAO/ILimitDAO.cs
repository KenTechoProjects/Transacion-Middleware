using System;
using System.Threading.Tasks;
using Middleware.Core.Model;
using Middleware.Service.Model;

namespace Middleware.Service.DAO
{
    public interface ILimitDAO
    {
        Task<Limit> Find(TransactionType transactionType);
    }
}