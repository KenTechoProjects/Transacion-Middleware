using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Middleware.Service.BAP;
using Middleware.Service.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Middleware.Client.BaseServces
{
    public static class HttpServices
    {
        public static IServiceCollection AddHtpClients(this IServiceCollection services)
        {
            services.AddHttpClient("HttpMessageHandler").ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    UseDefaultCredentials = true,
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, policy) =>
                    {
                        return true;
                    }
                };
            });
            services.AddHttpClient<IPaymentManager, PaymentManager>().ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    UseDefaultCredentials = true,
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, policy) =>
                    {
                        return true;
                    }
                };

            });
            services.AddHttpClient<IPaymentManager, PaymentManager>().ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                    UseDefaultCredentials = true,
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, policy) =>
                    {
                        return true;
                    }
                };

            });
            return services;

        }
    }
}
