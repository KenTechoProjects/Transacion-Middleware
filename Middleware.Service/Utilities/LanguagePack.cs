using System;
using System.Collections.Generic;
using Middleware.Service.Model;

namespace Middleware.Service.Utilities
{
    public class LanguagePack
    {
        public LanguagePack(string defaultMessage)
        {
            DefaultMessage = defaultMessage;
        }
        public string DefaultMessage { get; set; }
        public IDictionary<string,string> Mappings { get; set; }
        public IDictionary<OtpPurpose, string> NotificationMessages { get; set; }
        
    }
}