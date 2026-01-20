using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsWire.Models
{
    public class ProfileViewModel
    {
        [Display(Name = "First Name")]
        [Required(ErrorMessage = "First Name is required")]
        [MaxLength(50, ErrorMessage = "First Name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(ErrorMessage = "Last Name is required")]
        [MaxLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
        public string LastName { get; set; }

        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Display(Name = "Current Profile Picture")]
        public string? CurrentPicturePath { get; set; }

        [Display(Name = "Profile Picture")]
        [NotMapped]
        public IFormFile? ProfileImage { get; set; }

        // Statistics
        public int TotalNewsCount { get; set; }
        public int TotalFavoriteCount { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class UserProfileDashboardViewModel
    {
        public ProfileViewModel Profile { get; set; }
        public List<NewsDisplayViewModel> UserNews { get; set; }
        public List<NewsDisplayViewModel> FavoriteNews { get; set; }
        public ProfileStatisticsViewModel Statistics { get; set; }
        public string ActiveTab { get; set; } = "profile";
    }

    public class ProfileStatisticsViewModel
    {
        public int TotalArticles { get; set; }
        public int PublishedThisMonth { get; set; }
        public int TotalViews { get; set; }
        public int TotalFavorites { get; set; }
        public List<CategoryStatistic> CategoryBreakdown { get; set; }
        public List<MonthlyActivity> ActivityChart { get; set; }
    }

    public class CategoryStatistic
    {
        public string CategoryName { get; set; }
        public int ArticleCount { get; set; }
        public double Percentage { get; set; }
    }

    public class MonthlyActivity
    {
        public string Month { get; set; }
        public int ArticleCount { get; set; }
    }
}