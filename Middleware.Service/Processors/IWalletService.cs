using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Middleware.Service.DTOs;
using Middleware.Core.DTO;
using Middleware.Service.DTO;

namespace Middleware.Service.Processors
{
    public interface IWalletService
    {
        Task<ServiceResponse<Wallet>> GetWallet(string walletNumber);
        Task<ServiceResponse<string>> WalletEnquiry(string walletNumber);
        Task<ServiceResponse<IEnumerable<StatementRecord>>> GetTransactions(string walletNumber, DateTime start, DateTime end);
        Task<BasicResponse> WalletTransfer(BaseTransferRequest  walletTransferRequest);
        Task<BasicResponse> FundWallet(string walletNumber, decimal amount, string narration, string referenceNumber);
        Task<BasicResponse> ReverseTransaction(string reference, string walletNumber);
        Task<BasicResponse> ChargeWallet(string walletNumber, decimal amount, string narration, string referenceNumber);
        Task<ServiceResponse<UpgrateWalletResponse>> UpgradeWallet(UpgradeWalletDTO request);
    }
}
