using System;
using System.Collections.Generic;

namespace Middleware.Service.DTOs
{
    public class InstitutionResult
    {
        public IEnumerable<InstitutionDetails> Banks { get; set; }
        public IEnumerable<InstitutionDetails> MobileMoneyOperators { get; set; }
    }

    public class InstitutionDetails
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}