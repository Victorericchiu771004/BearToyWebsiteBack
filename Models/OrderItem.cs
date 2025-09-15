using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public int? ProductVariantId { get; set; }

    public string VariantDescription { get; set; } = null!;

    public bool IsGift { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ProductVariant? ProductVariant { get; set; }
}
