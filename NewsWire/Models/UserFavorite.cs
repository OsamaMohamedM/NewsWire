using System.ComponentModel.DataAnnotations;

namespace NewsWire.Models
{
    public class UserFavorite
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public CustomUser User { get; set; }
        
        [Required]
        public int NewsId { get; set; }
        public News News { get; set; }
        
        public DateTime FavoritedAt { get; set; } = DateTime.UtcNow;
    }
}