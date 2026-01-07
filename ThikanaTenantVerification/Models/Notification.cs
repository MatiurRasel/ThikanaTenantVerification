using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int? LandlordId { get; set; }

        public int? TenantId { get; set; }

        [Required]
        [MaxLength(500)]
        public string MessageBN { get; set; }

        [MaxLength(500)]
        public string? MessageEN { get; set; }

        [MaxLength(200)]
        public string? Subject { get; set; }

        [Required]
        [MaxLength(50)]
        public string NotificationType { get; set; }

        public bool IsRead { get; set; } = false;

        public bool IsImportant { get; set; } = false;

        [MaxLength(500)]
        public string? ActionLink { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
