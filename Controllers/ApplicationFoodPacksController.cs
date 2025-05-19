using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialWelfarre.Data;
using SocialWelfarre.Models;
using SocialWelfarre.Services;
using System.Text.Json; // Added for JsonSerializer

namespace SocialWelfarre.Controllers
{
    [Authorize(Roles = "Admin,Staff1,Staff2")]
    public class ApplicationFoodPacksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly SmsService _smsService;

        public ApplicationFoodPacksController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, SmsService smsService)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _smsService = smsService;
        }

        [Authorize(Roles = "Admin,Staff1,Staff2")]
        public async Task<IActionResult> Index()
        {
            var applications = await _context.ApplicationFoodPack.ToListAsync();
            return View(applications);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff1,Staff2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromIndex([Bind("FirstName,MiddleName,LastName,Packs,Age,Barangay,Address,ContactNumber,Reason,Status")] ApplicationFoodPack applicationFoodPack, IFormFile brgyCertFile, IFormFile validIdFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var maxFileSize = 5 * 1024 * 1024;

            var allStocks = await _context.StockIn_FoodPacks.OrderBy(s => s.Id).ToListAsync();
            var latestInventory = await _context.FoodPackInventories.OrderByDescending(i => i.RequestDate).FirstOrDefaultAsync();
            int totalPacks = 0;

            if (allStocks.Any())
            {
                totalPacks = allStocks.Sum(s => s.Add_Stock2);
                if (latestInventory != null)
                {
                    totalPacks = latestInventory.StockLeft;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "No stock available. Please add stock first.";
                return RedirectToAction(nameof(Index));
            }

            if (totalPacks < applicationFoodPack.Packs)
            {
                TempData["ErrorMessage"] = $"Insufficient stock available. Available stock: {totalPacks}, Requested: {applicationFoodPack.Packs}";
                return RedirectToAction(nameof(Index));
            }

            if (brgyCertFile != null && brgyCertFile.Length > 0)
            {
                var extension = Path.GetExtension(brgyCertFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("brgyCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Barangay Certificate.");
                    return View("Index", await _context.ApplicationFoodPack.ToListAsync());
                }
                if (brgyCertFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("brgyCertFile", "Barangay Certificate file size cannot exceed 5 MB.");
                    return View("Index", await _context.ApplicationFoodPack.ToListAsync());
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "brgy_certs");
                Directory.CreateDirectory(directoryPath);

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await brgyCertFile.CopyToAsync(stream);
                }

                applicationFoodPack.Brgy_Cert = fileName;
                applicationFoodPack.Brgy_Cert_Path = "/Uploads/brgy_certs/" + fileName;
            }
            else
            {
                ModelState.AddModelError("brgyCertFile", "Please upload a Barangay Certificate.");
                return View("Index", await _context.ApplicationFoodPack.ToListAsync());
            }

            if (validIdFile != null && validIdFile.Length > 0)
            {
                var extension = Path.GetExtension(validIdFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("validIdFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Valid ID.");
                    return View("Index", await _context.ApplicationFoodPack.ToListAsync());
                }
                if (validIdFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("validIdFile", "Valid ID file size cannot exceed 5 MB.");
                    return View("Index", await _context.ApplicationFoodPack.ToListAsync());
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "valid_ids");
                Directory.CreateDirectory(directoryPath);

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await validIdFile.CopyToAsync(stream);
                }

                applicationFoodPack.Valid_ID = fileName;
                applicationFoodPack.Valid_ID_Path = "/Uploads/valid_ids/" + fileName;
            }
            else
            {
                ModelState.AddModelError("validIdFile", "Please upload a Valid ID.");
                return View("Index", await _context.ApplicationFoodPack.ToListAsync());
            }

            applicationFoodPack.Status = ActiveStatus.Approved;
            applicationFoodPack.RequestDate = DateTime.Now;

            var latestStock = allStocks.LastOrDefault();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Add(applicationFoodPack);
            await _context.SaveChangesAsync();

            var activity = new AuditTrail
            {
                Action = "Create",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Food Packs",
                AffectedTable = "Food Packs"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            string message = "Your food pack request has been submitted and is pending approval.";
            await _smsService.SendSmsAsync(applicationFoodPack.ContactNumber, message);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Staff1,Staff2")]
        public async Task<IActionResult> GetDashboardCounts()
        {
            try
            {
                var counts = new
                {
                    pending = await _context.ApplicationFoodPack.CountAsync(a => a.Status == ActiveStatus.Pending),
                    approved = await _context.ApplicationFoodPack.CountAsync(a => a.Status == ActiveStatus.Approved),
                    rejected = await _context.ApplicationFoodPack.CountAsync(a => a.Status == ActiveStatus.Denied),
                    total = await _context.ApplicationFoodPack.CountAsync()
                };
                return Json(counts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch dashboard counts", details = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Staff1,Staff2")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var applicationFoodPack = await _context.ApplicationFoodPack
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationFoodPack == null)
                return NotFound();

            return View(applicationFoodPack);
        }

        [Authorize(Roles = "Admin,Staff1,Staff2")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff1,Staff2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,MiddleName,LastName,Packs,Age,Barangay,Address,ContactNumber,Reason,Status")] ApplicationFoodPack applicationFoodPack, IFormFile brgyCertFile, IFormFile validIdFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var maxFileSize = 5 * 1024 * 1024;

            if (brgyCertFile != null && brgyCertFile.Length > 0)
            {
                var extension = Path.GetExtension(brgyCertFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("brgyCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Barangay Certificate.");
                    return View(applicationFoodPack);
                }
                if (brgyCertFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("brgyCertFile", "Barangay Certificate file size cannot exceed 5 MB.");
                    return View(applicationFoodPack);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "brgy_certs");
                Directory.CreateDirectory(directoryPath);

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await brgyCertFile.CopyToAsync(stream);
                }

                applicationFoodPack.Brgy_Cert = fileName;
                applicationFoodPack.Brgy_Cert_Path = "/Uploads/brgy_certs/" + fileName;
            }
            else
            {
                ModelState.AddModelError("brgyCertFile", "Please upload a Barangay Certificate.");
                return View(applicationFoodPack);
            }

            if (validIdFile != null && validIdFile.Length > 0)
            {
                var extension = Path.GetExtension(validIdFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("validIdFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Valid ID.");
                    return View(applicationFoodPack);
                }
                if (validIdFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("validIdFile", "Valid ID file size cannot exceed 5 MB.");
                    return View(applicationFoodPack);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "valid_ids");
                Directory.CreateDirectory(directoryPath);

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await validIdFile.CopyToAsync(stream);
                }

                applicationFoodPack.Valid_ID = fileName;
                applicationFoodPack.Valid_ID_Path = "/Uploads/valid_ids/" + fileName;
            }
            else
            {
                ModelState.AddModelError("validIdFile", "Please upload a Valid ID.");
                return View(applicationFoodPack);
            }

            var allStocks = await _context.StockIn_FoodPacks.OrderBy(s => s.Id).ToListAsync();
            var latestInventory = await _context.FoodPackInventories.OrderByDescending(i => i.RequestDate).FirstOrDefaultAsync();
            var latestStock = allStocks.LastOrDefault();
            int totalPacks = 0;

            if (allStocks.Any())
            {
                totalPacks = allStocks.Sum(s => s.Add_Stock2);
                if (latestInventory != null)
                {
                    totalPacks = latestInventory.StockLeft;
                }
            }
            else
            {
                ModelState.AddModelError("", "No stock available. Please add stock first.");
                return View(applicationFoodPack);
            }

            if (totalPacks < applicationFoodPack.Packs)
            {
                ModelState.AddModelError("Packs", $"Insufficient stock available. Available stock: {totalPacks}, Requested: {applicationFoodPack.Packs}");
                return View(applicationFoodPack);
            }

            applicationFoodPack.Status = ActiveStatus.Approved;
            applicationFoodPack.RequestDate = DateTime.Now;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Add(applicationFoodPack);
            await _context.SaveChangesAsync();

            var inventory = new FoodPackInventory
            {
                ApplicationFoodPackId = applicationFoodPack.Id,
                StockInId = latestStock?.Id ?? 0,
                TotalPacks = totalPacks,
                StockLeft = totalPacks - applicationFoodPack.Packs,
                RequestDate = DateTime.Now,
                Barangay = applicationFoodPack.Barangay,
                Packs = applicationFoodPack.Packs
            };
            _context.FoodPackInventories.Add(inventory);
            await _context.SaveChangesAsync();

            var activity = new AuditTrail
            {
                Action = "Create",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Food Packs",
                AffectedTable = "Food Packs"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            string message = applicationFoodPack.Status == ActiveStatus.Approved
                ? "Your food pack request has been approved. Please wait for further instructions."
                : "Your food pack request has been denied.";
            await _smsService.SendSmsAsync(applicationFoodPack.ContactNumber, message);

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public IActionResult FoodPackPending()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FoodPackPending([Bind("Id,FirstName,MiddleName,LastName,Packs,Age,Barangay,Address,ContactNumber,Reason,Status")] ApplicationFoodPack applicationFoodPack, IFormFile brgyCertFile, IFormFile validIdFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var maxFileSize = 5 * 1024 * 1024;

            if (brgyCertFile != null && brgyCertFile.Length > 0)
            {
                var extension = Path.GetExtension(brgyCertFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("brgyCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Barangay Certificate.");
                    return View(applicationFoodPack);
                }
                if (brgyCertFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("brgyCertFile", "Barangay Certificate file size cannot exceed 5 MB.");
                    return View(applicationFoodPack);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "brgy_certs");
                Directory.CreateDirectory(directoryPath);

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await brgyCertFile.CopyToAsync(stream);
                }

                applicationFoodPack.Brgy_Cert = fileName;
                applicationFoodPack.Brgy_Cert_Path = "/Uploads/brgy_certs/" + fileName;
            }
            else
            {
                ModelState.AddModelError("brgyCertFile", "Please upload a Barangay Certificate.");
                return View(applicationFoodPack);
            }

            if (validIdFile != null && validIdFile.Length > 0)
            {
                var extension = Path.GetExtension(validIdFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("validIdFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Valid ID.");
                    return View(applicationFoodPack);
                }
                if (validIdFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("validIdFile", "Valid ID file size cannot exceed 5 MB.");
                    return View(applicationFoodPack);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "valid_ids");
                Directory.CreateDirectory(directoryPath);

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await validIdFile.CopyToAsync(stream);
                }

                applicationFoodPack.Valid_ID = fileName;
                applicationFoodPack.Valid_ID_Path = "/Uploads/valid_ids/" + fileName;
            }
            else
            {
                ModelState.AddModelError("validIdFile", "Please upload a Valid ID.");
                return View(applicationFoodPack);
            }

            applicationFoodPack.Status = ActiveStatus.Pending;
            applicationFoodPack.RequestDate = DateTime.Now;
            _context.Add(applicationFoodPack);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Application submitted successfully!";
            return RedirectToAction(nameof(FoodPackPending));
        }

        [Authorize(Roles = "Admin,Staff1,Staff2")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var applicationFoodPack = await _context.ApplicationFoodPack.FindAsync(id);
            if (applicationFoodPack == null)
                return NotFound();

            return View(applicationFoodPack);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff1,Staff2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,MiddleName,LastName,Packs,Age,Barangay,Address,ContactNumber,Brgy_Cert,Brgy_Cert_Path,Valid_ID,Valid_ID_Path,Reason,Status")] ApplicationFoodPack applicationFoodPack, IFormFile brgyCertFile, IFormFile validIdFile)
        {
            if (id != applicationFoodPack.Id)
                return NotFound();

            try
            {
                var existingRecord = await _context.ApplicationFoodPack.FindAsync(id);
                if (existingRecord == null)
                    return NotFound();

                if (existingRecord.Packs != applicationFoodPack.Packs || (existingRecord.Status != ActiveStatus.Approved && applicationFoodPack.Status == ActiveStatus.Approved))
                {
                    var latestInventory = await _context.FoodPackInventories.OrderByDescending(i => i.RequestDate).FirstOrDefaultAsync(i => i.Id != id);
                    var allStocks = await _context.StockIn_FoodPacks.OrderBy(s => s.Id).ToListAsync();
                    var latestStock = allStocks.LastOrDefault();
                    int totalPacks = 0;

                    if (allStocks.Any())
                    {
                        totalPacks = allStocks.Sum(s => s.Add_Stock2);
                        if (latestInventory != null)
                        {
                            totalPacks = latestInventory.StockLeft;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "No stock available. Please add stock first.");
                        return View(applicationFoodPack);
                    }

                    if (totalPacks < applicationFoodPack.Packs)
                    {
                        ModelState.AddModelError("Packs", $"Insufficient stock available. Available stock: {totalPacks}, Requested: {applicationFoodPack.Packs}");
                        return View(applicationFoodPack);
                    }
                }

                existingRecord.FirstName = applicationFoodPack.FirstName;
                existingRecord.MiddleName = applicationFoodPack.MiddleName;
                existingRecord.LastName = applicationFoodPack.LastName;
                existingRecord.Packs = applicationFoodPack.Packs;
                existingRecord.Age = applicationFoodPack.Age;
                existingRecord.Barangay = applicationFoodPack.Barangay;
                existingRecord.Address = applicationFoodPack.Address;
                existingRecord.ContactNumber = applicationFoodPack.ContactNumber;
                existingRecord.Reason = applicationFoodPack.Reason;
                existingRecord.Status = applicationFoodPack.Status;
                existingRecord.RequestDate = DateTime.Now;

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var maxFileSize = 5 * 1024 * 1024;

                if (brgyCertFile != null && brgyCertFile.Length > 0)
                {
                    var extension = Path.GetExtension(brgyCertFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("brgyCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Barangay Certificate.");
                        return View(applicationFoodPack);
                    }
                    if (brgyCertFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("brgyCertFile", "Barangay Certificate file size cannot exceed 5 MB.");
                        return View(applicationFoodPack);
                    }

                    if (!string.IsNullOrEmpty(existingRecord.Brgy_Cert_Path))
                    {
                        var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, existingRecord.Brgy_Cert_Path.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "brgy_certs");
                    Directory.CreateDirectory(directoryPath);

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(directoryPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await brgyCertFile.CopyToAsync(stream);
                    }

                    existingRecord.Brgy_Cert = fileName;
                    existingRecord.Brgy_Cert_Path = "/Uploads/brgy_certs/" + fileName;
                }

                if (validIdFile != null && validIdFile.Length > 0)
                {
                    var extension = Path.GetExtension(validIdFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("validIdFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Valid ID.");
                        return View(applicationFoodPack);
                    }
                    if (validIdFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("validIdFile", "Valid ID file size cannot exceed 5 MB.");
                        return View(applicationFoodPack);
                    }

                    if (!string.IsNullOrEmpty(existingRecord.Valid_ID_Path))
                    {
                        var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, existingRecord.Valid_ID_Path.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "valid_ids");
                    Directory.CreateDirectory(directoryPath);

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(directoryPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await validIdFile.CopyToAsync(stream);
                    }

                    existingRecord.Valid_ID = fileName;
                    existingRecord.Valid_ID_Path = "/Uploads/valid_ids/" + fileName;
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Update(existingRecord);

                await _context.SaveChangesAsync();

                if (existingRecord.Status == ActiveStatus.Approved)
                {
                    var inventory = await _context.FoodPackInventories.FirstOrDefaultAsync(i => i.Id == existingRecord.Id);
                    var allStocks = await _context.StockIn_FoodPacks.OrderBy(s => s.Id).ToListAsync();
                    var latestStock = allStocks.LastOrDefault();
                    var latestInventory = await _context.FoodPackInventories.OrderByDescending(i => i.RequestDate).FirstOrDefaultAsync(i => i.Id != existingRecord.Id);
                    int totalPacks = 0;

                    if (allStocks.Any())
                    {
                        totalPacks = allStocks.Sum(s => s.Add_Stock2);
                        if (latestInventory != null)
                        {
                            totalPacks = latestInventory.StockLeft;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "No stock available. Please add stock first.");
                        return View(applicationFoodPack);
                    }

                    if (inventory != null)
                    {
                        inventory.StockInId = latestStock?.Id ?? 0;
                        inventory.TotalPacks = totalPacks;
                        inventory.StockLeft = totalPacks - existingRecord.Packs;
                        inventory.RequestDate = DateTime.Now;
                        inventory.Barangay = existingRecord.Barangay;
                        inventory.Packs = existingRecord.Packs;
                        inventory.ApplicationFoodPackId = existingRecord.Id;
                        _context.Update(inventory);
                    }
                    else
                    {
                        inventory = new FoodPackInventory
                        {
                            StockInId = latestStock?.Id ?? 0,
                            TotalPacks = totalPacks,
                            StockLeft = totalPacks - existingRecord.Packs,
                            RequestDate = DateTime.Now,
                            Barangay = existingRecord.Barangay,
                            Packs = existingRecord.Packs,
                            ApplicationFoodPackId = existingRecord.Id
                        };
                        _context.FoodPackInventories.Add(inventory);
                    }
                    await _context.SaveChangesAsync();

                    var subsequentInventories = await _context.FoodPackInventories
                        .Where(i => i.RequestDate > inventory.RequestDate)
                        .OrderBy(i => i.RequestDate)
                        .ToListAsync();

                    int runningTotal = inventory.StockLeft;
                    foreach (var subsequent in subsequentInventories)
                    {
                        subsequent.TotalPacks = runningTotal;
                        subsequent.StockLeft = runningTotal - subsequent.Packs;
                        runningTotal = subsequent.StockLeft;
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var inventory = await _context.FoodPackInventories.FirstOrDefaultAsync(i => i.Id == existingRecord.Id);
                    if (inventory != null)
                    {
                        _context.FoodPackInventories.Remove(inventory);
                        await _context.SaveChangesAsync();

                        var remainingInventories = await _context.FoodPackInventories
                            .OrderBy(i => i.RequestDate)
                            .ToListAsync();

                        var allStocks = await _context.StockIn_FoodPacks.OrderBy(s => s.Id).ToListAsync();
                        int runningTotal = allStocks.Any() ? allStocks.Sum(s => s.Add_Stock2) : 0;

                        foreach (var inv in remainingInventories)
                        {
                            inv.TotalPacks = runningTotal;
                            inv.StockLeft = runningTotal - inv.Packs;
                            runningTotal = inv.StockLeft;
                        }

                        await _context.SaveChangesAsync();
                    }
                }

                var activity = new AuditTrail
                {
                    Action = "Edit",
                    TimeStamp = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserId = userId,
                    Moduie = "Food Packs",
                    AffectedTable = "Food Packs"
                };
                _context.Add(activity);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationFoodPackExists(applicationFoodPack.Id))
                    return NotFound();
                else
                    throw;
            }
        }

        [Authorize(Roles = "Admin,Staff1,Staff2")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var applicationFoodPack = await _context.ApplicationFoodPack
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationFoodPack == null)
                return NotFound();

            return View(applicationFoodPack);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Staff1,Staff2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var applicationFoodPack = await _context.ApplicationFoodPack.FindAsync(id);
            if (applicationFoodPack != null)
            {
                if (!string.IsNullOrEmpty(applicationFoodPack.Brgy_Cert_Path))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, applicationFoodPack.Brgy_Cert_Path.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
                if (!string.IsNullOrEmpty(applicationFoodPack.Valid_ID_Path))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, applicationFoodPack.Valid_ID_Path.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                var inventory = await _context.FoodPackInventories.FirstOrDefaultAsync(i => i.Id == id);
                if (inventory != null)
                    _context.FoodPackInventories.Remove(inventory);

                _context.ApplicationFoodPack.Remove(applicationFoodPack);
                await _context.SaveChangesAsync();

                var remainingInventories = await _context.FoodPackInventories
                    .OrderBy(i => i.RequestDate)
                    .ToListAsync();

                var allStocks = await _context.StockIn_FoodPacks.OrderBy(s => s.Id).ToListAsync();
                int runningTotal = allStocks.Any() ? allStocks.Sum(s => s.Add_Stock2) : 0;

                foreach (var inv in remainingInventories)
                {
                    inv.TotalPacks = runningTotal;
                    inv.StockLeft = runningTotal - inv.Packs;
                    runningTotal = inv.StockLeft;
                }
                await _context.SaveChangesAsync();
            }

            var activity = new AuditTrail
            {
                Action = "Delete",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Food Packs",
                AffectedTable = "Food Packs"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff1")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var picture = await _context.ApplicationFoodPack.FindAsync(id);
            if (picture == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest("Status is required.");
            }

            if (status.Equals("Process", StringComparison.OrdinalIgnoreCase))
            {
                picture.Status = ActiveStatus.Process;
            }
            else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                picture.Status = ActiveStatus.Denied;
            }
            else
            {
                return BadRequest("Invalid status value. Only 'Process' or 'Rejected' are allowed.");
            }

            _context.Update(picture);
            await _context.SaveChangesAsync();

            if (picture.Status == ActiveStatus.Denied)
            {
                var existingInventory = await _context.FoodPackInventories
                    .FirstOrDefaultAsync(i => i.ApplicationFoodPackId == picture.Id);
                if (existingInventory != null)
                {
                    _context.FoodPackInventories.Remove(existingInventory);
                    await _context.SaveChangesAsync();

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
                }
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var activity = new AuditTrail
            {
                Action = status.Equals("Process", StringComparison.OrdinalIgnoreCase) ? "Update Status to Process" : "Rejected",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Food Packs",
                AffectedTable = "Food Packs"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            string message = picture.Status == ActiveStatus.Process
                ? "Your request has been processed. Please wait for the confirmation."
                : "Your request has been rejected.";

            await _smsService.SendSmsAsync(picture.ContactNumber, message);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus1(int id, string status)
        {
            var picture = await _context.ApplicationFoodPack.FindAsync(id);
            if (picture == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest("Status is required.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int totalPacks = 0;
            if (status.Equals("Accepted", StringComparison.OrdinalIgnoreCase))
            {
                var allStocks = await _context.StockIn_FoodPacks.OrderBy(s => s.Restock_DateTime2).ToListAsync();
                var latestInventory = await _context.FoodPackInventories
                    .OrderByDescending(i => i.RequestDate)
                    .FirstOrDefaultAsync(i => i.ApplicationFoodPackId != id);
                var latestStock = allStocks.LastOrDefault();

                if (allStocks.Any())
                {
                    totalPacks = allStocks.Sum(s => s.Add_Stock2);
                    if (latestInventory != null)
                    {
                        totalPacks = latestInventory.StockLeft;
                    }
                }
                else
                {
                    ModelState.AddModelError("", "No stock available. Please add stock first.");
                    return RedirectToAction(nameof(Index));
                }

                if (totalPacks < picture.Packs)
                {
                    ModelState.AddModelError("", $"Insufficient stock available. Available stock: {totalPacks}, Requested: {picture.Packs}");
                    return RedirectToAction(nameof(Index));
                }
            }

            if (status.Equals("Accepted", StringComparison.OrdinalIgnoreCase))
            {
                picture.Status = ActiveStatus.Approved;
                picture.ApprovedById = userId;
            }
            else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                picture.Status = ActiveStatus.Denied;
                picture.ApprovedById = null;
            }
            else
            {
                return BadRequest("Invalid status value. Only 'Accepted' or 'Rejected' are allowed.");
            }

            _context.Update(picture);
            await _context.SaveChangesAsync();

            if (picture.Status == ActiveStatus.Approved)
            {
                var allStocks = await _context.StockIn_FoodPacks.OrderBy(s => s.Restock_DateTime2).ToListAsync();
                var latestStock = allStocks.LastOrDefault();
                var latestInventory = await _context.FoodPackInventories
                    .OrderByDescending(i => i.RequestDate)
                    .FirstOrDefaultAsync(i => i.ApplicationFoodPackId != id);

                totalPacks = allStocks.Any() ? allStocks.Sum(s => s.Add_Stock2) : 0;
                if (latestInventory != null)
                {
                    totalPacks = latestInventory.StockLeft;
                }

                var existingInventory = await _context.FoodPackInventories
                    .FirstOrDefaultAsync(i => i.ApplicationFoodPackId == picture.Id);

                if (existingInventory == null)
                {
                    var inventory = new FoodPackInventory
                    {
                        ApplicationFoodPackId = picture.Id,
                        StockInId = latestStock?.Id ?? 0,
                        TotalPacks = totalPacks,
                        StockLeft = totalPacks - picture.Packs,
                        RequestDate = DateTime.Now,
                        Barangay = picture.Barangay,
                        Packs = picture.Packs
                    };
                    _context.FoodPackInventories.Add(inventory);
                }
                else
                {
                    existingInventory.StockInId = latestStock?.Id ?? 0;
                    existingInventory.TotalPacks = totalPacks;
                    existingInventory.StockLeft = totalPacks - picture.Packs;
                    existingInventory.RequestDate = DateTime.Now;
                    existingInventory.Barangay = picture.Barangay;
                    existingInventory.Packs = picture.Packs;
                    _context.Update(existingInventory);
                }

                await _context.SaveChangesAsync();

                var subsequentInventories = await _context.FoodPackInventories
                    .Where(i => i.RequestDate > DateTime.Now)
                    .OrderBy(i => i.RequestDate)
                    .ToListAsync();

                int runningTotal = totalPacks - picture.Packs;
                foreach (var subsequent in subsequentInventories)
                {
                    subsequent.TotalPacks = runningTotal;
                    subsequent.StockLeft = runningTotal - subsequent.Packs;
                    runningTotal = subsequent.StockLeft;
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                var existingInventory = await _context.FoodPackInventories
                    .FirstOrDefaultAsync(i => i.ApplicationFoodPackId == picture.Id);
                if (existingInventory != null)
                {
                    _context.FoodPackInventories.Remove(existingInventory);
                    await _context.SaveChangesAsync();

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
                }
            }

            var activity = new AuditTrail
            {
                Action = status.Equals("Accepted", StringComparison.OrdinalIgnoreCase) ? "Accepted" : "Rejected",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Food Packs",
                AffectedTable = "Food Packs"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            string message = picture.Status == ActiveStatus.Approved
                ? "Your request has been accepted. Please proceed to Magsaysay CSWDO Office, Door 11."
                : "Your request has been rejected.";

            await _smsService.SendSmsAsync(picture.ContactNumber, message);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplications()
        {
            try
            {
                var applications = await _context.ApplicationFoodPack
                    .Select(a => new
                    {
                        a.Id,
                        a.FirstName,
                        a.MiddleName,
                        a.LastName,
                        a.Packs,
                        a.Age,
                        Barangay = a.Barangay.ToString(), // Convert enum to string
                        a.Address,
                        a.ContactNumber,
                        a.Brgy_Cert,
                        a.Brgy_Cert_Path,
                        a.Valid_ID,
                        a.Valid_ID_Path,
                        Reason = a.Reason.ToString(), // Convert enum to string
                        Status = a.Status.ToString(),
                        RequestDate = a.RequestDate
                    })
                    .ToListAsync();

                // Use System.Text.Json to serialize with enum string conversion
                var options = new JsonSerializerOptions
                {
                    Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                };
                return Json(applications, options);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch applications", details = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff1,Staff2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecalculateInventory()
        {
            var inventories = await _context.FoodPackInventories
                .OrderBy(i => i.RequestDate)
                .ToListAsync();

            if (!inventories.Any())
                return RedirectToAction(nameof(Index));

            var allStocks = await _context.StockIn_FoodPacks.OrderBy(s => s.Id).ToListAsync();
            if (!allStocks.Any())
            {
                ModelState.AddModelError("", "No stock available. Please add stock first.");
                return RedirectToAction("Index", "FoodPackInventory");
            }

            int runningTotal = allStocks.Sum(s => s.Add_Stock2);

            foreach (var inventory in inventories)
            {
                inventory.TotalPacks = runningTotal;
                inventory.StockLeft = runningTotal - inventory.Packs;
                runningTotal = inventory.StockLeft;
            }

            await _context.SaveChangesAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var activity = new AuditTrail
            {
                Action = "Recalculate Inventory",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Food Packs",
                AffectedTable = "FoodPackInventories"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "FoodPackInventory");
        }

        private bool ApplicationFoodPackExists(int id)
        {
            return _context.ApplicationFoodPack.Any(e => e.Id == id);
        }
    }
}