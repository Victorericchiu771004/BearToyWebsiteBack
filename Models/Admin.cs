using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BearToyWebsiteBack.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [Required(ErrorMessage = "管理員帳號為必填")]
        [StringLength(50, ErrorMessage = "管理員帳號長度不得超過50字")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "密碼為必填")]
        [StringLength(255)]
        public string PasswordHash { get; set; } = null!;

        [Required(ErrorMessage = "管理員姓名為必填")]
        [StringLength(100, ErrorMessage = "管理員姓名長度不得超過100字")]
        public string FullName { get; set; } = null!;

        [StringLength(200, ErrorMessage = "電子郵件長度不得超過200字")]
        [EmailAddress(ErrorMessage = "請輸入有效的電子郵件格式")]
        public string? Email { get; set; }

        [StringLength(20, ErrorMessage = "電話號碼長度不得超過20字")]
        public string? Phone { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLoginAt { get; set; }

        [StringLength(45)]
        public string? LastLoginIp { get; set; }

        public int LoginAttempts { get; set; } = 0;

        public DateTime? LockedUntil { get; set; }

        // 導航屬性
        public virtual ICollection<AdminRoleAssignment> AdminRoleAssignments { get; set; } = new List<AdminRoleAssignment>();
    }
}