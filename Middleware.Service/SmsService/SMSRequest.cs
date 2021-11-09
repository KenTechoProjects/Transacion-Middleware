using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.SmsService
{
    public class SMSRequest
    {
        public string RequestId { get; set; }
        public string AccountNumber { get; set; }
        public string MobileNo { get; set; }
        public string Message { get; set; }
        public string ChannelId { get; set; }
        public string ChannelKey { get; set; }
        public string CountryId { get; set; }
    }
}
