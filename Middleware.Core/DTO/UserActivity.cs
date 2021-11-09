using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Middleware.Core.DTO
{
    public class UserActivity
    {
        public long ID { get; set; }
 public long CustomerId { get; set; }
 public string Activity { get; set; }
 public string ActivityResult { get; set; }
 public string ResultDescription { get; set; }
 public DateTime ActivityDate { get; set; }
    }
}
