using System;
namespace Middleware.Service.Utilities
{
    public interface ICodeGenerator
    {
        string Generate(int length);
        string   ReferralCode(int length);
    }
}
