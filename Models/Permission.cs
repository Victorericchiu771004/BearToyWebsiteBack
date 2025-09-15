using System.ComponentModel.DataAnnotations;

namespace BearToyWebsiteBack.Models
{
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required(ErrorMessage = "權限代碼為必填")]
        [StringLength(100, ErrorMessage = "權限代碼長度不得超過100字")]
        public string PermissionCode { get; set; } = null!;

        [Required(ErrorMessage = "權限名稱為必填")]
        [StringLength(100, ErrorMessage = "權限名稱長度不得超過100字")]
        public string PermissionName { get; set; } = null!;

        [StringLength(50, ErrorMessage = "權限分類長度不得超過50字")]
        public string? Category { get; set; }

        [StringLength(200, ErrorMessage = "權限描述長度不得超過200字")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 導航屬性
        public virtual ICollection<AdminRolePermission> AdminRolePermissions { get; set; } = new List<AdminRolePermission>();
    }
}