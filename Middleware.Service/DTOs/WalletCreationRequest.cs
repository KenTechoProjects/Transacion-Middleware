using System;
namespace Middleware.Service.DTOs
{
    public class WalletCreationRequest
    {
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string BirthDate { get; set; }
        public string Gender { get; set; }
        public string Nationality { get; set; }
        public string Address { get; set; }
        public string MotherMaidenName { get; set; }
        public string State { get; set; }
        public string LGA { get; set; }
    }
}
