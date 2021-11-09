using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Core.DTO
{
    public class ErrorDetails
    {
        public long StatusCode { get; set; }
        public string Message { get; set; }
    }
}
