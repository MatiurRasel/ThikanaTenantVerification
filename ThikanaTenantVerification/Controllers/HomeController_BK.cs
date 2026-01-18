//using System.Diagnostics;
//using System.Text.Json;
//using Microsoft.AspNetCore.Identity.Data;
//using Microsoft.AspNetCore.Mvc;
//using ThikanaTenantVerification.Models;
//using ThikanaTenantVerification.Services;

//namespace ThikanaTenantVerification.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly IDataService _dataService;
//        private readonly ILogger<HomeController> _logger;

//        public HomeController(IDataService dataService, ILogger<HomeController> logger)
//        {
//            _dataService = dataService;
//            _logger = logger;
//        }

//        public IActionResult Index()
//        {
//            return View();
//        }

//        [HttpGet]
//        public IActionResult Login()
//        {
//            // If already logged in, redirect to dashboard
//            var userId = HttpContext.Session.GetInt32("UserId");
//            if (userId.HasValue)
//            {
//                return RedirectToAction("Dashboard", new { id = userId.Value });
//            }

//            return View();
//        }


//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Login(string username, string password)
//        {
//            try
//            {
//                // Login with mobile number and password
//                var user = await _dataService.AuthenticateUser(username, password);

//                if (user == null)
//                {
//                    ViewBag.Error = "ভুল মোবাইল নম্বর বা পাসওয়ার্ড";
//                    return View();
//                }

//                // Set session
//                HttpContext.Session.SetInt32("UserId", user.Id);
//                HttpContext.Session.SetString("UserName", user.FullNameBN);
//                HttpContext.Session.SetString("UserNID", user.NIDNumber ?? "");

//                // Record login
//                await _dataService.RecordLogin(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());

//                return RedirectToAction("Dashboard", new { id = user.Id });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "User login error");
//                ViewBag.Error = "লগইনে সমস্যা হয়েছে। পরে চেষ্টা করুন";
//                return View();
//            }
//        }

//        // POST: Home/RegisterDirect (Fallback method without AJAX)
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult RegisterDirect(string idNumber, string mobileNumber)
//        {
//            _logger.LogInformation($"RegisterDirect called with NID: {idNumber}, Mobile: {mobileNumber}");

//            // Simple validation
//            if (string.IsNullOrEmpty(idNumber) || string.IsNullOrEmpty(mobileNumber))
//            {
//                TempData["Error"] = "অনুগ্রহ করে NID এবং মোবাইল নম্বর দিন";
//                return RedirectToAction("Login");
//            }

//            // Store in TempData
//            TempData["MobileNumber"] = mobileNumber;
//            TempData["RegistrationStep"] = "otp";

//            // For demo, create mock NID data
//            var nidData = new NIDData
//            {
//                NIDNumber = idNumber,
//                FullNameBN = "ডেমো ব্যবহারকারী",
//                FullNameEN = "Demo User",
//                FatherNameBN = "ডেমো পিতার নাম",
//                FatherNameEN = "Demo Father Name",
//                MotherNameBN = "ডেমো মাতার নাম",
//                MotherNameEN = "Demo Mother Name",
//                DateOfBirth = new DateTime(1990, 1, 1),
//                Gender = "পুরুষ",
//                MaritalStatus = "অবিবাহিত",
//                Religion = "ইসলাম",
//                MobileNumber = mobileNumber,
//                Email = "demo@example.com",
//                PermanentAddress = "ডেমো ঠিকানা, ঢাকা"
//            };

//            TempData["NIDData"] = JsonSerializer.Serialize(nidData);
//            _logger.LogInformation($"RegisterDirect successful, redirecting to VerifyOTP");

//            return RedirectToAction("VerifyOTP");
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Register(string idNumber, string mobileNumber)
//        {
//            try
//            {
//                _logger.LogInformation($"Register called with NID: {idNumber}, Mobile: {mobileNumber}");
//                // Validate ID number length
//                if (string.IsNullOrEmpty(idNumber) ||
//                    !(new[] { 10, 13, 14, 17 }.Contains(idNumber.Length)))
//                {
//                    _logger.LogWarning($"Invalid NID length: {idNumber?.Length}");

