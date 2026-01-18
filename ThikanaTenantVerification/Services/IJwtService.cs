using System.Security.Claims;

namespace ThikanaTenantVerification.Services
{
    /// <summary>
    /// Interface for JWT token generation and validation services
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT token for the user with specified claims
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="mobileNumber">Mobile number</param>
        /// <param name="role">User role (Tenant, Landlord, Police, Admin, etc.)</param>
        /// <param name="additionalClaims">Additional claims to include in the token</param>
        /// <returns>JWT token string</returns>
        string GenerateToken(int userId, string mobileNumber, string role, Dictionary<string, string>? additionalClaims = null);

        /// <summary>
        /// Validates a JWT token and extracts claims
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>Claims principal if token is valid, null otherwise</returns>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Generates a refresh token
        /// </summary>
        /// <returns>Refresh token string</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Extracts user ID from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>User ID if found, null otherwise</returns>
        int? GetUserIdFromToken(string token);

        /// <summary>
        /// Extracts role from JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Role if found, null otherwise</returns>
        string? GetRoleFromToken(string token);
    }
}

