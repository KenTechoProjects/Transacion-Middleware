using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Middleware.Service.DTOs
{
    public class AirTimeBenefitiary
    {


        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public string ReferenceNumber { get; set; }
 
        public string BeneficiaryNumber { get; set; }
   
        public string BeneficiaryName { get; set; }
        [JsonIgnore]
        public string CustomerId { get; set; }
         
        public long BillerCode { get; set; }
   
        public string BillerName { get; set; }
       
        public string Alias { get; set; }
        [JsonIgnore]
        public PaymentType PaymentType { get; set; }
        [JsonIgnore]
        public string CountryId { get; set; }
        [JsonIgnore]
        public string WalletNumber { get; set; }
        [JsonIgnore]
        public bool IsDeleted { get; set; }
        [JsonIgnore]
        public bool IsActive { get; set; }


        public bool IsValid(out string source)
        {
            if ( BillerCode<=0)
            {
                source = "BillerCode";

                return false;

            }
            if (string.IsNullOrWhiteSpace(BeneficiaryName))
            {
                source = "BeneficiaryName ";
                return false;
            }
            

            if (string.IsNullOrWhiteSpace(BillerName))
            {
                source = "BillerName ";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(BillerName))
            {
                source = "BillerName ";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(CustomerId))
            {
                source = "CustomerId ";
                return false;
            }

            if (string.IsNullOrWhiteSpace(WalletNumber))
            {
                source = "WalletNumber ";
                return false;
            }


            if (string.IsNullOrWhiteSpace(BeneficiaryNumber))
            {
                source = "BeneficiaryNumber ";
                return false;
            }

            if (string.IsNullOrWhiteSpace(CountryId))
            {
                source = "CountryId ";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Alias))
            {
                source = "Alias";
                return false;
            }
            source = "";
            return true;
        }
    }

    public class AirTimeBenefitiaryResponse
    {


    
        public long Id { get; set; }
   
        public string ReferenceNumber { get; set; }
     
        public string CustomerId { get; set; }
      
        public string BeneficiaryNumber { get; set; }
       
        public string BeneficiaryName { get; set; }
        public long BillerCode { get; set; }
       
        public string BillerName { get; set; }

        public string Alias { get; set; }
   
        public PaymentType PaymentType { get; set; }
 
        public string CountryId { get; set; }
 
        public string WalletNumber { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }


       
    }

   public class AirTimeBenefitiarySingle
    {


        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public string ReferenceNumber { get; set; }
        [JsonIgnore]
        public string CustomerId { get; set; }
     
        public long BillerCode { get; set; }
     
        public string BillerName { get; set; }
     
        public string Alias { get; set; }
     
        public PaymentType PaymentType { get; set; }
        [JsonIgnore]
        public string CountryId { get; set; }
  
        public string BeneficiaryNumber { get; set; }
        
       
        public string BeneficiaryName { get; set; }
        [JsonIgnore]
        public string WalletNumber { get; set; }
        [JsonIgnore]
        public bool IsDeleted { get; set; }
        [JsonIgnore]
        public bool IsActive { get; set; }


        public bool IsValid(out string source)
        {
            if (BillerCode <= 0)
            {
                source = "BillerCode";

                return false;

            }

            if (string.IsNullOrWhiteSpace(BillerName))
            {
                source = "BillerName ";
                return false;
            }

            if (string.IsNullOrWhiteSpace(CustomerId))
            {
                source = "CustomerId ";
                return false;
            }

            if (string.IsNullOrWhiteSpace(WalletNumber))
            {
                source = "WalletNumber ";
                return false;
            }

            if (string.IsNullOrWhiteSpace(CountryId))
            {
                source = "CountryId ";
                return false;
            }
            if (string.IsNullOrWhiteSpace(Alias))
            {
                source = "Alias";
                return false;
            }
            source = "";
            return true;
        }
    }

}
