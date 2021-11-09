using Middleware.Service.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.Implementations
{
    public class CurrencyCode : ICurrencyCode
    {
        public string ConvertCountryCurrencyCode(string countryCode)
        {
            if (!string.IsNullOrWhiteSpace(countryCode))
            {
                switch (countryCode)
                {
                    case "01":
                        return "NGN";
                    case "02":
                        return "GHS";
                    case "03":
                        return "CDF";
                    case "04":
                        return "XOF";
                    case "05":
                        return "GNF";
                    case "06":
                        return "GMD";
                    case "07":
                        return "SLL";
                    default:
                        return string.Empty;
                }

            }
            else
            {
                return "Null";
            }
        }
        public string ConvertCountry(string countryCode)
        {
            if (!string.IsNullOrWhiteSpace(countryCode))
            {
                switch (countryCode)
                {
                    case "01":
                        return "NGN";
                    case "02":
                        return "GHA";
                    case "03":
                        return "COD";
                    case "04":
                        return "SEN";
                    case "05":
                        return "GIN";
                    case "06":
                        return "GM";
                    case "07":
                        return "SL";
                    default:
                        return string.Empty;
                }

            }
            else
            {
                return "Null";
            }
        }
    }
}
