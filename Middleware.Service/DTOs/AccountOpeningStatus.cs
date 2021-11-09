using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public enum AccountOpeningRecordStatus
    {
        INITIALISED = 1,
        ID_PROVIDED,
        PHOTO_PROVIDED,
        SIGNATURE_PROVIDED,
        COMPLETED
    }
}
