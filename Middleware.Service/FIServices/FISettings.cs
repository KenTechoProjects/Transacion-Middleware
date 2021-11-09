using System;
using System.Collections.Generic;

namespace Middleware.Service.FIServices
{
    public class FISettings
    {
        public string BaseAddress { get; set; }
        public string InquiryPath { get; set; }
        public string LocalTransferPath { get; set; }
        public string AccountsPath { get; set; }
        public string AppId { get; set; }
        public string AppKey { get; set; }
        public string AccountDetailsPath { get; set; }
        public string InterBankInquiryPath { get; set; }
        public string BankPath { get; set; }
        public string BranchesPath { get; set; }
        public string InterBankTransferPath { get; set; }
        public string SubsidiariesListPath { get; set; }
        public string FGTRatePath { get; set; }
        public string FGTTransferPath { get; set; }
        public int Timeout { get; set; }
        public string[] PermittedSchemeTypes { get; set; }
        public string[] DebitableCurrencyCodes { get; set; }
        public SchemeTypeFilters SchemeTypeFilters { get; set; }
        public IDictionary<string,string> BranchCodeMappings { get; set; }
    }    

    public class SchemeTypeFilters
    {
        public string[] DebitRestricted { get; set; }
      //  public string[] CreditRestricted { get; set; }
    }
}