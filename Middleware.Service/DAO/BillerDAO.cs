using Dapper;
using Middleware.Core.DAO;
using Middleware.Service.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.DAO
{
    public class BillerDAO : BaseDAO, IBillerDAO
    {
        public BillerDAO(IDbConnection connection) : base(connection)
        {
            if (_connection.State == ConnectionState.Closed) { _connection.Open(); }
        }

        public async Task<IEnumerable<Biller>> GetAllBillers()
        {
            return await _connection.QueryAsync<Biller>("SELECT * FROM Billers");

        }

        public async Task<IEnumerable<Biller>> GetBillersByType(int category)
        {
            return await _connection.QueryAsync<Biller>("SELECT * FROM Billers WHERE Category = @category AND IsActive = 1", new Biller { BillerType = (BillerType)category });
        }

        public async Task<IEnumerable<Biller>> GetActiveBillers(BillerType type)
        {
            return await _connection.QueryAsync<Biller>("SELECT * FROM Billers WHERE billertype = @type AND IsActive = 1", new { type = (int)type });
        }
    }
}
