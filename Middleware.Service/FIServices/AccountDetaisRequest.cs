using System;
namespace Middleware.Service.FIServices
{
    public class AccountDetaisRequest : BaseRequest
    {
        public string AccountNumber { get; set; }

        public AccountDetaisRequest(string accountNumber, string countryId) : base(countryId)
        {
            AccountNumber = accountNumber;
        }
    }
}
