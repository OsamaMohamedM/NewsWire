using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NewsWire.Models
{
    public class User : IdentityUser
    {
      
        [MaxLength(50)]
        public string? FirstName { get; set; }
        [MaxLength(50)]
        public string? LastName { get; set; }

        public string? profilePictureUrl { get; set; }
        public ICollection<News> news { get; set; }
    }
}