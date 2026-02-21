using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsWire.Models
{
    public class News
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [MaxLength(10000, ErrorMessage = "Content cannot exceed 10000 characters")]
        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public DateTime PublishedAt { get; set; }

        [Required(ErrorMessage = "Topic is required")]
        [MaxLength(100, ErrorMessage = "Topic cannot exceed 100 characters")]
        public string Topic { get; set; }

        public Category Category { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        public CustomUser? Author { get; set; }
        public string? AuthorId { get; set; }

        [NotMapped]
        [DisplayName("Upload Image")]
        public IFormFile? ImageFile { get; set; }
    }
}