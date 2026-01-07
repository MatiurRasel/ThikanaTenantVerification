namespace ThikanaTenantVerification.Models
{
    public class PoliceVerificationData
    {
        public string NIDNumber { get; set; }
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; }
        public List<string> CaseRecords { get; set; } = new List<string>();
        public string LastVerified { get; set; }
        public string DangerLevel { get; set; }
    }
}
