using System.ComponentModel.DataAnnotations;

namespace BearToyWebsiteBack.Models
{
    public class AdminRole
    {
        [Key]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "角色名稱為必填")]
        [StringLength(50, ErrorMessage = "角色名稱長度不得超過50字")]
        public string RoleName { get; set; } = null!;

        [StringLength(200, ErrorMessage = "角色描述長度不得超過200字")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 導航屬性
        public virtual ICollection<AdminRoleAssignment> AdminRoleAssignments { get; set; } = new List<AdminRoleAssignment>();
        public virtual ICollection<AdminRolePermission> AdminRolePermissions { get; set; } = new List<AdminRolePermission>();
    }
}