//                    return Json(new
//                    {
//                        success = false,
//                        message = "অনুগ্রহ করে সঠিক জাতীয় পরিচয়পত্র বা জন্ম নিবন্ধন নম্বর দিন (১০/১৩/১৪/১৭ ডিজিট)"
//                    });
//                }

//                // Validate mobile number
//                if (string.IsNullOrEmpty(mobileNumber) ||
//                    !System.Text.RegularExpressions.Regex.IsMatch(mobileNumber, @"^01[3-9]\d{8}$"))
//                {
//                    _logger.LogWarning($"Invalid mobile: {mobileNumber}");

//                    return Json(new
//                    {
//                        success = false,
//                        message = "অনুগ্রহ করে সঠিক মোবাইল নম্বর দিন"
//                    });
//                }

//                // Check if user already exists
//                var existingUser = await _dataService.GetUserByNIDAndMobile(idNumber, mobileNumber);
//                if (existingUser != null)
//                {
//                    _logger.LogInformation($"User already exists: {idNumber}");

//                    return Json(new
//                    {
//                        success = false,
//                        message = "এই NID এবং মোবাইল নম্বর দিয়ে ইতিমধ্যে রেজিস্ট্রেশন করা হয়েছে। লগইন করুন।"
//                    });
//                }

//                // Get NID data (for demo, create mock if not found)
//                var nidData = await _dataService.GetNIDDataAsync(idNumber);
//                if (nidData == null)
//                {
//                    _logger.LogInformation($"Creating demo NID data for: {idNumber}");

//                    // For demo, create mock NID data
//                    nidData = new NIDData
//                    {
//                        NIDNumber = idNumber,
//                        FullNameBN = "ডেমো ব্যবহারকারী",
//                        FullNameEN = "Demo User",
//                        FatherNameBN = "ডেমো পিতার নাম",
//                        FatherNameEN = "Demo Father Name",
//                        MotherNameBN = "ডেমো মাতার নাম",
//                        MotherNameEN = "Demo Mother Name",
//                        DateOfBirth = new DateTime(1990, 1, 1),
//                        Gender = "পুরুষ",
//                        MaritalStatus = "অবিবাহিত",
//                        Religion = "ইসলাম",
//                        MobileNumber = mobileNumber,
//                        Email = "demo@example.com",
//                        PermanentAddress = "ডেমো ঠিকানা, ঢাকা"
//                    };
//                }

//                // Verify mobile matches NID
//                if (nidData.MobileNumber != mobileNumber)
//                {
//                    // For demo, update mobile if doesn't match
//                    nidData.MobileNumber = mobileNumber;
//                }

//                // Store registration data in TempData
//                TempData["NIDData"] = JsonSerializer.Serialize(nidData);
//                TempData["MobileNumber"] = mobileNumber;
//                TempData["RegistrationStep"] = "otp";
                
//                _logger.LogInformation($"Registration successful for NID: {idNumber}");

//                return Json(new
//                {
//                    success = true,
//                    message = "NID যাচাই সফল! OTP পাঠানো হয়েছে।",
//                    mobileNumber = mobileNumber
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Registration error");
//                return Json(new { success = false, message = "একটি ত্রুটি হয়েছে। পরে চেষ্টা করুন" });
//            }
//        }

//        [HttpGet]
//        public IActionResult VerifyOTP()
//        {
//            if (TempData["MobileNumber"] == null)
//                return RedirectToAction("Login");

//            ViewBag.MobileNumber = TempData["MobileNumber"];
//            ViewBag.NIDData = TempData["NIDData"]?.ToString();
//            ViewBag.RegistrationStep = TempData["RegistrationStep"]?.ToString() ?? "otp";

//            TempData.Keep("MobileNumber");
//            TempData.Keep("NIDData");
//            TempData.Keep("RegistrationStep");

//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> VerifyOTP(string otp, string password = null, string confirmPassword = null)
//        {
//            const string DEFAULT_OTP = "123456";
//            var mobileNumber = TempData["MobileNumber"]?.ToString();
//            var nidDataJson = TempData["NIDData"]?.ToString();
//            var step = TempData["RegistrationStep"]?.ToString();

