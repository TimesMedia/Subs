using System.ComponentModel.DataAnnotations;
namespace Subs.MimsWeb.Models
{
    public class ContactRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string ContactNumber { get; set; }

        public string Message { get; set; }
    }

}