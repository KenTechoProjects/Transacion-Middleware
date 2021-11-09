using Middleware.Core.DAO;
using Middleware.Service.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.DAO
{
    public interface IProductDAO
    {
        Task<IEnumerable<Product>> GetAllProducts();
        Task<IEnumerable<Product>> GetActiveProducts(string billerCode);
        Task<Product> FindProduct(string productCode);
        Task<Product> GetAnyProduct(string billerCode);

    }
}
