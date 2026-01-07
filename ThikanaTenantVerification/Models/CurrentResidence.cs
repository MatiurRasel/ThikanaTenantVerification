using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("CurrentResidence")]
    public class CurrentResidence
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [MaxLength(100)]
        public string? FlatFloor { get; set; }

        [MaxLength(100)]
        public string? HouseHolding { get; set; }

        [MaxLength(200)]
        public string? Road { get; set; }

        [MaxLength(200)]
        public string? Area { get; set; }

        [MaxLength(20)]
        public string? PostCode { get; set; }

        [MaxLength(100)]
        public string? Thana { get; set; }

        [MaxLength(100)]
        public string? District { get; set; }

        [MaxLength(100)]
        public string? Division { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
