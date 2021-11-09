using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Middleware.Core.Model;
using Dapper;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Middleware.Core.DAO
{
    public class DeviceDAO : BaseDAO, IDeviceDAO
    {
        private readonly ILogger _log;
        public DeviceDAO(IDbConnection connection, ILoggerFactory log) : base(connection)
        {
            _log = log.CreateLogger(typeof(DeviceDAO));
        }

        public async Task<string> Add(Device device)
        {

            if (device.Customer_Id != null)
            {
                bool isavala = await CustomerIdIsValid((long)device.Customer_Id);
                if (isavala == false)
                {
                    return "Customer Id can not be null";
                }
            }
            else
            {
                return "Customer Id can not be null";
            }

            _log.LogInformation("Inside the Add method of DeviceDAO at {0}", DateTime.UtcNow);

            string sql = @"INSERT INTO Devices (Customer_Id, DeviceId, Model, IsActive, DateCreated) 
                        Values 
                            (@Customer_Id, @DeviceId, @Model, @IsActive, @DateCreated)";
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            await _connection.ExecuteAsync(sql, device, UnitOfWorkSession?.GetTransaction());
            return "saved";


        }
        public async Task<bool> CustomerIdIsValid(long customerId)
        {
            var sql = "Select * from Customers where Id=@customerId;";
            if (_connection.State == ConnectionState.Closed) { _connection.Open(); }

            var isAvalaib = await _connection.QueryFirstOrDefaultAsync<Customer>(sql, new { customerId }, UnitOfWorkSession?.GetTransaction());
            if (isAvalaib != null)
            {
                return true;
            }
            return false;
        }
        public async Task<Device> Find(string deviceId)
        {
            var sql = "Select d.*, c.* from Devices d, Customers c where d.Customer_Id = c.id and d.DeviceId = @deviceId";
            var data = (await _connection.QueryAsync<Device, Customer, Device>(sql,
                (d, c) =>
                {
                    d.Customer = c;
                    return d;
                }, new { DeviceId = deviceId },
                splitOn: "ID"
                )).FirstOrDefault();
            return data;
        }

        public async Task<Device> FindByCustomerIdAndDeviceId(long csutomerId, string deviceId)
        {
            _log.LogInformation("Inside the FindByCustomerId method of the DeviceDAO at {0}", DateTime.UtcNow);

            var sql = "Select d.*, c.* from Devices d, Customers c where d.Customer_Id = c.id and d.DeviceId = @deviceId";
            var result = (await _connection.QueryAsync<Device, Customer, Device>(sql,
                (d, c) =>
                {
                    d.Customer = c;
                    return d;
                }, new { csutomerId, deviceId },
                splitOn: "ID"
                )).FirstOrDefault();
            return result;

        }
        public async Task Update(Device device)
        {

            string sql = @"UPDATE Devices SET 
                            Customer_Id = @Customer_Id,
                            Model = @Model, 
                            IsActive = @IsActive
                           WHERE ID = @ID";
            var data = await _connection.ExecuteAsync(sql, device, UnitOfWorkSession?.GetTransaction());
        }

        public async Task<IEnumerable<Device>> FindByCustomerId(long customer_id)
        {
            var data = await _connection.QueryAsync<Device>("Select * from Devices where Customer_Id = @customer_id",
                                                                    new Device { Customer_Id = customer_id });
            return data;
        }

        private async Task<Device> GetDevice(string deviceID)
        {
            var query = "SELECT customer_id FROM Devices WHERE deviceId=@deviceID";
            var result = await _connection.QueryFirstOrDefaultAsync<Device>(query, new { deviceID });

            return result;
        }
        public async Task<bool> IsAvailable(string deviceID)
        {
            var query = "SELECT customer_id FROM Devices WHERE deviceId=@deviceID";
            var result = await _connection.ExecuteScalarAsync<long?>(query, new { deviceID });
            return result == null;
        }

        public async Task<int> CountAssignedDevices(long customerId)
        {
            var query = "SELECT COUNT(id) FROM Devices WHERE customer_id = @customerId";
            var data = await _connection.ExecuteScalarAsync<int>(query, new { customerId });
            return data;
        }

        public async Task<int> UseDevices(long customerId)
        {
            var query = "UPDATE Devices WHERE customer_id = @customerId";
            var data = await _connection.ExecuteScalarAsync<int>(query, new { customerId });
            return data;
        }
    }
}
