using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Service.Model;
using Newtonsoft.Json;

namespace Middleware.Service.Utilities
{
    public class FileSystemLanguageProvider : ILanguageConfigurationProvider
    {
        private readonly LanguageSettings _settings;
        private const string FILE_EXTENSION = ".json";
        private readonly IDictionary<string, LanguagePack> _packs;
        private readonly ILogger _logger;
        public FileSystemLanguageProvider(IOptions<LanguageSettings> settingsProvider,
            ILoggerFactory logger)
        {
            _settings = settingsProvider.Value;
            _logger = logger.CreateLogger(typeof(FileSystemLanguageProvider));
            _packs = new Dictionary<string, LanguagePack>(_settings.Bundles.Length);
            CreateLanguagePacks();
        }

        private void CreateLanguagePacks()
        {
            var serializer = JsonSerializer.CreateDefault();
            foreach (var bundle in _settings.Bundles)
            {
                try
                {
                    var pack = new LanguagePack(bundle.DefaultMessage);
                    var location = $"{_settings.BaseLocation}{Path.DirectorySeparatorChar}{bundle.LanguageCode}{FILE_EXTENSION}";
                    if (!File.Exists(location))
                    {
                        _logger.LogWarning("No bundle file found for language - {0}. file location - {1}",
                            new[] { bundle.LanguageCode, location });
                        continue;
                    }
                    var reader = File.OpenText(location);

                    pack.Mappings = (Dictionary<string, string>)serializer.Deserialize(reader, typeof(Dictionary<string, string>));
                    var messages = new Dictionary<OtpPurpose, string>();
                    if (!string.IsNullOrEmpty(bundle.NewDeviceMessage))
                    {
                        messages[OtpPurpose.DEVICE_SWITCH] = bundle.NewDeviceMessage;
                    }
                    if (!string.IsNullOrEmpty(bundle.NewWalletMessage))
                    {
                        messages[OtpPurpose.WALLET_OPENING] = bundle.NewWalletMessage;
                    }
                    if (!string.IsNullOrEmpty(bundle.PasswordResetMessage))
                    {
                        messages[OtpPurpose.PASSWORD_RESET] = bundle.PasswordResetMessage;
                    }
                    pack.NotificationMessages = messages;
                    _packs[bundle.LanguageCode] = pack;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to load language pack for language - {0}",
                         bundle.LanguageCode);
                }

            }
        }

        public LanguagePack GetPack(string language)
        {
            try
            {
                return (_packs.TryGetValue(language, out var pack)) ? pack : null;
            }
            catch (Exception ex)
            {
                return new LanguagePack("Error code can not be null") { };
            }
        }
    }
}
