using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Middleware.Core.Model;

namespace Middleware.Core.DAO
{
    public class DeviceHistoryDAO : BaseDAO, IDeviceHistoryDAO
    {
        public DeviceHistoryDAO(IDbConnection connection) : base(connection)
        {
        }

        public async Task Add(Device device)
        {
            string query = @"INSERT INTO DeviceHistories (Customer_Id, DeviceId, Model, IsActive, DateCreated) 
                        Values 
                            (@Customer_Id, @DeviceId, @Model, @IsActive, @DateCreated)";
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            await _connection.ExecuteAsync(query, device, UnitOfWorkSession?.GetTransaction());
        }
    }
}
