using BearToyWebsiteBack.Models;
using BearToyWebsiteBack.Models.ViewModels;
using BearToyWebsiteBack.Attributes;
using BearToyWebsiteBack.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BearToyWebsiteBack.Controllers
{
    [Authorize]
    public class AdminManageController : Controller
    {
        private readonly BearToyDbContext _context;
        private readonly IAdminAuthService _adminAuthService;

        public AdminManageController(BearToyDbContext context, IAdminAuthService adminAuthService)
        {
            _context = context;
            _adminAuthService = adminAuthService;
        }

        // GET: AdminManage
        [AdminPermission(AdminPermissions.AdminManage)]
        public async Task<IActionResult> Index()
        {
            var admins = await _context.Admins
                .Include(a => a.AdminRoleAssignments)
                .ThenInclude(ara => ara.AdminRole)
                .OrderBy(a => a.Username)
                .ToListAsync();

            var adminViewModels = admins.Select(a => new AdminManageViewModel
            {
                AdminId = a.AdminId,
                Username = a.Username,
                FullName = a.FullName,
                Email = a.Email,
                Phone = a.Phone,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                LastLoginAt = a.LastLoginAt,
                IsLocked = a.LockedUntil.HasValue && a.LockedUntil > DateTime.Now,
                Roles = a.AdminRoleAssignments.Select(ara => new AdminRoleInfo
                {
                    RoleId = ara.AdminRole.RoleId,
                    RoleName = ara.AdminRole.RoleName,
                    Description = ara.AdminRole.Description
                }).ToList()
            }).ToList();

            return View(adminViewModels);
        }

        // GET: AdminManage/Details/5
        [AdminPermission(AdminPermissions.AdminManage)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins
                .Include(a => a.AdminRoleAssignments)
                .ThenInclude(ara => ara.AdminRole)
                .ThenInclude(ar => ar.AdminRolePermissions)
                .ThenInclude(arp => arp.Permission)
                .FirstOrDefaultAsync(a => a.AdminId == id);

            if (admin == null)
            {
                return NotFound();
            }

            var viewModel = new AdminDetailViewModel
            {
                AdminId = admin.AdminId,
                Username = admin.Username,
                FullName = admin.FullName,
                Email = admin.Email,
                Phone = admin.Phone,
                IsActive = admin.IsActive,
                CreatedAt = admin.CreatedAt,
                LastLoginAt = admin.LastLoginAt,
                LastLoginIp = admin.LastLoginIp,
                LoginAttempts = admin.LoginAttempts,
                IsLocked = admin.LockedUntil.HasValue && admin.LockedUntil > DateTime.Now,
                LockedUntil = admin.LockedUntil,
                Roles = admin.AdminRoleAssignments.Select(ara => new AdminRoleInfo
                {
                    RoleId = ara.AdminRole.RoleId,
                    RoleName = ara.AdminRole.RoleName,
                    Description = ara.AdminRole.Description
                }).ToList(),
                Permissions = admin.AdminRoleAssignments
                    .SelectMany(ara => ara.AdminRole.AdminRolePermissions)
                    .Select(arp => new AdminPermissionInfo
                    {
                        PermissionId = arp.Permission.PermissionId,
                        PermissionCode = arp.Permission.PermissionCode,
                        PermissionName = arp.Permission.PermissionName,
                        Category = arp.Permission.Category
                    })
                    .Distinct()
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.PermissionName)
                    .ToList()
            };

            return View(viewModel);
        }

        // GET: AdminManage/Create
        [AdminPermission(AdminPermissions.AdminManage)]
        public async Task<IActionResult> Create()
        {
            var roles = await _context.AdminRoles
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoleName)
                .ToListAsync();

            ViewBag.Roles = roles;
            return View();
        }

        // POST: AdminManage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.AdminManage)]
        public async Task<IActionResult> Create(CreateAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 檢查使用者名稱是否已存在
                var existingAdmin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Username == model.Username);
                
                if (existingAdmin != null)
                {
                    ModelState.AddModelError("Username", "此使用者名稱已存在");
                }
                else
                {
                    var admin = new Admin
                    {
                        Username = model.Username,
                        PasswordHash = _adminAuthService.HashPassword(model.Password),
                        FullName = model.FullName,
                        Email = model.Email,
                        Phone = model.Phone,
                        IsActive = model.IsActive,
                        CreatedAt = DateTime.Now,
                        LoginAttempts = 0
                    };

                    _context.Admins.Add(admin);
                    await _context.SaveChangesAsync();

                    // 指派角色
                    foreach (var roleId in model.SelectedRoleIds)
                    {
                        var roleAssignment = new AdminRoleAssignment
                        {
                            AdminId = admin.AdminId,
                            RoleId = roleId,
                            CreatedAt = DateTime.Now,
                            CreatedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0")
                        };
                        _context.AdminRoleAssignments.Add(roleAssignment);
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "管理員建立成功！";
                    return RedirectToAction(nameof(Index));
                }
            }

            var roles = await _context.AdminRoles
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoleName)
                .ToListAsync();

            ViewBag.Roles = roles;
            return View(model);
        }

        // GET: AdminManage/Edit/5
        [AdminPermission(AdminPermissions.AdminManage)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins
                .Include(a => a.AdminRoleAssignments)
                .FirstOrDefaultAsync(a => a.AdminId == id);

            if (admin == null)
            {
                return NotFound();
            }

            var model = new EditAdminViewModel
            {
                AdminId = admin.AdminId,
                Username = admin.Username,
                FullName = admin.FullName,
                Email = admin.Email,
                Phone = admin.Phone,
                IsActive = admin.IsActive,
                SelectedRoleIds = admin.AdminRoleAssignments.Select(ara => ara.RoleId).ToList()
            };

            var roles = await _context.AdminRoles
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoleName)
                .ToListAsync();

            ViewBag.Roles = roles;
            return View(model);
        }

        // POST: AdminManage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.AdminManage)]
        public async Task<IActionResult> Edit(int id, EditAdminViewModel model)
        {
            if (id != model.AdminId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var admin = await _context.Admins
                        .Include(a => a.AdminRoleAssignments)
                        .FirstOrDefaultAsync(a => a.AdminId == id);

                    if (admin == null)
                    {
                        return NotFound();
                    }

                    // 檢查使用者名稱是否與其他管理員重複
                    var existingAdmin = await _context.Admins
                        .FirstOrDefaultAsync(a => a.Username == model.Username && a.AdminId != id);
                    
                    if (existingAdmin != null)
                    {
                        ModelState.AddModelError("Username", "此使用者名稱已存在");
                    }
                    else
                    {
                        admin.Username = model.Username;
                        admin.FullName = model.FullName;
                        admin.Email = model.Email;
                        admin.Phone = model.Phone;
                        admin.IsActive = model.IsActive;

                        // 更新密碼（如果有提供新密碼）
                        if (!string.IsNullOrEmpty(model.NewPassword))
                        {
                            admin.PasswordHash = _adminAuthService.HashPassword(model.NewPassword);
                        }

                        // 更新角色指派
                        _context.AdminRoleAssignments.RemoveRange(admin.AdminRoleAssignments);
                        foreach (var roleId in model.SelectedRoleIds)
                        {
                            var roleAssignment = new AdminRoleAssignment
                            {
                                AdminId = admin.AdminId,
                                RoleId = roleId,
                                CreatedAt = DateTime.Now,
                                CreatedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0")
                            };
                            _context.AdminRoleAssignments.Add(roleAssignment);
                        }

                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "管理員資料更新成功！";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminExists(model.AdminId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var roles = await _context.AdminRoles
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoleName)
                .ToListAsync();

            ViewBag.Roles = roles;
            return View(model);
        }

        // POST: AdminManage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.AdminManage)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentAdminId = HttpContext.Session.GetInt32("AdminId");
            if (currentAdminId.HasValue && id == currentAdminId.Value)
            {
                TempData["ErrorMessage"] = "無法刪除自己的帳號！";
                return RedirectToAction(nameof(Index));
            }

            var admin = await _context.Admins.FindAsync(id);
            if (admin != null)
            {
                // 軟刪除：將帳號設為停用而非直接刪除
                admin.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "管理員帳號已停用！";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: AdminManage/Unlock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.AdminManage)]
        public async Task<IActionResult> Unlock(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin != null)
            {
                await _adminAuthService.UnlockAdminAsync(admin.Username);
                TempData["SuccessMessage"] = "管理員帳號已解除鎖定！";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AdminExists(int id)
        {
            return _context.Admins.Any(e => e.AdminId == id);
        }
    }
}