//            if (string.IsNullOrEmpty(mobileNumber))
//                return RedirectToAction("Login");

//            // Accept either default OTP or any 6-digit OTP for testing
//            bool isValidOtp = otp == DEFAULT_OTP || (otp?.Length == 6 && otp.All(char.IsDigit));

//            if (!isValidOtp)
//            {
//                ViewBag.Error = "ভুল OTP। ডিফল্ট OTP: 123456";
//                ViewBag.MobileNumber = mobileNumber;
//                ViewBag.NIDData = nidDataJson;
//                ViewBag.RegistrationStep = step;

//                TempData.Keep("MobileNumber");
//                TempData.Keep("NIDData");
//                TempData.Keep("RegistrationStep");

//                return View();
//            }

//            // If step is otp (first verification), move to password step
//            if (step == "otp" && string.IsNullOrEmpty(password))
//            {
//                TempData["RegistrationStep"] = "password";
//                TempData["StoredOTP"] = otp; // STORE THE OTP HERE
//                TempData.Keep("MobileNumber");
//                TempData.Keep("NIDData");

//                ViewBag.MobileNumber = mobileNumber;
//                ViewBag.NIDData = nidDataJson;
//                ViewBag.RegistrationStep = "password";
//                ViewBag.StoredOTP = otp; // PASS TO VIEW

//                return View();
//            }

//            // If step is password, complete registration
//            if (step == "password" && !string.IsNullOrEmpty(password))
//            {
//                // The OTP should come from TempData now
//                var storedOTP = TempData["StoredOTP"]?.ToString() ?? otp;

//                // Validate passwords
//                if (password.Length < 8)
//                {
//                    ViewBag.Error = "অনুগ্রহ করে কমপক্ষে ৮ অক্ষরের পাসওয়ার্ড দিন";
//                    ViewBag.MobileNumber = mobileNumber;
//                    ViewBag.NIDData = nidDataJson;
//                    ViewBag.RegistrationStep = "password";
//                    ViewBag.StoredOTP = storedOTP;

//                    TempData.Keep("MobileNumber");
//                    TempData.Keep("NIDData");
//                    TempData.Keep("RegistrationStep");
//                    TempData.Keep("StoredOTP");

//                    return View();
//                }

//                if (password != confirmPassword)
//                {
//                    ViewBag.Error = "পাসওয়ার্ড মিলছে না";
//                    ViewBag.MobileNumber = mobileNumber;
//                    ViewBag.NIDData = nidDataJson;
//                    ViewBag.RegistrationStep = "password";
//                    ViewBag.StoredOTP = storedOTP;

//                    TempData.Keep("MobileNumber");
//                    TempData.Keep("NIDData");
//                    TempData.Keep("RegistrationStep");
//                    TempData.Keep("StoredOTP");

//                    return View();
//                }

//                // Validate password strength
//                if (!IsPasswordStrong(password))
//                {
//                    ViewBag.Error = "পাসওয়ার্ডে কমপক্ষে ১টি বড় হাতের অক্ষর, ১টি ছোট হাতের অক্ষর, ১টি সংখ্যা এবং ১টি বিশেষ অক্ষর থাকতে হবে";
//                    ViewBag.MobileNumber = mobileNumber;
//                    ViewBag.NIDData = nidDataJson;
//                    ViewBag.RegistrationStep = "password";
//                    ViewBag.StoredOTP = storedOTP;

//                    TempData.Keep("MobileNumber");
//                    TempData.Keep("NIDData");
//                    TempData.Keep("RegistrationStep");
//                    TempData.Keep("StoredOTP");

//                    return View();
//                }

//                // Create user from NID data
//                var nidData = JsonSerializer.Deserialize<NIDData>(nidDataJson);
//                if (nidData == null)
//                {
//                    ViewBag.Error = "NID তথ্য পাওয়া যায়নি";
//                    return View();
//                }

//                var newUser = await _dataService.CreateUserFromNIDDataWithPassword(nidData, password);

//                if (newUser == null)
//                {
//                    ViewBag.Error = "ব্যবহারকারী তৈরি করতে সমস্যা হয়েছে";
//                    return View();
//                }

