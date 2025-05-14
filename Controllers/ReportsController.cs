using Microsoft.AspNetCore.Mvc;
using SocialWelfarre.Models;
using SocialWelfare.Models.ViewModels;
using System.Linq;
using SocialWelfarre.Data;

namespace SocialWelfarre.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ReportsController
        public ActionResult Index()
        {
            var model = new SocialWelfareReportViewModel
            {
                FoodPacks = _context.ApplicationFoodPack
                    .OrderByDescending(x => x.Id)
                    .ToList(),
                Indigencies = _context.Certificate_Of_Indigencies
                    .OrderByDescending(x => x.Id)
                    .ToList(),
                Consultations = _context.Consultations
                    .OrderByDescending(x => x.Id)
                    .ToList()
            };

            // Calculate summary statistics
            ViewBag.TotalFoodPacks = model.FoodPacks.Sum(x => x.Packs);
            ViewBag.TotalIndigencies = model.Indigencies.Count();
            ViewBag.TotalConsultations = model.Consultations.Count();
            ViewBag.ApprovedFoodPacks = model.FoodPacks.Count(x => x.Status == ActiveStatus.Approved);
            ViewBag.PendingConsultations = model.Consultations.Count(x => x.Consultation_status == ActiveStatus2.Pending);

            return View(model);
        }

        // ... (keep your other actions as they are)
    }
}