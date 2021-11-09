using System;
using System.Collections.Generic;

namespace Middleware.Service.Utilities
{
    public interface ILanguageConfigurationProvider
    {
        LanguagePack GetPack(string language);
    }
}