using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ThikanaTenantVerification.Data;
using ThikanaTenantVerification.Models;

namespace ThikanaTenantVerification.Services
{
    public interface IDataService
    {
        // User Methods
        Task<User?> GetUserById(int id);
        Task<User?> GetUserByIdNumber(string idNumber);
        Task<User?> GetUserByNIDAndMobile(string nid, string mobile);
        Task<User?> AuthenticateUser(string username, string password);
        Task<User?> CreateUserFromNIDDataWithPassword(NIDData nidData, string password);
        Task<int> CreateUserFromNIDData(NIDData nidData);
        Task UpdateUser(User user);
        Task RecordLogin(int userId, string ipAddress);
        Task<int> CalculateCompletionPercentage(int userId);

        // NID Data Methods
        Task<NIDData?> GetNIDDataAsync(string idNumber);

        // OTP Methods
        Task SaveOTP(string mobileNumber, string otp);
        Task<bool> VerifyOTP(string mobileNumber, string otp);

        // Profile Section Methods
        Task AddEmergencyContact(int userId, EmergencyContact contact);
        Task AddFamilyMember(int userId, FamilyMember member);
        Task AddHouseWorker(int userId, HouseWorker worker);
        Task SaveCurrentResidence(int userId, CurrentResidence residence);

        // Current Landlord Methods
        Task SaveCurrentLandlord(int userId, CurrentLandlord landlord);
        Task<CurrentLandlord?> GetCurrentLandlord(int userId);
        Task UpdateCurrentLandlord(int userId, CurrentLandlord landlord);

        // Police Verification Methods
        Task<List<PoliceVerificationData>> GetPoliceVerificationData();
        Task<PoliceVerificationData?> GetPoliceVerificationForNID(string nidNumber);

        // Notification Methods
        Task CreateNotification(int? landlordId, int? tenantId, string messageBN,
                               string? messageEN, string type, bool isImportant = false);
        Task<List<Notification>> GetNotificationsForLandlord(int landlordId);
        Task MarkNotificationAsRead(int notificationId);
        Task NotifyLandlord(User user);

        // Verification Log Methods
        Task CreateVerificationLog(int userId, string verifiedBy, string verifierType,
                                  string verificationType, string status, string? comments = null);
        Task<List<VerificationLog>> GetVerificationLogs(int userId);

        // Document Methods
        Task AddDocumentAttachment(int userId, DocumentAttachment document);
        Task<List<DocumentAttachment>> GetUserDocuments(int userId);

        // PDF Generation
        Task<byte[]> GenerateVerificationPDF(User user);

