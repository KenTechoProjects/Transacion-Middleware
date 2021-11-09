using Middleware.Service.DTOs;
using System;
using System.ComponentModel.DataAnnotations;

namespace Middleware.Service.Onboarding
{
    public class ResetPinRequest
    {
        public Answer Answer { get; set; }
        public string NewPin { get; set; }
        public string ConfirmNewPin { get; set; }
    }
    public class PhoneNumberModel
    {
        [Required(ErrorMessage = "Phone number is Required!")]
        public string PhoneNumber { get; set; }
    }
    public class ResetPinRequest_ : PhoneNumberModel
    {

        [Range(1, long.MaxValue, ErrorMessage = "QuestionId is Required!")]
        public long QuestionId { get; set; }

        [Required(ErrorMessage = "Answer is Required!")]
        public string Answer { get; set; }

        public string Pin { get; set; }

        [Compare(nameof(Pin), ErrorMessage = "Pin Mismatch!")]
        public string ConfirmPin { get; set; }
    }
}
