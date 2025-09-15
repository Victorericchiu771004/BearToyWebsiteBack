using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace BearToyWebsiteBack.Attributes
{
    public class AdminPermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _permissionCode;

        public AdminPermissionAttribute(string permissionCode)
        {
            _permissionCode = permissionCode;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 檢查是否已認證
            if (!context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                context.Result = new RedirectToActionResult("Login", "AdminAuth", null);
                return;
            }

            // 檢查是否為後台管理員
            var adminType = context.HttpContext.User.FindFirst("AdminType")?.Value;
            if (adminType != "BackendAdmin")
            {
                context.Result = new RedirectToActionResult("AccessDenied", "AdminAuth", null);
                return;
            }

            // 檢查權限
            var userPermissions = context.HttpContext.User.FindAll("Permission").Select(c => c.Value);
            if (!userPermissions.Contains(_permissionCode))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "AdminAuth", null);
                return;
            }
        }
    }

    // 權限常數定義
    public static class AdminPermissions
    {
        // 商品管理
        public const string ProductView = "product.view";
        public const string ProductCreate = "product.create";
        public const string ProductEdit = "product.edit";
        public const string ProductDelete = "product.delete";

        // 訂單管理
        public const string OrderView = "order.view";
        public const string OrderCreate = "order.create";
        public const string OrderEdit = "order.edit";
        public const string OrderDelete = "order.delete";

        // 會員管理
        public const string MemberView = "member.view";
        public const string MemberEdit = "member.edit";
        public const string MemberDelete = "member.delete";

        // 行銷活動管理
        public const string PromotionView = "promotion.view";
        public const string PromotionCreate = "promotion.create";
        public const string PromotionEdit = "promotion.edit";
        public const string PromotionDelete = "promotion.delete";
        public const string PromotionAnalytics = "promotion.analytics";

        // 系統管理
        public const string AdminManage = "admin.manage";
        public const string SystemConfig = "system.config";
        public const string ReportView = "report.view";
    }
}