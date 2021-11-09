using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Core.DTO;
using Middleware.Core.Model;

namespace Middleware.Core.DAO
{
    public interface ICaseDAO : ITransactionCoordinator
    {
        Task<long> Add(Case item);
        Task<Case> Find(string reference);
        Task<Case> Find(long customerId, AccountType type);
        Task Update(Case item);
        Task<IEnumerable<Case>> FindByState(CaseState caseState);

    }
}
