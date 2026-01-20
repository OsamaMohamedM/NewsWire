using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsWire.Models;

namespace NewsWire.Data.ModelsConfiguration
{
    public class ContactUsConfiguration : IEntityTypeConfiguration<ContactUs>
    {
        public void Configure(EntityTypeBuilder<ContactUs> builder)
        {
            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(100); builder.Property(c => c.Email)
                  .IsRequired()
                  .HasMaxLength(100);
            builder.ToTable(t => t.HasCheckConstraint("CK_Customer_Email", "Email LIKE '%@%.%'"));

            builder.Property(c => c.Subject)
                   .HasMaxLength(300)
                   .IsRequired();
            builder.Property(c => c.Message)
                   .HasMaxLength(1500)
                   .IsRequired();
            builder.Property(c => c.Name)
                   .HasMaxLength(300)
                   .IsRequired();
        }
    }
}