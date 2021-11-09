using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.Processors
{
    public interface ICurrencyCode
    {
        string ConvertCountryCurrencyCode(string countryCode);
        string ConvertCountry(string countryCode);
    }
}
