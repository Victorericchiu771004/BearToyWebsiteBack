using BearToyWebsiteBack.Attributes;
using BearToyWebsiteBack.Models;
using Microsoft.EntityFrameworkCore;

namespace BearToyWebsiteBack.Services
{
    public class AdminInitializeService : IAdminInitializeService
    {
        private readonly AdminDbContext _context;
        private readonly IAdminAuthService _adminAuthService;
        private readonly ILogger<AdminInitializeService> _logger;

        public AdminInitializeService(
            AdminDbContext context, 
            IAdminAuthService adminAuthService,
            ILogger<AdminInitializeService> logger)
        {
            _context = context;
            _adminAuthService = adminAuthService;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // 確保資料庫已建立
                await _context.Database.EnsureCreatedAsync();
                
                await SeedDefaultDataAsync();
                
                _logger.LogInformation("管理員系統初始化完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "管理員系統初始化失敗");
                throw;
            }
        }

        public async Task SeedDefaultDataAsync()
        {
            try
            {
                // 1. 建立預設權限
                await SeedPermissionsAsync();
                
                // 2. 建立預設角色
                await SeedRolesAsync();
                
                // 3. 建立角色權限關聯
                await SeedRolePermissionsAsync();
                
                // 4. 建立預設超級管理員
                await SeedSuperAdminAsync();

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("預設資料建立完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立預設資料失敗");
                throw;
            }
        }

        private async Task SeedPermissionsAsync()
        {
            var permissions = new[]
            {
                new { Code = AdminPermissions.ProductView, Name = "查看商品", Category = "商品管理" },
                new { Code = AdminPermissions.ProductCreate, Name = "新增商品", Category = "商品管理" },
                new { Code = AdminPermissions.ProductEdit, Name = "編輯商品", Category = "商品管理" },
                new { Code = AdminPermissions.ProductDelete, Name = "刪除商品", Category = "商品管理" },
                
                new { Code = AdminPermissions.OrderView, Name = "查看訂單", Category = "訂單管理" },
                new { Code = AdminPermissions.OrderCreate, Name = "新增訂單", Category = "訂單管理" },
                new { Code = AdminPermissions.OrderEdit, Name = "編輯訂單", Category = "訂單管理" },
                new { Code = AdminPermissions.OrderDelete, Name = "刪除訂單", Category = "訂單管理" },
                
                new { Code = AdminPermissions.MemberView, Name = "查看會員", Category = "會員管理" },
                new { Code = AdminPermissions.MemberEdit, Name = "編輯會員", Category = "會員管理" },
                new { Code = AdminPermissions.MemberDelete, Name = "刪除會員", Category = "會員管理" },
                
                new { Code = AdminPermissions.PromotionView, Name = "查看行銷活動", Category = "行銷活動管理" },
                new { Code = AdminPermissions.PromotionCreate, Name = "新增行銷活動", Category = "行銷活動管理" },
                new { Code = AdminPermissions.PromotionEdit, Name = "編輯行銷活動", Category = "行銷活動管理" },
                new { Code = AdminPermissions.PromotionDelete, Name = "刪除行銷活動", Category = "行銷活動管理" },
                new { Code = AdminPermissions.PromotionAnalytics, Name = "行銷活動分析", Category = "行銷活動管理" },
                
                new { Code = AdminPermissions.AdminManage, Name = "管理員管理", Category = "系統管理" },
                new { Code = AdminPermissions.SystemConfig, Name = "系統設定", Category = "系統管理" },
                new { Code = AdminPermissions.ReportView, Name = "查看報表", Category = "報表管理" }
            };

            foreach (var perm in permissions)
            {
                var existingPermission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.PermissionCode == perm.Code);

                if (existingPermission == null)
                {
                    _context.Permissions.Add(new Permission
                    {
                        PermissionCode = perm.Code,
                        PermissionName = perm.Name,
                        Category = perm.Category,
                        Description = perm.Name,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                }
            }
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[]
            {
                new { Name = "超級管理員", Description = "擁有所有系統權限" },
                new { Name = "商品管理員", Description = "負責商品相關功能" },
                new { Name = "訂單管理員", Description = "負責訂單相關功能" },
                new { Name = "客服人員", Description = "負責會員服務相關功能" }
            };

            foreach (var role in roles)
            {
                var existingRole = await _context.AdminRoles
                    .FirstOrDefaultAsync(r => r.RoleName == role.Name);

                if (existingRole == null)
                {
                    _context.AdminRoles.Add(new AdminRole
                    {
                        RoleName = role.Name,
                        Description = role.Description,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedRolePermissionsAsync()
        {
            // 超級管理員 - 所有權限
            var superAdminRole = await _context.AdminRoles
                .FirstOrDefaultAsync(r => r.RoleName == "超級管理員");
            
            if (superAdminRole != null)
            {
                var allPermissions = await _context.Permissions.ToListAsync();
                
                foreach (var permission in allPermissions)
                {
                    var existingRolePermission = await _context.AdminRolePermissions
                        .FirstOrDefaultAsync(rp => rp.RoleId == superAdminRole.RoleId && 
                                                  rp.PermissionId == permission.PermissionId);

                    if (existingRolePermission == null)
                    {
                        _context.AdminRolePermissions.Add(new AdminRolePermission
                        {
                            RoleId = superAdminRole.RoleId,
                            PermissionId = permission.PermissionId,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }

            // 商品管理員 - 商品相關權限
            var productManagerRole = await _context.AdminRoles
                .FirstOrDefaultAsync(r => r.RoleName == "商品管理員");
            
            if (productManagerRole != null)
            {
                var productPermissions = await _context.Permissions
                    .Where(p => p.Category == "商品管理")
                    .ToListAsync();

                foreach (var permission in productPermissions)
                {
                    var existingRolePermission = await _context.AdminRolePermissions
                        .FirstOrDefaultAsync(rp => rp.RoleId == productManagerRole.RoleId && 
                                                  rp.PermissionId == permission.PermissionId);

                    if (existingRolePermission == null)
                    {
                        _context.AdminRolePermissions.Add(new AdminRolePermission
                        {
                            RoleId = productManagerRole.RoleId,
                            PermissionId = permission.PermissionId,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedSuperAdminAsync()
        {
            var existingSuperAdmin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Username == "admin");

            if (existingSuperAdmin == null)
            {
                var superAdmin = new Admin
                {
                    Username = "admin",
                    PasswordHash = _adminAuthService.HashPassword("admin123"),
                    FullName = "系統管理員",
                    Email = "admin@beartoy.com",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _context.Admins.Add(superAdmin);
                await _context.SaveChangesAsync();

                // 指派超級管理員角色
                var superAdminRole = await _context.AdminRoles
                    .FirstOrDefaultAsync(r => r.RoleName == "超級管理員");

                if (superAdminRole != null)
                {
                    _context.AdminRoleAssignments.Add(new AdminRoleAssignment
                    {
                        AdminId = superAdmin.AdminId,
                        RoleId = superAdminRole.RoleId,
                        CreatedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
                
                _logger.LogInformation("預設超級管理員帳號已建立：admin/admin123");
            }
        }
    }
}