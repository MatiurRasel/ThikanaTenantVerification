using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ThikanaTenantVerification.Data;
using ThikanaTenantVerification.Models;
using ThikanaTenantVerification.ViewModels;

namespace ThikanaTenantVerification.Controllers
{
    public class SystemAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SystemAdminController> _logger;

        public SystemAdminController(ApplicationDbContext context, ILogger<SystemAdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(SystemAdminLoginVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var admin = await _context.SystemAdmins
                    .FirstOrDefaultAsync(a => a.Email == model.Email && a.IsActive);

                if (admin == null || !BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash))
                {
                    ModelState.AddModelError("", "ভুল ইমেইল বা পাসওয়ার্ড");
                    return View(model);
                }

                // Update last login
                admin.LastLogin = DateTime.Now;
                _context.Update(admin);
                await _context.SaveChangesAsync();

                // Create admin session
                HttpContext.Session.SetInt32("AdminId", admin.Id);
                HttpContext.Session.SetString("AdminName", admin.Name);
                HttpContext.Session.SetString("AdminEmail", admin.Email);
                HttpContext.Session.SetString("AdminAccessLevel", admin.AccessLevel);

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System admin login error");
                ModelState.AddModelError("", "একটি ত্রুটি হয়েছে। পরে চেষ্টা করুন");
                return View(model);
            }
        }

        [HttpGet]
        [AdminAuthorize("Super")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [AdminAuthorize("Super")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SystemAdminCreateVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Check if email already exists
                if (await _context.SystemAdmins.AnyAsync(a => a.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "এই ইমেইলটি ইতিমধ্যে রেজিস্টার্ড");
                    return View(model);
                }

                var admin = new SystemAdmin
                {
                    Name = model.Name,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Designation = model.Designation,
                    MobileNumber = model.MobileNumber,
                    NIDNumber = model.NIDNumber,
                    PoliceStation = model.PoliceStation,
                    Thana = model.Thana,
                    District = model.District,
                    Division = model.Division,
                    AccessLevel = model.AccessLevel,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.SystemAdmins.Add(admin);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "সিস্টেম অ্যাডমিন সফলভাবে তৈরি হয়েছে";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System admin creation error");
                ModelState.AddModelError("", "একটি ত্রুটি হয়েছে। পরে চেষ্টা করুন");
                return View(model);
            }
        }

        [HttpGet]
        [AdminAuthorize("Super,Division,District")]
        public async Task<IActionResult> List()
        {
            var currentAdminId = HttpContext.Session.GetInt32("AdminId");
            var currentAccessLevel = HttpContext.Session.GetString("AdminAccessLevel");

            IQueryable<SystemAdmin> query = _context.SystemAdmins;

            // Filter based on access level
            if (currentAccessLevel == "Division")
            {
                var currentAdmin = await _context.SystemAdmins.FindAsync(currentAdminId);
                query = query.Where(a => a.Division == currentAdmin.Division);
            }
            else if (currentAccessLevel == "District")
            {
                var currentAdmin = await _context.SystemAdmins.FindAsync(currentAdminId);
                query = query.Where(a => a.District == currentAdmin.District);
            }
            else if (currentAccessLevel == "Thana")
            {
                var currentAdmin = await _context.SystemAdmins.FindAsync(currentAdminId);
                query = query.Where(a => a.Thana == currentAdmin.Thana);
            }

            var admins = await query.OrderBy(a => a.Name).ToListAsync();
            return View(admins);
        }

        [HttpGet]
        [AdminAuthorize]
        public IActionResult Dashboard()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
                return RedirectToAction("Login");

            ViewBag.AdminName = HttpContext.Session.GetString("AdminName");
            ViewBag.AccessLevel = HttpContext.Session.GetString("AdminAccessLevel");

            return View();
        }

        [HttpGet]
        [AdminAuthorize]
        public async Task<IActionResult> Profile()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");
            if (adminId == null)
                return RedirectToAction("Login");

            var admin = await _context.SystemAdmins.FindAsync(adminId);
            if (admin == null)
            {
                TempData["ErrorMessage"] = "ব্যবহারকারী পাওয়া যায়নি";
                return RedirectToAction("Dashboard");
            }

            return View(admin);
        }

        [HttpPost]
        [AdminAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(SystemAdmin model)
        {
            if (!ModelState.IsValid)
                return View("Profile", model);

            try
            {
                var adminId = HttpContext.Session.GetInt32("AdminId");
                var admin = await _context.SystemAdmins.FindAsync(adminId);

                if (admin == null)
                {
                    TempData["ErrorMessage"] = "ব্যবহারকারী পাওয়া যায়নি";
                    return RedirectToAction("Dashboard");
                }

                admin.Name = model.Name;
                admin.Designation = model.Designation;
                admin.MobileNumber = model.MobileNumber;
                admin.NIDNumber = model.NIDNumber;
                admin.PoliceStation = model.PoliceStation;
                admin.Thana = model.Thana;
                admin.District = model.District;
                admin.Division = model.Division;

                _context.Update(admin);
                await _context.SaveChangesAsync();

                // Update session
                HttpContext.Session.SetString("AdminName", admin.Name);

                TempData["SuccessMessage"] = "প্রোফাইল সফলভাবে আপডেট করা হয়েছে";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profile update error");
                ModelState.AddModelError("", "একটি ত্রুটি হয়েছে। পরে চেষ্টা করুন");
                return View("Profile", model);
            }
        }

        [HttpGet]
        [AdminAuthorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [AdminAuthorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var adminId = HttpContext.Session.GetInt32("AdminId");
                var admin = await _context.SystemAdmins.FindAsync(adminId);

                if (admin == null)
                {
                    TempData["ErrorMessage"] = "ব্যবহারকারী পাওয়া যায়নি";
                    return RedirectToAction("Dashboard");
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, admin.PasswordHash))
                {
                    ModelState.AddModelError("CurrentPassword", "বর্তমান পাসওয়ার্ড ভুল");
                    return View(model);
                }

                // Update password
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                _context.Update(admin);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "পাসওয়ার্ড সফলভাবে পরিবর্তন করা হয়েছে";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password change error");
                ModelState.AddModelError("", "একটি ত্রুটি হয়েছে। পরে চেষ্টা করুন");
                return View(model);
            }
        }

        [HttpPost]
        [AdminAuthorize("Super")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var admin = await _context.SystemAdmins.FindAsync(id);
                if (admin == null)
                {
                    return Json(new { success = false, message = "অ্যাডমিন পাওয়া যায়নি" });
                }

                // Cannot deactivate super admin
                if (admin.AccessLevel == "Super" && admin.Id == HttpContext.Session.GetInt32("AdminId"))
                {
                    return Json(new { success = false, message = "আপনি নিজেকে নিষ্ক্রিয় করতে পারবেন না" });
                }

                admin.IsActive = !admin.IsActive;
                _context.Update(admin);
                await _context.SaveChangesAsync();

                var message = admin.IsActive ? "সক্রিয়" : "নিষ্ক্রিয়";
                return Json(new { success = true, message = $"অ্যাডমিন সফলভাবে {message} করা হয়েছে", isActive = admin.IsActive });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Toggle admin status error");
                return Json(new { success = false, message = "একটি ত্রুটি হয়েছে" });
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }

    // Custom Authorization Attribute
    public class AdminAuthorizeAttribute : TypeFilterAttribute
    {
        public AdminAuthorizeAttribute(string accessLevel = null) : base(typeof(AdminAuthorizeFilter))
        {
            Arguments = new object[] { accessLevel };
        }
    }

    public class AdminAuthorizeFilter : IAuthorizationFilter
    {
        private readonly string _requiredAccessLevel;

        public AdminAuthorizeFilter(string requiredAccessLevel)
        {
            _requiredAccessLevel = requiredAccessLevel;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var adminId = context.HttpContext.Session.GetInt32("AdminId");
            var currentAccessLevel = context.HttpContext.Session.GetString("AdminAccessLevel");

            if (adminId == null)
            {
                context.Result = new RedirectToActionResult("Login", "SystemAdmin", null);
                return;
            }

            if (!string.IsNullOrEmpty(_requiredAccessLevel))
            {
                var requiredLevels = _requiredAccessLevel.Split(',');
                if (!requiredLevels.Contains(currentAccessLevel))
                {
                    context.Result = new ForbidResult();
                }
            }
        }
    }
}
