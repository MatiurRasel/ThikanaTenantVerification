using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("CurrentLandlord")]
    public class CurrentLandlord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string NameBN { get; set; }

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; }

        [MaxLength(20)]
        public string? NIDNumber { get; set; }

        [Required]
        [MaxLength(500)]
        public string Address { get; set; }

        [Required]
        public DateTime FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public bool IsVerified { get; set; } = false;

        public DateTime? VerificationDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
