using System;
using System.Threading.Tasks;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
namespace Middleware.Service.Fakes
{
    public class FakeWalletCreator : IWalletCreator
    {
        
        public Task<ServiceResponse<WalletCompletionResponse>> OpenWallet(WalletCreationRequest request)
        {
            var payload = new WalletCompletionResponse
            {
                CustomerName = "Test Customer",
                 HasCeiling = true,
                 MaximumTransactionAmount = 200000,
                 WalletNumber = request.PhoneNumber,
                 WalletType = "Basic"
                  
            };
            var response = new ServiceResponse<WalletCompletionResponse>(true);
            response.SetPayload(payload);
            return Task.FromResult(response);
        }
    }
}
