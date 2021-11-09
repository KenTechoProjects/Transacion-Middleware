using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.Model
{
   public class UserActivity
    {
        public long ID { get; set; }
        public long CustomerId { get; set; }
        public string Activity { get; set; }
        public string ActivityResult { get; set; }
        public string walletNumber { get; set; }
        public string ResultDescription { get; set; }
        public DateTime ActivityDate { get; set; }
    }

    //public enum ActionType
    //{
    //    Login = 1,
    //    Logout,
    //    ChangePassword,
    //    ResetPassword,
    //    ChangePin,
    //    Transfer,
    //    Airtime,
    //    BillPayment
    //}

    public enum ActionResult
    {
        Successful = 1,
        Failed
    }
}
