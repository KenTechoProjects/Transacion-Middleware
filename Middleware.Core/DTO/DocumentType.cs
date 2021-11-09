using System;
namespace Middleware.Core.DTO
{
    [Flags]
    public enum DocumentType
    {
        PICTURE=1,
        IDENTIFICATION, 
        SIGNATURE,
        UTILITY_BILL
    }
}
