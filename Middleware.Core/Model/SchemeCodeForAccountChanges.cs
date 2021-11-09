using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Core.Model
{
    public class SchemeCodeForAccountChanges
    {
        
        public string Scheme_Code_Charged { get; set; }
        public string Scheme_Code_NotCharged { get; set; }
        public double Charges { get; set; }
    }
}
