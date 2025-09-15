using System.ComponentModel.DataAnnotations;

namespace BearToyWebsiteBack.Models
{
    public class AdminRolePermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 導航屬性
        public virtual AdminRole AdminRole { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}