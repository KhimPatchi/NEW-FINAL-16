using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SocialWelfarre.Data;
using SocialWelfarre.Models;

namespace SocialWelfarre.Controllers
{
    public class FoodPackInventoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FoodPackInventoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ViewModel for combining FoodPackInventory and ApplicationFoodPack data
        public class FoodPackInventoryViewModel
        {
            public int Id { get; set; }
            public string FullName { get; set; }
            public int Packs { get; set; }
            public Barangay? Barangay { get; set; }
            public Reason? Reason { get; set; }
            public DateTime RequestDate { get; set; }
            public int TotalPacks { get; set; }
            public int StockLeft { get; set; }
        }

        [Authorize(Roles = "Admin,Staff1,Staff2")]
        // GET: FoodPackInventory
        public async Task<IActionResult> Index()
        {
            var inventories = await _context.FoodPackInventories
                .Join(
                    _context.ApplicationFoodPack.Where(a => a.Status == ActiveStatus.Approved),
                    inventory => inventory.ApplicationFoodPackId,
                    application => application.Id,
                    (inventory, application) => new FoodPackInventoryViewModel
                    {
                        Id = inventory.Id,
                        FullName = application.FullName,
                        Packs = inventory.Packs,
                        Barangay = inventory.Barangay,
                        Reason = application.Reason,
                        RequestDate = inventory.RequestDate,
                        TotalPacks = inventory.TotalPacks,
                        StockLeft = inventory.StockLeft
                    }
                )
                .ToListAsync();

            var stockHistory = await _context.StockIn_FoodPacks
                .OrderByDescending(s => s.Restock_DateTime2)
                .ToListAsync();

            return View(Tuple.Create(inventories.AsEnumerable(), stockHistory.AsEnumerable()));
        }

        [Authorize(Roles = "Admin,Staff1,Staff2")]
        public IActionResult Dashboard()
        {
            var model = _context.FoodPackInventories.ToList() ?? new List<FoodPackInventory>();
            return View(model);
        }

        // GET: FoodPackInventory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.FoodPackInventories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        // GET: FoodPackInventory/AddStock
        public IActionResult AddStock()
        {
            return View();
        }

        // POST: FoodPackInventory/AddStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStock([Bind("Add_Stock2")] StockIn_FoodPacks stock)
        {
            if (ModelState.IsValid)
            {
                stock.Restock_DateTime2 = DateTime.Now;
                _context.StockIn_FoodPacks.Add(stock);
                await _context.SaveChangesAsync();

                // Recalculate inventory
                var inventories = await _context.FoodPackInventories
                    .OrderBy(i => i.RequestDate)
                    .ToListAsync();

                var allStocks = await _context.StockIn_FoodPacks
                    .OrderBy(s => s.Restock_DateTime2)
                    .ToListAsync();

                int runningTotal = allStocks.Sum(s => s.Add_Stock2);

                foreach (var inventory in inventories)
                {
                    inventory.TotalPacks = runningTotal;
                    inventory.StockLeft = runningTotal - inventory.Packs;
                    runningTotal = inventory.StockLeft;
                }

                await _context.SaveChangesAsync();

                // Log audit trail
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var activity = new AuditTrail
                {
                    Action = "Add Stock",
                    TimeStamp = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserId = userId,
                    Moduie = "Food Pack Inventory",
                    AffectedTable = "StockIn_FoodPacks"
                };
                _context.Add(activity);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(stock);
        }

        // GET: FoodPackInventory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.FoodPackInventories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventory == null)
            {
                return NotFound();
            }

            // Populate StockInId dropdown
            ViewBag.StockInId = new SelectList(
                _context.StockIn_FoodPacks
                    .Select(s => new { s.Id, Display = $"Stock #{s.Id} - {s.Add_Stock2} packs ({s.Restock_DateTime2})" }),
                "Id",
                "Display",
                inventory.StockInId
            );

            return View(inventory);
        }

