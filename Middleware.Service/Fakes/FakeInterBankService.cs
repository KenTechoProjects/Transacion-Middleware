using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;

namespace Middleware.Service.Fakes
{
    public class FakeInterBankService : IExternalTransferService
    {
        public Task<ServiceResponse<dynamic>> GetAccountName(string accountNumber, string bankCode)
        {
            var response = new ServiceResponse<dynamic>(true);
            response.SetPayload(new { AccountName = "Other Bank Customer" });
            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<Branch>>> GetBranches(string bankCode)
        {
            var branches = new [] { new Branch
            {
                BranchCode = "001",
                BranchName = "Head Office"
            } };
            var response = new ServiceResponse<IEnumerable<Branch>>(true);
            response.SetPayload(branches);
            return Task.FromResult(response);
        }

        public Task<BasicResponse> Transfer(BaseTransferRequest request, string reference)
        {
            var response = new BasicResponse(true);
           
            return Task.FromResult(response);
        }
    }
}