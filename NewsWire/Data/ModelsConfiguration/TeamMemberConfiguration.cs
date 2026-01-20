using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsWire.Models;

namespace NewsWire.Data.ModelsConfiguration
{
    public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
    {
        public void Configure(EntityTypeBuilder<TeamMember> builder)
        {
            builder.ToTable("TeamMembers");
            builder.HasKey(tm => tm.Id);
            builder.Property(tm => tm.Name)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(tm => tm.JobTitle)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(tm => tm.ImageUrl)
                   .HasMaxLength(500);
        }
    }
}