using Newtonsoft.Json;
using ThikanaTenantVerification.Models;

namespace ThikanaTenantVerification.Services
{
    /// <summary>
    /// Mock service that simulates government API calls for NID and Police verification
    /// Uses JSON files for demo data
    /// </summary>
    public class ApiMockService : IApiMockService
    {
        private readonly ILogger<ApiMockService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _mockDataPath;
        private List<NIDData>? _nidDataCache;
        private List<PoliceVerificationData>? _policeDataCache;
        private List<Landlord>? _landlordCache;
        private List<MobileNIDMapping>? _mobileMappingCache;

        /// <summary>
        /// Initializes a new instance of ApiMockService
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="configuration">Application configuration</param>
        public ApiMockService(ILogger<ApiMockService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _mockDataPath = Path.Combine(Directory.GetCurrentDirectory(), 
                configuration["ApiSettings:MockDataPath"] ?? "Data/Mock");

            // Initialize caches
            LoadMockData();
        }

        /// <inheritdoc/>
        public async Task<NIDData?> GetNIDDataAsync(string nidNumber)
        {
            try
            {
                // Simulate API delay
                await Task.Delay(500);

                if (_nidDataCache == null)
                {
                    LoadMockData();
                }

                // Search by NID or Birth Certificate number
                var nidData = _nidDataCache?.FirstOrDefault(n => 
                    n.NIDNumber == nidNumber || 
                    n.BirthCertificateNumber == nidNumber);

                if (nidData == null)
                {
                    _logger.LogWarning("NID data not found for: {NIDNumber}", nidNumber);
                    return null;
                }

                _logger.LogInformation("NID data retrieved for: {NIDNumber}", nidNumber);
                return nidData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting NID data for: {NIDNumber}", nidNumber);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateMobileWithNIDAsync(string mobileNumber, string nidNumber)
        {
            try
            {
                await Task.Delay(300);

                if (_mobileMappingCache == null)
                {
                    LoadMockData();
                }

                // Check if mobile number is registered with this NID
                var mapping = _mobileMappingCache?.FirstOrDefault(m => 
                    m.MobileNumber == mobileNumber && 
                    m.NIDNumber == nidNumber && 
                    m.IsValid);

                if (mapping != null)
                {
                    _logger.LogInformation("Mobile {MobileNumber} validated with NID {NIDNumber}", mobileNumber, nidNumber);
                    return true;
                }

                // Also check from NID data directly
                var nidData = await GetNIDDataAsync(nidNumber);
                if (nidData != null && nidData.MobileNumber == mobileNumber)
                {
                    return true;
                }

                _logger.LogWarning("Mobile {MobileNumber} does not match NID {NIDNumber}", mobileNumber, nidNumber);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating mobile with NID");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<PoliceVerificationData?> GetPoliceVerificationAsync(string nidNumber)
        {
            try
            {
                await Task.Delay(800); // Simulate longer API call

                if (_policeDataCache == null)
                {
                    LoadMockData();
                }

                var policeData = _policeDataCache?.FirstOrDefault(p => p.NIDNumber == nidNumber);

                if (policeData == null)
                {
                    // Default: assume valid if not in database (for demo)
                    _logger.LogInformation("Police data not found for {NIDNumber}, returning default valid record", nidNumber);
                    return new PoliceVerificationData
                    {
                        NIDNumber = nidNumber,
                        IsValid = true,
                        ValidationMessage = "পরিষ্কার রেকর্ড - তথ্য পাওয়া যায়নি (ডেমো)",
                        CaseRecords = new List<object>(),
                        LastVerified = DateTime.Now.ToString("yyyy-MM-dd"),
                        DangerLevel = "নিম্ন",
                        PoliceStation = "ডিফল্ট থানা",
                        VerifiedBy = "সিস্টেম"
                    };
                }

                _logger.LogInformation("Police verification data retrieved for: {NIDNumber} | Valid: {IsValid}", 
                    nidNumber, policeData.IsValid);
                return policeData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting police verification for: {NIDNumber}", nidNumber);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<Landlord?> ValidateLandlordAsync(string mobileNumber)
        {
            try
            {
                await Task.Delay(300);

                if (_landlordCache == null)
                {
                    LoadMockData();
                }

                var landlord = _landlordCache?.FirstOrDefault(l => 
                    l.MobileNumber == mobileNumber && 
                    l.IsActive);

                if (landlord != null)
                {
                    _logger.LogInformation("Landlord found: {MobileNumber}", mobileNumber);
                }
                else
                {
                    _logger.LogWarning("Landlord not found or inactive: {MobileNumber}", mobileNumber);
                }

                return landlord;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating landlord: {MobileNumber}", mobileNumber);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<string> SendOTPAsync(string mobileNumber)
        {
            try
            {
                // Simulate SMS sending delay
                await Task.Delay(1000);

                // Generate 6-digit OTP
                var random = new Random();
                var otp = random.Next(100000, 999999).ToString();

                _logger.LogInformation("OTP sent to {MobileNumber}: {OTP}", mobileNumber, otp);

                // In production, this would call SMS gateway API
                // For demo, return the OTP so it can be used for verification
                return otp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP to: {MobileNumber}", mobileNumber);
                throw;
            }
        }

        /// <summary>
        /// Loads mock data from JSON files into memory cache
        /// </summary>
        private void LoadMockData()
        {
            try
            {
                // Load NID Data
                var nidFilePath = Path.Combine(_mockDataPath, "NIDData.json");
                if (File.Exists(nidFilePath))
                {
                    var nidJson = File.ReadAllText(nidFilePath);
                    _nidDataCache = JsonConvert.DeserializeObject<List<NIDData>>(nidJson);
                }

                // Load Police Verification Data
                var policeFilePath = Path.Combine(_mockDataPath, "PoliceVerification.json");
                if (File.Exists(policeFilePath))
                {
                    var policeJson = File.ReadAllText(policeFilePath);
                    _policeDataCache = JsonConvert.DeserializeObject<List<PoliceVerificationData>>(policeJson);
                }

                // Load Landlord Data
                var landlordFilePath = Path.Combine(_mockDataPath, "LandlordUsers.json");
                if (File.Exists(landlordFilePath))
                {
                    var landlordJson = File.ReadAllText(landlordFilePath);
                    _landlordCache = JsonConvert.DeserializeObject<List<Landlord>>(landlordJson);
                }

                // Load Mobile-NID Mapping
                var mappingFilePath = Path.Combine(_mockDataPath, "MobileNIDMapping.json");
                if (File.Exists(mappingFilePath))
                {
                    var mappingJson = File.ReadAllText(mappingFilePath);
                    _mobileMappingCache = JsonConvert.DeserializeObject<List<MobileNIDMapping>>(mappingJson);
                }

                _logger.LogInformation("Mock data loaded successfully from {Path}", _mockDataPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading mock data from {Path}", _mockDataPath);
            }
        }
    }

    /// <summary>
    /// Helper class for mobile-NID mapping
    /// </summary>
    public class MobileNIDMapping
    {
        public string MobileNumber { get; set; } = string.Empty;
        public string? NIDNumber { get; set; }
        public bool IsValid { get; set; }
        public string? VerificationDate { get; set; }
        public string? Remarks { get; set; }
    }
}

