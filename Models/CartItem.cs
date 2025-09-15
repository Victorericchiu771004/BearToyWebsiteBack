using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class CartItem
{
    public int Id { get; set; }

    public string CartId { get; set; } = null!;

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public DateTime DateCreated { get; set; }

    public int? VariantId { get; set; }

    public string? VariantDescription { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductVariant? Variant { get; set; }
}
