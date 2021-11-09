using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.BackgroundService.Services
{
    public interface IReversalService
    {
        Task ReverseWalletTransactions(); 
        Task ReverseAccountTransactions(); 
    }
}
