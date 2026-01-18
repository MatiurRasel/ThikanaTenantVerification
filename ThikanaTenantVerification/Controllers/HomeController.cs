using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThikanaTenantVerification.Models;
using ThikanaTenantVerification.Services;

namespace ThikanaTenantVerification.Controllers
{
    /// <summary>
    /// Home controller for tenant verification application
    /// Handles landing page, registration, login, and dashboard
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IDataService _dataService;
        private readonly IOtpService _otpService;
        private readonly IApiMockService _apiMockService;
        private readonly IJwtService _jwtService;
        private readonly ILoggingService _loggingService;
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of HomeController
        /// </summary>
        public HomeController(
            IDataService dataService,
            IOtpService otpService,
            IApiMockService apiMockService,
            IJwtService jwtService,
            ILoggingService loggingService,
            ILogger<HomeController> logger)
        {
            _dataService = dataService;
            _otpService = otpService;
            _apiMockService = apiMockService;
            _jwtService = jwtService;
            _loggingService = loggingService;
            _logger = logger;
        }

        /// <summary>
        /// Landing page - Eye-catching with Police gradient and sticky header
        /// </summary>
        /// <returns>Landing page view</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// User login page - OTP-based authentication
        /// </summary>
        /// <returns>Login view</returns>
        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to dashboard
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }

        /// <summary>
        /// Handles OTP-based login request
        /// User provides mobile number, receives OTP, then verifies
        /// </summary>
        /// <param name="mobileNumber">Mobile number for login</param>
        /// <returns>JSON response with OTP sent status</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestLoginOtp(string mobileNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(mobileNumber) || !System.Text.RegularExpressions.Regex.IsMatch(mobileNumber, @"^01[3-9]\d{8}$"))
                {
                    return Json(new { success = false, message = "অনুগ্রহ করে সঠিক মোবাইল নম্বর দিন" });
                }

                // Check if user exists
                var user = await _dataService.GetUserByIdNumber(mobileNumber);
                if (user == null)
                {
                    return Json(new { success = false, message = "এই মোবাইল নম্বর দিয়ে কোন অ্যাকাউন্ট নেই। রেজিস্ট্রেশন করুন।" });
                }

                // Generate and send OTP
                var otp = await _otpService.GenerateAndSaveOtpAsync(mobileNumber);
                
                await _loggingService.LogAuthenticationEventAsync(
                    "LOGIN_OTP_REQUESTED", 
                    user.Id, 
                    mobileNumber, 
                    HttpContext.Connection.RemoteIpAddress?.ToString(), 
                    true, 
                    "OTP requested for login"
                );

                // For demo, return OTP in response (REMOVE IN PRODUCTION)
                return Json(new 
                { 
                    success = true, 
                    message = $"OTP পাঠানো হয়েছে {mobileNumber} নম্বরে। (ডেমো: OTP = {otp})",
                    mobileNumber = mobileNumber,
                    demoOtp = otp // REMOVE THIS IN PRODUCTION
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting login OTP");
                await _loggingService.LogErrorAsync("Error requesting login OTP", ex);
                return Json(new { success = false, message = "একটি ত্রুটি হয়েছে। পরে চেষ্টা করুন" });
            }
        }

        /// <summary>
        /// Verifies OTP and logs user in (OTP-based authentication - no password required)
        /// </summary>
        /// <param name="mobileNumber">Mobile number</param>
        /// <param name="otpCode">OTP code to verify</param>
        /// <returns>JSON response with login status</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyLoginOtp(string mobileNumber, string otpCode)
        {
            try
            {
                if (string.IsNullOrEmpty(mobileNumber) || string.IsNullOrEmpty(otpCode))
                {
                    return Json(new { success = false, message = "মোবাইল নম্বর এবং OTP দিন" });
                }

                // Verify OTP
                var isValidOtp = await _otpService.VerifyOtpAsync(mobileNumber, otpCode);
                if (!isValidOtp)
                {
                    await _loggingService.LogAuthenticationEventAsync(
                        "LOGIN_OTP_VERIFY_FAILED",
                        null,
                        mobileNumber,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        false,
                        "Invalid OTP"
                    );

                    return Json(new { success = false, message = "ভুল OTP কোড। আবার চেষ্টা করুন।" });
                }

                // Get user
                var user = await _dataService.GetUserByIdNumber(mobileNumber);
                if (user == null)
                {
                    return Json(new { success = false, message = "ব্যবহারকারী পাওয়া যায়নি" });
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user.Id, user.MobileNumber, "Tenant");

                // Set session
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.FullNameBN);
                HttpContext.Session.SetString("UserNID", user.NIDNumber ?? "");
                HttpContext.Session.SetString("JWTToken", token);

                // Record login
                await _dataService.RecordLogin(user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());

                await _loggingService.LogAuthenticationEventAsync(
                    "LOGIN_SUCCESS",
                    user.Id,
                    mobileNumber,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    true,
                    "OTP-based login successful"
                );

                return Json(new 
                { 
                    success = true, 
                    message = "লগইন সফল!",
                    redirectUrl = Url.Action("Dashboard"/*, new { id = user.Id }*/)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying login OTP");
                await _loggingService.LogErrorAsync("Error verifying login OTP", ex);
                return Json(new { success = false, message = "একটি ত্রুটি হয়েছে। পরে চেষ্টা করুন" });
            }
        }

        /// <summary>
        /// Handles user registration request
        /// Validates NID/Birth Certificate and mobile number, sends OTP
        /// NO PASSWORD REQUIRED - OTP acts as authentication
        /// </summary>
        /// <param name="idNumber">NID or Birth Certificate number (13/14/15/17 digits)</param>
        /// <param name="mobileNumber">Mobile number (must match NID record)</param>
        /// <returns>JSON response with registration status</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string idNumber, string mobileNumber)
        {
            try
            {
                _logger.LogInformation("Register called with NID: {NID}, Mobile: {Mobile}", idNumber, mobileNumber);

                // Validate ID number length (10, 13, 14, 15, or 17 digits)
                if (string.IsNullOrEmpty(idNumber) || !(new[] { 10, 13, 14, 15, 17 }.Contains(idNumber.Length)))
                {
                    return Json(new
                    {
                        success = false,
                        message = "অনুগ্রহ করে সঠিক জাতীয় পরিচয়পত্র বা জন্ম নিবন্ধন নম্বর দিন (১০/১৩/১৪/১৫/১৭ ডিজিট)"
                    });
                }

                // Validate mobile number
                if (string.IsNullOrEmpty(mobileNumber) || !System.Text.RegularExpressions.Regex.IsMatch(mobileNumber, @"^01[3-9]\d{8}$"))
                {
                    return Json(new
                    {
                        success = false,
                        message = "অনুগ্রহ করে সঠিক মোবাইল নম্বর দিন (01XXXXXXXXX)"
                    });
                }

                // Check if user already exists
                var existingUser = await _dataService.GetUserByNIDAndMobile(idNumber, mobileNumber);
                if (existingUser != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "এই NID এবং মোবাইল নম্বর দিয়ে ইতিমধ্যে রেজিস্ট্রেশন করা হয়েছে। লগইন করুন।",
                        redirectToLogin = true
                    });
                }

                // Get NID data from government API (mock service)
                var nidData = await _apiMockService.GetNIDDataAsync(idNumber);
                if (nidData == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "NID তথ্য পাওয়া যায়নি। অনুগ্রহ করে সঠিক NID নম্বর দিন।"
                    });
                }

                // Validate mobile number matches NID record
                var mobileMatches = await _apiMockService.ValidateMobileWithNIDAsync(mobileNumber, idNumber);
                if (!mobileMatches && nidData.MobileNumber != mobileNumber)
                {
                    // For demo, we'll allow it but log a warning
                    _logger.LogWarning("Mobile number {Mobile} does not match NID {NID} record, but allowing for demo", mobileNumber, idNumber);
                    // In production, return error:
                    // return Json(new { success = false, message = "এই মোবাইল নম্বর এই NID এর সাথে মিলছে না।" });
                }

                // Update mobile number in NID data if different (for demo flexibility)
                nidData.MobileNumber = mobileNumber;

                // Generate and send OTP
                var otp = await _otpService.GenerateAndSaveOtpAsync(mobileNumber);

                // Store registration data in TempData for OTP verification step
                TempData["NIDData"] = JsonSerializer.Serialize(nidData);
                TempData["MobileNumber"] = mobileNumber;
                TempData["IdNumber"] = idNumber;
                TempData["RegistrationStep"] = "otp_verification";

                await _loggingService.LogInformationAsync(
                    $"Registration OTP sent to {mobileNumber} for NID {idNumber}",
                    null,
                    new Dictionary<string, object> { { "NID", idNumber }, { "Mobile", mobileNumber } }
                );

                // For demo, return OTP in response (REMOVE IN PRODUCTION)
                return Json(new
                {
                    success = true,
                    message = $"NID যাচাই সফল! OTP পাঠানো হয়েছে {mobileNumber} নম্বরে।",
                    mobileNumber = mobileNumber,
                    demoOtp = otp, // REMOVE THIS IN PRODUCTION
                    redirectUrl = Url.Action("VerifyOTP")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                await _loggingService.LogErrorAsync("Registration error", ex);
                return Json(new { success = false, message = "একটি ত্রুটি হয়েছে। পরে চেষ্টা করুন" });
            }
        }

        /// <summary>
        /// OTP verification page for registration
        /// </summary>
        /// <returns>OTP verification view</returns>
        [HttpGet]
        public IActionResult VerifyOTP()
        {
            if (TempData["MobileNumber"] == null || TempData["RegistrationStep"]?.ToString() != "otp_verification")
            {
                return RedirectToAction("Login");
            }

            ViewBag.MobileNumber = TempData["MobileNumber"];
            ViewBag.NIDData = TempData["NIDData"]?.ToString();
            ViewBag.RegistrationStep = "otp_verification";

            TempData.Keep("MobileNumber");
            TempData.Keep("NIDData");
            TempData.Keep("IdNumber");
            TempData.Keep("RegistrationStep");

            return View();
        }

        /// <summary>
        /// Verifies OTP and completes registration
        /// Creates user account WITHOUT password - OTP is used for authentication
        /// User can optionally set password later
        /// </summary>
        /// <param name="otp">OTP code to verify</param>
        /// <returns>Redirects to dashboard on success</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(string otp)
        {
            try
            {
                var mobileNumber = TempData["MobileNumber"]?.ToString();
                var nidDataJson = TempData["NIDData"]?.ToString();
                var step = TempData["RegistrationStep"]?.ToString();

                if (string.IsNullOrEmpty(mobileNumber) || step != "otp_verification")
                {
                    return RedirectToAction("Login");
                }

                // Verify OTP
                var isValidOtp = await _otpService.VerifyOtpAsync(mobileNumber, otp);
                if (!isValidOtp)
                {
                    ViewBag.Error = "ভুল OTP কোড। আবার চেষ্টা করুন।";
                    ViewBag.MobileNumber = mobileNumber;
                    ViewBag.NIDData = nidDataJson;
                    ViewBag.RegistrationStep = "otp_verification";

                    TempData.Keep("MobileNumber");
                    TempData.Keep("NIDData");
                    TempData.Keep("IdNumber");
                    TempData.Keep("RegistrationStep");

                    return View();
                }

                // Deserialize NID data
                var nidData = JsonSerializer.Deserialize<NIDData>(nidDataJson ?? "{}");
                if (nidData == null)
                {
                    ViewBag.Error = "NID তথ্য পাওয়া যায়নি";
                    return View();
                }

                // Create user WITHOUT password - OTP-based authentication
                // PasswordHash will be null initially, user can set it later if desired
                var userId = await _dataService.CreateUserFromNIDData(nidData);
                if (userId <= 0)
                {
                    ViewBag.Error = "ব্যবহারকারী তৈরি করতে সমস্যা হয়েছে";
                    return View();
                }

                var newUser = await _dataService.GetUserById(userId);
                if (newUser == null)
                {
                    ViewBag.Error = "ব্যবহারকারী পাওয়া যায়নি";
                    return View();
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(newUser.Id, newUser.MobileNumber, "Tenant");

                // Set session
                HttpContext.Session.SetInt32("UserId", newUser.Id);
                HttpContext.Session.SetString("UserName", newUser.FullNameBN);
                HttpContext.Session.SetString("UserNID", newUser.NIDNumber ?? "");
                HttpContext.Session.SetString("JWTToken", token);

                await _loggingService.LogAuthenticationEventAsync(
                    "REGISTRATION_SUCCESS",
                    newUser.Id,
                    mobileNumber,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    true,
                    "User registered successfully with OTP-based authentication (no password)"
                );

                // Clear TempData
                TempData.Remove("MobileNumber");
                TempData.Remove("NIDData");
                TempData.Remove("IdNumber");
                TempData.Remove("RegistrationStep");

                return RedirectToAction("Dashboard"/*, new { id = newUser.Id }*/);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyOTP");
                await _loggingService.LogErrorAsync("Error in VerifyOTP", ex);
                ViewBag.Error = "একটি ত্রুটি হয়েছে। পরে চেষ্টা করুন";
                return View();
            }
        }

        /// <summary>
        /// User dashboard showing completion percentage and profile sections
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Dashboard view</returns>
        [HttpGet]
        //[Authorize(Policy = "Tenant")]
        public async Task<IActionResult> Dashboard(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue || userId.Value != id)
            {
                return RedirectToAction("Login");
            }

            var user = await _dataService.GetUserById(id);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var completionPercentage = await _dataService.CalculateCompletionPercentage(id);
            user.CompletionPercentage = completionPercentage;
            await _dataService.UpdateUser(user);

            ViewBag.CompletionPercentage = completionPercentage;
            return View(user);
        }

        /// <summary>
        /// Logout user and clear session
        /// </summary>
        /// <returns>Redirects to landing page</returns>
        [HttpGet]
        public IActionResult Logout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var mobileNumber = HttpContext.Session.GetString("UserName");

            HttpContext.Session.Clear();

            if (userId.HasValue && !string.IsNullOrEmpty(mobileNumber))
            {
                _ = Task.Run(async () =>
                {
                    await _loggingService.LogAuthenticationEventAsync(
                        "LOGOUT",
                        userId.Value,
                        mobileNumber,
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        true,
                        "User logged out"
                    );
                });
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Error page
        /// </summary>
        /// <returns>Error view</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Additional action methods for profile sections would go here...
        // (EmergencyContact, FamilyMembers, HouseWorkers, etc. - keeping existing implementations)

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

    }
}

