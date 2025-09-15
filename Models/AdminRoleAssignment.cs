using System.ComponentModel.DataAnnotations;

namespace BearToyWebsiteBack.Models
{
    public class AdminRoleAssignment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AdminId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? CreatedBy { get; set; }

        // 導航屬性
        public virtual Admin Admin { get; set; } = null!;
        public virtual AdminRole AdminRole { get; set; } = null!;
    }
}