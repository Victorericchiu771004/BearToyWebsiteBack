using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int CategoryId { get; set; }

    public int? SubCategoryId { get; set; }

    public string Sku { get; set; } = null!;

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category Category { get; set; } = null!;

    public virtual DeliveryInfo? DeliveryInfo { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductPromotion> ProductPromotions { get; set; } = new List<ProductPromotion>();

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public virtual ICollection<PromotionExclusion> PromotionExclusions { get; set; } = new List<PromotionExclusion>();

    public virtual ICollection<PromotionGift> PromotionGifts { get; set; } = new List<PromotionGift>();

    public virtual SubCategory? SubCategory { get; set; }
}
