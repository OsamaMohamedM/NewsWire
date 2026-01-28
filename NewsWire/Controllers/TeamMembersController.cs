using Microsoft.AspNetCore.Mvc;
using NewsWire.Services;

namespace NewsWire.Controllers
{
    public class TeamMembersController : BaseController
    {
        public TeamMembersController(ILogger<TeamMembersController> logger) : base(logger)
        {
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                return HandleException(ex, nameof(Index));
            }
        }
    }
}
