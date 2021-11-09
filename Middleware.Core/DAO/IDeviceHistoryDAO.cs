using Middleware.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Core.DAO
{
    public interface IDeviceHistoryDAO
    {
        Task Add(Device device);
    }
}
