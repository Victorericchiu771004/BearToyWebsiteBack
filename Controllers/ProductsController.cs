using BearToyWebsiteBack.Models;
using BearToyWebsiteBack.Attributes;
using BearToyWebsiteBack.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BearToyWebsiteBack.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly BearToyDbContext _context;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(BearToyDbContext context, IImageProcessingService imageProcessingService,
                                IConfiguration configuration, ILogger<ProductsController> logger)
        {
            _context = context;
            _imageProcessingService = imageProcessingService;
            _configuration = configuration;
            _logger = logger;
        }

        // GET: Products
        [AdminPermission(AdminPermissions.ProductView)]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(products);
        }

        // GET: Products/Details/5
        [AdminPermission(AdminPermissions.ProductView)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        [AdminPermission(AdminPermissions.ProductCreate)]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.ProductCreate)]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,StockQuantity,IsFeatured,CategoryId,SubCategoryId,Sku")] Product product,
            IFormFile? mainImage, List<IFormFile> galleryImages)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;

                _context.Add(product);
                await _context.SaveChangesAsync();

                // 處理主圖上傳
                if (mainImage != null && mainImage.Length > 0)
                {
                    await ProcessSingleProductImage(product.Id, mainImage, "main", true);
                }

                // 處理商品內容圖上傳
                if (galleryImages != null && galleryImages.Count > 0)
                {
                    await ProcessMultipleProductImages(product.Id, galleryImages, "gallery");
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategories, "Id", "Name", product.SubCategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        [AdminPermission(AdminPermissions.ProductEdit)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductImages.OrderBy(pi => pi.SortOrder))
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // 記錄載入的圖片數量以供除錯
            _logger.LogInformation($"Edit GET - ProductId: {id}, ProductImages Count: {product.ProductImages.Count}");


            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategories, "Id", "Name", product.SubCategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.ProductEdit)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,StockQuantity,IsFeatured,CategoryId,SubCategoryId,Sku,CreatedAt")] Product product,
            IFormFile? mainImage, List<IFormFile> galleryImages)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            var hasImageChanges = false;
            var errorMessages = new List<string>();

            // 處理主圖上傳
            if (mainImage != null && mainImage.Length > 0)
            {
                try
                {
                    _logger.LogInformation($"處理主圖上傳 - 檔名: {mainImage.FileName}, 大小: {mainImage.Length} bytes");

                    // 先刪除舊的主圖
                    await DeleteImagesByCategory(product.Id, "main");
                    await ProcessSingleProductImage(product.Id, mainImage, "main", true);

                    hasImageChanges = true;
                    _logger.LogInformation($"主圖上傳成功");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"主圖上傳失敗 - 檔名: {mainImage.FileName}");
                    errorMessages.Add($"主圖上傳失敗: {ex.Message}");
                }
            }

            // 處理商品內容圖上傳
            if (galleryImages != null && galleryImages.Count > 0)
            {
                try
                {
                    _logger.LogInformation($"處理內容圖上傳 - 檔案數量: {galleryImages.Count}");
                    await ProcessMultipleProductImages(product.Id, galleryImages, "gallery");

                    hasImageChanges = true;
                    _logger.LogInformation($"內容圖上傳成功 - 數量: {galleryImages.Count}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"內容圖上傳失敗");
                    errorMessages.Add($"部分或全部內容圖上傳失敗: {ex.Message}");
                }
            }

            // 處理基本資料更新
            if (ModelState.IsValid)
            {
                try
                {
                    product.UpdatedAt = DateTime.Now;
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    if (errorMessages.Any())
                    {
                        // 有圖片錯誤但基本資料更新成功
                        TempData["WarningMessage"] = $"商品資料已更新，但圖片處理有問題: {string.Join(", ", errorMessages)}";
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"更新商品資料失敗 - ProductId: {product.Id}");
                    errorMessages.Add($"商品資料更新失敗: {ex.Message}");
                }
            }

            // 如果有任何錯誤，顯示給使用者
            if (errorMessages.Any())
            {
                foreach (var error in errorMessages)
                {
                    ModelState.AddModelError("", error);
                }
            }

            // 重新載入產品資料包含圖片
            var productWithImages = await _context.Products
                .Include(p => p.ProductImages.OrderBy(pi => pi.SortOrder))
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if (productWithImages == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", productWithImages.CategoryId);
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategories, "Id", "Name", productWithImages.SubCategoryId);
            return View(productWithImages);
        }

        // GET: Products/Delete/5
        [AdminPermission(AdminPermissions.ProductDelete)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.SubCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.ProductDelete)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                // 刪除所有產品圖片檔案
                await _imageProcessingService.DeleteProductImagesAsync(id);

                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Products/DeleteImage
        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var productImage = await _context.ProductImages.FindAsync(imageId);
            if (productImage != null)
            {
                // 構建實際檔案路徑
                var imageBasePath = _configuration["ImageFolder"];
                var relativePath = productImage.ImageUrl.Replace("/images/", "");
                var fullPath = Path.Combine(imageBasePath, relativePath);

                await _imageProcessingService.DeleteImageFileAsync(fullPath);
                _context.ProductImages.Remove(productImage);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        private async Task ProcessSingleProductImage(int productId, IFormFile imageFile, string category, bool isMainImage)
        {
            try
            {
                var result = await _imageProcessingService.ProcessProductImageAsync(productId, imageFile, category);

                var productImage = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = result.RelativeUrl,
                    AltText = Path.GetFileNameWithoutExtension(imageFile.FileName),
                    IsMainImage = isMainImage,
                    SortOrder = category == "main" ? 0 : await GetNextSortOrder(productId, category),
                    ImageType = "產品圖片",
                    ImageCategory = category,
                    FileName = result.FileName,
                    FileSize = result.FileSize,
                    Width = result.Width,
                    Height = result.Height,
                    ThumbnailUrl = result.ThumbnailUrl,
                    CreatedAt = DateTime.Now
                };

                _context.ProductImages.Add(productImage);
                await _context.SaveChangesAsync();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", "圖片處理失敗，請稍後再試");
                throw;
            }
        }

        private async Task ProcessMultipleProductImages(int productId, List<IFormFile> images, string category)
        {
            // 檢查 gallery 圖片數量限制
            if (category == "gallery")
            {
                var currentCount = await _context.ProductImages
                    .CountAsync(pi => pi.ProductId == productId && pi.ImageCategory == "gallery");

                var remainingSlots = 9 - currentCount;
                if (remainingSlots <= 0)
                {
                    ModelState.AddModelError("", "商品內容圖已達上限 (9張)");
                    return;
                }

                // 只處理剩餘可用的數量
                if (images.Count > remainingSlots)
                {
                    ModelState.AddModelError("", $"只能再新增 {remainingSlots} 張圖片，已自動選取前 {remainingSlots} 張");
                    images = images.Take(remainingSlots).ToList();
                }
            }

            int sortOrder = await GetNextSortOrder(productId, category);
            var failedImages = new List<string>();

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    try
                    {
                        var result = await _imageProcessingService.ProcessProductImageAsync(productId, image, category);

                        var productImage = new ProductImage
                        {
                            ProductId = productId,
                            ImageUrl = result.RelativeUrl,
                            AltText = Path.GetFileNameWithoutExtension(image.FileName),
                            IsMainImage = false,
                            SortOrder = ++sortOrder,
                            ImageType = "產品圖片",
                            ImageCategory = category,
                            FileName = result.FileName,
                            FileSize = result.FileSize,
                            Width = result.Width,
                            Height = result.Height,
                            ThumbnailUrl = result.ThumbnailUrl,
                            CreatedAt = DateTime.Now
                        };

                        _context.ProductImages.Add(productImage);
                    }
                    catch (ArgumentException ex)
                    {
                        failedImages.Add($"{image.FileName}: {ex.Message}");
                    }
                    catch (InvalidOperationException)
                    {
                        failedImages.Add($"{image.FileName}: 圖片處理失敗");
                    }
                }
            }

            if (failedImages.Any())
            {
                ModelState.AddModelError("", $"部分圖片處理失敗: {string.Join(", ", failedImages)}");
            }

            await _context.SaveChangesAsync();
        }

        private async Task<int> GetNextSortOrder(int productId, string category)
        {
            return await _context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.ImageCategory == category)
                .MaxAsync(pi => (int?)pi.SortOrder) ?? 0;
        }

        private async Task DeleteImagesByCategory(int productId, string category)
        {
            var images = await _context.ProductImages
                .Where(pi => pi.ProductId == productId && pi.ImageCategory == category)
                .ToListAsync();

            var imageBasePath = _configuration["ImageFolder"];

            foreach (var image in images)
            {
                var relativePath = image.ImageUrl.Replace("/images/", "");
                var fullPath = Path.Combine(imageBasePath, relativePath);
                await _imageProcessingService.DeleteImageFileAsync(fullPath);
            }

            _context.ProductImages.RemoveRange(images);
            await _context.SaveChangesAsync();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // POST: Products/UpdateImageOrder
        [HttpPost]
        public async Task<IActionResult> UpdateImageOrder([FromBody] List<int> imageIds)
        {
            try
            {
                for (int i = 0; i < imageIds.Count; i++)
                {
                    var image = await _context.ProductImages.FindAsync(imageIds[i]);
                    if (image != null)
                    {
                        image.SortOrder = i + 1;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新圖片排序失敗");
                return Json(new { success = false, message = "更新排序失敗" });
            }
        }

        // Ajax 方法：取得子分類
        [HttpGet]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            var subCategories = await _context.SubCategories
                .Where(s => s.CategoryId == categoryId)
                .Select(s => new { value = s.Id, text = s.Name })
                .ToListAsync();

            return Json(subCategories);
        }
    }
}