namespace ThikanaTenantVerification.Models
{
    /// <summary>
    /// Police verification data model for criminal record checks
    /// </summary>
    public class PoliceVerificationData
    {
        /// <summary>
        /// NID number of the person being verified
        /// </summary>
        public string NIDNumber { get; set; } = string.Empty;

        /// <summary>
        /// Whether the person has a clean record
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Validation message from police database
        /// </summary>
        public string ValidationMessage { get; set; } = string.Empty;

        /// <summary>
        /// List of case records (can be objects or strings)
        /// </summary>
        public List<object> CaseRecords { get; set; } = new List<object>();

        /// <summary>
        /// Date when last verified (format: yyyy-MM-dd)
        /// </summary>
        public string LastVerified { get; set; } = string.Empty;

        /// <summary>
        /// Danger level: নিম্ন, মাধ্যম, উচ্চ (Low, Medium, High)
        /// </summary>
        public string DangerLevel { get; set; } = "নিম্ন";

        /// <summary>
        /// Police station name that performed verification
        /// </summary>
        public string PoliceStation { get; set; } = string.Empty;

        /// <summary>
        /// Name/ID of officer who verified
        /// </summary>
        public string VerifiedBy { get; set; } = string.Empty;
    }
}
