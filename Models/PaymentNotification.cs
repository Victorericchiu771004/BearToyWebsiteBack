using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class PaymentNotification
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string UserId { get; set; } = null!;

    public string BankName { get; set; } = null!;

    public string AccountName { get; set; } = null!;

    public decimal TransferAmount { get; set; }

    public DateTime TransferDate { get; set; }

    public string? Last5DigitsOfAccount { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = null!;

    public string? AdminRemarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
