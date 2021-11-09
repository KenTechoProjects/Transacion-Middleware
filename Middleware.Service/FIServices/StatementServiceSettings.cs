using System;
namespace Middleware.Service.FIServices
{
    public class StatementServiceSettings
    {
        public string BaseAddress { get; set; }
        public string AppId { get; set; }
        public string AppKey { get; set; }
        public string StatementPath { get; set; }
        public int NumberOftransactiondisplayedRecords { get; set; }
        public int Timeout { get; set; }
    }
}
