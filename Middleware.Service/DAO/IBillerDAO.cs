using Middleware.Core.DAO;
using Middleware.Service.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.DAO
{
    public interface IBillerDAO : ITransactionCoordinator
    {
        Task<IEnumerable<Biller>> GetAllBillers();
        Task<IEnumerable<Biller>> GetActiveBillers(BillerType type);
    }
}
