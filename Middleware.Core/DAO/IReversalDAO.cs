using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Core.Model;

namespace Middleware.Core.DAO
{
    public interface IReversalDAO : ITransactionCoordinator
    {
        Task<Reversal> Find(long Id);
        Task Add(Reversal reversal);
        Task Update(Reversal reversal);
        Task<IEnumerable<Reversal>> FindByTypeAndStatus(ReversalType reversalType, ReversalStatus reversalStatus);
    }
}