        // POST: FoodPackInventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ApplicationFoodPackId,StockInId,Packs,Barangay,RequestDate")] FoodPackInventory inventory)
        {
            if (id != inventory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check stock availability
                    var allStocks = await _context.StockIn_FoodPacks
                        .OrderBy(s => s.Restock_DateTime2)
                        .ToListAsync();
                    var latestInventory = await _context.FoodPackInventories
                        .Where(i => i.Id != id)
                        .OrderByDescending(i => i.RequestDate)
                        .FirstOrDefaultAsync();

                    int totalPacks = allStocks.Any() ? allStocks.Sum(s => s.Add_Stock2) : 0;
                    if (latestInventory != null)
                    {
                        totalPacks = latestInventory.StockLeft;
                    }

                    if (totalPacks < inventory.Packs)
                    {
                        ModelState.AddModelError("Packs", $"Insufficient stock available. Available stock: {totalPacks}, Requested: {inventory.Packs}");
                        ViewBag.StockInId = new SelectList(
                            _context.StockIn_FoodPacks
                                .Select(s => new { s.Id, Display = $"Stock #{s.Id} - {s.Add_Stock2} packs ({s.Restock_DateTime2})" }),
                            "Id",
                            "Display",
                            inventory.StockInId
                        );
                        return View(inventory);
                    }

                    // Update inventory
                    var existingInventory = await _context.FoodPackInventories.FindAsync(id);
                    if (existingInventory == null)
                    {
                        return NotFound();
                    }

                    existingInventory.StockInId = inventory.StockInId;
                    existingInventory.Packs = inventory.Packs;
                    existingInventory.Barangay = inventory.Barangay;
                    existingInventory.RequestDate = inventory.RequestDate;
                    existingInventory.TotalPacks = totalPacks;
                    existingInventory.StockLeft = totalPacks - inventory.Packs;

                    _context.Update(existingInventory);
                    await _context.SaveChangesAsync();

                    // Recalculate subsequent inventories
                    var subsequentInventories = await _context.FoodPackInventories
                        .Where(i => i.RequestDate > existingInventory.RequestDate)
                        .OrderBy(i => i.RequestDate)
                        .ToListAsync();

                    int runningTotal = existingInventory.StockLeft;
                    foreach (var subsequent in subsequentInventories)
                    {
                        subsequent.TotalPacks = runningTotal;
                        subsequent.StockLeft = runningTotal - subsequent.Packs;
                        runningTotal = subsequent.StockLeft;
                    }

                    await _context.SaveChangesAsync();

                    // Log audit trail
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var activity = new AuditTrail
                    {
                        Action = "Edit Inventory",
                        TimeStamp = DateTime.Now,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserId = userId,
                        Moduie = "Food Pack Inventory",
                        AffectedTable = "FoodPackInventories"
                    };
                    _context.Add(activity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodPackInventoryExists(inventory.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdown on validation failure
            ViewBag.StockInId = new SelectList(
                _context.StockIn_FoodPacks
                    .Select(s => new { s.Id, Display = $"Stock #{s.Id} - {s.Add_Stock2} packs ({s.Restock_DateTime2})" }),
                "Id",
                "Display",
                inventory.StockInId
            );
            return View(inventory);
        }

        // GET: FoodPackInventory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.FoodPackInventories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        // POST: FoodPackInventory/DeleteConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventory = await _context.FoodPackInventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            _context.FoodPackInventories.Remove(inventory);
            await _context.SaveChangesAsync();

            // Recalculate inventory
            var remainingInventories = await _context.FoodPackInventories
                .OrderBy(i => i.RequestDate)
                .ToListAsync();

            var allStocks = await _context.StockIn_FoodPacks
                .OrderBy(s => s.Restock_DateTime2)
                .ToListAsync();

            int runningTotal = allStocks.Any() ? allStocks.Sum(s => s.Add_Stock2) : 0;

            foreach (var inv in remainingInventories)
            {
                inv.TotalPacks = runningTotal;
                inv.StockLeft = runningTotal - inv.Packs;
                runningTotal = inv.StockLeft;
            }

            await _context.SaveChangesAsync();

            // Log audit trail
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var activity = new AuditTrail
            {
                Action = "Delete Inventory",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Food Pack Inventory",
                AffectedTable = "FoodPackInventories"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool FoodPackInventoryExists(int id)
        {
            return _context.FoodPackInventories.Any(e => e.Id == id);
        }
    }
}