        // Password Methods
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string password);
    }

    public class DataService : IDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataService> _logger;

        public DataService(ApplicationDbContext context, ILogger<DataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region User Methods

        public async Task<User?> GetUserById(int id)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.EmergencyContacts)
                    .Include(u => u.FamilyMembers)
                    .Include(u => u.HouseWorkers)
                    .Include(u => u.CurrentResidence)
                    .Include(u => u.CurrentLandlord)
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {Id}", id);
                return null;
            }
        }

        public async Task<User?> GetUserByIdNumber(string idNumber)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.NIDNumber == idNumber ||
                        u.BirthCertificateNumber == idNumber ||
                        u.MobileNumber == idNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID number: {IdNumber}", idNumber);
                return null;
            }
        }

        public async Task<User?> GetUserByNIDAndMobile(string nid, string mobile)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u =>
                        (u.NIDNumber == nid || u.BirthCertificateNumber == nid) &&
                        u.MobileNumber == mobile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by NID and mobile: {NID}, {Mobile}", nid, mobile);
                return null;
            }
        }

        public async Task<User?> AuthenticateUser(string username, string password)
        {
            try
            {
                // Find user by NID, birth certificate, or mobile
                var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.NIDNumber == username ||
                        u.BirthCertificateNumber == username ||
                        u.MobileNumber == username);

                if (user == null) return null;

                // Verify password
                var isValid = VerifyPassword(user.PasswordHash, password);

                if (isValid)
                {
                    // Update last login
                    user.LastLogin = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return user;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating user: {Username}", username);
                return null;
            }
        }

        public async Task<User?> CreateUserFromNIDDataWithPassword(NIDData nidData, string password)
        {
            try
            {
                // Check if user already exists
                var existingUser = await GetUserByNIDAndMobile(nidData.NIDNumber, nidData.MobileNumber);
                if (existingUser != null)
                {
                    return existingUser;
                }

                var user = new User
                {
                    NIDNumber = nidData.NIDNumber,
                    BirthCertificateNumber = nidData.BirthCertificateNumber,
                    FullNameBN = nidData.FullNameBN,
                    FullNameEN = nidData.FullNameEN,
                    FatherNameBN = nidData.FatherNameBN,
                    FatherNameEN = nidData.FatherNameEN,
                    MotherNameBN = nidData.MotherNameBN,
                    MotherNameEN = nidData.MotherNameEN,
                    DateOfBirth = nidData.DateOfBirth,
                    Gender = nidData.Gender,
                    MaritalStatus = nidData.MaritalStatus,
                    Religion = nidData.Religion,
                    MobileNumber = nidData.MobileNumber,
                    Email = nidData.Email,
                    PermanentAddress = nidData.PermanentAddress,
                    ProfileImage = nidData.ProfileImage,
                    PasswordHash = HashPassword(password),
                    IsVerified = false,
                    VerificationStatus = "Pending",
                    CompletionPercentage = 25,
                    RegistrationDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create verification log
                await CreateVerificationLog(user.Id, "System", "System", "Registration", "Pending",
                    "User registered via NID verification");

                // Create verification request
                var verificationRequest = new PoliceVerificationRequest
                {
                    UserId = user.Id,
                    NIDNumber = user.NIDNumber,
                    Status = "Pending",
                    PoliceStation = "Default Station",
                    RequestDate = DateTime.Now
                };

                _context.PoliceVerificationRequests.Add(verificationRequest);
                await _context.SaveChangesAsync();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user from NID data");
                return null;
            }
        }

        public async Task<int> CreateUserFromNIDData(NIDData nidData)
        {
            try
            {
                var user = new User
                {
                    NIDNumber = nidData.NIDNumber,
                    BirthCertificateNumber = nidData.BirthCertificateNumber,
                    FullNameBN = nidData.FullNameBN,
                    FullNameEN = nidData.FullNameEN,
                    FatherNameBN = nidData.FatherNameBN,
                    FatherNameEN = nidData.FatherNameEN,
                    MotherNameBN = nidData.MotherNameBN,
                    MotherNameEN = nidData.MotherNameEN,
                    DateOfBirth = nidData.DateOfBirth,
                    Gender = nidData.Gender,
                    MaritalStatus = nidData.MaritalStatus,
                    Religion = nidData.Religion,
                    MobileNumber = nidData.MobileNumber,
                    Email = nidData.Email,
                    PermanentAddress = nidData.PermanentAddress,
                    ProfileImage = nidData.ProfileImage,
                    PasswordHash = HashPassword("Default@123"),
                    IsVerified = false,
                    VerificationStatus = "Pending",
                    CompletionPercentage = 0,
                    RegistrationDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return user.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user from NID data");
                return -1;
            }
        }

        public async Task UpdateUser(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.Now;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                throw;
            }
        }

        public async Task RecordLogin(int userId, string ipAddress)
        {
            try
            {
                var user = await GetUserById(userId);
                if (user != null)
                {
                    user.LastLogin = DateTime.Now;
                    user.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    // Log login activity
                    await CreateVerificationLog(userId, "System", "System", "Login", "Success",
                        $"User logged in from IP: {ipAddress}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording login for user: {UserId}", userId);
            }
        }

        public async Task<int> CalculateCompletionPercentage(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.EmergencyContacts)
                    .Include(u => u.FamilyMembers)
                    .Include(u => u.HouseWorkers)
                    .Include(u => u.CurrentResidence)
                    .Include(u => u.CurrentLandlord)
                    .Include(u => u.PreviousLandlords)
                    .Include(u => u.DocumentAttachments)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null) return 0;

                int totalFields = 15;
                int completedFields = 0;

                // Basic info (5 fields)
                if (!string.IsNullOrEmpty(user.FullNameBN)) completedFields++;
                if (!string.IsNullOrEmpty(user.FatherNameBN)) completedFields++;
                if (!string.IsNullOrEmpty(user.MobileNumber)) completedFields++;
                if (!string.IsNullOrEmpty(user.PermanentAddress)) completedFields++;
                if (user.DateOfBirth != default) completedFields++;

                // Emergency contacts
                if (user.EmergencyContacts.Any()) completedFields++;

                // Family members
                if (user.FamilyMembers.Any()) completedFields++;

                // Current residence
                if (user.CurrentResidence != null) completedFields++;

                // Current landlord
                if (user.CurrentLandlord != null) completedFields++;

                // House workers
                if (user.HouseWorkers.Any()) completedFields++;

                // Previous landlords
                if (user.PreviousLandlords.Any()) completedFields++;

                // NID document
                if (user.DocumentAttachments.Any(d => d.DocumentType == "NID")) completedFields++;

                // Photo document
                if (user.DocumentAttachments.Any(d => d.DocumentType == "Photo")) completedFields++;

                // Agreement document
                if (user.DocumentAttachments.Any(d => d.DocumentType == "Agreement")) completedFields++;

                // Utility bill document
                if (user.DocumentAttachments.Any(d => d.DocumentType == "UtilityBill")) completedFields++;

                var percentage = (completedFields * 100) / totalFields;
                user.CompletionPercentage = percentage;
                await _context.SaveChangesAsync();

                return percentage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating completion percentage for user: {UserId}", userId);
                return 0;
            }
        }

        #endregion

        #region NID Data Methods

        public async Task<NIDData?> GetNIDDataAsync(string idNumber)
        {
            try
            {
                // For demo purposes, create mock NID data
                // In production, this would call a government API
                await Task.Delay(500); // Simulate API call

                return new NIDData
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
                    MobileNumber = "01712345678",
                    Email = "demo@example.com",
                    PermanentAddress = "ডেমো ঠিকানা, ঢাকা"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NID data for: {IdNumber}", idNumber);
                return null;
            }
        }

        #endregion

        #region OTP Methods

        public async Task SaveOTP(string mobileNumber, string otp)
        {
            try
            {
                var otpRecord = new OTP
                {
                    MobileNumber = mobileNumber,
                    OTPCode = otp,
                    ExpiryTime = DateTime.Now.AddMinutes(5),
                    IsUsed = false,
                    CreatedAt = DateTime.Now
                };

                _context.OTPs.Add(otpRecord);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving OTP for: {MobileNumber}", mobileNumber);
            }
        }

        public async Task<bool> VerifyOTP(string mobileNumber, string otp)
        {
            try
            {
                var otpRecord = await _context.OTPs
                    .FirstOrDefaultAsync(o =>
                        o.MobileNumber == mobileNumber &&
                        o.OTPCode == otp &&
                        !o.IsUsed &&
                        o.ExpiryTime > DateTime.Now);

                if (otpRecord != null)
                {
                    otpRecord.IsUsed = true;
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for: {MobileNumber}", mobileNumber);
                return false;
            }
        }

        #endregion

        #region Profile Section Methods

        public async Task AddEmergencyContact(int userId, EmergencyContact contact)
        {
            try
            {
                contact.UserId = userId;
                contact.CreatedAt = DateTime.Now;
                _context.EmergencyContacts.Add(contact);
                await _context.SaveChangesAsync();

                // Update completion percentage
                await CalculateCompletionPercentage(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding emergency contact for user: {UserId}", userId);
                throw;
            }
        }

        public async Task AddFamilyMember(int userId, FamilyMember member)
        {
            try
            {
                member.UserId = userId;
                member.CreatedAt = DateTime.Now;
                _context.FamilyMembers.Add(member);
                await _context.SaveChangesAsync();

                // Update completion percentage
                await CalculateCompletionPercentage(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding family member for user: {UserId}", userId);
                throw;
            }
        }

        public async Task AddHouseWorker(int userId, HouseWorker worker)
        {
            try
            {
                worker.UserId = userId;
                worker.CreatedAt = DateTime.Now;
                worker.UpdatedAt = DateTime.Now;
                _context.HouseWorkers.Add(worker);
                await _context.SaveChangesAsync();

                // Update completion percentage
                await CalculateCompletionPercentage(userId);

                // Create police verification request for house worker
                var verificationRequest = new PoliceVerificationRequest
                {
                    UserId = userId,
                    NIDNumber = worker.NIDNumber,
                    Status = "Pending",
                    PoliceStation = "Default Station",
                    RequestDate = DateTime.Now
                };

                _context.PoliceVerificationRequests.Add(verificationRequest);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding house worker for user: {UserId}", userId);
                throw;
            }
        }

        public async Task SaveCurrentResidence(int userId, CurrentResidence residence)
        {
            try
            {
                var existing = await _context.CurrentResidences
                    .FirstOrDefaultAsync(cr => cr.UserId == userId);

                if (existing != null)
                {
                    existing.FlatFloor = residence.FlatFloor;
                    existing.HouseHolding = residence.HouseHolding;
                    existing.Road = residence.Road;
                    existing.Area = residence.Area;
                    existing.PostCode = residence.PostCode;
                    existing.Thana = residence.Thana;
                    existing.District = residence.District;
                    existing.Division = residence.Division;
                    existing.UpdatedAt = DateTime.Now;
                }
                else
                {
                    residence.UserId = userId;
                    residence.CreatedAt = DateTime.Now;
                    residence.UpdatedAt = DateTime.Now;
                    _context.CurrentResidences.Add(residence);
                }

                await _context.SaveChangesAsync();

                // Update completion percentage
                await CalculateCompletionPercentage(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving current residence for user: {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Current Landlord Methods

        public async Task SaveCurrentLandlord(int userId, CurrentLandlord landlord)
        {
            try
            {
                var existing = await GetCurrentLandlord(userId);
                if (existing != null)
                {
                    existing.NameBN = landlord.NameBN;
                    existing.MobileNumber = landlord.MobileNumber;
                    existing.NIDNumber = landlord.NIDNumber;
                    existing.Address = landlord.Address;
                    existing.FromDate = landlord.FromDate;
                    existing.ToDate = landlord.ToDate;
                    existing.UpdatedAt = DateTime.Now;
                }
                else
                {
                    landlord.UserId = userId;
                    landlord.CreatedAt = DateTime.Now;
                    landlord.UpdatedAt = DateTime.Now;
                    _context.CurrentLandlords.Add(landlord);
                }

                await _context.SaveChangesAsync();

                // Update completion percentage
                await CalculateCompletionPercentage(userId);

                // Create verification log
                await CreateVerificationLog(userId, "System", "System", "LandlordInfo", "Updated",
                    "Current landlord information saved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving current landlord for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<CurrentLandlord?> GetCurrentLandlord(int userId)
        {
            try
            {
                return await _context.CurrentLandlords
                    .FirstOrDefaultAsync(cl => cl.UserId == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current landlord for user: {UserId}", userId);
                return null;
            }
        }

        public async Task UpdateCurrentLandlord(int userId, CurrentLandlord landlord)
        {
            try
            {
                var existing = await GetCurrentLandlord(userId);
                if (existing != null)
                {
                    existing.NameBN = landlord.NameBN;
                    existing.MobileNumber = landlord.MobileNumber;
                    existing.NIDNumber = landlord.NIDNumber;
                    existing.Address = landlord.Address;
                    existing.FromDate = landlord.FromDate;
                    existing.ToDate = landlord.ToDate;
                    existing.IsVerified = landlord.IsVerified;
                    existing.VerificationDate = landlord.VerificationDate;
                    existing.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating current landlord for user: {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Police Verification Methods

        public async Task<List<PoliceVerificationData>> GetPoliceVerificationData()
        {
            try
            {
                // For demo, return mock data
                await Task.Delay(100);

                return new List<PoliceVerificationData>
                {
                    new PoliceVerificationData
                    {
                        NIDNumber = "1990123456789",
                        IsValid = true,
                        ValidationMessage = "পরিষ্কার রেকর্ড",
                        LastVerified = DateTime.Now.ToString("yyyy-MM-dd"),
                        DangerLevel = "নিম্ন"
                    },
                    new PoliceVerificationData
                    {
                        NIDNumber = "1991123456789",
                        IsValid = true,
                        ValidationMessage = "পরিষ্কার রেকর্ড",
                        LastVerified = DateTime.Now.ToString("yyyy-MM-dd"),
                        DangerLevel = "নিম্ন"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting police verification data");
                return new List<PoliceVerificationData>();
            }
        }

        public async Task<PoliceVerificationData?> GetPoliceVerificationForNID(string nidNumber)
        {
            try
            {
                var data = await GetPoliceVerificationData();
                return data.FirstOrDefault(p => p.NIDNumber == nidNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting police verification for NID: {NIDNumber}", nidNumber);
                return null;
            }
        }

        #endregion

        #region Notification Methods

        public async Task CreateNotification(int? landlordId, int? tenantId, string messageBN,
                                           string? messageEN, string type, bool isImportant = false)
        {
            try
            {
                var notification = new Notification
                {
                    LandlordId = landlordId,
                    TenantId = tenantId,
                    MessageBN = messageBN,
                    MessageEN = messageEN,
                    NotificationType = type,
                    IsImportant = isImportant,
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
            }
        }

        public async Task<List<Notification>> GetNotificationsForLandlord(int landlordId)
        {
            try
            {
                return await _context.Notifications
                    .Where(n => n.LandlordId == landlordId && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for landlord: {LandlordId}", landlordId);
                return new List<Notification>();
            }
        }

        public async Task MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.Id == notificationId);

                if (notification != null)
                {
                    notification.IsRead = true;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read: {NotificationId}", notificationId);
            }
        }

        public async Task NotifyLandlord(User user)
        {
            try
            {
                var currentLandlord = await GetCurrentLandlord(user.Id);
                if (currentLandlord != null)
                {
                    await CreateNotification(
                        landlordId: null, // Would need landlord entity
                        tenantId: user.Id,
                        messageBN: $"{user.FullNameBN} এর যাচাইকরণ প্রক্রিয়া {user.CompletionPercentage}% সম্পূর্ণ হয়েছে",
                        messageEN: $"Verification process for {user.FullNameEN} is {user.CompletionPercentage}% complete",
                        type: "ProgressUpdate",
                        isImportant: user.CompletionPercentage >= 90
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying landlord for user: {UserId}", user.Id);
            }
        }

        #endregion

        #region Verification Log Methods

        public async Task CreateVerificationLog(int userId, string verifiedBy, string verifierType,
                                              string verificationType, string status, string? comments = null)
        {
            try
            {
                var log = new VerificationLog
                {
                    UserId = userId,
                    VerifiedBy = verifiedBy,
                    VerifierType = verifierType,
                    VerificationType = verificationType,
                    Status = status,
                    Comments = comments,
                    VerifiedAt = DateTime.Now
                };

                _context.VerificationLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating verification log for user: {UserId}", userId);
            }
        }

        public async Task<List<VerificationLog>> GetVerificationLogs(int userId)
        {
            try
            {
                return await _context.VerificationLogs
                    .Where(log => log.UserId == userId)
                    .OrderByDescending(log => log.VerifiedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting verification logs for user: {UserId}", userId);
                return new List<VerificationLog>();
            }
        }

        #endregion

        #region Document Methods

        public async Task AddDocumentAttachment(int userId, DocumentAttachment document)
        {
            try
            {
                document.UserId = userId;
                document.UploadedAt = DateTime.Now;
                _context.DocumentAttachments.Add(document);
                await _context.SaveChangesAsync();

                // Update completion percentage
                await CalculateCompletionPercentage(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding document attachment for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<List<DocumentAttachment>> GetUserDocuments(int userId)
        {
            try
            {
                return await _context.DocumentAttachments
                    .Where(d => d.UserId == userId)
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting documents for user: {UserId}", userId);
                return new List<DocumentAttachment>();
            }
        }

        #endregion

        #region PDF Generation

        public async Task<byte[]> GenerateVerificationPDF(User user)
        {
            try
            {
                // Simple PDF generation - in production, use a library like iTextSharp or QuestPDF
                string content = $"ঠিকানা - ভাড়াটিয়া যাচাইকরণ রিপোর্ট\n\n" +
                               $"প্রদানের তারিখ: {DateTime.Now:dd-MMM-yyyy}\n\n" +
                               $"ব্যক্তিগত তথ্য:\n" +
                               $"নাম: {user.FullNameBN}\n" +
                               $"পিতার নাম: {user.FatherNameBN}\n" +
                               $"মাতার নাম: {user.MotherNameBN}\n" +
                               $"জন্ম তারিখ: {user.DateOfBirth:dd-MMM-yyyy}\n" +
                               $"এনআইডি নম্বর: {user.NIDNumber}\n" +
                               $"মোবাইল নম্বর: {user.MobileNumber}\n\n" +
                               $"যাচাইকরণ অবস্থা: {user.VerificationStatus}\n" +
                               $"প্রক্রিয়া সম্পূর্ণতা: {user.CompletionPercentage}%\n\n" +
                               $"এই রিপোর্টটি ঠিকানা ভাড়াটিয়া যাচাইকরণ সিস্টেম দ্বারা জেনারেট করা হয়েছে।";

                return Encoding.UTF8.GetBytes(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for user: {UserId}", user.Id);
                return Array.Empty<byte>();
            }
        }

        #endregion

        #region Password Methods

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            var hash = HashPassword(password);
            return hashedPassword == hash;
        }

        #endregion
    }
}