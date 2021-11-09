using System;
namespace Middleware.Service.FIServices
{
    public class GetBranchesRequest : BaseRequest
    {
        public string BankCode { get; set; }

        public GetBranchesRequest(string countryId, string bankCode) : base(countryId)
        {
            BankCode = bankCode;
        }
    }
}
