using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("PoliceVerificationRequests")]
    public class PoliceVerificationRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string NIDNumber { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [Required]
        [MaxLength(200)]
        public string PoliceStation { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public DateTime? ResponseDate { get; set; }

        [MaxLength(50)]
        public string? PoliceId { get; set; }

        [MaxLength(500)]
        public string? Comments { get; set; }

        public bool HasCriminalRecord { get; set; } = false;

        public string? CriminalRecordDetails { get; set; }

        [MaxLength(50)]
        public string OverallStatus { get; set; } = "Clean";

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
