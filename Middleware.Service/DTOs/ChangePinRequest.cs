using Middleware.Service.Onboarding;
using System;
using System.ComponentModel.DataAnnotations;

namespace Middleware.Service.DTOs
{
    public class ChangePinRequest
    {
        public string OldPin { get; set; }
        public string NewPin { get; set; }
        public string ConfirmNewPin { get; set; }
    }

    //public class ChangePinRequest : PhoneNumberModel
    //{


    //    [Required(ErrorMessage = "Old pin is required!")]
    //    public string OldPin { get; set; }

    //    [Required(ErrorMessage = "New pin is required!")]

    //    public string NewPin { get; set; }

    //    [Compare(nameof(NewPin), ErrorMessage = "Pin Mismatch!")]
    //    public string ConfirmNewPin { get; set; }
    //}
}
