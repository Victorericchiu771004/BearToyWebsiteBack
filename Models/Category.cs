using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<PromotionExclusion> PromotionExclusions { get; set; } = new List<PromotionExclusion>();

    public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
}
