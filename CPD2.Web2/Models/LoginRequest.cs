using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPD2.Web2.Models
{
     public struct LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public int? CustomerId { get; set; }
        public int CountryId { get; set; }
        public bool ResetFlag { get; set; }
    }
}



