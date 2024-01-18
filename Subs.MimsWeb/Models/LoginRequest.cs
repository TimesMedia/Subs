namespace Subs.MimsWeb.Models
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int? CustomerId { get; set; }
        public int CountryId { get; set; }
        public bool ResetFlag { get; set; }
        public System.DateTime TimeStamp { get; set; }
    }
}