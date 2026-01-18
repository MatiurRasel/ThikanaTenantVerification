namespace ThikanaTenantVerification.Services
{
    /// <summary>
    /// Interface for comprehensive logging service supporting multiple logging destinations
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Logs an information message
        /// </summary>
        /// <param name="message">Log message</param>
        /// <param name="userId">User ID (optional)</param>
        /// <param name="additionalData">Additional data to log (optional)</param>
        Task LogInformationAsync(string message, int? userId = null, Dictionary<string, object>? additionalData = null);

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="userId">User ID (optional)</param>
        /// <param name="additionalData">Additional data to log (optional)</param>
        Task LogWarningAsync(string message, int? userId = null, Dictionary<string, object>? additionalData = null);

        /// <summary>
        /// Logs an error message with exception
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="exception">Exception object</param>
        /// <param name="userId">User ID (optional)</param>
        /// <param name="additionalData">Additional data to log (optional)</param>
        Task LogErrorAsync(string message, Exception exception, int? userId = null, Dictionary<string, object>? additionalData = null);

        /// <summary>
        /// Logs authentication events
        /// </summary>
        /// <param name="eventType">Type of authentication event (Login, Logout, Registration, etc.)</param>
        /// <param name="userId">User ID</param>
        /// <param name="mobileNumber">Mobile number</param>
        /// <param name="ipAddress">IP address</param>
        /// <param name="success">Whether the operation was successful</param>
        /// <param name="details">Additional details (optional)</param>
        Task LogAuthenticationEventAsync(string eventType, int? userId, string mobileNumber, string? ipAddress, bool success, string? details = null);

        /// <summary>
        /// Logs audit trail for data modifications
        /// </summary>
        /// <param name="action">Action performed (Create, Update, Delete)</param>
        /// <param name="entity">Entity name</param>
        /// <param name="entityId">Entity ID</param>
        /// <param name="userId">User ID who performed the action</param>
        /// <param name="oldValues">Previous values (JSON string)</param>
        /// <param name="newValues">New values (JSON string)</param>
        /// <param name="ipAddress">IP address</param>
        Task LogAuditAsync(string action, string entity, int entityId, int? userId, string? oldValues = null, string? newValues = null, string? ipAddress = null);

        /// <summary>
        /// Logs verification events
        /// </summary>
        /// <param name="verificationType">Type of verification (NID, Police, Landlord, etc.)</param>
        /// <param name="userId">User ID</param>
        /// <param name="status">Verification status</param>
        /// <param name="verifiedBy">Who performed the verification</param>
        /// <param name="comments">Additional comments</param>
        Task LogVerificationAsync(string verificationType, int userId, string status, string verifiedBy, string? comments = null);
    }
}

