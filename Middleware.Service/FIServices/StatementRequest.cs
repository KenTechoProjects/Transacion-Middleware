using System;
namespace Middleware.Service.FIServices
{
    public class StatementRequest : BaseRequest
    {
        public StatementRequest(string countryID) : base(countryID)
        {
        }

        public string AccountNumber { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool IsLetterHead { get; } = false;
    }
}
