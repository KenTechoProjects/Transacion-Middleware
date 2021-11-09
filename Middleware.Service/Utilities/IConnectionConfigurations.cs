using System.Data;

namespace Middleware.Service.Utilities
{
    public interface IConnectionConfigurations
    {
        IDbTransaction Transaction();
    }
}