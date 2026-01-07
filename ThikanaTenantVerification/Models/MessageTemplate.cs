using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("MessageTemplates")]
    public class MessageTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string TemplateName { get; set; }

        [Required]
        [MaxLength(50)]
        public string TemplateType { get; set; } // SMS, Email, Notification

        [MaxLength(200)]
        public string? SubjectBN { get; set; }

        [MaxLength(200)]
        public string? SubjectEN { get; set; }

        [Required]
        public string BodyBN { get; set; }

        public string? BodyEN { get; set; }

        [MaxLength(500)]
        public string? Variables { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}