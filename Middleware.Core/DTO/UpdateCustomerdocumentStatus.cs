using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Core.DTO
{
    public class UpdateCustomerdocumentStatus
    {
        public string PhoneNuumber { get; set; }
        public DocumentType DocumentType { get; set; }
        [DefaultValue(false)]
        public bool? DocumentStatus { get; set; }
        public bool IsValid(out string message)
        {
            if (string.IsNullOrWhiteSpace(PhoneNuumber))
            {
                message = "Empty phone number";
                return false;

            }
            message = string.Empty;
            return true;
    }
    }

}
