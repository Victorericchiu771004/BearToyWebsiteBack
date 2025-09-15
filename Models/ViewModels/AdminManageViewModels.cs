using System.ComponentModel.DataAnnotations;

namespace BearToyWebsiteBack.Models.ViewModels
{
    public class AdminManageViewModel
    {
        public int AdminId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsLocked { get; set; }
        public List<AdminRoleInfo> Roles { get; set; } = new List<AdminRoleInfo>();
    }

    public class AdminDetailViewModel
    {
        public int AdminId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string LastLoginIp { get; set; } = string.Empty;
        public int LoginAttempts { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockedUntil { get; set; }
        public List<AdminRoleInfo> Roles { get; set; } = new List<AdminRoleInfo>();
        public List<AdminPermissionInfo> Permissions { get; set; } = new List<AdminPermissionInfo>();
    }

    public class CreateAdminViewModel
    {
        [Required(ErrorMessage = "使用者名稱為必填")]
        [StringLength(50, ErrorMessage = "使用者名稱不能超過50個字元")]
        [Display(Name = "使用者名稱")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "密碼為必填")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在6-100個字元之間")]
        [DataType(DataType.Password)]
        [Display(Name = "密碼")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "確認密碼為必填")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "密碼與確認密碼不相符")]
        [Display(Name = "確認密碼")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "姓名為必填")]
        [StringLength(100, ErrorMessage = "姓名不能超過100個字元")]
        [Display(Name = "姓名")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email為必填")]
        [StringLength(200, ErrorMessage = "Email不能超過200個字元")]
        [EmailAddress(ErrorMessage = "請輸入有效的Email格式")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "電話號碼不能超過20個字元")]
        [Display(Name = "電話號碼")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "帳號啟用")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "指派角色")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();
    }

    public class EditAdminViewModel
    {
        public int AdminId { get; set; }

        [Required(ErrorMessage = "使用者名稱為必填")]
        [StringLength(50, ErrorMessage = "使用者名稱不能超過50個字元")]
        [Display(Name = "使用者名稱")]
        public string Username { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在6-100個字元之間")]
        [DataType(DataType.Password)]
        [Display(Name = "新密碼（留空表示不變更）")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "密碼與確認密碼不相符")]
        [Display(Name = "確認新密碼")]
        public string? ConfirmNewPassword { get; set; }

        [Required(ErrorMessage = "姓名為必填")]
        [StringLength(100, ErrorMessage = "姓名不能超過100個字元")]
        [Display(Name = "姓名")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email為必填")]
        [StringLength(200, ErrorMessage = "Email不能超過200個字元")]
        [EmailAddress(ErrorMessage = "請輸入有效的Email格式")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "電話號碼不能超過20個字元")]
        [Display(Name = "電話號碼")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "帳號啟用")]
        public bool IsActive { get; set; }

        [Display(Name = "指派角色")]
        public List<int> SelectedRoleIds { get; set; } = new List<int>();
    }

    public class AdminRoleInfo
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class AdminPermissionInfo
    {
        public int PermissionId { get; set; }
        public string PermissionCode { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}