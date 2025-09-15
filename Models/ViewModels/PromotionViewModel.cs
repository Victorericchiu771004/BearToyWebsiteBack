using System.ComponentModel.DataAnnotations;

namespace BearToyWebsiteBack.Models.ViewModels
{
    public class PromotionViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "活動名稱為必填")]
        [StringLength(100, ErrorMessage = "活動名稱不可超過100字")]
        [Display(Name = "活動名稱")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "活動描述為必填")]
        [StringLength(500, ErrorMessage = "活動描述不可超過500字")]
        [Display(Name = "活動描述")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "促銷類型為必填")]
        [Display(Name = "促銷類型")]
        public string PromotionType { get; set; } = "";

        [Required(ErrorMessage = "折扣類型為必填")]
        [Display(Name = "折扣類型")]
        public string DiscountType { get; set; } = "";

        [Required(ErrorMessage = "折扣值為必填")]
        [Range(0, 999999, ErrorMessage = "折扣值必須大於0")]
        [Display(Name = "折扣值")]
        public decimal DiscountValue { get; set; }

        [Range(0, 999999, ErrorMessage = "最低訂單金額不可為負數")]
        [Display(Name = "最低訂單金額")]
        public decimal MinOrderAmount { get; set; }

        [Range(0, 999999, ErrorMessage = "最高折扣金額不可為負數")]
        [Display(Name = "最高折扣金額")]
        public decimal MaxDiscountAmount { get; set; }

        [Range(0, 999999, ErrorMessage = "最高訂單折扣金額不可為負數")]
        [Display(Name = "最高訂單折扣金額")]
        public decimal MaxOrderDiscountAmount { get; set; }

        [Range(0, 999, ErrorMessage = "最低訂單數量不可為負數")]
        [Display(Name = "最低訂單數量")]
        public int MinOrderQuantity { get; set; }

        [Required(ErrorMessage = "開始時間為必填")]
        [Display(Name = "開始時間")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "結束時間為必填")]
        [Display(Name = "結束時間")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(30);

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; } = true;

        [Range(0, 999999, ErrorMessage = "使用限制不可為負數")]
        [Display(Name = "使用限制")]
        public int UsageLimit { get; set; }

        [Display(Name = "已使用次數")]
        public int UsedCount { get; set; }

        [StringLength(10, ErrorMessage = "標籤顏色不可超過10字")]
        [Display(Name = "標籤顏色")]
        public string BadgeColor { get; set; } = "#FF6B6B";

        [Required(ErrorMessage = "標籤文字為必填")]
        [StringLength(50, ErrorMessage = "標籤文字不可超過50字")]
        [Display(Name = "標籤文字")]
        public string BadgeText { get; set; } = "";

        [Display(Name = "排序順序")]
        public int SortOrder { get; set; }

        [Display(Name = "可與其他促銷疊加")]
        public bool CanStackWithOthers { get; set; }

        [StringLength(50, ErrorMessage = "排除群組不可超過50字")]
        [Display(Name = "排除群組")]
        public string ExclusionGroup { get; set; } = "";

        [Display(Name = "優先順序")]
        public int Priority { get; set; }

        [StringLength(50, ErrorMessage = "促銷代碼不可超過50字")]
        [Display(Name = "促銷代碼")]
        public string PromoCode { get; set; } = "";

        [Display(Name = "需要促銷代碼")]
        public bool RequireCode { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        // 關聯資料
        [Display(Name = "適用產品")]
        public List<int> SelectedProductIds { get; set; } = new List<int>();

        [Display(Name = "排除產品")]
        public List<int> ExcludedProductIds { get; set; } = new List<int>();

        [Display(Name = "排除類別")]
        public List<int> ExcludedCategoryIds { get; set; } = new List<int>();

        // 顯示用資料
        public string StatusDisplayName => IsActive ? "啟用" : "停用";
        public string DateRangeDisplay => $"{StartDate:yyyy/MM/dd} - {EndDate:yyyy/MM/dd}";
        public bool IsExpired => EndDate < DateTime.Now;
        public bool IsNotStarted => StartDate > DateTime.Now;
        public string CurrentStatus
        {
            get
            {
                if (!IsActive) return "已停用";
                if (IsNotStarted) return "未開始";
                if (IsExpired) return "已過期";
                return "進行中";
            }
        }
        
        // 使用率計算
        public double UsagePercentage => UsageLimit > 0 ? (double)UsedCount / UsageLimit * 100 : 0;
    }

    public class PromotionListViewModel
    {
        public List<PromotionViewModel> Promotions { get; set; } = new List<PromotionViewModel>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public string? CurrentFilter { get; set; }
        public string? CurrentStatus { get; set; }
        public string? CurrentType { get; set; }
    }

    public class PromotionAnalyticsViewModel
    {
        public int PromotionId { get; set; }
        public string PromotionName { get; set; } = "";
        public int TotalUsage { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal TotalOrderAmount { get; set; }
        public int UniqueUsers { get; set; }
        public double ConversionRate { get; set; }
        public double ROI { get; set; }
        
        // 圖表資料
        public List<DailyUsageData> DailyUsageData { get; set; } = new List<DailyUsageData>();
        public List<ProductPerformanceData> TopProducts { get; set; } = new List<ProductPerformanceData>();
    }

    public class DailyUsageData
    {
        public DateTime Date { get; set; }
        public int Usage { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ProductPerformanceData
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }
}