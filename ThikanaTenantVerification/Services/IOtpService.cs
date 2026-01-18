namespace ThikanaTenantVerification.Services
{
    /// <summary>
    /// Interface for OTP generation and verification service
    /// </summary>
    public interface IOtpService
    {
        /// <summary>
        /// Generates and saves an OTP for the given mobile number
        /// </summary>
        /// <param name="mobileNumber">Mobile number to generate OTP for</param>
        /// <returns>Generated OTP code</returns>
        Task<string> GenerateAndSaveOtpAsync(string mobileNumber);

        /// <summary>
        /// Verifies the provided OTP for the mobile number
        /// </summary>
        /// <param name="mobileNumber">Mobile number</param>
        /// <param name="otpCode">OTP code to verify</param>
        /// <returns>True if OTP is valid and not expired, false otherwise</returns>
        Task<bool> VerifyOtpAsync(string mobileNumber, string otpCode);

        /// <summary>
        /// Sends OTP via SMS (mock implementation for demo)
        /// </summary>
        /// <param name="mobileNumber">Mobile number to send OTP</param>
        /// <param name="otpCode">OTP code to send</param>
        /// <returns>True if sent successfully, false otherwise</returns>
        Task<bool> SendOtpSmsAsync(string mobileNumber, string otpCode);

        /// <summary>
        /// Checks if OTP generation is allowed for the mobile number (rate limiting)
        /// </summary>
        /// <param name="mobileNumber">Mobile number to check</param>
        /// <returns>True if allowed, false if rate limited</returns>
        Task<bool> CanGenerateOtpAsync(string mobileNumber);

        /// <summary>
        /// Gets remaining time in seconds until next OTP can be requested
        /// </summary>
        /// <param name="mobileNumber">Mobile number</param>
        /// <returns>Seconds remaining, 0 if can generate now</returns>
        Task<int> GetResendWaitTimeAsync(string mobileNumber);
    }
}

