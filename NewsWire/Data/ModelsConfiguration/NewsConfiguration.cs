using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsWire.Models;

namespace NewsWire.Data.ModelsConfiguration
{
    public class NewsConfiguration : IEntityTypeConfiguration<News>
    {
        public void Configure(EntityTypeBuilder<News> builder)
        {
            builder.ToTable("News");
            builder.HasKey(n => n.Id);
            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(n => n.Content)
                .IsRequired();

            builder.Property(n => n.ImageUrl)
                .HasMaxLength(500)
                .IsRequired();
            builder.Property(n => n.PublishedAt)
                   .HasDefaultValueSql("GETDATE()")
                   ;

            builder.Property(n => n.Topic)
                  .HasMaxLength(100)
                  .IsRequired();

            builder.HasOne(n => n.Category)
                   .WithMany(c => c.NewsItems)
                   .HasConstraintName("FK_News_Category")
                   .HasForeignKey(n => n.CategoryId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}