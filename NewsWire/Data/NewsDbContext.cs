using Microsoft.EntityFrameworkCore;
using NewsWire.Models;

namespace NewsWire.Data
{
    public class NewsDbContext : DbContext
    {
        public NewsDbContext(DbContextOptions<NewsDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NewsDbContext).Assembly);
        }

        public DbSet<News> News { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
    }
}