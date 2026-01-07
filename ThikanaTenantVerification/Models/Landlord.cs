using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("Landlords")]
    public class Landlord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string NameBN { get; set; }

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; }

        [MaxLength(20)]
        public string? NIDNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }
    }
}