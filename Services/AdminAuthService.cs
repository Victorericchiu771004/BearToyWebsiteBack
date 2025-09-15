using BearToyWebsiteBack.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BearToyWebsiteBack.Services
{
    public class AdminAuthService : IAdminAuthService
    {
        private readonly BearToyDbContext _context;
        private readonly ILogger<AdminAuthService> _logger;
        private const int MaxLoginAttempts = 5;
        private const int LockoutMinutes = 30;

        public AdminAuthService(BearToyDbContext context, ILogger<AdminAuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AdminLoginResult> LoginAsync(string username, string password, string clientIp)
        {
            try
            {
                var admin = await _context.Admins
                    .Include(a => a.AdminRoleAssignments)
                        .ThenInclude(ara => ara.AdminRole)
                    .FirstOrDefaultAsync(a => a.Username == username && a.IsActive);

                if (admin == null)
                {
                    _logger.LogWarning("登入失敗：找不到使用者 {Username}", username);
                    return new AdminLoginResult 
                    { 
                        Success = false, 
                        ErrorMessage = "帳號或密碼錯誤" 
                    };
                }

                // 檢查帳號是否被鎖定
                if (admin.LockedUntil.HasValue && admin.LockedUntil > DateTime.Now)
                {
                    _logger.LogWarning("登入失敗：帳號 {Username} 已被鎖定", username);
                    return new AdminLoginResult 
                    { 
                        Success = false, 
                        ErrorMessage = $"帳號已被鎖定，請於 {admin.LockedUntil:yyyy/MM/dd HH:mm} 後再試" 
                    };
                }

                // 驗證密碼
                if (!VerifyPassword(password, admin.PasswordHash))
                {
                    // 增加失敗次數
                    admin.LoginAttempts++;
                    
                    // 如果超過最大嘗試次數，鎖定帳號
                    if (admin.LoginAttempts >= MaxLoginAttempts)
                    {
                        admin.LockedUntil = DateTime.Now.AddMinutes(LockoutMinutes);
                        _logger.LogWarning("帳號 {Username} 因多次登入失敗被鎖定", username);
                    }

                    await _context.SaveChangesAsync();

                    return new AdminLoginResult 
                    { 
                        Success = false, 
                        ErrorMessage = "帳號或密碼錯誤" 
                    };
                }

                // 登入成功，重置失敗次數
                admin.LoginAttempts = 0;
                admin.LockedUntil = null;
                admin.LastLoginAt = DateTime.Now;
                admin.LastLoginIp = clientIp;

                await _context.SaveChangesAsync();

                // 取得權限
                var permissions = await GetAdminPermissionsAsync(admin.AdminId);

                _logger.LogInformation("管理員 {Username} 成功登入", username);

                return new AdminLoginResult 
                { 
                    Success = true, 
                    Admin = admin,
                    Permissions = permissions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登入過程發生錯誤：{Username}", username);
                return new AdminLoginResult 
                { 
                    Success = false, 
                    ErrorMessage = "系統錯誤，請稍後再試" 
                };
            }
        }

        public async Task<bool> LogoutAsync(int adminId)
        {
            try
            {
                _logger.LogInformation("管理員 {AdminId} 登出", adminId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登出過程發生錯誤：{AdminId}", adminId);
                return false;
            }
        }

        public async Task<bool> HasPermissionAsync(int adminId, string permissionCode)
        {
            try
            {
                var hasPermission = await _context.AdminRoleAssignments
                    .Where(ara => ara.AdminId == adminId)
                    .Join(_context.AdminRolePermissions,
                        ara => ara.RoleId,
                        arp => arp.RoleId,
                        (ara, arp) => arp)
                    .Join(_context.Permissions,
                        arp => arp.PermissionId,
                        p => p.PermissionId,
                        (arp, p) => p)
                    .AnyAsync(p => p.PermissionCode == permissionCode && p.IsActive);

                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查權限時發生錯誤：AdminId={AdminId}, PermissionCode={PermissionCode}", adminId, permissionCode);
                return false;
            }
        }

        public async Task<List<string>> GetAdminPermissionsAsync(int adminId)
        {
            try
            {
                var permissions = await _context.AdminRoleAssignments
                    .Where(ara => ara.AdminId == adminId)
                    .Join(_context.AdminRolePermissions,
                        ara => ara.RoleId,
                        arp => arp.RoleId,
                        (ara, arp) => arp)
                    .Join(_context.Permissions,
                        arp => arp.PermissionId,
                        p => p.PermissionId,
                        (arp, p) => p)
                    .Where(p => p.IsActive)
                    .Select(p => p.PermissionCode)
                    .Distinct()
                    .ToListAsync();

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得管理員權限時發生錯誤：{AdminId}", adminId);
                return new List<string>();
            }
        }

        public async Task<Admin?> GetAdminByIdAsync(int adminId)
        {
            try
            {
                return await _context.Admins
                    .Include(a => a.AdminRoleAssignments)
                        .ThenInclude(ara => ara.AdminRole)
                    .FirstOrDefaultAsync(a => a.AdminId == adminId && a.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得管理員資料時發生錯誤：{AdminId}", adminId);
                return null;
            }
        }

        public async Task<bool> IsAdminLockedAsync(string username)
        {
            try
            {
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Username == username);

                return admin?.LockedUntil.HasValue == true && admin.LockedUntil > DateTime.Now;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查帳號鎖定狀態時發生錯誤：{Username}", username);
                return false;
            }
        }

        public async Task<bool> UnlockAdminAsync(string username)
        {
            try
            {
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Username == username);

                if (admin != null)
                {
                    admin.LockedUntil = null;
                    admin.LoginAttempts = 0;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("帳號 {Username} 已解除鎖定", username);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解除帳號鎖定時發生錯誤：{Username}", username);
                return false;
            }
        }

        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = password + "BearToy2025!";
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(hashedBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }
}