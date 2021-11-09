using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.FIServices
{
    public class AccountOpeningSettings
    {
        public string BaseAddress { get; set; }
        public string AppId { get; set; }
        public string AppKey { get; set; }
        public string OpenAccountEndpoint { get; set; }
        public string PNDRemovalEndpoint { get; set; }
    }
}
