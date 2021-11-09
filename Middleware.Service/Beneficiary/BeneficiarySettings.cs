using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.Beneficiary
{
    public class BeneficiarySettings
    {
        public string BaseUrl { get; set; }
        public string AppId { get; set; }
        public string AppKey { get; set; }
     //   public BeneficiaryEndpoints EndPoints { get; set; }
    }

    //public class BeneficiaryEndpoints
    //{
    //    public string AddPaymentBeneficiary { get; set; }
    //    public string GetPaymentBeneficiary { get; set; }
    //    public string DeletePaymentBeneficiary { get; set; }
    //    public string AddTransferBeneficiary { get; set; }
    //    public string GetTransferBeneficiary { get; set; }
    //    public string DeleteTransferBeneficiary { get; set; }
    //}
}
