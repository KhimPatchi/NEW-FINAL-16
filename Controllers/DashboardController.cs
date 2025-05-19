using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SocialWelfarre.Controllers
{
    [Authorize(Roles = "Admin,Staff1,Staff2")]
    public class DashboardController : Controller
    {
        // GET: DashboardController
        public ActionResult AllDashboard()
        {
            return View();
        }

    }
}
