using BearToyWebsiteBack.Models;

namespace BearToyWebsiteBack.Services;

public interface IImageProcessingService
{
    Task<ProcessedImageResult> ProcessProductImageAsync(int productId, IFormFile imageFile, string imageCategory);
    Task<bool> DeleteProductImagesAsync(int productId, string imageCategory = "");
    Task<bool> DeleteImageFileAsync(string imagePath);
    string GetImageUrl(int productId, string imageCategory, string fileName);
    string GetImageBasePath();
}

public class ProcessedImageResult
{
    public string OriginalImagePath { get; set; } = string.Empty;
    public string ThumbnailImagePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string RelativeUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
}