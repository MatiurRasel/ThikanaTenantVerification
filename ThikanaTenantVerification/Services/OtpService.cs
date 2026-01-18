using Microsoft.EntityFrameworkCore;
using ThikanaTenantVerification.Data;
using ThikanaTenantVerification.Models;

namespace ThikanaTenantVerification.Services
{
    /// <summary>
    /// Service for OTP generation, validation, and SMS sending
    /// Implements IOtpService interface
    /// </summary>
    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OtpService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IApiMockService _apiMockService;
        private readonly ILoggingService _loggingService;

        /// <summary>
        /// Initializes a new instance of OtpService
        /// </summary>
        public OtpService(
            ApplicationDbContext context,
            ILogger<OtpService> logger,
            IConfiguration configuration,
            IApiMockService apiMockService,
            ILoggingService loggingService)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _apiMockService = apiMockService;
            _loggingService = loggingService;
        }

        /// <inheritdoc/>
        public async Task<string> GenerateAndSaveOtpAsync(string mobileNumber)
        {
            try
            {
                // Check rate limiting
                if (!await CanGenerateOtpAsync(mobileNumber))
                {
                    throw new InvalidOperationException("OTP generation rate limited. Please wait before requesting again.");
                }

                // Generate 6-digit OTP
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();

                var expiryMinutes = _configuration.GetValue<int>("OtpSettings:ExpiryMinutes", 5);
                var expiryTime = DateTime.UtcNow.AddMinutes(expiryMinutes);

                // Mark old OTPs as used
                var oldOtps = await _context.OTPs
                    .Where(o => o.MobileNumber == mobileNumber && !o.IsUsed && o.ExpiryTime > DateTime.UtcNow)
                    .ToListAsync();

                foreach (var oldOtp in oldOtps)
                {
                    oldOtp.IsUsed = true;
                }

                // Save new OTP
                var otp = new OTP
                {
                    MobileNumber = mobileNumber,
                    OTPCode = otpCode,
                    ExpiryTime = expiryTime,
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.OTPs.Add(otp);
                await _context.SaveChangesAsync();

                // Send OTP via SMS
                await SendOtpSmsAsync(mobileNumber, otpCode);

                _logger.LogInformation("OTP generated for mobile: {MobileNumber}", mobileNumber);
                await _loggingService.LogInformationAsync($"OTP generated for mobile: {mobileNumber}", 
                    null, new Dictionary<string, object> { { "MobileNumber", mobileNumber }, { "ExpiryMinutes", expiryMinutes } });

                return otpCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP for mobile: {MobileNumber}", mobileNumber);
                await _loggingService.LogErrorAsync($"Error generating OTP for mobile: {mobileNumber}", ex);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> VerifyOtpAsync(string mobileNumber, string otpCode)
        {
            try
            {
                var otp = await _context.OTPs
                    .Where(o => o.MobileNumber == mobileNumber &&
                               o.OTPCode == otpCode &&
                               !o.IsUsed &&
                               o.ExpiryTime > DateTime.UtcNow)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otp != null)
                {
                    // Mark OTP as used
                    otp.IsUsed = true;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("OTP verified successfully for mobile: {MobileNumber}", mobileNumber);
                    await _loggingService.LogAuthenticationEventAsync("OTP_VERIFIED", null, mobileNumber, null, true, "OTP verification successful");

                    return true;
                }

                _logger.LogWarning("Invalid or expired OTP for mobile: {MobileNumber}", mobileNumber);
                await _loggingService.LogAuthenticationEventAsync("OTP_VERIFY_FAILED", null, mobileNumber, null, false, "Invalid or expired OTP");

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for mobile: {MobileNumber}", mobileNumber);
                await _loggingService.LogErrorAsync($"Error verifying OTP for mobile: {mobileNumber}", ex);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SendOtpSmsAsync(string mobileNumber, string otpCode)
        {
            try
            {
                // Use ApiMockService to send OTP (which simulates SMS sending)
                var sentOtp = await _apiMockService.SendOTPAsync(mobileNumber);

                // In production, integrate with actual SMS gateway (Banglalink, Robi, GP, etc.)
                // For demo, log the OTP (in production, never log OTPs)
                _logger.LogInformation("OTP SMS sent to {MobileNumber} | OTP: {OTP} (Demo mode - OTP visible for testing)", 
                    mobileNumber, otpCode);

                await _loggingService.LogInformationAsync($"OTP SMS sent to {mobileNumber}", null, 
                    new Dictionary<string, object> { { "MobileNumber", mobileNumber }, { "SmsProvider", "Demo" } });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP SMS to: {MobileNumber}", mobileNumber);
                await _loggingService.LogErrorAsync($"Error sending OTP SMS to: {mobileNumber}", ex);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> CanGenerateOtpAsync(string mobileNumber)
        {
            try
            {
                var resendWaitSeconds = _configuration.GetValue<int>("OtpSettings:ResendWaitSeconds", 60);
                var cutoffTime = DateTime.UtcNow.AddSeconds(-resendWaitSeconds);

                var recentOtp = await _context.OTPs
                    .Where(o => o.MobileNumber == mobileNumber && o.CreatedAt > cutoffTime)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                return recentOtp == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking OTP rate limit for: {MobileNumber}", mobileNumber);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<int> GetResendWaitTimeAsync(string mobileNumber)
        {
            try
            {
                var resendWaitSeconds = _configuration.GetValue<int>("OtpSettings:ResendWaitSeconds", 60);
                var cutoffTime = DateTime.UtcNow.AddSeconds(-resendWaitSeconds);

                var recentOtp = await _context.OTPs
                    .Where(o => o.MobileNumber == mobileNumber && o.CreatedAt > cutoffTime)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (recentOtp == null)
                {
                    return 0;
                }

                var waitTime = (int)(resendWaitSeconds - (DateTime.UtcNow - recentOtp.CreatedAt).TotalSeconds);
                return Math.Max(0, waitTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting resend wait time for: {MobileNumber}", mobileNumber);
                return 0;
            }
        }
    }
}

