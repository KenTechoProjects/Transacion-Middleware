using System;
namespace Middleware.Service
{
    public class SystemSettings
    {
        public string CountryId { get; set; }
        public string BankCode { get; set; }
        public string WalletCode { get; set; }
        public int StatementOffset { get; set; }
        public string NarrationPrefix { get; set; }
        public string LogingPath { get; set; }
        public string DocumentDirectory { get; set; }
        public string SelfieDirectory { get; set; }
        public string SelfieBaseUrl{ get; set; }
        public string ImageFormat { get; set; }
        public string DocumentDaseUrl { get; set; }
        public string BaseUrl { get; set; }
        public string PhotoReturnedBaseUrl { get; set; }
        public string DocumentReturnedBaseUrl { get; set; }
        public FloatAccountEntrySettings WalletFundingAccount { get; set; }
        public FloatAccountEntrySettings WalletChargingAccount { get; set; }
        public int  CodeLength { get; set; }
        public int OtpDuration { get; set; }
        public int MaxDeviceCount { get; set; }
        public string ReviewProfile { get; set; }
        public int AccountNumberLength { get; set; }
        public int InvalidLoginCount { get; set; }
        public int ServiceTimeout { get; set; }     
        public bool IsTest { get; set; }
        public string EncryptionKey { get; set; }
    }
     
    public class FloatAccountEntrySettings
    {
        public string AccountNumber { get; set; }
        //  public string BranchCode { get; set; }
        // public string Narration { get; set; }
        
    }

}

