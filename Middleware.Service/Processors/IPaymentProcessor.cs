using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.Processors
{
    public interface IPaymentProcessor
    {
        Task<ServiceResponse<BillsPayResponse>> MakePaymentByWallet(WalletBillsPayRequest request);
        Task<ServiceResponse<BillsPayResponse>> MakePaymentByAccount(AccountBillsPayRequest request);
    }
}
