using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.Processors
{
    public interface ISMSService
    {
        Task SendSMS(string accountNumber, string phoneNumber, string message);
    }
}
