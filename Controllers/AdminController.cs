using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace SocialWelfarre.Controllers
{
    [Authorize(Roles = "Admin,Staff1,Staff2")]
    public class AdminController : Controller
    {

        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult _DashboardLayout()
        {
            return View();
        }
        public IActionResult CertificateOfIndigencyDashboard()
        {
            return View();
        }
        public IActionResult ConsultationDashboard()
        {
            return View();
        }
        public IActionResult DisasterKitDashboard()
        {
            return View();
        }
    }
    }