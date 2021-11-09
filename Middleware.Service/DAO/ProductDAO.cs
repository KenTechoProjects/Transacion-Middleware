using Dapper;
using Middleware.Service.Model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Middleware.Core.DAO;

namespace Middleware.Service.DAO
{
    public class ProductDAO : BaseDAO, IProductDAO
    {
        public ProductDAO(IDbConnection connection) : base(connection)
        {

        }

     

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await _connection.QueryAsync<Product>("SELECT * FROM Products");
        }

        public async Task<Product> FindProduct(string productCode)
        {
            var sql = "Select p.*, b.* from Products p, Billers b where p.biller_Id = b.id and p.productcode = @productcode";
            var product= (await _connection.QueryAsync<Product, Biller, Product>(sql,
                (p, b) =>
                {
                    p.Biller = b;
                    return p;
                }, new { productCode},
                splitOn: "ID"
                )).FirstOrDefault();
            return product;
        }

        public async Task<IEnumerable<Product>> GetActiveProducts(string billerCode)
        {
            var query = "SELECT * FROM Products WHERE IsActive = 1 AND biller_id = (SELECT TOP 1 id FROM Billers WHERE billercode = @billercode AND billertype = 2)";
            return await _connection.QueryAsync<Product>(query, new { billerCode});
        }

        public async Task<Product> GetAnyProduct(string billerCode)
        {
            var query = "SELECT * FROM Products WHERE IsActive = 1 AND ProductType = 1 AND biller_id = (SELECT TOP 1 id FROM Billers WHERE billercode = @billercode)";
            return await _connection.QueryFirstOrDefaultAsync<Product>(query, new { billerCode});
        }
    }
}
