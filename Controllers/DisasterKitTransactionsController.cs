using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialWelfarre.Data;
using SocialWelfarre.Models;

namespace SocialWelfarre.Controllers
{
    public class DisasterKitTransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DisasterKitTransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DisasterKitTransactions
        public async Task<IActionResult> Index()
        {
            return View(await _context.DisasterKitTransactions.ToListAsync());
        }

        // GET: DisasterKitTransactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disasterKitTransaction = await _context.DisasterKitTransactions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (disasterKitTransaction == null)
            {
                return NotFound();
            }

            return View(disasterKitTransaction);
        }

        // GET: DisasterKitTransactions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DisasterKitTransactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Barangay3,Reason3,TransactionDate,TransactionTime,NumberOfPacks3")] DisasterKitTransaction disasterKitTransaction)
        {
            // Calculate TotalPacks1 based on all stock entries
            var allStocks = await _context.StockIn_DisasterKit.OrderBy(s => s.Id).ToListAsync();
            var latestInventory = await _context.DisasterKitInventories.OrderByDescending(i => i.RequestDate1).FirstOrDefaultAsync();
            var latestStock = allStocks.LastOrDefault();
            int totalPacks = 0;

            if (allStocks.Any())
            {
                totalPacks = allStocks.Sum(s => s.Add_Stock1); // Sum all stock entries
                if (latestInventory != null)
                {
                    totalPacks = latestInventory.StockLeft; // Use the latest StockLeft if inventory exists
                }
            }
            else
            {
                ModelState.AddModelError("", "No stock available. Please add stock first.");
                return View(disasterKitTransaction);
            }

            if (totalPacks < disasterKitTransaction.NumberOfPacks3)
            {
                ModelState.AddModelError("NumberOfPacks3", $"Insufficient stock available. Available stock: {totalPacks}, Requested: {disasterKitTransaction.NumberOfPacks3}");
                return View(disasterKitTransaction);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Add(disasterKitTransaction);
            await _context.SaveChangesAsync();

            // Create DisasterKitInventory record
            var inventory = new DisasterKitInventory
            {
                StockInId1 = latestStock?.Id ?? 0,
                TotalPacks1 = totalPacks,
                RequestDate1 = DateTime.Now,
                Barangay3 = disasterKitTransaction.Barangay3,
                Reason3 = disasterKitTransaction.Reason3,
                TransactionDate = disasterKitTransaction.TransactionDate,
                TransactionTime = disasterKitTransaction.TransactionTime,
                NumberOfPacks3 = disasterKitTransaction.NumberOfPacks3
            };
            _context.DisasterKitInventories.Add(inventory);
            await _context.SaveChangesAsync();

            // Log audit trail
            var activity = new AuditTrail
            {
                Action = "Create",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Disaster Kit",
                AffectedTable = "Disaster Kit"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: DisasterKitTransactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disasterKitTransaction = await _context.DisasterKitTransactions.FindAsync(id);
            if (disasterKitTransaction == null)
            {
                return NotFound();
            }
            return View(disasterKitTransaction);
        }

        // POST: DisasterKitTransactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Barangay3,Reason3,TransactionDate,TransactionTime,NumberOfPacks3")] DisasterKitTransaction disasterKitTransaction)
        {
            if (id != disasterKitTransaction.Id)
            {
                return NotFound();
            }

            try
            {
                var existingTransaction = await _context.DisasterKitTransactions.FindAsync(id);
                if (existingTransaction == null)
                {
                    return NotFound();
                }

                // Fetch all stock entries and the latest inventory (excluding the current transaction)
                var allStocks = await _context.StockIn_DisasterKit.OrderBy(s => s.Id).ToListAsync();
                var latestInventory = await _context.DisasterKitInventories
                    .Where(i => i.Id != id) // Exclude the current transaction's inventory record
                    .OrderByDescending(i => i.RequestDate1)
                    .FirstOrDefaultAsync();
                int totalPacks = 0;

                if (allStocks.Any())
                {
                    totalPacks = allStocks.Sum(s => s.Add_Stock1); // Sum all stock entries
                    if (latestInventory != null)
                    {
                        totalPacks = latestInventory.StockLeft; // Use the latest StockLeft if inventory exists
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No stock available. Please add stock first.");
                    return View(disasterKitTransaction);
                }

                // Check stock if NumberOfPacks3 changes
                if (existingTransaction.NumberOfPacks3 != disasterKitTransaction.NumberOfPacks3)
                {
                    if (totalPacks < disasterKitTransaction.NumberOfPacks3)
                    {
                        ModelState.AddModelError("NumberOfPacks3", $"Insufficient stock available. Available stock: {totalPacks}, Requested: {disasterKitTransaction.NumberOfPacks3}");
                        return View(disasterKitTransaction);
                    }
                }

                // Update transaction fields
                existingTransaction.Barangay3 = disasterKitTransaction.Barangay3;
                existingTransaction.Reason3 = disasterKitTransaction.Reason3;
                existingTransaction.TransactionDate = disasterKitTransaction.TransactionDate;
                existingTransaction.TransactionTime = disasterKitTransaction.TransactionTime;
                existingTransaction.NumberOfPacks3 = disasterKitTransaction.NumberOfPacks3;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Update(existingTransaction);
                await _context.SaveChangesAsync();

                // Update or create DisasterKitInventory
                var inventory = await _context.DisasterKitInventories.FirstOrDefaultAsync(i => i.Id == existingTransaction.Id);
                var latestStock = allStocks.LastOrDefault();

                if (inventory != null)
                {
                    inventory.StockInId1 = latestStock?.Id ?? 0;
                    inventory.TotalPacks1 = totalPacks;
                    inventory.RequestDate1 = DateTime.Now;
                    inventory.Barangay3 = existingTransaction.Barangay3;
                    inventory.Reason3 = existingTransaction.Reason3;
                    inventory.TransactionDate = existingTransaction.TransactionDate;
                    inventory.TransactionTime = existingTransaction.TransactionTime;
                    inventory.NumberOfPacks3 = existingTransaction.NumberOfPacks3;
                    _context.Update(inventory);
                }
                else
                {
                    inventory = new DisasterKitInventory
                    {
                        StockInId1 = latestStock?.Id ?? 0,
                        TotalPacks1 = totalPacks,
                        RequestDate1 = DateTime.Now,
                        Barangay3 = existingTransaction.Barangay3,
                        Reason3 = existingTransaction.Reason3,
                        TransactionDate = existingTransaction.TransactionDate,
                        TransactionTime = existingTransaction.TransactionTime,
                        NumberOfPacks3 = existingTransaction.NumberOfPacks3
                    };
                    _context.DisasterKitInventories.Add(inventory);
                }
                await _context.SaveChangesAsync();

                // Update subsequent inventories
                var subsequentInventories = await _context.DisasterKitInventories
                    .Where(i => i.RequestDate1 > inventory.RequestDate1)
                    .OrderBy(i => i.RequestDate1)
                    .ToListAsync();

                int runningTotal = inventory.StockLeft;
                foreach (var subsequent in subsequentInventories)
                {
                    subsequent.TotalPacks1 = runningTotal;
                    runningTotal = subsequent.StockLeft;
                }
                await _context.SaveChangesAsync();

                // Log audit trail
                var activity = new AuditTrail
                {
                    Action = "Edit",
                    TimeStamp = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserId = userId,
                    Moduie = "Disaster Kit Transaction",
                    AffectedTable = "DisasterKitTransaction"
                };
                _context.Add(activity);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DisasterKitTransactionExists(disasterKitTransaction.Id))
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

        // GET: DisasterKitTransactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var disasterKitTransaction = await _context.DisasterKitTransactions
                .FirstOrDefaultAsync(m => m.Id == id);
            if (disasterKitTransaction == null)
            {
                return NotFound();
            }

            return View(disasterKitTransaction);
        }

        // POST: DisasterKitTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var disasterKitTransaction = await _context.DisasterKitTransactions.FindAsync(id);
            if (disasterKitTransaction != null)
            {
                // Delete related DisasterKitInventory
                var inventory = await _context.DisasterKitInventories.FirstOrDefaultAsync(i => i.Id == id);
                if (inventory != null)
                {
                    _context.DisasterKitInventories.Remove(inventory);
                }

                _context.DisasterKitTransactions.Remove(disasterKitTransaction);
                await _context.SaveChangesAsync();

                // Update subsequent inventories after deletion
                var remainingInventories = await _context.DisasterKitInventories
                    .OrderBy(i => i.RequestDate1)
                    .ToListAsync();

                var allStocks = await _context.StockIn_DisasterKit.OrderBy(s => s.Id).ToListAsync();
                int runningTotal = allStocks.Any() ? allStocks.Sum(s => s.Add_Stock1) : 0;

                foreach (var inv in remainingInventories)
                {
                    inv.TotalPacks1 = runningTotal;
                    runningTotal = inv.StockLeft;
                }
                await _context.SaveChangesAsync();
            }

            // Log audit trail
            var activity = new AuditTrail
            {
                Action = "Delete",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Disaster Kit Transaction",
                AffectedTable = "DisasterKitTransaction"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: DisasterKitTransactions/RecalculateInventory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecalculateInventory()
        {
            var inventories = await _context.DisasterKitInventories
                .OrderBy(i => i.RequestDate1)
                .ToListAsync();

            if (!inventories.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var allStocks = await _context.StockIn_DisasterKit.OrderBy(s => s.Id).ToListAsync();
            if (!allStocks.Any())
            {
                ModelState.AddModelError("", "No stock available. Please add stock first.");
                return RedirectToAction("Index", "DisasterKitInventories");
            }

            int runningTotal = allStocks.Sum(s => s.Add_Stock1);

            foreach (var inventory in inventories)
            {
                inventory.TotalPacks1 = runningTotal;
                runningTotal = inventory.StockLeft;
            }

            await _context.SaveChangesAsync();

            // Log audit trail
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var activity = new AuditTrail
            {
                Action = "Recalculate Inventory",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Disaster Kit Inventory",
                AffectedTable = "DisasterKitInventories"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "DisasterKitInventories");
        }

        private bool DisasterKitTransactionExists(int id)
        {
            return _context.DisasterKitTransactions.Any(e => e.Id == id);
        }
    }
}