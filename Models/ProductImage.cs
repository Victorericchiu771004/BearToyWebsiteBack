using System;
using System.Collections.Generic;

namespace BearToyWebsiteBack.Models;

public partial class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string AltText { get; set; } = null!;

    public bool IsMainImage { get; set; }

    public int SortOrder { get; set; }

    public string ImageType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
