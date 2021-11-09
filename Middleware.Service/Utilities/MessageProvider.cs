using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Middleware.Service.Model;

namespace Middleware.Service.Utilities
{
    public class MessageProvider : IMessageProvider
    {
        private readonly ILogger _log;

        readonly ILanguageConfigurationProvider _provider;

        public MessageProvider(ILanguageConfigurationProvider provider, ILoggerFactory log)
        {
            _provider = provider;
            _log = log.CreateLogger(typeof(MessageProvider));
        }


        public string GetMessage(string code, string language)
        {
            try
            {


                var bundle = _provider.GetPack(language);
                if (bundle == null)
                {
                    //throw new Exception("Invalid language configuration");
                    return "Invalid language configuration";
                }
                if (bundle.Mappings != null)
                {
                    if (bundle.Mappings.TryGetValue(code, out var message))
                    {
                        return message;
                    }
                }


                return bundle.DefaultMessage;
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "An error ocuured");
            }
            return null;

        }

        public string GetNotificationMessage(OtpPurpose messageId, string language)
        {
            try
            {

                var bundle = _provider.GetPack(language);
                if (bundle == null)
                {
                    return "Invalid language configuration"; // throw new Exception("Invalid language configuration");
                }
                if (!bundle.NotificationMessages.TryGetValue(messageId, out var message))
                {
                    throw new KeyNotFoundException($"No message found for the specified key - {messageId}");
                }
                return message;
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex, "server error occurrd");
                return null;
            }
        }
    }
}