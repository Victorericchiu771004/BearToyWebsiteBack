using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class Promotion
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string PromotionType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal MinOrderAmount { get; set; }

    public decimal MaxDiscountAmount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public int UsageLimit { get; set; }

    public int UsedCount { get; set; }

    public string BadgeColor { get; set; } = null!;

    public string BadgeText { get; set; } = null!;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool CanStackWithOthers { get; set; }

    public string DiscountType { get; set; } = null!;

    public string ExclusionGroup { get; set; } = null!;

    public decimal MaxOrderDiscountAmount { get; set; }

    public int MinOrderQuantity { get; set; }

    public int Priority { get; set; }

    public string PromoCode { get; set; } = null!;

    public bool RequireCode { get; set; }

    public virtual ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();

    public virtual ICollection<PromotionExclusion> PromotionExclusions { get; set; } = new List<PromotionExclusion>();

    public virtual ICollection<PromotionGift> PromotionGifts { get; set; } = new List<PromotionGift>();

    public virtual ICollection<UserPromotionUsage> UserPromotionUsages { get; set; } = new List<UserPromotionUsage>();
}
