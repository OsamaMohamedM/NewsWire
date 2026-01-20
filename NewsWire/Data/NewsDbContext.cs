using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewsWire.Models;

namespace NewsWire.Data
{
    public class NewsDbContext : IdentityDbContext<User>
    {
        public NewsDbContext(DbContextOptions<NewsDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure UserFavorite relationships
            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.User)
                .WithMany()
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.News)
                .WithMany()
                .HasForeignKey(uf => uf.NewsId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure unique favorite per user per news
            modelBuilder.Entity<UserFavorite>()
                .HasIndex(uf => new { uf.UserId, uf.NewsId })
                .IsUnique();

            // Configure News-User relationship
            modelBuilder.Entity<News>()
                .HasOne(n => n.Author)
                .WithMany(u => u.news)
                .HasForeignKey(n => n.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NewsDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<News> News { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
    }
}