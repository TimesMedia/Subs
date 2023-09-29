using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Subs.MimsWeb.Models
{
    public class PayUAmount
    {
        [Required]
        public decimal Amount {get; set; }
    }
}