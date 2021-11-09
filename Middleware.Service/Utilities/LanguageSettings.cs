using System;
namespace Middleware.Service.Utilities
{
    public class LanguageSettings
    {
        public string BaseLocation { get; set; }
        public BundleSettings[] Bundles { get; set; }

    }

    public class BundleSettings
    {
        public string LanguageCode { get; set; }
        public string DefaultMessage { get; set; }
        public string NewWalletMessage { get; set; }
        public string NewDeviceMessage { get; set; }
        public string PasswordResetMessage { get; set; }
    }
}
