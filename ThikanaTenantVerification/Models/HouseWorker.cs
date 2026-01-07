using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("HouseWorkers")]
    public class HouseWorker
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string WorkerType { get; set; }

        [Required]
        [MaxLength(200)]
        public string NameBN { get; set; }

        [MaxLength(200)]
        public string? NameEN { get; set; }

        [Required]
        [MaxLength(20)]
        public string NIDNumber { get; set; }

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; }

        [Required]
        [MaxLength(500)]
        public string PermanentAddress { get; set; }

        public bool IsValidFromPolice { get; set; } = true;

        [MaxLength(500)]
        public string? ValidationMessage { get; set; }

        public bool IsDangerFlag { get; set; } = false;

        public DateTime? PoliceVerifiedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
