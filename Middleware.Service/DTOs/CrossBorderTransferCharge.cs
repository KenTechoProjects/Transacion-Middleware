using System;
namespace Middleware.Service.DTOs
{
    public class CrossBorderTransferCharge
    {

        public decimal SMSFee { get; set; }
        public decimal VATAmount { get; set; }
        public decimal TransactionFee { get; set; }

        public decimal TotalCharge
        {
            get
            {
                return SMSFee + VATAmount + TransactionFee;
            }
        }

        public CrossBorderTransferCharge(decimal transactionFee, decimal vatAmount, decimal smsFee)
        {
            TransactionFee = transactionFee;
            VATAmount = vatAmount;
            SMSFee = smsFee;
        }
    }
}
