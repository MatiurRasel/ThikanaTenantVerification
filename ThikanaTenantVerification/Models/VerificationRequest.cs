using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("VerificationRequests")]
    public class VerificationRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequestType { get; set; } // New, Update, Renew

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Rejected

        [MaxLength(20)]
        public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.Now;

        public DateTime? ProcessedAt { get; set; }

        [MaxLength(100)]
        public string? ProcessedBy { get; set; }

        [MaxLength(500)]
        public string? AdminComments { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}