using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Middleware.Service.DTOs;
using Middleware.Service.Processors;
namespace Middleware.Service.Fakes
{
    public class FakeNotifier : INotifier
    {
        private readonly ILogger _logger;

        public FakeNotifier(ILogger<FakeNotifier> logger)
        {
            _logger = logger;
        }

        public Task SendSMS(string userId, string phoneNumber, string message)
        {
            _logger.LogInformation("Sending SMS to {0} - {1}", new [] {phoneNumber, message });
            return Task.CompletedTask;
        }

       
    }
}
