using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SocialWelfarre.Models;

namespace SocialWelfarre.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Apply()
        {
            return View();
        }
        public IActionResult Widget()
        {
            return View();
        }
        public IActionResult Dashboard()
        {
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public ActionResult VerifyCode(string code)
        {
            var storedCode = TempData["TwoFACode"]?.ToString();

            if (code == storedCode)
            {
                // Success - log in or continue
                return RedirectToAction("Index", "Home");

            }

            ViewBag.Error = "Invalid code.";
            return View();
        }

    }
}
