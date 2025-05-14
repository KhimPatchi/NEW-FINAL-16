using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialWelfarre.Data;
using SocialWelfarre.Models;

namespace SocialWelfarre.Controllers
{
    public class StockIn_DisasterKitController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StockIn_DisasterKitController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StockIn_DisasterKit
        public async Task<IActionResult> Index()
        {
            return View(await _context.StockIn_DisasterKit.ToListAsync());
        }

        // GET: StockIn_DisasterKit/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockIn_DisasterKit = await _context.StockIn_DisasterKit
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stockIn_DisasterKit == null)
            {
                return NotFound();
            }

            return View(stockIn_DisasterKit);
        }

        // GET: StockIn_DisasterKit/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: StockIn_DisasterKit/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Add_Stock1,StockInDate")] StockIn_DisasterKit stockIn_DisasterKit)
        {
            if (stockIn_DisasterKit.Add_Stock1 <= 0)
            {
                ModelState.AddModelError("Add_Stock1", "Stock must be greater than 0.");
                return View(stockIn_DisasterKit);
            }

            stockIn_DisasterKit.StockInDate = DateTime.Now;
            _context.Add(stockIn_DisasterKit);
            await _context.SaveChangesAsync();

            // Recalculate inventory after adding stock
            await RecalculateInventory();

            return RedirectToAction("Index", "DisasterKitInventories");
        }

        // GET: StockIn_DisasterKit/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockIn_DisasterKit = await _context.StockIn_DisasterKit.FindAsync(id);
            if (stockIn_DisasterKit == null)
            {
                return NotFound();
            }
            return View(stockIn_DisasterKit);
        }

        // POST: StockIn_DisasterKit/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Add_Stock1,StockInDate")] StockIn_DisasterKit stockIn_DisasterKit)
        {
            if (id != stockIn_DisasterKit.Id)
            {
                return NotFound();
            }

            if (stockIn_DisasterKit.Add_Stock1 <= 0)
            {
                ModelState.AddModelError("Add_Stock1", "Stock must be greater than 0.");
                return View(stockIn_DisasterKit);
            }

            try
            {
                stockIn_DisasterKit.StockInDate = DateTime.Now; // Update timestamp on edit
                _context.Update(stockIn_DisasterKit);
                await _context.SaveChangesAsync();

                // Recalculate inventory after editing stock
                await RecalculateInventory();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockIn_DisasterKitExists(stockIn_DisasterKit.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Index", "DisasterKitInventories");
        }

        // GET: StockIn_DisasterKit/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stockIn_DisasterKit = await _context.StockIn_DisasterKit
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stockIn_DisasterKit == null)
            {
                return NotFound();
            }

            return View(stockIn_DisasterKit);
        }

        // POST: StockIn_DisasterKit/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stockIn_DisasterKit = await _context.StockIn_DisasterKit.FindAsync(id);
            if (stockIn_DisasterKit != null)
            {
                _context.StockIn_DisasterKit.Remove(stockIn_DisasterKit);
                await _context.SaveChangesAsync();

                // Recalculate inventory after deleting stock
                await RecalculateInventory();
            }

            return RedirectToAction("Index", "DisasterKitInventories");
        }

        private async Task RecalculateInventory()
        {
            var inventories = await _context.DisasterKitInventories
                .OrderBy(i => i.RequestDate1)
                .ToListAsync();

            if (!inventories.Any())
            {
                return;
            }

            var allStocks = await _context.StockIn_DisasterKit
                .OrderBy(s => s.StockInDate)
                .ToListAsync();

            if (!allStocks.Any())
            {
                foreach (var inv in inventories)
                {
                    inv.TotalPacks1 = 0;
                    inv.StockInId1 = 0;
                }
                await _context.SaveChangesAsync();
                return;
            }

            int runningTotal = 0;
            int stockIndex = 0;

            foreach (var inventory in inventories)
            {
                // Add stock entries that occurred before this inventory's RequestDate1
                while (stockIndex < allStocks.Count && allStocks[stockIndex].StockInDate <= inventory.RequestDate1)
                {
                    runningTotal += allStocks[stockIndex].Add_Stock1;
                    stockIndex++;
                }

                inventory.TotalPacks1 = runningTotal;
                inventory.StockInId1 = stockIndex > 0 ? allStocks[stockIndex - 1].Id : 0;
                runningTotal -= inventory.NumberOfPacks3;
            }

            await _context.SaveChangesAsync();
        }

        private bool StockIn_DisasterKitExists(int id)
        {
            return _context.StockIn_DisasterKit.Any(e => e.Id == id);
        }
    }
}