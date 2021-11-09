using System;
namespace Middleware.Core.DTO
{
    public enum DocumentState
    {
        NEW,
        UPDATED,
        REVIEW_ONGOING,
        INVALID,
        VALID
    }
    public enum DocumentStatus
    {
        PENDING,
        APPROVED,
        REJECTED
    }
}
