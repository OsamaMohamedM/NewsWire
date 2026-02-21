using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace NewsWire.Models
{
    public class CustomUser : IdentityUser
    {
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public ICollection<News> News { get; set; } = new List<News>();
    }
}