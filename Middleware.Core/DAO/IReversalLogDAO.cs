using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Core.Model;

namespace Middleware.Core.DAO
{
    public interface IReversalLogDAO : ITransactionCoordinator
    {
        Task Add(ReversalLog reversalLog);
    }
}
