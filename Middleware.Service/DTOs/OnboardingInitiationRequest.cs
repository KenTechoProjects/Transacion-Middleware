using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class OnboardingInitiationRequest
    {
        public string AccountNumber { get; set; }
        public string BranchCode { get; set; }
        public string DeviceId { get; set; }
        public string DeviceModel { get; set; }


        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if (string.IsNullOrEmpty(AccountNumber))
            {
                problemSource = "Account Number";
                return false;
            }
            if (string.IsNullOrEmpty(BranchCode))
            {
                problemSource = "Branch Code";
                return false;
            }
            if (string.IsNullOrEmpty(DeviceId))
            {
                problemSource = "Device";
                return false;
            }
            if (string.IsNullOrEmpty(DeviceModel))
            {
                problemSource = "Device Model";
                return false;
            }
            return true;
        }

    }



}