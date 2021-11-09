using System;

namespace Middleware.Service.DTOs
{
    public class ForexRate
    {
        public string Source { get; set; }
        public string Target {get;set;}

        public decimal Rate { get; set; }
    }
}