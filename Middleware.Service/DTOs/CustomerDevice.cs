using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Service.DTOs
{
    public class CustomerDevice
    {
        public string DeviceId { get; set; }
        public bool IsActive { get; set; }
        public string Model { get; set; }
        public long? Customer_Id { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}
