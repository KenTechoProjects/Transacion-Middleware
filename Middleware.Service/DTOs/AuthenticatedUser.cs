using System;
using System.Text.Json.Serialization;

namespace Middleware.Service.DTOs
{
    public class AuthenticatedUser
    {
        [JsonIgnore]
        public long Id { get; set; }
        public string WalletNumber { get; set; }
        public string UserName { get; set; }
        [JsonIgnore] 
        public string BankId { get; set; }
       
    }


    
    public class GetTranactionByDateRrange
    {
        
        public DateTime StartDate    { get; set; }
        public DateTime EndDate    { get; set; }
        public string TransactionIdentifier { get; set; }
      

    }
 
}
