using System;
namespace Middleware.Service.FIServices
{
    public class AccountDetailsResponse : BaseResponse
    {
        public string CustomerID { get; set; }
        public string AccountName { get; set; }
    }
}
