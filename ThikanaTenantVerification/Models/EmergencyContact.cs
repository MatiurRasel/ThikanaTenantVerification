using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("EmergencyContacts")]
    public class EmergencyContact
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string NameBN { get; set; }

        [Required]
        [MaxLength(100)]
        public string Relationship { get; set; }

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; }

        [MaxLength(20)]
        public string? NIDNumber { get; set; }

        [Required]
        [MaxLength(500)]
        public string Address { get; set; }

        public bool IsNIDVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
