using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class UserPromotionUsage
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public int PromotionId { get; set; }

    public int OrderId { get; set; }

    public DateTime UsedAt { get; set; }

    public decimal DiscountAmount { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Promotion Promotion { get; set; } = null!;
}