//                // Set user session
//                HttpContext.Session.SetInt32("UserId", newUser.Id);
//                HttpContext.Session.SetString("UserName", newUser.FullNameBN);
//                HttpContext.Session.SetString("UserNID", newUser.NIDNumber ?? "");

//                // Clear TempData
//                TempData.Remove("MobileNumber");
//                TempData.Remove("NIDData");
//                TempData.Remove("RegistrationStep");
//                TempData.Remove("StoredOTP");

//                return RedirectToAction("Dashboard", new { id = newUser.Id });
//            }

//            return RedirectToAction("Login");
//        }
//        private bool IsPasswordStrong(string password)
//        {
//            if (password.Length < 8) return false;
//            if (!password.Any(char.IsUpper)) return false;
//            if (!password.Any(char.IsLower)) return false;
//            if (!password.Any(char.IsDigit)) return false;

//            var specialCharacters = "!@#$%^&*";
//            if (!password.Any(c => specialCharacters.Contains(c))) return false;

//            return true;
//        }

//        // Add this class for registration request
//        public class RegisterRequest
//        {
//            public string IdNumber { get; set; }
//            public string MobileNumber { get; set; }
//        }


//        [HttpGet]
//        public async Task<IActionResult> Dashboard(int id)
//        {
//            var user = await _dataService.GetUserById(id);
//            if (user == null)
//                return RedirectToAction("Login");

//            var completionPercentage = await _dataService.CalculateCompletionPercentage(id);
//            user.CompletionPercentage = completionPercentage;
//            await _dataService.UpdateUser(user);

//            ViewBag.CompletionPercentage = completionPercentage;
//            return View(user);
//        }

//        [HttpGet]
//        public async Task<IActionResult> EmergencyContact(int userId)
//        {
//            var user = await _dataService.GetUserById(userId);
//            if (user == null)
//                return RedirectToAction("Login");

//            return View(user);
//        }

//        [HttpPost]
//        public async Task<IActionResult> AddEmergencyContact(int userId, EmergencyContact contact)
//        {
//            await _dataService.AddEmergencyContact(userId, contact);
//            return RedirectToAction("Dashboard", new { id = userId });
//        }

//        [HttpGet]
//        public async Task<IActionResult> FamilyMembers(int userId)
//        {
//            var user = await _dataService.GetUserById(userId);
//            if (user == null)
//                return RedirectToAction("Login");

//            return View(user);
//        }

//        [HttpPost]
//        public async Task<IActionResult> AddFamilyMember(int userId, FamilyMember member)
//        {
//            // Validate NID if provided
//            if (!string.IsNullOrEmpty(member.NIDNumber))
//            {
//                var nidData = await _dataService.GetNIDDataAsync(member.NIDNumber);
//                member.IsNIDVerified = nidData != null;
//            }

//            await _dataService.AddFamilyMember(userId, member);
//            return RedirectToAction("Dashboard", new { id = userId });
//        }

//        [HttpGet]
//        public async Task<IActionResult> HouseWorkers(int userId)
//        {
//            var user = await _dataService.GetUserById(userId);
//            if (user == null)
//                return RedirectToAction("Login");

//            var policeData = await _dataService.GetPoliceVerificationData();
//            ViewBag.PoliceData = policeData;
//            return View(user);
//        }

//        [HttpPost]
//        public async Task<IActionResult> AddHouseWorker(int userId, HouseWorker worker)
//        {
//            // Police verification
//            var policeData = await _dataService.GetPoliceVerificationForNID(worker.NIDNumber);
//            if (policeData != null)
//            {
//                worker.IsValidFromPolice = policeData.IsValid;
//                worker.ValidationMessage = policeData.ValidationMessage;
//                worker.IsDangerFlag = policeData.DangerLevel == "উচ্চ";
//            }

//            await _dataService.AddHouseWorker(userId, worker);
//            return RedirectToAction("Dashboard", new { id = userId });
//        }

//        [HttpGet]
//        public async Task<IActionResult> CurrentResidence(int userId)
//        {
//            var user = await _dataService.GetUserById(userId);
//            if (user == null)
//                return RedirectToAction("Login");

