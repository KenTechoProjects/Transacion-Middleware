using System;

namespace Middleware.Service.Model
{
    public class Institution
    {
        public long Id { get; set; }
        public string InstitutionName { get; set; }
        public string InstitutionCode { get; set; }
        public bool IsEnabled { get; set; }
        public InstitutionCategory Category { get; set; }
    }

    public enum InstitutionCategory
    {
        BANK = 1,
        MOBILE_MONEY
    }
}