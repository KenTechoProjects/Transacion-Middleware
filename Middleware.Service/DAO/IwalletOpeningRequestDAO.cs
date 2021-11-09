using System;
using System.Threading.Tasks;
using Middleware.Core.DAO;
using Middleware.Service.Model;
namespace Middleware.Service.DAO
{
    public interface IWalletOpeningRequestDAO : ITransactionCoordinator
    {
        Task<long> Add(WalletOpeningRequest entity);
        Task<WalletOpeningRequest> Find(string walletNumber);
        Task Update(WalletOpeningRequest entity);
        



    }
}
