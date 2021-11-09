using System;
using System.Text.Json.Serialization;

namespace Middleware.Core.Model
{
    public class Device
    {
        public long Id { get; set; }
        public string DeviceId { get; set; }   
        [JsonIgnore]
        public bool IsActive { get; set; }
        public string Model { get; set; }
        [JsonIgnore]
        public Customer Customer { get; set; }  
        [JsonIgnore]
        public long? Customer_Id { get; set; } 
        [JsonIgnore]
        public DateTime? DateCreated { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if (string.IsNullOrEmpty(DeviceId))
            {
                problemSource = "Device Id";
                return false;
            }
            if (string.IsNullOrEmpty(Model))
            {
                problemSource = "Device Model";
                return false;
            }
            return true;
        }
    }
}
