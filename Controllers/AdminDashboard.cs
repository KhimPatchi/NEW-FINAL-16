using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SocialWelfarre.Controllers
{
    [Authorize(Roles = "Admin,Staff1,Staff2")]
    public class AdminDashboard : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
