using System;
using Middleware.Service.Model;

namespace Middleware.Service.Utilities
{
    public interface IMessageProvider
    {
        string GetMessage(string code, string language);
        string GetNotificationMessage(OtpPurpose messageId, string language);
       
    }
}