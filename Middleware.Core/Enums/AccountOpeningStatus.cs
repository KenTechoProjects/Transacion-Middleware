using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Core.Enums
{
    public enum AccountOpeningStatus
    {
  LocalCustomerRecordPending=1,
        SelfiePending,
        SignaturePending,
        IdentificationPending,
        LocalAccountOpeningRequestPending
      ,AccountOpeningPending,Completed
    }
}
