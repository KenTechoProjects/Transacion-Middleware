using System;
using System.Threading.Tasks;
using Middleware.Service.DTOs;

namespace Middleware.Service.Processors
{
    public interface INotifier
    {
        Task SendSMS(string userId, string phoneNumber, string message);
       // Task<BasicResponse> SendSMS_Test(string accountNumber, string phoneNumber, string message, string language);
    }
}
