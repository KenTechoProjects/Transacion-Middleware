using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Reversal.Services
{
    public interface ICaseTransmitter
    {
        Task TransmitCases();
        Task TransmitUpdatedDocuments();
    }
}
