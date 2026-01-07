using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ThikanaTenantVerification.Models;
using ThikanaTenantVerification.Services;

namespace ThikanaTenantVerification.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDataService _dataService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IDataService dataService, ILogger<HomeController> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string idNumber, string mobileNumber)
        {
            try
            {
                // Validate ID number length
                if (string.IsNullOrEmpty(idNumber) || (idNumber.Length != 10 && idNumber.Length != 13 &&
                    idNumber.Length != 17 && idNumber.Length != 14))
                {
                    ViewBag.Error = "অনুগ্রহ করে সঠিক জাতীয় পরিচয়পত্র বা জন্ম নিবন্ধন নম্বর দিন (১০/১৩/১৪/১৭ ডিজিট)";
                    return View();
                }

                // Check if user exists in database (registered user)
                var existingUser = await _dataService.GetUserByNIDAndMobile(idNumber, mobileNumber);

                if (existingUser != null)
                {
                    // This is an existing registered user
                    TempData["MobileNumber"] = mobileNumber;
                    TempData["UserId"] = existingUser.Id.ToString();
                    TempData["IsNewUser"] = false; // Existing user

                    return RedirectToAction("VerifyOTP");
                }

                // Check if this is a new user (NID exists but not registered)
                var nidData = await _dataService.GetNIDDataAsync(idNumber);
                if (nidData == null)
                {
                    ViewBag.Error = "জাতীয় পরিচয়পত্র/জন্ম নিবন্ধন নম্বর পাওয়া যায়নি";
                    return View();
                }

                if (nidData.MobileNumber != mobileNumber)
                {
                    ViewBag.Error = "মোবাইল নম্বর মিলছে না";
                    return View();
                }

                // This is a new user - store NID data for registration
                TempData["NIDData"] = JsonSerializer.Serialize(nidData);
                TempData["MobileNumber"] = mobileNumber;
                TempData["IsNewUser"] = true; // New user needs password setup

                return RedirectToAction("VerifyOTP");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                ViewBag.Error = "একটি ত্রুটি হয়েছে। পরে চেষ্টা করুন";
                return View();
            }
        }

        // Add this method for password strength validation
        private bool IsPasswordStrong(string password)
        {
            // Check for at least 8 characters
            if (password.Length < 8) return false;

            // Check for at least one uppercase letter
            if (!password.Any(char.IsUpper)) return false;

            // Check for at least one lowercase letter
            if (!password.Any(char.IsLower)) return false;

            // Check for at least one digit
            if (!password.Any(char.IsDigit)) return false;

            // Check for at least one special character
            var specialCharacters = "!@#$%^&*";
            if (!password.Any(c => specialCharacters.Contains(c))) return false;

            return true;
        }

        [HttpPost]
        public async Task<IActionResult> UserLogin(string username, string password)
        {
            try
            {
                // Check if username is NID or mobile
                var user = await _dataService.AuthenticateUser(username, password);

                if (user == null)
                {
                    ViewBag.Error = "ভুল NID/মোবাইল নম্বর বা পাসওয়ার্ড";
                    return View("UserLogin");
                }

                // Set session
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.FullNameBN);
                HttpContext.Session.SetString("UserNID", user.NIDNumber ?? "");

                // Record login
                await _dataService.RecordLogin(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());

                return RedirectToAction("Dashboard", new { id = user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User login error");
                ViewBag.Error = "লগইনে সমস্যা হয়েছে। পরে চেষ্টা করুন";
                return View("UserLogin");
            }
        }

        [HttpGet]
        public IActionResult UserLogin()
        {
            // If already logged in, redirect to dashboard
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return RedirectToAction("Dashboard", new { id = userId.Value });
            }

            return View();
        }

        [HttpGet]
        public IActionResult VerifyOTP()
        {
            if (TempData["MobileNumber"] == null)
                return RedirectToAction("Login");

            ViewBag.MobileNumber = TempData["MobileNumber"];
            ViewBag.IsNewUser = TempData["IsNewUser"] != null;
            ViewBag.UserId = TempData["UserId"]?.ToString();

            TempData.Keep("MobileNumber");
            TempData.Keep("NIDData");
            TempData.Keep("UserId");
            TempData.Keep("IsNewUser");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(string otp, string password, string confirmPassword,
            bool isNewUser = false, int? userId = null)
        {
            // Default OTP value for bypassing verification
            const string DEFAULT_OTP = "123456";

            var mobileNumber = TempData["MobileNumber"]?.ToString();
            var nidDataJson = TempData["NIDData"]?.ToString();
            var storedUserId = TempData["UserId"]?.ToString();

            if (string.IsNullOrEmpty(mobileNumber))
                return RedirectToAction("Login");

            // Accept either the default OTP or any 6-digit OTP for testing
            bool isValidOtp = otp == DEFAULT_OTP || (otp?.Length == 6 && otp.All(char.IsDigit));

            if (!isValidOtp)
            {
                ViewBag.Error = "ভুল OTP। ডিফল্ট OTP: 123456";
                ViewBag.MobileNumber = mobileNumber;
                ViewBag.IsNewUser = isNewUser;
                ViewBag.UserId = storedUserId;

                TempData.Keep("MobileNumber");
                TempData.Keep("NIDData");
                TempData.Keep("UserId");
                TempData.Keep("IsNewUser");

                return View();
            }

            // For existing users (login)
            if (!isNewUser && !string.IsNullOrEmpty(storedUserId) && int.TryParse(storedUserId, out int existingUserId))
            {
                var user = await _dataService.GetUserById(existingUserId);
                if (user != null)
                {
                    // Set user session
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.FullNameBN);
                    HttpContext.Session.SetString("UserNID", user.NIDNumber ?? "");

                    return RedirectToAction("Dashboard", new { id = user.Id });
                }
            }

            // For new user registration with password setup
            if (isNewUser && !string.IsNullOrEmpty(nidDataJson))
            {
                // Validate passwords
                if (string.IsNullOrEmpty(password) || password.Length < 8)
                {
                    ViewBag.Error = "অনুগ্রহ করে কমপক্ষে ৮ অক্ষরের পাসওয়ার্ড দিন";
                    ViewBag.MobileNumber = mobileNumber;
                    ViewBag.IsNewUser = true;

                    TempData.Keep("MobileNumber");
                    TempData.Keep("NIDData");
                    TempData.Keep("IsNewUser");

                    return View();
                }

                if (password != confirmPassword)
                {
                    ViewBag.Error = "পাসওয়ার্ড মিলছে না";
                    ViewBag.MobileNumber = mobileNumber;
                    ViewBag.IsNewUser = true;

                    TempData.Keep("MobileNumber");
                    TempData.Keep("NIDData");
                    TempData.Keep("IsNewUser");

                    return View();
                }

                // Validate password strength
                if (!IsPasswordStrong(password))
                {
                    ViewBag.Error = "পাসওয়ার্ডে কমপক্ষে ১টি বড় হাতের অক্ষর, ১টি ছোট হাতের অক্ষর, ১টি সংখ্যা এবং ১টি বিশেষ অক্ষর থাকতে হবে";
                    ViewBag.MobileNumber = mobileNumber;
                    ViewBag.IsNewUser = true;

                    TempData.Keep("MobileNumber");
                    TempData.Keep("NIDData");
                    TempData.Keep("IsNewUser");

                    return View();
                }

                // Create new user from NID data with password
                var nidData = JsonSerializer.Deserialize<NIDData>(nidDataJson);
                var newUser = await _dataService.CreateUserFromNIDDataWithPassword(nidData, password);

                if (newUser == null)
                {
                    ViewBag.Error = "ব্যবহারকারী তৈরি করতে সমস্যা হয়েছে";
                    ViewBag.MobileNumber = mobileNumber;
                    ViewBag.IsNewUser = true;

                    TempData.Keep("MobileNumber");
                    TempData.Keep("NIDData");
                    TempData.Keep("IsNewUser");

                    return View();
                }

                // Set user session
                HttpContext.Session.SetInt32("UserId", newUser.Id);
                HttpContext.Session.SetString("UserName", newUser.FullNameBN);
                HttpContext.Session.SetString("UserNID", newUser.NIDNumber ?? "");

                TempData["Success"] = "রেজিস্ট্রেশন সফল! আপনার অ্যাকাউন্ট তৈরি হয়েছে।";

                return RedirectToAction("Dashboard", new { id = newUser.Id });
            }

            // Fallback
            return RedirectToAction("Login");
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(int id)
        {
            var user = await _dataService.GetUserById(id);
            if (user == null)
                return RedirectToAction("Login");

            var completionPercentage = await _dataService.CalculateCompletionPercentage(id);
            user.CompletionPercentage = completionPercentage;
            await _dataService.UpdateUser(user);

            ViewBag.CompletionPercentage = completionPercentage;
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EmergencyContact(int userId)
        {
            var user = await _dataService.GetUserById(userId);
            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmergencyContact(int userId, EmergencyContact contact)
        {
            await _dataService.AddEmergencyContact(userId, contact);
            return RedirectToAction("Dashboard", new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> FamilyMembers(int userId)
        {
            var user = await _dataService.GetUserById(userId);
            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> AddFamilyMember(int userId, FamilyMember member)
        {
            // Validate NID if provided
            if (!string.IsNullOrEmpty(member.NIDNumber))
            {
                var nidData = await _dataService.GetNIDDataAsync(member.NIDNumber);
                member.IsNIDVerified = nidData != null;
            }

            await _dataService.AddFamilyMember(userId, member);
            return RedirectToAction("Dashboard", new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> HouseWorkers(int userId)
        {
            var user = await _dataService.GetUserById(userId);
            if (user == null)
                return RedirectToAction("Login");

            var policeData = await _dataService.GetPoliceVerificationData();
            ViewBag.PoliceData = policeData;
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> AddHouseWorker(int userId, HouseWorker worker)
        {
            // Police verification
            var policeData = await _dataService.GetPoliceVerificationForNID(worker.NIDNumber);
            if (policeData != null)
            {
                worker.IsValidFromPolice = policeData.IsValid;
                worker.ValidationMessage = policeData.ValidationMessage;
                worker.IsDangerFlag = policeData.DangerLevel == "উচ্চ";
            }

            await _dataService.AddHouseWorker(userId, worker);
            return RedirectToAction("Dashboard", new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> CurrentResidence(int userId)
        {
            var user = await _dataService.GetUserById(userId);
            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCurrentResidence(int userId, CurrentResidence residence)
        {
            await _dataService.SaveCurrentResidence(userId, residence);

            // Check completion and notify landlord if > 90%
            var user = await _dataService.GetUserById(userId);
            var completion = user?.CompletionPercentage ?? 0;

            if (completion >= 90 && user != null)
            {
                await _dataService.NotifyLandlord(user);
            }

            return RedirectToAction("Dashboard", new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> CurrentLandlord(int userId)
        {
            var user = await _dataService.GetUserById(userId);
            if (user == null)
                return RedirectToAction("Login");

            var currentLandlord = await _dataService.GetCurrentLandlord(userId);
            ViewBag.UserId = userId;

            return View(currentLandlord);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCurrentLandlord(int userId, CurrentLandlord model)
        {
            // Validate NID if provided
            if (!string.IsNullOrEmpty(model.NIDNumber))
            {
                var nidData = await _dataService.GetNIDDataAsync(model.NIDNumber);
                if (nidData != null)
                {
                    model.IsVerified = true;
                    model.VerificationDate = DateTime.Now;
                }
            }

            await _dataService.SaveCurrentLandlord(userId, model);

            // Create notification for landlord
            var user = await _dataService.GetUserById(userId);
            if (user != null)
            {
                await _dataService.CreateNotification(
                    landlordId: null, // In real app, get landlord ID from model
                    tenantId: userId,
                    messageBN: $"ভাড়াটিয়া {user.FullNameBN} এর তথ্য আপডেট হয়েছে",
                    messageEN: $"Tenant {user.FullNameEN} information updated",
                    type: "Update",
                    isImportant: true
                );
            }

            return RedirectToAction("Dashboard", new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> Documents(int userId)
        {
            var user = await _dataService.GetUserById(userId);
            if (user == null)
                return RedirectToAction("Login");

            var documents = await _dataService.GetUserDocuments(userId);
            ViewBag.UserId = userId;

            return View(documents);
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocument(int userId, IFormFile file, string documentType)
        {
            if (file != null && file.Length > 0)
            {
                // In real app, save file to server
                var document = new DocumentAttachment
                {
                    DocumentType = documentType,
                    FileName = file.FileName,
                    FilePath = $"/uploads/{userId}/{file.FileName}",
                    FileSize = file.Length,
                    MimeType = file.ContentType
                };

                await _dataService.AddDocumentAttachment(userId, document);
            }

            return RedirectToAction("Documents", new { id = userId });
        }

        [HttpGet]
        public async Task<IActionResult> VerificationLogs(int userId)
        {
            var user = await _dataService.GetUserById(userId);
            if (user == null)
                return RedirectToAction("Login");

            var logs = await _dataService.GetVerificationLogs(userId);
            ViewBag.UserId = userId;

            return View(logs);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadVerification(int userId)
        {
            var user = await _dataService.GetUserById(userId);
            if (user == null || user.CompletionPercentage < 90)
            {
                return RedirectToAction("Dashboard", new { id = userId });
            }

            var pdfContent = await _dataService.GenerateVerificationPDF(user);
            return File(pdfContent, "application/pdf", $"TenantVerification_{user.NIDNumber}.pdf");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Add these methods to your HomeController class

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerifyNID(string idNumber, string mobileNumber)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(idNumber) || string.IsNullOrEmpty(mobileNumber))
                {
                    return Json(new { success = false, message = "অনুগ্রহ করে NID এবং মোবাইল নম্বর দিন" });
                }

                // Validate NID length
                if (!(new[] { 10, 13, 14, 17 }.Contains(idNumber.Length)))
                {
                    return Json(new { success = false, message = "অনুগ্রহ করে সঠিক জাতীয় পরিচয়পত্র বা জন্ম নিবন্ধন নম্বর দিন (১০/১৩/১৪/১৭ ডিজিট)" });
                }

                // Validate mobile number
                if (!System.Text.RegularExpressions.Regex.IsMatch(mobileNumber, @"^01[3-9]\d{8}$"))
                {
                    return Json(new { success = false, message = "অনুগ্রহ করে সঠিক মোবাইল নম্বর দিন" });
                }

                // Check if user already exists
                var existingUser = await _dataService.GetUserByNIDAndMobile(idNumber, mobileNumber);
                if (existingUser != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "এই NID এবং মোবাইল নম্বর দিয়ে ইতিমধ্যে রেজিস্ট্রেশন করা হয়েছে। লগইন করুন।"
                    });
                }

                // Get NID data (simulated for demo)
                var nidData = await _dataService.GetNIDDataAsync(idNumber);
                if (nidData == null)
                {
                    // For demo, create mock NID data
                    nidData = new NIDData
                    {
                        NIDNumber = idNumber,
                        FullNameBN = "ডেমো ব্যবহারকারী",
                        FullNameEN = "Demo User",
                        FatherNameBN = "ডেমো পিতার নাম",
                        FatherNameEN = "Demo Father Name",
                        MotherNameBN = "ডেমো মাতার নাম",
                        MotherNameEN = "Demo Mother Name",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        Gender = "পুরুষ",
                        MaritalStatus = "অবিবাহিত",
                        Religion = "ইসলাম",
                        MobileNumber = mobileNumber,
                        Email = "demo@example.com",
                        PermanentAddress = "ডেমো ঠিকানা, ঢাকা"
                    };
                }

                // Save NID data in TempData for later steps
                TempData["NIDData"] = JsonSerializer.Serialize(nidData);
                TempData["MobileNumber"] = mobileNumber;
                TempData["IsNewUser"] = true;

                return Json(new
                {
                    success = true,
                    message = "NID যাচাই সফল! OTP পাঠানো হয়েছে।",
                    nidData = nidData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NID verification error");
                return Json(new { success = false, message = "একটি ত্রুটি হয়েছে। পরে চেষ্টা করুন" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CompleteRegistration(string idNumber, string mobileNumber,
            string password, string confirmPassword)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(password) || password.Length < 8)
                {
                    return Json(new { success = false, message = "অনুগ্রহ করে কমপক্ষে ৮ অক্ষরের পাসওয়ার্ড দিন" });
                }

                if (password != confirmPassword)
                {
                    return Json(new { success = false, message = "পাসওয়ার্ড মিলছে না" });
                }

                // Check password requirements
                if (!IsPasswordStrong(password))
                {
                    return Json(new
                    {
                        success = false,
                        message = "পাসওয়ার্ডে কমপক্ষে ১টি বড় হাতের অক্ষর, ১টি ছোট হাতের অক্ষর, ১টি সংখ্যা এবং ১টি বিশেষ অক্ষর থাকতে হবে"
                    });
                }

                // Get or create NID data
                var nidData = await _dataService.GetNIDDataAsync(idNumber);
                if (nidData == null)
                {
                    // Create demo NID data
                    nidData = new NIDData
                    {
                        NIDNumber = idNumber,
                        FullNameBN = "ডেমো ব্যবহারকারী",
                        FullNameEN = "Demo User",
                        FatherNameBN = "ডেমো পিতার নাম",
                        FatherNameEN = "Demo Father Name",
                        MotherNameBN = "ডেমো মাতার নাম",
                        MotherNameEN = "Demo Mother Name",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        Gender = "পুরুষ",
                        MaritalStatus = "অবিবাহিত",
                        Religion = "ইসলাম",
                        MobileNumber = mobileNumber,
                        Email = "demo@example.com",
                        PermanentAddress = "ডেমো ঠিকানা, ঢাকা"
                    };
                }

                // Create user with password
                var newUser = await _dataService.CreateUserFromNIDDataWithPassword(nidData, password);

                if (newUser == null)
                {
                    return Json(new { success = false, message = "ব্যবহারকারী তৈরি করতে সমস্যা হয়েছে" });
                }

                return Json(new
                {
                    success = true,
                    message = "রেজিস্ট্রেশন সফল! লগইন পৃষ্ঠায় নিয়ে যাওয়া হচ্ছে...",
                    userId = newUser.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                return Json(new { success = false, message = "একটি ত্রুটি হয়েছে। পরে চেষ্টা করুন" });
            }
        }

    }
}