//            return View(user);
//        }

//        [HttpPost]
//        public async Task<IActionResult> SaveCurrentResidence(int userId, CurrentResidence residence)
//        {
//            await _dataService.SaveCurrentResidence(userId, residence);

//            // Check completion and notify landlord if > 90%
//            var user = await _dataService.GetUserById(userId);
//            var completion = user?.CompletionPercentage ?? 0;

//            if (completion >= 90 && user != null)
//            {
//                await _dataService.NotifyLandlord(user);
//            }

//            return RedirectToAction("Dashboard", new { id = userId });
//        }

//        [HttpGet]
//        public async Task<IActionResult> CurrentLandlord(int userId)
//        {
//            var user = await _dataService.GetUserById(userId);
//            if (user == null)
//                return RedirectToAction("Login");

//            var currentLandlord = await _dataService.GetCurrentLandlord(userId);
//            ViewBag.UserId = userId;

//            return View(currentLandlord);
//        }

//        [HttpPost]
//        public async Task<IActionResult> SaveCurrentLandlord(int userId, CurrentLandlord model)
//        {
//            // Validate NID if provided
//            if (!string.IsNullOrEmpty(model.NIDNumber))
//            {
//                var nidData = await _dataService.GetNIDDataAsync(model.NIDNumber);
//                if (nidData != null)
//                {
//                    model.IsVerified = true;
//                    model.VerificationDate = DateTime.Now;
//                }
//            }

//            await _dataService.SaveCurrentLandlord(userId, model);

//            // Create notification for landlord
//            var user = await _dataService.GetUserById(userId);
//            if (user != null)
//            {
//                await _dataService.CreateNotification(
//                    landlordId: null, // In real app, get landlord ID from model
//                    tenantId: userId,
//                    messageBN: $"ভাড়াটিয়া {user.FullNameBN} এর তথ্য আপডেট হয়েছে",
//                    messageEN: $"Tenant {user.FullNameEN} information updated",
//                    type: "Update",
//                    isImportant: true
//                );
//            }

//            return RedirectToAction("Dashboard", new { id = userId });
//        }

//        [HttpGet]
//        public async Task<IActionResult> Documents(int userId)
//        {
//            var user = await _dataService.GetUserById(userId);
//            if (user == null)
//                return RedirectToAction("Login");

//            var documents = await _dataService.GetUserDocuments(userId);
//            ViewBag.UserId = userId;

//            return View(documents);
//        }

//        [HttpPost]
//        public async Task<IActionResult> UploadDocument(int userId, IFormFile file, string documentType)
//        {
//            if (file != null && file.Length > 0)
//            {
//                // In real app, save file to server
//                var document = new DocumentAttachment
//                {
//                    DocumentType = documentType,
//                    FileName = file.FileName,
//                    FilePath = $"/uploads/{userId}/{file.FileName}",
//                    FileSize = file.Length,
//                    MimeType = file.ContentType
//                };

//                await _dataService.AddDocumentAttachment(userId, document);
//            }

//            return RedirectToAction("Documents", new { id = userId });
//        }

//        [HttpGet]
//        public async Task<IActionResult> VerificationLogs(int userId)
//        {
//            var user = await _dataService.GetUserById(userId);
//            if (user == null)
//                return RedirectToAction("Login");

//            var logs = await _dataService.GetVerificationLogs(userId);
//            ViewBag.UserId = userId;

//            return View(logs);
//        }

//        [HttpGet]
//        public async Task<IActionResult> DownloadVerification(int userId)
//        {
//            var user = await _dataService.GetUserById(userId);
//            if (user == null || user.CompletionPercentage < 90)
//            {
//                return RedirectToAction("Dashboard", new { id = userId });
//            }

//            var pdfContent = await _dataService.GenerateVerificationPDF(user);
//            return File(pdfContent, "application/pdf", $"TenantVerification_{user.NIDNumber}.pdf");
//        }

//        [HttpGet]
//        public IActionResult Logout()
//        {
//            HttpContext.Session.Clear();
//            return RedirectToAction("Index");
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }

        
//    }
//}