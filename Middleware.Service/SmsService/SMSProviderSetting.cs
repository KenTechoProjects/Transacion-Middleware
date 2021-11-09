using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.SmsService
{
    public class SMSProviderSetting
    {
        public string ChannelId { get; set; }
        public string ChannelKey { get; set; }
        public string CountryId { get; set; }
        public string SMSBaseEndPoint { get; set; }
        public string SendSMSEndPoint { get; set; }
        public string RequestFormat { get; set; }
    }
}
