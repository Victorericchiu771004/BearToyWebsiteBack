using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class Order
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public DateTime OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public string ShippingAddress { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string ShippingMethod { get; set; } = null!;

    public string ShippingStatus { get; set; } = null!;

    public string? TrackingNumber { get; set; }

    public string? StorePickupCode { get; set; }

    public string? StoreId { get; set; }

    public string? StoreName { get; set; }

    public DateTime? PaymentConfirmationDate { get; set; }

    public DateTime? ShippingDate { get; set; }

    public DateTime? CompletionDate { get; set; }

    public string? Notes { get; set; }

    public string? RecipientName { get; set; }

    public string? ContactPhone { get; set; }

    public string? Email { get; set; }

    public string? PostalCode { get; set; }

    public string? City { get; set; }

    public string? District { get; set; }

    public string? Address { get; set; }

    public string? StoreAddress { get; set; }

    public string? OrderNumber { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<PaymentNotification> PaymentNotifications { get; set; } = new List<PaymentNotification>();

    public virtual ICollection<UserPromotionUsage> UserPromotionUsages { get; set; } = new List<UserPromotionUsage>();
}
