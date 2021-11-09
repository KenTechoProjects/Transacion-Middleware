using Middleware.Service.DTOs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.Processors
{
    public interface ITransactionTracker
    {
        Task<BasicResponse> AddTransactionReference(long username, string transRef); 
        Task<bool> Exists(string transRef); 
    }
}
