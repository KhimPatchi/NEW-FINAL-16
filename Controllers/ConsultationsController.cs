using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialWelfarre.Data;
using SocialWelfarre.Models;
using Microsoft.AspNetCore.Http;
using SocialWelfarre.Services;
using Microsoft.AspNetCore.Authorization;

namespace SocialWelfarre.Controllers
{
    [Authorize(Roles = "Admin,Staff1,Staff2")]
    public class ConsultationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly SmsService _smsService;

        public ConsultationsController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, SmsService smsService)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _smsService = smsService;
        }

        // GET: Consultations
        public async Task<IActionResult> Index()
        {
            return View(await _context.Consultations.ToListAsync());
        }

        // GET: Consultations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (consultation == null)
            {
                return NotFound();
            }

            return View(consultation);
        }

        public async Task<IActionResult> GetDashboardCounts()
        {
            try
            {
                var counts = new
                {
                    pending = await _context.Consultations.CountAsync(c => c.Consultation_status == ActiveStatus2.Pending || c.Consultation_status == null),
                    approved = await _context.Consultations.CountAsync(c => c.Consultation_status == ActiveStatus2.Approved),
                    rejected = await _context.Consultations.CountAsync(c => c.Consultation_status == ActiveStatus2.Denied),
                    total = await _context.Consultations.CountAsync()
                };
                return Json(counts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch dashboard counts", details = ex.Message });
            }
        }

        public async Task<IActionResult> GetAllConsultations()
        {
            try
            {
                var consultations = await _context.Consultations
                    .Select(c => new
                    {
                        c.Id,
                        firstName2 = c.First_Name2,
                        middleName2 = c.Middle_Name2,
                        lastName2 = c.Last_Name2,
                        contactNumber2 = c.ContactNumber2,
                        brgyCert2 = c.Brgy_Cert2,
                        brgyCertPath2 = c.Brgy_Cert_Path2,
                        proofSoloParent2 = c.Proof_SoloParent2,
                        proofSoloParentPath2 = c.Proof_SoloParent_Path2,
                        birthCert2 = c.Birth_Cert2,
                        birthCertPath2 = c.Birth_Cert_Path2,
                        validId2 = c.Valid_ID2,
                        validIdPath2 = c.Valid_ID_Path2,
                        x1Pic2 = c.x1_Pic2,
                        x1PicPath2 = c.x1_Pic_Path2,
                        consultationDate = c.Consultation_Date.ToString("yyyy-MM-dd"),
                        consultationTime = c.Consultation_Time,
                        consultationStatus = c.Consultation_status != null ? c.Consultation_status.ToString() : "Pending"
                    })
                    .ToListAsync();
                return Json(consultations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to fetch consultations", details = ex.Message });
            }
        }

        // GET: Consultations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Consultations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,First_Name2,Middle_Name2,Last_Name2, ContactNumber2, Consultation_Date,Consultation_Time,Consultation_status")] Consultation consultation,
            IFormFile brgyCertFile,
            IFormFile proofSoloParentFile,
            IFormFile birthCertFile,
            IFormFile validIdFile,
            IFormFile x1PicFile)
        {
            // Validate uploaded files
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var maxFileSize = 5 * 1024 * 1024; // 5 MB

            // Validate Barangay Certificate file
            if (brgyCertFile != null && brgyCertFile.Length > 0)
            {
                var extension = Path.GetExtension(brgyCertFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("brgyCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Barangay Certificate.");
                    return View(consultation);
                }
                if (brgyCertFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("brgyCertFile", "Barangay Certificate file size cannot exceed 5 MB.");
                    return View(consultation);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "brgy_certs");
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

                consultation.Brgy_Cert2 = fileName;
                consultation.Brgy_Cert_Path2 = "/Uploads/brgy_certs/" + fileName;
            }
            else
            {
                ModelState.AddModelError("brgyCertFile", "Please upload a Barangay Certificate.");
                return View(consultation);
            }

            // Validate Proof of Solo Parent file
            if (proofSoloParentFile != null && proofSoloParentFile.Length > 0)
            {
                var extension = Path.GetExtension(proofSoloParentFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("proofSoloParentFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Proof of Solo Parent.");
                    return View(consultation);
                }
                if (proofSoloParentFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("proofSoloParentFile", "Proof of Solo Parent file size cannot exceed 5 MB.");
                    return View(consultation);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "solo_parent_proofs");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await proofSoloParentFile.CopyToAsync(stream);
                }

                consultation.Proof_SoloParent2 = fileName;
                consultation.Proof_SoloParent_Path2 = "/Uploads/solo_parent_proofs/" + fileName;
            }
            else
            {
                ModelState.AddModelError("proofSoloParentFile", "Please upload a Proof of Solo Parent.");
                return View(consultation);
            }

            // Validate Birth Certificate file
            if (birthCertFile != null && birthCertFile.Length > 0)
            {
                var extension = Path.GetExtension(birthCertFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("birthCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Birth Certificate.");
                    return View(consultation);
                }
                if (birthCertFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("birthCertFile", "Birth Certificate file size cannot exceed 5 MB.");
                    return View(consultation);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "birth_certs");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await birthCertFile.CopyToAsync(stream);
                }

                consultation.Birth_Cert2 = fileName;
                consultation.Birth_Cert_Path2 = "/Uploads/birth_certs/" + fileName;
            }
            else
            {
                ModelState.AddModelError("birthCertFile", "Please upload a Birth Certificate.");
                return View(consultation);
            }

            // Validate Valid ID file
            if (validIdFile != null && validIdFile.Length > 0)
            {
                var extension = Path.GetExtension(validIdFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("validIdFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Valid ID.");
                    return View(consultation);
                }
                if (validIdFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("validIdFile", "Valid ID file size cannot exceed 5 MB.");
                    return View(consultation);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "valid_ids");
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

                consultation.Valid_ID2 = fileName;
                consultation.Valid_ID_Path2 = "/Uploads/valid_ids/" + fileName;
            }
            else
            {
                ModelState.AddModelError("validIdFile", "Please upload a Valid ID.");
                return View(consultation);
            }

            // Validate 1x1 Picture file
            if (x1PicFile != null && x1PicFile.Length > 0)
            {
                var extension = Path.GetExtension(x1PicFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("x1PicFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for 1x1 Picture.");
                    return View(consultation);
                }
                if (x1PicFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("x1PicFile", "1x1 Picture file size cannot exceed 5 MB.");
                    return View(consultation);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "x1_pics");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await x1PicFile.CopyToAsync(stream);
                }

                consultation.x1_Pic2 = fileName;
                consultation.x1_Pic_Path2 = "/Uploads/x1_pics/" + fileName;
            }
            else
            {
                ModelState.AddModelError("x1PicFile", "Please upload a 1x1 Picture.");
                return View(consultation);
            }

            // Set status to Approved by default
            consultation.Consultation_status = (ActiveStatus2)Enum.Parse(typeof(ActiveStatus2), "Approved");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _context.Add(consultation);
            await _context.SaveChangesAsync();

            var activity = new AuditTrail
            {
                Action = "Create",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Consultation",
                AffectedTable = "Consultations"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public IActionResult ConsultationPending()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsultationPending(
            [Bind("Id,First_Name2,Middle_Name2,Last_Name2,ContactNumber2, Consultation_Date,Consultation_Time,Consultation_status")] Consultation consultation,
            IFormFile brgyCertFile,
            IFormFile proofSoloParentFile,
            IFormFile birthCertFile,
            IFormFile validIdFile,
            IFormFile x1PicFile)
        {
            // Validate uploaded files
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var maxFileSize = 5 * 1024 * 1024; // 5 MB

            // Validate Barangay Certificate file
            if (brgyCertFile != null && brgyCertFile.Length > 0)
            {
                var extension = Path.GetExtension(brgyCertFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("brgyCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Barangay Certificate.");
                    return View(consultation);
                }
                if (brgyCertFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("brgyCertFile", "Barangay Certificate file size cannot exceed 5 MB.");
                    return View(consultation);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "brgy_certs");
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

                consultation.Brgy_Cert2 = fileName;
                consultation.Brgy_Cert_Path2 = "/Uploads/brgy_certs/" + fileName;
            }
            else
            {
                ModelState.AddModelError("brgyCertFile", "Please upload a Barangay Certificate.");
                return View(consultation);
            }

            // Validate Proof of Solo Parent file
            if (proofSoloParentFile != null && proofSoloParentFile.Length > 0)
            {
                var extension = Path.GetExtension(proofSoloParentFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("proofSoloParentFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Proof of Solo Parent.");
                    return View(consultation);
                }
                if (proofSoloParentFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("proofSoloParentFile", "Proof of Solo Parent file size cannot exceed 5 MB.");
                    return View(consultation);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "solo_parent_proofs");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await proofSoloParentFile.CopyToAsync(stream);
                }

                consultation.Proof_SoloParent2 = fileName;
                consultation.Proof_SoloParent_Path2 = "/Uploads/solo_parent_proofs/" + fileName;
            }
            else
            {
                ModelState.AddModelError("proofSoloParentFile", "Please upload a Proof of Solo Parent.");
                return View(consultation);
            }

            // Validate Birth Certificate file
            if (birthCertFile != null && birthCertFile.Length > 0)
            {
                var extension = Path.GetExtension(birthCertFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("birthCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Birth Certificate.");
                    return View(consultation);
                }
                if (birthCertFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("birthCertFile", "Birth Certificate file size cannot exceed 5 MB.");
                    return View(consultation);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "birth_certs");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await birthCertFile.CopyToAsync(stream);
                }

                consultation.Birth_Cert2 = fileName;
                consultation.Birth_Cert_Path2 = "/Uploads/birth_certs/" + fileName;
            }
            else
            {
                ModelState.AddModelError("birthCertFile", "Please upload a Birth Certificate.");
                return View(consultation);
            }

            // Validate Valid ID file
            if (validIdFile != null && validIdFile.Length > 0)
            {
                var extension = Path.GetExtension(validIdFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("validIdFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Valid ID.");
                    return View(consultation);
                }
                if (validIdFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("validIdFile", "Valid ID file size cannot exceed 5 MB.");
                    return View(consultation);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "valid_ids");
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

                consultation.Valid_ID2 = fileName;
                consultation.Valid_ID_Path2 = "/Uploads/valid_ids/" + fileName;
            }
            else
            {
                ModelState.AddModelError("validIdFile", "Please upload a Valid ID.");
                return View(consultation);
            }

            // Validate 1x1 Picture file
            if (x1PicFile != null && x1PicFile.Length > 0)
            {
                var extension = Path.GetExtension(x1PicFile.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("x1PicFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for 1x1 Picture.");
                    return View(consultation);
                }
                if (x1PicFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("x1PicFile", "1x1 Picture file size cannot exceed 5 MB.");
                    return View(consultation);
                }

                var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "x1_pics");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileName = Guid.NewGuid().ToString() + extension;
                var filePath = Path.Combine(directoryPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await x1PicFile.CopyToAsync(stream);
                }

                consultation.x1_Pic2 = fileName;
                consultation.x1_Pic_Path2 = "/Uploads/x1_pics/" + fileName;
            }
            else
            {
                ModelState.AddModelError("x1PicFile", "Please upload a 1x1 Picture.");
                return View(consultation);
            }

        
            _context.Add(consultation);
            await _context.SaveChangesAsync();
            consultation.Consultation_status = (ActiveStatus2)Enum.Parse(typeof(ActiveStatus2), "Pending");

            
           

            return RedirectToAction(nameof(ConsultationPending));
        }

        // GET: Consultations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation == null)
            {
                return NotFound();
            }
            return View(consultation);
        }

        // POST: Consultations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,First_Name2,Middle_Name2,Last_Name2,ContactNumber2, Consultation_Date,Consultation_Time,Consultation_status")] Consultation consultation,
            IFormFile brgyCertFile,
            IFormFile proofSoloParentFile,
            IFormFile birthCertFile,
            IFormFile validIdFile,
            IFormFile x1PicFile)
        {
            if (id != consultation.Id)
            {
                return NotFound();
            }

            try
            {
                var existingRecord = await _context.Consultations.FindAsync(id);
                if (existingRecord == null)
                {
                    return NotFound();
                }

                // Update fields
                existingRecord.First_Name2 = consultation.First_Name2;
                existingRecord.Middle_Name2 = consultation.Middle_Name2;
                existingRecord.Last_Name2 = consultation.Last_Name2;
                existingRecord.ContactNumber2 = consultation.ContactNumber2;
                existingRecord.Consultation_Date = consultation.Consultation_Date;
                existingRecord.Consultation_Time = consultation.Consultation_Time;
                existingRecord.Consultation_status = consultation.Consultation_status;

                // Validate and update files
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var maxFileSize = 5 * 1024 * 1024; // 5 MB

                // Barangay Certificate
                if (brgyCertFile != null && brgyCertFile.Length > 0)
                {
                    var extension = Path.GetExtension(brgyCertFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("brgyCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Barangay Certificate.");
                        return View(consultation);
                    }
                    if (brgyCertFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("brgyCertFile", "Barangay Certificate file size cannot exceed 5 MB.");
                        return View(consultation);
                    }

                    if (!string.IsNullOrEmpty(existingRecord.Brgy_Cert_Path2))
                    {
                        var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, existingRecord.Brgy_Cert_Path2.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "brgy_certs");
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

                    existingRecord.Brgy_Cert2 = fileName;
                    existingRecord.Brgy_Cert_Path2 = "/Uploads/brgy_certs/" + fileName;
                }

                // Proof of Solo Parent
                if (proofSoloParentFile != null && proofSoloParentFile.Length > 0)
                {
                    var extension = Path.GetExtension(proofSoloParentFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("proofSoloParentFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Proof of Solo Parent.");
                        return View(consultation);
                    }
                    if (proofSoloParentFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("proofSoloParentFile", "Proof of Solo Parent file size cannot exceed 5 MB.");
                        return View(consultation);
                    }

                    if (!string.IsNullOrEmpty(existingRecord.Proof_SoloParent_Path2))
                    {
                        var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, existingRecord.Proof_SoloParent_Path2.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "solo_parent_proofs");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(directoryPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await proofSoloParentFile.CopyToAsync(stream);
                    }

                    existingRecord.Proof_SoloParent2 = fileName;
                    existingRecord.Proof_SoloParent_Path2 = "/Uploads/solo_parent_proofs/" + fileName;
                }

                // Birth Certificate
                if (birthCertFile != null && birthCertFile.Length > 0)
                {
                    var extension = Path.GetExtension(birthCertFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("birthCertFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Birth Certificate.");
                        return View(consultation);
                    }
                    if (birthCertFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("birthCertFile", "Birth Certificate file size cannot exceed 5 MB.");
                        return View(consultation);
                    }

                    if (!string.IsNullOrEmpty(existingRecord.Birth_Cert_Path2))
                    {
                        var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, existingRecord.Birth_Cert_Path2.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "birth_certs");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(directoryPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await birthCertFile.CopyToAsync(stream);
                    }

                    existingRecord.Birth_Cert2 = fileName;
                    existingRecord.Birth_Cert_Path2 = "/Uploads/birth_certs/" + fileName;
                }

                // Valid ID
                if (validIdFile != null && validIdFile.Length > 0)
                {
                    var extension = Path.GetExtension(validIdFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("validIdFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for Valid ID.");
                        return View(consultation);
                    }
                    if (validIdFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("validIdFile", "Valid ID file size cannot exceed 5 MB.");
                        return View(consultation);
                    }

                    if (!string.IsNullOrEmpty(existingRecord.Valid_ID_Path2))
                    {
                        var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, existingRecord.Valid_ID_Path2.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "valid_ids");
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

                    existingRecord.Valid_ID2 = fileName;
                    existingRecord.Valid_ID_Path2 = "/Uploads/valid_ids/" + fileName;
                }

                // 1x1 Picture
                if (x1PicFile != null && x1PicFile.Length > 0)
                {
                    var extension = Path.GetExtension(x1PicFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("x1PicFile", "Only .jpg, .jpeg, .png, and .pdf files are allowed for 1x1 Picture.");
                        return View(consultation);
                    }
                    if (x1PicFile.Length > maxFileSize)
                    {
                        ModelState.AddModelError("x1PicFile", "1x1 Picture file size cannot exceed 5 MB.");
                        return View(consultation);
                    }

                    if (!string.IsNullOrEmpty(existingRecord.x1_Pic_Path2))
                    {
                        var oldFilePath = Path.Combine(_hostingEnvironment.WebRootPath, existingRecord.x1_Pic_Path2.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var directoryPath = Path.Combine(_hostingEnvironment.WebRootPath, "Uploads", "x1_pics");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fileName = Guid.NewGuid().ToString() + extension;
                    var filePath = Path.Combine(directoryPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await x1PicFile.CopyToAsync(stream);
                    }

                    existingRecord.x1_Pic2 = fileName;
                    existingRecord.x1_Pic_Path2 = "/Uploads/x1_pics/" + fileName;
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Update(existingRecord);
                await _context.SaveChangesAsync();

                var activity = new AuditTrail
                {
                    Action = "Edit",
                    TimeStamp = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserId = userId,
                    Moduie = "Consultation",
                    AffectedTable = "Consultations"
                };
                _context.Add(activity);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsultationExists(consultation.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: Consultations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultation = await _context.Consultations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (consultation == null)
            {
                return NotFound();
            }

            return View(consultation);
        }

        // POST: Consultations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation != null)
            {
                // Delete associated files
                if (!string.IsNullOrEmpty(consultation.Brgy_Cert_Path2))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, consultation.Brgy_Cert_Path2.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                if (!string.IsNullOrEmpty(consultation.Proof_SoloParent_Path2))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, consultation.Proof_SoloParent_Path2.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                if (!string.IsNullOrEmpty(consultation.Birth_Cert_Path2))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, consultation.Birth_Cert_Path2.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                if (!string.IsNullOrEmpty(consultation.Valid_ID_Path2))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, consultation.Valid_ID_Path2.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                if (!string.IsNullOrEmpty(consultation.x1_Pic_Path2))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, consultation.x1_Pic_Path2.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Consultations.Remove(consultation);
            }

            var activity = new AuditTrail
            {
                Action = "Delete",
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Consultation",
                AffectedTable = "Consultations"
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
            var picture = await _context.Consultations.FindAsync(id);
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
                picture.Consultation_status = ActiveStatus2.Process;
                auditAction = "Update Status to Process";
            }
            else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                picture.Consultation_status = ActiveStatus2.Denied;
                auditAction = "Rejected";
            }
            else
            {
                return BadRequest("Invalid status value. Only 'Process' or 'Rejected' are allowed.");
            }

            _context.Update(picture);
            await _context.SaveChangesAsync();

            // Send SMS message
            string message = picture.Consultation_status == ActiveStatus2.Process
                ? "Your request has been processed. Please wait for the confirmation."
                : "Your request has been rejected.";

            await _smsService.SendSmsAsync(picture.ContactNumber2, message);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var activity = new AuditTrail
            {
                Action = auditAction,
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Consultation",
                AffectedTable = "Consultations"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Picture/UpdateStatus/5
        [HttpPost]
        [Authorize(Roles = "Admin,Staff2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus1(int id, string status)
        {
            var picture = await _context.Consultations.FindAsync(id);
            if (picture == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                return BadRequest("Status is required.");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string auditAction;
            if (status.Equals("Accepted", StringComparison.OrdinalIgnoreCase))
            {
                picture.Consultation_status = ActiveStatus2.Approved;
                auditAction = "Accepted";
                picture.ApprovedById = userId;
            }
            else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                picture.Consultation_status = ActiveStatus2.Denied;
                auditAction = "Rejected";
                picture.ApprovedById = null;
            }
            else
            {
                return BadRequest("Invalid status value. Only 'Accepted' or 'Rejected' are allowed.");
            }

            _context.Update(picture);
            await _context.SaveChangesAsync();

            // Send SMS message
            string message = picture.Consultation_status == ActiveStatus2.Approved
                ? "Your request has been accepted. Please proceed to Magsaysay CSWDO Office, Door 11."
                : "Your request has been rejected.";

            await _smsService.SendSmsAsync(picture.ContactNumber2, message);


            var activity = new AuditTrail
            {
                Action = auditAction,
                TimeStamp = DateTime.Now,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserId = userId,
                Moduie = "Consultation",
                AffectedTable = "Consultations"
            };
            _context.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        private bool ConsultationExists(int id)
        {
            return _context.Consultations.Any(e => e.Id == id);
        }
    }
}