using Microsoft.AspNetCore.Mvc;
using BearToyWebsiteBack.Services;
using BearToyWebsiteBack.Models.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace BearToyWebsiteBack.Controllers
{
    public class AdminAuthController : Controller
    {
        private readonly IAdminAuthService _adminAuthService;
        private readonly ILogger<AdminAuthController> _logger;

        public AdminAuthController(IAdminAuthService adminAuthService, ILogger<AdminAuthController> logger)
        {
            _adminAuthService = adminAuthService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // 如果已經登入，重定向到儀表板
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new AdminLoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AdminLoginViewModel model)
        {
            try
            {
                ViewData["ReturnUrl"] = model.ReturnUrl;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "未知";
                var loginResult = await _adminAuthService.LoginAsync(model.Username, model.Password, clientIp);

                if (loginResult.Success && loginResult.Admin != null)
                {
                    // 建立身份聲明
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, loginResult.Admin.AdminId.ToString()),
                        new Claim(ClaimTypes.Name, loginResult.Admin.Username),
                        new Claim("FullName", loginResult.Admin.FullName),
                        new Claim("AdminType", "BackendAdmin")
                    };

                    // 加入權限聲明
                    foreach (var permission in loginResult.Permissions)
                    {
                        claims.Add(new Claim("Permission", permission));
                    }

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    _logger.LogInformation("管理員 {Username} 成功登入系統", model.Username);

                    // 重定向到返回URL或預設頁面
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, loginResult.ErrorMessage ?? "登入失敗");
                    _logger.LogWarning("管理員 {Username} 登入失敗：{Error}", model.Username, loginResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "管理員登入過程發生錯誤：{Username}", model.Username);
                ModelState.AddModelError(string.Empty, "系統發生錯誤，請稍後再試");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var username = User.FindFirstValue(ClaimTypes.Name);

                if (int.TryParse(adminId, out int id))
                {
                    await _adminAuthService.LogoutAsync(id);
                    _logger.LogInformation("管理員 {Username} 成功登出系統", username);
                }

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction("Login", "AdminAuth");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "管理員登出過程發生錯誤");
                return RedirectToAction("Login", "AdminAuth");
            }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}