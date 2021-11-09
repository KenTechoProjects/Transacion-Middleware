using System;
using Middleware.Service.Model;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Middleware.Service.DAO
{
    public interface IInstitutionDAO
    {
        Task<IEnumerable<Institution>> FindByCategory(InstitutionCategory category);
        Task<IEnumerable<Institution>> FindByCategoryAndStatus(InstitutionCategory category, bool status);
        Task<IEnumerable<Institution>> FindByStatus(bool status);
        Task<IEnumerable<Institution>> GetAll();
        Task<Institution> FindByInstitutionCode(string institutionCode);
    }
}
