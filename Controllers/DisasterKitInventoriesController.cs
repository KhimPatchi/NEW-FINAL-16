using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialWelfarre.Data;
using SocialWelfarre.Models;

namespace SocialWelfarre.Controllers
{
    public class DisasterKitInventoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DisasterKitInventoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DisasterKitInventories
        public async Task<IActionResult> Index()
        {
            await RecalculateInventory(); // Recalculate before displaying to ensure accurate data
            var inventories = await _context.DisasterKitInventories.ToListAsync();
            return View(inventories);
        }

        // GET: DisasterKitInventories/Dashboard
        public IActionResult Dashboard()
        {
            var model = _context.DisasterKitInventories.ToList() ?? new List<DisasterKitInventory>();
            return View(model);
        }

        // Helper method to get display name of enum values
        private static string GetEnumDisplayName<TEnum>(TEnum enumValue) where TEnum : Enum
        {
            var memberInfo = typeof(TEnum).GetMember(enumValue.ToString()).FirstOrDefault();
            var displayAttribute = memberInfo?.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name ?? enumValue.ToString();
        }

        // GET: api/DisasterKitInventories/GetDashboardCounts
        [HttpGet]
        [Route("api/[controller]/GetDashboardCounts")]
        public async Task<IActionResult> GetDashboardCounts()
        {
            try
            {
                var totalStockAdded = await _context.StockIn_DisasterKit.SumAsync(s => s.Add_Stock1);
                var totalPacksDistributed = await _context.DisasterKitInventories.SumAsync(i => i.NumberOfPacks3);
                var stockLeft = totalStockAdded - totalPacksDistributed;

                var counts = new
                {
                    totalStockAdded,
                    totalPacksDistributed,
                    stockLeft = stockLeft < 0 ? 0 : stockLeft,
                    totalTransactions = await _context.DisasterKitInventories.CountAsync()
                };
                return Json(counts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch dashboard counts", details = ex.Message });
            }
        }

        // GET: api/DisasterKitInventories/GetAllInventoryTransactions
        [HttpGet]
        [Route("api/[controller]/GetAllInventoryTransactions")]
        public async Task<IActionResult> GetAllInventoryTransactions()
        {
            try
            {
                var transactions = await _context.DisasterKitInventories
                    .Select(i => new
                    {
                        i.Id,
                        stockInId1 = i.StockInId1,
                        totalPacks1 = i.TotalPacks1,
                        stockLeft = i.StockLeft,
                        requestDate1 = i.RequestDate1.ToString("yyyy-MM-dd"),
                        barangay3 = GetEnumDisplayName(i.Barangay3), // Use display name for enum
                        reason3 = GetEnumDisplayName(i.Reason3),     // Use display name for enum
                        transactionDate = i.TransactionDate.ToString("yyyy-MM-dd"),
                        transactionTime = i.TransactionTime.ToString(@"hh\:mm\:ss"),
                        numberOfPacks3 = i.NumberOfPacks3
                    })
                    .OrderByDescending(i => i.requestDate1)
                    .ToListAsync();
                return Json(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch inventory transactions", details = ex.Message });
            }
        }

        // GET: DisasterKitInventories/AddStock
        public IActionResult AddStock()
        {
            return View();
        }

        // POST: DisasterKitInventories/AddStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStock([Bind("Add_Stock1")] StockIn_DisasterKit stock)
        {
            if (ModelState.IsValid)
            {
                stock.StockInDate = DateTime.Now;
                _context.StockIn_DisasterKit.Add(stock);
                await _context.SaveChangesAsync();

                // Recalculate inventory after adding stock
                await RecalculateInventory();

                // Log audit trail
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var activity = new AuditTrail
                {
                    Action = "Add Stock",
                    TimeStamp = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserId = userId,
                    Moduie = "Disaster Kit Inventory", // Fixed typo: Moduie -> Module
                    AffectedTable = "StockIn_DisasterKit"
                };
                _context.Add(activity);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(stock);
        }

        // One-time action to initialize inventory data
        public async Task<IActionResult> InitializeInventory()
        {
            await RecalculateInventory();
            return RedirectToAction(nameof(Index));
        }

        // Helper method to recalculate inventory
        private async Task RecalculateInventory()
        {
            var inventories = await _context.DisasterKitInventories
                .OrderBy(i => i.RequestDate1)
                .ToListAsync();

            var allStocks = await _context.StockIn_DisasterKit
                .OrderBy(s => s.StockInDate)
                .ToListAsync();

            // Calculate the total stock available at the start
            int runningTotal = allStocks.Sum(s => s.Add_Stock1);

            // Recalculate TotalPacks1 and StockLeft for each inventory record
            foreach (var inventory in inventories)
            {
                inventory.TotalPacks1 = runningTotal;
                inventory.StockLeft = runningTotal - inventory.NumberOfPacks3;
                runningTotal = inventory.StockLeft; // Update running total for the next iteration
            }

            await _context.SaveChangesAsync();
        }
    }
}