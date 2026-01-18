using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ThikanaTenantVerification.Data;
using ThikanaTenantVerification.Models;

namespace ThikanaTenantVerification.Services
{
    /// <summary>
    /// Comprehensive logging service that supports multiple logging destinations:
    /// - Database (AuditLogs table)
    /// - File (Notepad/Text files)
    /// - Console (via Serilog)
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoggingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _logFilePath;

        /// <summary>
        /// Initializes a new instance of LoggingService
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="logger">Serilog logger</param>
        /// <param name="configuration">Application configuration</param>
        public LoggingService(ApplicationDbContext context, ILogger<LoggingService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _logFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"custom-{DateTime.Now:yyyyMMdd}.txt");

            // Ensure log directory exists
            var logDir = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
        }

        /// <inheritdoc/>
        public async Task LogInformationAsync(string message, int? userId = null, Dictionary<string, object>? additionalData = null)
        {
            try
            {
                // Log to Serilog (Console + File)
                _logger.LogInformation("User: {UserId} | {Message} | Data: {Data}", 
                    userId, message, additionalData != null ? JsonConvert.SerializeObject(additionalData) : "");

                // Log to custom file
                await WriteToFileAsync("INFO", message, userId, additionalData);

                // Log to database if enabled
                if (_configuration.GetValue<bool>("SecuritySettings:EnableAuditLog", true))
                {
                    await WriteToDatabaseAsync("Information", "System", 0, message, null, null, null, additionalData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LogInformationAsync");
            }
        }

        /// <inheritdoc/>
        public async Task LogWarningAsync(string message, int? userId = null, Dictionary<string, object>? additionalData = null)
        {
            try
            {
                _logger.LogWarning("User: {UserId} | {Message} | Data: {Data}", 
                    userId, message, additionalData != null ? JsonConvert.SerializeObject(additionalData) : "");

                await WriteToFileAsync("WARN", message, userId, additionalData);

                if (_configuration.GetValue<bool>("SecuritySettings:EnableAuditLog", true))
                {
                    await WriteToDatabaseAsync("Warning", "System", 0, message, null, null, null, additionalData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LogWarningAsync");
            }
        }

        /// <inheritdoc/>
        public async Task LogErrorAsync(string message, Exception exception, int? userId = null, Dictionary<string, object>? additionalData = null)
        {
            try
            {
                var exceptionDetails = exception != null 
                    ? $"Exception: {exception.GetType().Name} | Message: {exception.Message} | StackTrace: {exception.StackTrace}" 
                    : "";

                _logger.LogError(exception, "User: {UserId} | {Message} | {ExceptionDetails} | Data: {Data}", 
                    userId, message, exceptionDetails, additionalData != null ? JsonConvert.SerializeObject(additionalData) : "");

                await WriteToFileAsync("ERROR", $"{message} | {exceptionDetails}", userId, additionalData);

                if (_configuration.GetValue<bool>("SecuritySettings:EnableAuditLog", true))
                {
                    var errorData = additionalData ?? new Dictionary<string, object>();
                    if (exception != null)
                    {
                        errorData["ExceptionType"] = exception.GetType().Name;
                        errorData["ExceptionMessage"] = exception.Message;
                        errorData["StackTrace"] = exception.StackTrace ?? "";
                    }

                    await WriteToDatabaseAsync("Error", "System", 0, message, null, null, null, errorData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LogErrorAsync");
            }
        }

        /// <inheritdoc/>
        public async Task LogAuthenticationEventAsync(string eventType, int? userId, string mobileNumber, string? ipAddress, bool success, string? details = null)
        {
            try
            {
                var message = $"Authentication Event: {eventType} | Mobile: {mobileNumber} | Success: {success}";
                if (!string.IsNullOrEmpty(details))
                {
                    message += $" | Details: {details}";
                }

                _logger.LogInformation("Auth Event | User: {UserId} | {Message}", userId, message);

                var additionalData = new Dictionary<string, object>
                {
                    { "EventType", eventType },
                    { "MobileNumber", mobileNumber },
                    { "IPAddress", ipAddress ?? "Unknown" },
                    { "Success", success },
                    { "Details", details ?? "" }
                };

                await WriteToFileAsync("AUTH", message, userId, additionalData);

                if (_configuration.GetValue<bool>("SecuritySettings:EnableAuditLog", true))
                {
                    await WriteToDatabaseAsync(
                        action: eventType,
                        entity: "Authentication",
                        entityId: userId ?? 0,
                        message: message,
                        oldValues: null,
                        newValues: JsonConvert.SerializeObject(additionalData),
                        ipAddress: ipAddress
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LogAuthenticationEventAsync");
            }
        }

        /// <inheritdoc/>
        public async Task LogAuditAsync(string action, string entity, int entityId, int? userId, string? oldValues = null, string? newValues = null, string? ipAddress = null)
        {
            try
            {
                var message = $"Audit | Action: {action} | Entity: {entity} | EntityId: {entityId}";
                _logger.LogInformation("Audit | User: {UserId} | {Message}", userId, message);

                var additionalData = new Dictionary<string, object>
                {
                    { "Action", action },
                    { "Entity", entity },
                    { "EntityId", entityId },
                    { "IPAddress", ipAddress ?? "Unknown" }
                };

                await WriteToFileAsync("AUDIT", message, userId, additionalData);

                if (_configuration.GetValue<bool>("SecuritySettings:EnableAuditLog", true))
                {
                    var auditLog = new AuditLog
                    {
                        UserId = userId?.ToString() ?? "System",
                        Action = action,
                        Entity = entity,
                        EntityId = entityId,
                        OldValues = oldValues,
                        NewValues = newValues,
                        IpAddress = ipAddress,
                        Timestamp = DateTime.UtcNow
                    };

                    _context.AuditLogs.Add(auditLog);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LogAuditAsync");
            }
        }

        /// <inheritdoc/>
        public async Task LogVerificationAsync(string verificationType, int userId, string status, string verifiedBy, string? comments = null)
        {
            try
            {
                var message = $"Verification | Type: {verificationType} | Status: {status} | VerifiedBy: {verifiedBy}";
                if (!string.IsNullOrEmpty(comments))
                {
                    message += $" | Comments: {comments}";
                }

                _logger.LogInformation("Verification | User: {UserId} | {Message}", userId, message);

                var additionalData = new Dictionary<string, object>
                {
                    { "VerificationType", verificationType },
                    { "Status", status },
                    { "VerifiedBy", verifiedBy },
                    { "Comments", comments ?? "" }
                };

                await WriteToFileAsync("VERIFY", message, userId, additionalData);

                // Also create verification log in database
                var verificationLog = new VerificationLog
                {
                    UserId = userId,
                    VerifiedBy = verifiedBy,
                    VerifierType = verifiedBy.Contains("Police") ? "Police" : verifiedBy.Contains("Admin") ? "Admin" : "System",
                    VerificationType = verificationType,
                    Status = status,
                    Comments = comments,
                    VerifiedAt = DateTime.UtcNow
                };

                _context.VerificationLogs.Add(verificationLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LogVerificationAsync");
            }
        }

        /// <summary>
        /// Writes log entry to file
        /// </summary>
        private async Task WriteToFileAsync(string level, string message, int? userId, Dictionary<string, object>? additionalData = null)
        {
            try
            {
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] " +
                              $"User: {userId ?? 0} | {message}";

                if (additionalData != null && additionalData.Any())
                {
                    logEntry += $" | Data: {JsonConvert.SerializeObject(additionalData)}";
                }

                logEntry += Environment.NewLine;

                await File.AppendAllTextAsync(_logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to log file");
            }
        }

        /// <summary>
        /// Writes log entry to database
        /// </summary>
        private async Task WriteToDatabaseAsync(string action, string entity, int entityId, string message, string? oldValues, string? newValues, string? ipAddress, Dictionary<string, object>? additionalData = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserId = "System",
                    Action = action,
                    Entity = entity,
                    EntityId = entityId,
                    OldValues = oldValues,
                    NewValues = newValues ?? (additionalData != null ? JsonConvert.SerializeObject(additionalData) : null),
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to database log");
            }
        }
    }
}

