using Middleware.Core.DAO;
using Middleware.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.DAO
{
 public   interface IAccountOpeningRequestDAO : ITransactionCoordinator
    {
        Task<AccountOpeningRequest> Add(AccountOpeningRequest request);
        Task<AccountOpeningRequest> Find(string walletNumber);
        Task<long> Update(AccountOpeningRequest entity); 

    }
}
