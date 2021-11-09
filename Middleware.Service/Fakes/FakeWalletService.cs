using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Core.DTO;
using Middleware.Service.DTO;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;

namespace Middleware.Service.Fakes
{
    public class FakeWalletService : IWalletService
    {
        public FakeWalletService()
        {
        }

        public Task<BasicResponse> ChargeWallet(string walletNumber, decimal amount, string narration, string referenceNumber)
        {
            return Task.FromResult(new BasicResponse(true));
        }

        public Task<BasicResponse> FundWallet(string walletNumber, decimal amount, string narration, string referenceNumber)
        {
            return Task.FromResult(new BasicResponse(true));
        }

        public Task<ServiceResponse<IEnumerable<StatementRecord>>> GetTransactions(string walletNumber, DateTime start, DateTime end)
        {
            var records = new List<StatementRecord>();
            records.Add(new StatementRecord { Amount = 2000, Date = DateTime.Now.AddDays(-1), Description = "wallet pay for tv", IsCredit = false, PostedDate = DateTime.Now.AddDays(-1) });
            records.Add(new StatementRecord { Amount = 50000, Date = DateTime.Now.AddDays(-2), Description = "pay from a friend", IsCredit = true, PostedDate = DateTime.Now.AddDays(-2) });
            records.Add(new StatementRecord { Amount = 200000, Date = DateTime.Now.AddDays(-3), Description = "wallet transfer", IsCredit = true, PostedDate = DateTime.Now.AddDays(-3) });
            records.Add(new StatementRecord { Amount = 5000, Date = DateTime.Now.AddDays(-4), Description = "wallet pay for domestic energy", IsCredit = false, PostedDate = DateTime.Now.AddDays(-4) });

            var response = new ServiceResponse<IEnumerable<StatementRecord>>(true);
            response.SetPayload(records);
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<Wallet>> GetWallet(string walletNumber)
        {
            var wallet = new Wallet
            {
                Balance = new AccountBalance
                {
                    BookBalance = 500,
                    AvailableBalance = 500
                },
                WalletNumber = walletNumber,
                WalletType = "Basic",
                Currency = "XOF", WalletName = "Olanrewaju Okanrende"
            };
            var response = new ServiceResponse<Wallet>(true);
            response.SetPayload(wallet);
            return Task.FromResult(response);
        }

        public Task<BasicResponse> ReverseTransaction(string reference, string walletNumber)
        {
            return Task.FromResult(new BasicResponse(true));

        }

        public Task<ServiceResponse<UpgrateWalletResponse>> UpgradeWallet(UpgradeWalletDTO request)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<string>> WalletEnquiry(string walletNumber)
        {
            var response = new ServiceResponse<string>(true);
            response.SetPayload("Olanrewaju Okanrende");
            return Task.FromResult(response);
        }

        public Task<BasicResponse> WalletTransfer(BaseTransferRequest walletTransferRequest)
        {
            return Task.FromResult(new BasicResponse(true));
        }

        Task<BasicResponse> IWalletService.ChargeWallet(string walletNumber, decimal amount, string narration, string referenceNumber)
        {
            throw new NotImplementedException();
        }

        Task<BasicResponse> IWalletService.FundWallet(string walletNumber, decimal amount, string narration, string referenceNumber)
        {
            throw new NotImplementedException();
        }

        Task<ServiceResponse<IEnumerable<StatementRecord>>> IWalletService.GetTransactions(string walletNumber, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        Task<ServiceResponse<Wallet>> IWalletService.GetWallet(string walletNumber)
        {
            throw new NotImplementedException();
        }

        Task<BasicResponse> IWalletService.ReverseTransaction(string reference, string walletNumber)
        {
            throw new NotImplementedException();
        }

       

        Task<ServiceResponse<string>> IWalletService.WalletEnquiry(string walletNumber)
        {
            throw new NotImplementedException();
        }
 
    }
}
