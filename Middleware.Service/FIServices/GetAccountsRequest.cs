using System;

namespace Middleware.Service.FIServices
{
    public class GetAccountsRequest : BaseRequest
    {
        public GetAccountsRequest(string countryId, string customerId) : base(countryId)
        {
            CustomerId = customerId;
        }

        public string CustomerId { get; set; }
    }
}