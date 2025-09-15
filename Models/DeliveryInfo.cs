using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class DeliveryInfo
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int StockStatus { get; set; }

    public string EstimatedDeliveryTime { get; set; } = null!;

    public string DeliveryDescription { get; set; } = null!;

    public string PreOrderDescription { get; set; } = null!;

    public bool Support24HourDelivery { get; set; }

    public int SuggestedQuantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
