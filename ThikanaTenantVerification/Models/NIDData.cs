namespace ThikanaTenantVerification.Models
{
    public class NIDData
    {
        public string NIDNumber { get; set; }
        public string? BirthCertificateNumber { get; set; }
        public string FullNameBN { get; set; }
        public string? FullNameEN { get; set; }
        public string FatherNameBN { get; set; }
        public string? FatherNameEN { get; set; }
        public string? MotherNameBN { get; set; }
        public string? MotherNameEN { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string Religion { get; set; }
        public string MobileNumber { get; set; }
        public string? Email { get; set; }
        public string PermanentAddress { get; set; }
        public string? ProfileImage { get; set; }
    }
}
