using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class PostalCode
{
    public int Id { get; set; }

    public string PostalCode1 { get; set; } = null!;

    public string City { get; set; } = null!;

    public string District { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
