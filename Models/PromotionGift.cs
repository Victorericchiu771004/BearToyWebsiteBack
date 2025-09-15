using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class PromotionGift
{
    public int Id { get; set; }

    public int PromotionId { get; set; }

    public int GiftProductId { get; set; }

    public int GiftQuantity { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product GiftProduct { get; set; } = null!;

    public virtual Promotion Promotion { get; set; } = null!;
}
