using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikanaTenantVerification.Models
{
    [Table("FamilyMembers")]
    public class FamilyMember
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string NameBN { get; set; }

        [MaxLength(200)]
        public string? NameEN { get; set; }

        [Required]
        [MaxLength(100)]
        public string Relationship { get; set; }

        [Required]
        public int Age { get; set; }

        [MaxLength(100)]
        public string? Occupation { get; set; }

        [MaxLength(15)]
        public string? MobileNumber { get; set; }

        [MaxLength(20)]
        public string? NIDNumber { get; set; }

        public bool IsNIDVerified { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
