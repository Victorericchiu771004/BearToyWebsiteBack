using SkiaSharp;
using BearToyWebsiteBack.Models;

namespace BearToyWebsiteBack.Services;

public class ImageProcessingService : IImageProcessingService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImageProcessingService> _logger;
    private readonly string _imageBasePath;

    public ImageProcessingService(IConfiguration configuration, ILogger<ImageProcessingService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _imageBasePath = _configuration["ImageFolder"] ?? throw new InvalidOperationException("ImageFolder not configured");
    }

    public async Task<ProcessedImageResult> ProcessProductImageAsync(int productId, IFormFile imageFile, string imageCategory)
    {
        // 基本驗證
        if (imageFile == null || imageFile.Length == 0)
            throw new ArgumentException("圖片檔案無效");

        // 檔案大小驗證 (最大 10MB)
        if (imageFile.Length > 10 * 1024 * 1024)
            throw new ArgumentException("圖片檔案大小不可超過 10MB");

        // 檔案類型驗證
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var extension = Path.GetExtension(imageFile.FileName).ToLower();
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            throw new ArgumentException("不支援的圖片格式，請使用 JPG、PNG、GIF 或 WebP 格式");

        // MIME類型驗證
        var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (string.IsNullOrEmpty(imageFile.ContentType) || !allowedMimeTypes.Contains(imageFile.ContentType.ToLower()))
            throw new ArgumentException("不支援的圖片 MIME 類型");

        // 分類驗證
        var allowedCategories = new[] { "main", "gallery" };
        if (!allowedCategories.Contains(imageCategory.ToLower()))
            throw new ArgumentException($"不支援的圖片分類: {imageCategory}");

        try
        {
            var productFolderPath = Path.Combine(_imageBasePath, "products", $"product_{productId}");
            var categoryFolderPath = Path.Combine(productFolderPath, imageCategory);

            // 確保目錄存在
            Directory.CreateDirectory(categoryFolderPath);
            if (imageCategory == "gallery")
            {
                Directory.CreateDirectory(Path.Combine(productFolderPath, "thumbnails"));
            }

            // 產生唯一檔名，避免特殊字元
            var originalFileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
            originalFileName = SanitizeFileName(originalFileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{originalFileName}{extension}";

            var imagePath = Path.Combine(categoryFolderPath, uniqueFileName);
            ProcessedImageResult result = new();

            using (var inputStream = imageFile.OpenReadStream())
            using (var skBitmap = SKBitmap.Decode(inputStream))
            {
                if (skBitmap == null)
                    throw new ArgumentException("無法解析圖片檔案");

                result.Width = skBitmap.Width;
                result.Height = skBitmap.Height;
                result.FileName = uniqueFileName;

                // 根據圖片分類處理
                switch (imageCategory.ToLower())
                {
                    case "main":
                        // 主圖：保持原始尺寸，但限制檔案大小
                        await ProcessMainImageAsync(skBitmap, imagePath);
                        break;

                    case "gallery":
                        // 商品內容圖：調整為 500x500px
                        await ProcessGalleryImageAsync(skBitmap, imagePath);
                        // 同時產生縮圖 200x200px
                        var thumbnailPath = Path.Combine(productFolderPath, "thumbnails", uniqueFileName);
                        await ProcessThumbnailImageAsync(skBitmap, thumbnailPath);
                        result.ThumbnailImagePath = thumbnailPath;
                        result.ThumbnailUrl = GetImageUrl(productId, "thumbnails", uniqueFileName);
                        break;

                    default:
                        throw new ArgumentException($"不支援的圖片分類: {imageCategory}");
                }

                result.OriginalImagePath = imagePath;
                result.RelativeUrl = GetImageUrl(productId, imageCategory, uniqueFileName);

                // 取得檔案大小
                var fileInfo = new FileInfo(imagePath);
                result.FileSize = fileInfo.Length;

                _logger.LogInformation($"已處理圖片: {uniqueFileName}，分類: {imageCategory}，產品ID: {productId}");
                return result;
            }
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _logger.LogError(ex, $"處理圖片時發生錯誤，產品ID: {productId}，分類: {imageCategory}");
            throw new InvalidOperationException("圖片處理失敗，請稍後再試", ex);
        }
    }

    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "image";

        // 移除非法字元
        var invalidChars = Path.GetInvalidFileNameChars().Concat(new[] { ' ', '(', ')', '[', ']' }).ToArray();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // 限制長度
        if (sanitized.Length > 50)
            sanitized = sanitized.Substring(0, 50);

        return string.IsNullOrWhiteSpace(sanitized) ? "image" : sanitized;
    }

    private async Task ProcessMainImageAsync(SKBitmap bitmap, string outputPath)
    {
        // 主圖保持原始尺寸，但壓縮品質以控制檔案大小
        await Task.Run(() =>
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(GetSkiaFormat(outputPath), 85); // 85% 品質
            using var stream = File.OpenWrite(outputPath);
            data.SaveTo(stream);
        });
    }

    private async Task ProcessGalleryImageAsync(SKBitmap bitmap, string outputPath)
    {
        // 商品內容圖調整為 500x500px
        await Task.Run(() =>
        {
            var (newWidth, newHeight) = CalculateResizeKeepAspect(bitmap.Width, bitmap.Height, 500, 500);

            using var resizedBitmap = bitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
            using var image = SKImage.FromBitmap(resizedBitmap);
            using var data = image.Encode(GetSkiaFormat(outputPath), 80); // 80% 品質
            using var stream = File.OpenWrite(outputPath);
            data.SaveTo(stream);
        });
    }

    private async Task ProcessThumbnailImageAsync(SKBitmap bitmap, string outputPath)
    {
        // 縮圖調整為 200x200px，並確保檔案大小小於 100KB
        await Task.Run(() =>
        {
            var (newWidth, newHeight) = CalculateResizeKeepAspect(bitmap.Width, bitmap.Height, 200, 200);

            using var resizedBitmap = bitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
            using var image = SKImage.FromBitmap(resizedBitmap);

            // 為縮圖使用較低的品質以控制檔案大小
            using var data = image.Encode(GetSkiaFormat(outputPath), 70);
            using var stream = File.OpenWrite(outputPath);
            data.SaveTo(stream);
        });

        // 檢查檔案大小，如果超過 100KB 則進一步壓縮
        var fileInfo = new FileInfo(outputPath);
        if (fileInfo.Length > 102400) // 100KB
        {
            await Task.Run(() =>
            {
                using var compressedBitmap = SKBitmap.Decode(outputPath);
                using var image = SKImage.FromBitmap(compressedBitmap);
                using var data = image.Encode(GetSkiaFormat(outputPath), 60); // 降低品質到 60%
                using var stream = File.OpenWrite(outputPath);
                data.SaveTo(stream);
            });
        }
    }

    private SKEncodedImageFormat GetSkiaFormat(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        return extension switch
        {
            ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
            ".png" => SKEncodedImageFormat.Png,
            ".gif" => SKEncodedImageFormat.Gif,
            ".webp" => SKEncodedImageFormat.Webp,
            _ => SKEncodedImageFormat.Jpeg
        };
    }

    private (int width, int height) CalculateResizeKeepAspect(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
    {
        if (originalWidth <= maxWidth && originalHeight <= maxHeight)
            return (originalWidth, originalHeight);

        var ratioX = (double)maxWidth / originalWidth;
        var ratioY = (double)maxHeight / originalHeight;
        var ratio = Math.Min(ratioX, ratioY);

        return ((int)(originalWidth * ratio), (int)(originalHeight * ratio));
    }

    public string GetImageUrl(int productId, string imageCategory, string fileName)
    {
        return $"/images/products/product_{productId}/{imageCategory}/{fileName}";
    }

    public string GetImageBasePath()
    {
        return _imageBasePath;
    }

    public async Task<bool> DeleteProductImagesAsync(int productId, string imageCategory = "")
    {
        try
        {
            var productFolderPath = Path.Combine(_imageBasePath, "products", $"product_{productId}");

            if (string.IsNullOrEmpty(imageCategory))
            {
                // 刪除整個產品圖片資料夾
                if (Directory.Exists(productFolderPath))
                {
                    Directory.Delete(productFolderPath, true);
                }
            }
            else
            {
                // 刪除特定分類的圖片
                var categoryFolderPath = Path.Combine(productFolderPath, imageCategory);
                if (Directory.Exists(categoryFolderPath))
                {
                    Directory.Delete(categoryFolderPath, true);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"刪除產品圖片失敗，產品ID: {productId}，分類: {imageCategory}");
            return false;
        }
    }

    public async Task<bool> DeleteImageFileAsync(string imagePath)
    {
        try
        {
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);

                // 如果是 gallery 圖片，同時刪除對應的縮圖
                if (imagePath.Contains(Path.DirectorySeparatorChar + "gallery" + Path.DirectorySeparatorChar))
                {
                    var thumbnailPath = imagePath.Replace(
                        Path.DirectorySeparatorChar + "gallery" + Path.DirectorySeparatorChar,
                        Path.DirectorySeparatorChar + "thumbnails" + Path.DirectorySeparatorChar);
                    if (File.Exists(thumbnailPath))
                    {
                        File.Delete(thumbnailPath);
                    }
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"刪除圖片檔案失敗: {imagePath}");
            return false;
        }
    }
}