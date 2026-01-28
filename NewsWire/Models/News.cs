namespace NewsWire.Models
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public string ImageUrl { get; set; }
        public DateTime PublishedAt { get; set; }

        public string Topic { get; set; }

        public Category Category { get; set; }
        public int CategoryId { get; set; }

        public CustomUser? Author { get; set; }
        public string? AuthorId { get; set; }
    }
}