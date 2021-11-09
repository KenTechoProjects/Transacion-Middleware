using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Service.DTOs;

namespace Middleware.Service.Processors
{
    public interface IExternalTransferService
    {
        Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber, string bankCode);
        Task<BasicResponse> Transfer(BaseTransferRequest request, string reference);
        Task<ServiceResponse<IEnumerable<Branch>>> GetBranches(string bankCode);
    }
}