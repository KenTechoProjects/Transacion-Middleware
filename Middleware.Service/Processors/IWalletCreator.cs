using System;
using System.Threading.Tasks;
using Middleware.Service.DTOs;

namespace Middleware.Service.Processors
{
    public interface IWalletCreator
    {
        Task<ServiceResponse<WalletCompletionResponse>> OpenWallet(WalletCreationRequest request);
    }
}
