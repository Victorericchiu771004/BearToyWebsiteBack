using System.ComponentModel.DataAnnotations;

namespace BearToyWebsiteBack.Models.ViewModels
{
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "請輸入帳號")]
        [Display(Name = "管理員帳號")]
        [StringLength(50, ErrorMessage = "帳號長度不得超過50字")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "請輸入密碼")]
        [Display(Name = "密碼")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在6-100字之間")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "記住我")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}