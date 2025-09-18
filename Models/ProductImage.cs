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

    public string ImageCategory { get; set; } = null!; // main, gallery, thumbnails

    public string FileName { get; set; } = null!;

    public long FileSize { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public string? ThumbnailUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
