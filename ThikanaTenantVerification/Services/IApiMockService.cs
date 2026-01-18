using ThikanaTenantVerification.Models;

namespace ThikanaTenantVerification.Services
{
    /// <summary>
    /// Interface for API mocking service that simulates government NID and Police verification APIs
    /// </summary>
    public interface IApiMockService
    {
        /// <summary>
        /// Gets NID data from mock government API
        /// </summary>
        /// <param name="nidNumber">NID or Birth Certificate number</param>
        /// <returns>NIDData object if found, null otherwise</returns>
        Task<NIDData?> GetNIDDataAsync(string nidNumber);

        /// <summary>
        /// Validates mobile number against NID number from government database
        /// </summary>
        /// <param name="mobileNumber">Mobile number to validate</param>
        /// <param name="nidNumber">NID number to cross-check</param>
        /// <returns>True if mobile number matches NID, false otherwise</returns>
        Task<bool> ValidateMobileWithNIDAsync(string mobileNumber, string nidNumber);

        /// <summary>
        /// Gets police verification data for a given NID number
        /// </summary>
        /// <param name="nidNumber">NID number to verify</param>
        /// <returns>PoliceVerificationData object</returns>
        Task<PoliceVerificationData?> GetPoliceVerificationAsync(string nidNumber);

        /// <summary>
        /// Validates if a landlord exists and is active in the system
        /// </summary>
        /// <param name="mobileNumber">Landlord mobile number</param>
        /// <returns>Landlord object if found, null otherwise</returns>
        Task<Landlord?> ValidateLandlordAsync(string mobileNumber);

        /// <summary>
        /// Generates and sends OTP to mobile number (mock implementation)
        /// </summary>
        /// <param name="mobileNumber">Mobile number to send OTP</param>
        /// <returns>Generated OTP code (for demo purposes)</returns>
        Task<string> SendOTPAsync(string mobileNumber);
    }
}

