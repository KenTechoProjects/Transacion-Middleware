using System;
using System.Collections.Generic;
using System.Text;

namespace Middleware.Service.InterWalletServices
{
    public class InterWalletTransferSettings
    {
        public string BaseAddress { get; set; }
        public string AppId { get; set; }
        public string AppKey { get; set; }
        public string CountryId { get; set; }
        public int RequestTimeout { get; set; }
        public EndPoints EndPoints { get; set; }
        public string FBNWalletSchemeCode { get; set; }
        public decimal InterwalletFeePercentage { get; set; }
    }

    public class EndPoints
    {
        public string NameEnquiry { get; set; }
        public string WalletSchemes { get; set; }
        public string TransactionStatus { get; set; }
        public string Transfer { get; set; }
    }
}
