using Middleware.Core.DTO;
using Middleware.Service.DAO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Middleware.Service.DTOs
{
    public class TransferBeneficiary
    {
       
        public string Reference { get; set; }
        public string AccountName { get; set; }
        public string AccountId { get; set; }
        public string InstitutionCode { get; set; }
        public string InstitutionName { get; set; }
        public AccountType AccountType { get; set; }
        public string Alias { get; set; }
        public string AdditionalParams { get; set; }

        public bool IsValid(out string problemSource)
        {
            problemSource = string.Empty;
            if (string.IsNullOrEmpty(AccountId))
            {
                problemSource = "Account Id";
                return false;
            }
          //  if (string.IsNullOrWhiteSpace(InstitutionName))
          //  {
          //      problemSource = "Institution Name";
          //      return false;
          //  }
          //var accountNew=  Utilities.Util.GetonyNumbers(AccountId);

          //  if (accountNew.Length > 19)
          //  {
          //      var accnt = Utilities.Util.GetonyNumbers(AccountId);
          //       AccountId = accnt;
          //  }

            //if (AccountId.Any(c => char.IsDigit(c) == false))
            //{
            //    problemSource = "Account Id";
            //    return false;
            //}
            return true;
        }

    }
}