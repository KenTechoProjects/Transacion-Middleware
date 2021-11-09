using System;
namespace Middleware.Core.DTO
{
    public enum CaseState
    {
        NEW,
        READY,
        TRANSMITTED,
        IN_PROGRESS,
        UPDATE_REQUIRED,
        COMPLETED
    }
}
