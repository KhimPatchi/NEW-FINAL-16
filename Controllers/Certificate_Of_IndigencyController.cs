using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialWelfarre.Data;
using SocialWelfarre.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SocialWelfarre.Services;
using Microsoft.AspNetCore.Authorization;

namespace SocialWelfarre.Controllers
{
    public class Certificate_Of_IndigencyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly SmsService _smsService;

        public Certificate_Of_IndigencyController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, SmsService smsService)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _smsService = smsService;
        }

        // GET: Certificate_Of_Indigency
        [Authorize(Roles = "Admin,Staff1,Staff2")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Certificate_Of_Indigencies.ToListAsync());
        }
        [Authorize(Roles = "Admin,Staff1,Staff2")]
        public async Task<IActionResult> GetAllApplications()
        {
            try
            {
                var applications = await _context.Certificate_Of_Indigencies
                    .Select(a => new
                    {
                        a.Id,
                        a.FirstName1,
                        a.MiddleName1,
                        a.LastName1,
                        a.Age1,
                        a.Barangay1,
                        a.No_Rquested1,
                        a.Address1,
                        a.ContactNumber1,
                        a.Brgy_Cert1,
                        a.Brgy_Cert_Path1,
                        a.Valid_ID1,
                        a.Valid_ID_Path1,
                        a.Reason1,
                        Status1 = a.Status1.ToString(), // Convert enum to string
                        a.RequestDate1
                    })
                    .ToListAsync();
                return Json(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch applications", details = ex.Message });
            }
        }
        [Authorize(Roles = "Admin,Staff1,Staff2")]
        public async Task<IActionResult> GetDashboardCounts()
        {
            try
            {
                var counts = new
                {
                    pending = await _context.Certificate_Of_Indigencies.CountAsync(a => a.Status1 == ActiveStatus1.Pending),
                    approved = await _context.Certificate_Of_Indigencies.CountAsync(a => a.Status1 == ActiveStatus1.Approved),
                    rejected = await _context.Certificate_Of_Indigencies.CountAsync(a => a.Status1 == ActiveStatus1.Denied),
                    total = await _context.Certificate_Of_Indigencies.CountAsync()
                };
                return Json(counts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch dashboard counts", details = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Staff1,Staff2")]
        // GET: Certificate_Of_Indigency/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate_Of_Indigency = await _context.Certificate_Of_Indigencies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificate_Of_Indigency == null)
            {
                return NotFound();
            }

            return View(certificate_Of_Indigency);
        }

        // POST: Certificate_Of_Indigency/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Staff1,Staff2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName1,MiddleName1,LastName1,Age1,Barangay1,No_Rquested1,Address1,ContactNumber1,Reason1,Status1", "RequestDate1")] Certificate_Of_Indigency certificate_Of_Indigency, IFormFile brgyCertFile, IFormFile validIdFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var maxFileSize = 5 * 1024 * 1024; // 5 MB

            if (brgyCertFile != null && brgyCertFile.Length > 0)
            {
                var extension = Path.GetExtension(brgyCertFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("brgyCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Barangay Certificate.");
                    return View("Index", await _context.Certificate_Of_Indigencies.ToListAsync());
                }
                if (brgyCertFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("brgyCertFile", "Barangay Certificate file size cannot exceed 5 MB.");
                    return View("Index", await _context.Certificate_Of_Indigencies.ToListAsync());
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "brgy_certs");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await brgyCertFile.CopyToAsync(stream);
                }

                certificate_Of_Indigency.Brgy_Cert1 = fileName;
                certificate_Of_Indigency.Brgy_Cert_Path1 = "/uploads/brgy_certs/" + fileName;
            }
            else
            {
                ModelState.AddModelError("brgyCertFile", "Please upload a Barangay Certificate.");
                return View("Index", await _context.Certificate_Of_Indigencies.ToListAsync());
            }

            if (validIdFile != null && validIdFile.Length > 0)
            {
                var extension = Path.GetExtension(validIdFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("validIdFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Valid ID.");
                    return View("Index", await _context.Certificate_Of_Indigencies.ToListAsync());
                }
                if (validIdFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("validIdFile", "Valid ID file size cannot exceed 5 MB.");
                    return View("Index", await _context.Certificate_Of_Indigencies.ToListAsync());
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "valid_ids");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await validIdFile.CopyToAsync(stream);
                }

                certificate_Of_Indigency.Valid_ID1 = fileName;
                certificate_Of_Indigency.Valid_ID_Path1 = "/Uploads/valid_ids/" + fileName;
            }
            else
            {
                ModelState.AddModelError("validIdFile", "Please upload a Valid ID.");
                return View("Index", await _context.Certificate_Of_Indigencies.ToListAsync());
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            certificate_Of_Indigency.Status1 = (ActiveStatus1)Enum.Parse(typeof(ActiveStatus1), "Approved");
            _context.Add(certificate_Of_Indigency);
            await _context.SaveChangesAsync();

            var activity = new AuditTrail
            {
                Action = "Create",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Certificate Of Indigency",
                AffectedTable = "Certificate Of Indigency"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public IActionResult CertificateOfIndigencyPending()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CertificateOfIndigencyPending(
       [Bind("Id,FirstName1,MiddleName1,LastName1,Age1,Barangay1,No_Rquested1,Address1,ContactNumber1,Reason1,Status1,RequestDate1")]
    Certificate_Of_Indigency certificate_Of_Indigency,
       IFormFile brgyCertFile,
       IFormFile validIdFile)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var maxFileSize = 5 * 1024 * 1024; // 5 MB

            // === Barangay Certificate File Validation ===
            if (brgyCertFile != null && brgyCertFile.Length > 0)
            {
                var extension = Path.GetExtension(brgyCertFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("brgyCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Barangay Certificate.");
                    return View("Index", await _context.Certificate_Of_Indigencies.ToListAsync());
                }

                if (brgyCertFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("brgyCertFile", "Barangay Certificate file size cannot exceed 5 MB.");
                    return View("Index", await _context.Certificate_Of_Indigencies.ToListAsync());
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "brgy_certs");
                Directory.CreateDirectory(directoryPath); // Safe even if exists

                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await brgyCertFile.CopyToAsync(stream);
                }

                certificate_Of_Indigency.Brgy_Cert1 = fileName;
                certificate_Of_Indigency.Brgy_Cert_Path1 = "/Uploads/brgy_certs/" + fileName;
            }
            else
            {
                ModelState.AddModelError("brgyCertFile", "Please upload a Barangay Certificate.");
                return View("Index", await _context.Certificate_Of_Indigencies.ToListAsync());
            }

            // === Valid ID File Validation ===
            if (validIdFile != null && validIdFile.Length > 0)
            {
                var extension = Path.GetExtension(validIdFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("validIdFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Valid ID.");
                    return View("Index", await _context.Certificate_Of_Indigencies.ToListAsync());
                }

                if (validIdFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("validIdFile", "Valid ID file size cannot exceed 5 MB.");
                    return View("Index", await _context.Certificate_Of_Indigencies.ToListAsync());
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "valid_ids");
                Directory.CreateDirectory(directoryPath);

                var fileName = Guid.NewGuid() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await validIdFile.CopyToAsync(stream);
                }

                certificate_Of_Indigency.Valid_ID1 = fileName;
                certificate_Of_Indigency.Valid_ID_Path1 = "/Uploads/valid_ids/" + fileName;
            }
            else
            {
                ModelState.AddModelError("validIdFile", "Please upload a Valid ID.");
                return View("Index", await _context.Certificate_Of_Indigencies.ToListAsync());
            }

            // === Save Record ===
            certificate_Of_Indigency.Status1 = (ActiveStatus1)Enum.Parse(typeof(ActiveStatus1), "Pending");

            _context.Add(certificate_Of_Indigency);
            await _context.SaveChangesAsync();



            return RedirectToAction(nameof(CertificateOfIndigencyPending));
        }

        [Authorize(Roles = "Admin,Staff1,Staff2")]
        // GET: Certificate_Of_Indigency/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificate = await _context.Certificate_Of_Indigencies.FindAsync(id);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff1,Staff2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName1,MiddleName1,LastName1,Age1,Barangay1,No_Rquested1,Address1,ContactNumber1,Brgy_Cert1,Brgy_Cert_Path1,Valid_ID1,Valid_ID_Path1,Reason1,Status1, RequestDate1")] Certificate_Of_Indigency certificate, IFormFile brgyCertFile, IFormFile validIdFile)
        {
            if (id != certificate.Id)
            {
                return NotFound();
            }

            try
            {
                var existingRecord = await _context.Certificate_Of_Indigencies.FindAsync(id);
                if (existingRecord == null)
                {
                    return NotFound();
                }

                // Update text fields
                existingRecord.FirstName1 = certificate.FirstName1;
                existingRecord.MiddleName1 = certificate.MiddleName1;
                existingRecord.LastName1 = certificate.LastName1;
                existingRecord.Age1 = certificate.Age1;
                existingRecord.Barangay1 = certificate.Barangay1;
                existingRecord.No_Rquested1 = certificate.No_Rquested1;
                existingRecord.Address1 = certificate.Address1;
                existingRecord.ContactNumber1 = certificate.ContactNumber1;
                existingRecord.Reason1 = certificate.Reason1;
                existingRecord.Status1 = certificate.Status1;
                existingRecord.RequestDate1 = certificate.RequestDate1;
                // File validation setup
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var maxFileSize = 5 * 1024 * 1024; // 5 MB

                // Barangay Certificate upload
                if (brgyCertFile != null && brgyCertFile.Length > 0)
                {
                    var extension = Path.GetExtension(brgyCertFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("brgyCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Barangay Certificate.");
                        return View("certificate_Of_Indigency", certificate);
                    }
                    if (brgyCertFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("brgyCertFile", "Barangay Certificate file size cannot exceed 5 MB.");
                        return View("certificate_Of_Indigency", certificate);
                    }

                    if (!string.IsNullOrEmpty(existingRecord.Brgy_Cert_Path1))
                    {
                        var oldPath = Path.Combine(_hostingEnvironment.WebRootPath, existingRecord.Brgy_Cert_Path1.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    var directory = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "brgy_certs");
                    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                    var fileName = Guid.NewGuid() + extension;
                    var filePath = Path.Combine(directory, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await brgyCertFile.CopyToAsync(stream);
                    }

                    existingRecord.Brgy_Cert1 = fileName;
                    existingRecord.Brgy_Cert_Path1 = "/Uploads/brgy_certs/" + fileName;
                }

                // Valid ID upload
                if (validIdFile != null && validIdFile.Length > 0)
                {
                    var extension = Path.GetExtension(validIdFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("validIdFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Valid ID.");
                        return View("certificate_Of_Indigency", certificate);
                    }
                    if (validIdFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("validIdFile", "Valid ID file size cannot exceed 5 MB.");
                        return View("certificate_Of_Indigency", certificate);
                    }

                    if (!string.IsNullOrEmpty(existingRecord.Valid_ID_Path1))
                    {
                        var oldPath = Path.Combine(_hostingEnvironment.WebRootPath, existingRecord.Valid_ID_Path1.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    var directory = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "valid_ids");
                    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                    var fileName = Guid.NewGuid() + extension;
                    var filePath = Path.Combine(directory, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await validIdFile.CopyToAsync(stream);
                    }

                    existingRecord.Valid_ID1 = fileName;
                    existingRecord.Valid_ID_Path1 = "/Uploads/valid_ids/" + fileName;
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Update(existingRecord);
                await _context.SaveChangesAsync();

                var audit = new AuditTrail
                {
                    Action = "Edit",
                    TimeStamp = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserId = userId,
                    Moduie = "Certificate Of Indigency",
                    AffectedTable = " Certificate Of Indigency"
                };
                _context.Add(audit);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Certificate_Of_IndigencyExists(certificate.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [Authorize(Roles = "Admin,Staff1,Staff2")]
        // GET: Certificate_Of_Indigency/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var certificate_Of_Indigency = await _context.Certificate_Of_Indigencies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (certificate_Of_Indigency == null)
            {
                return NotFound();
            }

            return View(certificate_Of_Indigency);
        }
        [Authorize(Roles = "Admin,Staff1,Staff2")]
        // POST: Certificate_Of_Indigency/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var certificate_Of_Indigency = await _context.Certificate_Of_Indigencies.FindAsync(id);
            if (certificate_Of_Indigency != null)
            {
                // Delete associated files
                if (!string.IsNullOrEmpty(certificate_Of_Indigency.Brgy_Cert_Path1))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, certificate_Of_Indigency.Brgy_Cert_Path1.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                if (!string.IsNullOrEmpty(certificate_Of_Indigency.Valid_ID_Path1))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, certificate_Of_Indigency.Valid_ID_Path1.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Certificate_Of_Indigencies.Remove(certificate_Of_Indigency);
            }

            var activity = new AuditTrail
            {
                Action = "Delete",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Certificate Of Indigency",
                AffectedTable = "Certificate Of Indigency"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // POST: Picture/UpdateStatus/5
        [HttpPost]
        [Authorize(Roles = "Admin,Staff1")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var picture = await _context.Certificate_Of_Indigencies.FindAsync(id);
            if (picture == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest("Status is required.");
            }

            string auditAction;
            if (status.Equals("Process", StringComparison.OrdinalIgnoreCase))
            {
                picture.Status1 = ActiveStatus1.Process;
                auditAction = "Update Status to Process";
            }
            else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                picture.Status1 = ActiveStatus1.Denied;
                auditAction = "Rejected";
            }
            else
            {
                return BadRequest("Invalid status value. Only 'Process' or 'Rejected' are allowed.");
            }

            _context.Update(picture);
            await _context.SaveChangesAsync();

            string message = picture.Status1 == ActiveStatus1.Process
                ? "Your request has been processed. Please wait for the confirmation."
                : "Your request has been rejected.";

            await _smsService.SendSmsAsync(picture.ContactNumber1, message);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var activity = new AuditTrail
            {
                Action = auditAction,
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Certificate Of Indigency",
                AffectedTable = "Certificate Of Indigency"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Staff2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus1(int id, string status)
        {
            var picture = await _context.Certificate_Of_Indigencies.FindAsync(id);
            if (picture == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest("Status is required.");
            }

            string auditAction;
            if (status.Equals("Accepted", StringComparison.OrdinalIgnoreCase))
            {
                picture.Status1 = ActiveStatus1.Approved;
                auditAction = "Accepted";
                picture.ApprovedById = userId; // Set the approver to current user
            }
            else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                picture.Status1 = ActiveStatus1.Denied;
                auditAction = "Rejected";
                picture.ApprovedById = null; // Clear approver if rejected
            }
            else
            {
                return BadRequest("Invalid status value. Only 'Accepted' or 'Rejected' are allowed.");
            }

            _context.Update(picture);
            await _context.SaveChangesAsync();

            // Send SMS message
            string message = picture.Status1 == ActiveStatus1.Approved
                ? "Your request has been accepted. Please proceed to Magsaysay CSWDO Office, Door 11."
                : "Your request has been rejected.";

            await _smsService.SendSmsAsync(picture.ContactNumber1, message);


            var activity = new AuditTrail
            {
                Action = auditAction,
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Certificate Of Indigency",
                AffectedTable = "Certificate Of Indigency"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool Certificate_Of_IndigencyExists(int id)
        {
            return _context.Certificate_Of_Indigencies.Any(e => e.Id == id);
        }
    }
}
