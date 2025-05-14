using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialWelfarre.Data;
using SocialWelfarre.Models;

namespace SocialWelfarre.Controllers
{
    public class StockIn_FoodPacksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockIn_FoodPacksController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.StockIn_FoodPacks.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Add_Stock")] StockIn_FoodPacks stockIn)
        {
            if (ModelState.IsValid)
            {
                if (stockIn.Add_Stock2 <= 0)
                {
                    ModelState.AddModelError("Add_Stock", "Stock must be greater than 0.");
                    return View(stockIn);
                }

                _context.Add(stockIn);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "FoodPackInventories");
            }
            return View(stockIn);
        }

        private bool StockInFoodPacksExists(int id)
        {
            return _context.StockIn_FoodPacks.Any(e => e.Id == id);
        }
    }
}