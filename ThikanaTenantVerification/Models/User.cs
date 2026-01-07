using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(20)]
        public string NIDNumber { get; set; }

        [MaxLength(20)]
        public string? BirthCertificateNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string FullNameBN { get; set; }

        [MaxLength(200)]
        public string? FullNameEN { get; set; }

        [Required]
        [MaxLength(200)]
        public string FatherNameBN { get; set; }

        [MaxLength(200)]
        public string? FatherNameEN { get; set; }

        [MaxLength(200)]
        public string? MotherNameBN { get; set; }

        [MaxLength(200)]
        public string? MotherNameEN { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; }

        [Required]
        [MaxLength(20)]
        public string MaritalStatus { get; set; }

        [Required]
        [MaxLength(50)]
        public string Religion { get; set; }

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        [MaxLength(500)]
        public string PermanentAddress { get; set; }

        [MaxLength(500)]
        public string? ProfileImage { get; set; }

        [MaxLength(255)]
        public string? PasswordHash { get; set; }

        public bool IsVerified { get; set; } = false;

        [MaxLength(50)]
        public string VerificationStatus { get; set; } = "Pending";

        public int CompletionPercentage { get; set; } = 0;

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<EmergencyContact> EmergencyContacts { get; set; } = new List<EmergencyContact>();
        public virtual ICollection<FamilyMember> FamilyMembers { get; set; } = new List<FamilyMember>();
        public virtual ICollection<HouseWorker> HouseWorkers { get; set; } = new List<HouseWorker>();
        public virtual ICollection<PreviousLandlord> PreviousLandlords { get; set; } = new List<PreviousLandlord>();
        public virtual CurrentLandlord? CurrentLandlord { get; set; }
        public virtual CurrentResidence? CurrentResidence { get; set; }
        public virtual ICollection<DocumentAttachment> DocumentAttachments { get; set; } = new List<DocumentAttachment>();
        public virtual ICollection<VerificationLog> VerificationLogs { get; set; } = new List<VerificationLog>();
    }

    public class UserCredentials
    {
        public int UserId { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
