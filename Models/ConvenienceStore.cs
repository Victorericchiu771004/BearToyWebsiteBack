using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class ConvenienceStore
{
    public int Id { get; set; }

    public string StoreId { get; set; } = null!;

    public string StoreName { get; set; } = null!;

    public string StoreType { get; set; } = null!;

    public string City { get; set; } = null!;

    public string District { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
