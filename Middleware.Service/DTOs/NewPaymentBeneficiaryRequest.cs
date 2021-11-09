using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.DTOs
{
    public class NewPaymentBeneficiaryRequest
    {
        public PaymentBeneficiary Beneficiary { get; set; }
        public Answer Answer { get; set; }

        public bool IsValid(out string problemSource)
        {
            if (!Beneficiary.IsValid(out problemSource))
            {
                return false;
            }
            return true;
        }
    }
    public class RemovePaymentBeneficiaryPaymentRequest
    {
        public long BeneficiaryId { get; set; }
  
        public Answer Answer { get; set; }

        public bool IsValid(out string problemSource)
        {
            if (BeneficiaryId <= 0)
            {
                problemSource = "BeneficiaryId";
                return false;
            }


            if (Answer.QuestionID <= 0)
            {
                problemSource = "QuestionID ";
                return false;
            }

           
            if (string.IsNullOrWhiteSpace(Answer.Response))
            {
                problemSource = "Response ";
                return false;
            }
            problemSource = "";
            return true;
        }
    }


    public class NewPaymentBeneficiaryPaymentRequest
    {
        public AirTimeBenefitiarySingle Beneficiary { get; set; }
        public Answer Answer { get; set; }

        public bool IsValid(out string problemSource)
        {
            if (!Beneficiary.IsValid(out problemSource))
            {
                return false;
            }
            return true;
        }
    }



}
