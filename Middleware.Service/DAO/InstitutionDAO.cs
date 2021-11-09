using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Middleware.Service.Model;

namespace Middleware.Service.DAO
{
    public class InstitutionDAO : IInstitutionDAO
    {
        private readonly IDbConnection _connection;

        public InstitutionDAO(IDbConnection connection)
        {
            _connection = connection;
            if (_connection.State == ConnectionState.Closed) { _connection.Open(); }
        }

        public async Task<IEnumerable<Institution>> FindByCategory(InstitutionCategory category)
        {
            var result =  await _connection.QueryAsync<Institution>("SELECT * FROM Institutions  WHERE category = @category",
                                                                    new Institution { Category = category });
            return result;
        }

        public async Task<Institution> FindByInstitutionCode(string institutionCode)
        {
            var result = await _connection.QueryFirstOrDefaultAsync<Institution>("SELECT * FROM Institutions  WHERE InstitutionCode = @InstitutionCode",
                                                                    new Institution {  InstitutionCode = institutionCode });
            return result;
        }

        public async Task<IEnumerable<Institution>> FindByCategoryAndStatus(InstitutionCategory category, bool isEnabled)
        {
                var data= await _connection.QueryAsync<Institution>("SELECT * FROM Institutions WHERE category = @category and isEnabled = @isEnabled",
                                                                       new Institution { Category = category, IsEnabled = isEnabled });
            return data;
        }

        public async Task<IEnumerable<Institution>> FindByStatus(bool isEnabled)
        {
                var data= await _connection.QueryAsync<Institution>("SELECT * FROM Institutions WHERE isEnabled = @isEnabled",
                                                                       new Institution { IsEnabled = isEnabled });
            return data;
        }

        public async Task<IEnumerable<Institution>> GetAll()
        {
                var data= await _connection.QueryAsync<Institution>("SELECT * FROM Institutions");
            return data;
        }
    }
}
