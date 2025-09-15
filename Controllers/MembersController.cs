using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BearToyWebsiteBack.Models;
using BearToyWebsiteBack.Data;
using BearToyWebsiteBack.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace BearToyWebsiteBack.Controllers
{
    [Authorize]
    public class MembersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _identityContext;

        public MembersController(UserManager<IdentityUser> userManager, ApplicationDbContext identityContext)
        {
            _userManager = userManager;
            _identityContext = identityContext;
        }

        [AdminPermission(AdminPermissions.MemberView)]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.Id)
                .ToListAsync();
            
            return View(users);
        }

        [AdminPermission(AdminPermissions.MemberView)]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [AdminPermission(AdminPermissions.MemberEdit)]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.MemberEdit)]
        public async Task<IActionResult> Edit(string id, IdentityUser user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "會員資料更新成功！";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "更新失敗：" + ex.Message);
                }
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.MemberDelete)]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "會員已成功刪除！";
                }
                else
                {
                    TempData["ErrorMessage"] = "刪除失敗！";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}