using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("VerificationLogs")]
    public class VerificationLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string VerifiedBy { get; set; }

        [Required]
        [MaxLength(50)]
        public string VerifierType { get; set; }

        [Required]
        [MaxLength(50)]
        public string VerificationType { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        [MaxLength(500)]
        public string? Comments { get; set; }

        public DateTime VerifiedAt { get; set; } = DateTime.Now;

        public DateTime? NextVerificationDate { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
