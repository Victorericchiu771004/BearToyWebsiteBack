using BearToyWebsiteBack.Models;

namespace BearToyWebsiteBack.Services
{
    public interface IAdminAuthService
    {
        Task<AdminLoginResult> LoginAsync(string username, string password, string clientIp);
        Task<bool> LogoutAsync(int adminId);
        Task<bool> HasPermissionAsync(int adminId, string permissionCode);
        Task<List<string>> GetAdminPermissionsAsync(int adminId);
        Task<Admin?> GetAdminByIdAsync(int adminId);
        Task<bool> IsAdminLockedAsync(string username);
        Task<bool> UnlockAdminAsync(string username);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }

    public class AdminLoginResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Admin? Admin { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }
}