using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class PromotionExclusion
{
    public int Id { get; set; }

    public int PromotionId { get; set; }

    public int? ProductId { get; set; }

    public int? CategoryId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual Product? Product { get; set; }

    public virtual Promotion Promotion { get; set; } = null!;
}
