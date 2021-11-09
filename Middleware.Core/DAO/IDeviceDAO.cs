using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Core.Model;

namespace Middleware.Core.DAO
{
    public interface IDeviceDAO : ITransactionCoordinator
    {
        Task<Device> Find(string deviceKey);     
        Task<bool> CustomerIdIsValid(long customerId);
        Task<string> Add(Device device);
        Task Update(Device device);
        Task<IEnumerable<Device>> FindByCustomerId(long customer_id);
        Task<bool> IsAvailable(string deviceID);
        Task<Device> FindByCustomerIdAndDeviceId(long csutomerId, string deviceId);
        Task<int> CountAssignedDevices(long customerId);
    }
}
