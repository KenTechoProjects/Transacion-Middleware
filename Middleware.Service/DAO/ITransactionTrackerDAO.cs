using Middleware.Core.DAO;
using Middleware.Service.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.DAO
{
    public interface ITransactionTrackerDAO : ITransactionCoordinator
    {
        Task<long> Add(Tracker trackingInfo);
        Task<bool> Find(string transactionReference);
    }
}
