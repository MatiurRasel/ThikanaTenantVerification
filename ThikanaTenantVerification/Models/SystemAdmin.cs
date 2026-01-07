using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("SystemAdmins")]
    public class SystemAdmin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(100)]
        public string Designation { get; set; }

        [Required]
        [MaxLength(15)]
        public string MobileNumber { get; set; }

        [MaxLength(20)]
        public string? NIDNumber { get; set; }

        [MaxLength(200)]
        public string? PoliceStation { get; set; }

        [MaxLength(100)]
        public string? Thana { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        [MaxLength(100)]
        public string? Division { get; set; }

        [Required]
        [MaxLength(50)]
        public string AccessLevel { get; set; } // Station, Thana, District, Division, Super

        [MaxLength(255)]
        public string? PasswordResetToken { get; set; }

        public DateTime? TokenExpiry { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }
    }
}