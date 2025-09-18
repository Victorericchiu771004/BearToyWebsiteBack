namespace BearToyWebsiteBack.Services;

public interface IImageSyncService
{
    /// <summary>
    /// 同步檔案系統中的圖片到資料庫
    /// </summary>
    /// <param name="productId">商品 ID，如果為 null 則同步所有商品</param>
    /// <returns>同步結果</returns>
    Task<ImageSyncResult> SyncImagesAsync(int? productId = null);

    /// <summary>
    /// 檢查特定商品的圖片同步狀態
    /// </summary>
    /// <param name="productId">商品 ID</param>
    /// <returns>同步狀態資訊</returns>
    Task<ImageSyncStatus> CheckSyncStatusAsync(int productId);

    /// <summary>
    /// 清理資料庫中不存在檔案系統的圖片記錄
    /// </summary>
    /// <returns>清理結果</returns>
    Task<CleanupResult> CleanupOrphanedRecordsAsync();
}

public class ImageSyncResult
{
    public int AddedRecords { get; set; }
    public int UpdatedRecords { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Messages { get; set; } = new();
}

public class ImageSyncStatus
{
    public int ProductId { get; set; }
    public int DatabaseRecords { get; set; }
    public int FileSystemFiles { get; set; }
    public List<string> MissingInDatabase { get; set; } = new();
    public List<string> MissingInFileSystem { get; set; } = new();
}

public class CleanupResult
{
    public int RemovedRecords { get; set; }
    public List<string> Messages { get; set; } = new();
}