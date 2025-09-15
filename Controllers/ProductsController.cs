using BearToyWebsiteBack.Models;
using BearToyWebsiteBack.Attributes;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(BearToyDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> Create([Bind("Name,Description,Price,StockQuantity,IsFeatured,CategoryId,SubCategoryId,Sku")] Product product, List<IFormFile> productImages)
        {
            if (ModelState.IsValid)
            {
                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;
                
                _context.Add(product);
                await _context.SaveChangesAsync();

                // 處理圖片上傳
                if (productImages != null && productImages.Count > 0)
                {
                    await ProcessProductImages(product.Id, productImages);
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
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
                
            if (product == null)
            {
                return NotFound();
            }
            
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategories, "Id", "Name", product.SubCategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.ProductEdit)]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,StockQuantity,IsFeatured,CategoryId,SubCategoryId,Sku,CreatedAt")] Product product, List<IFormFile> productImages)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    product.UpdatedAt = DateTime.Now;
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    // 處理新上傳的圖片
                    if (productImages != null && productImages.Count > 0)
                    {
                        await ProcessProductImages(product.Id, productImages);
                    }
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
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewData["SubCategoryId"] = new SelectList(_context.SubCategories, "Id", "Name", product.SubCategoryId);
            return View(product);
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
                // 刪除相關圖片檔案
                foreach (var image in product.ProductImages)
                {
                    DeleteImageFile(image.ImageUrl);
                }
                
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
                DeleteImageFile(productImage.ImageUrl);
                _context.ProductImages.Remove(productImage);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        private async Task ProcessProductImages(int productId, List<IFormFile> images)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            int sortOrder = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .MaxAsync(pi => (int?)pi.SortOrder) ?? 0;

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    var productImage = new ProductImage
                    {
                        ProductId = productId,
                        ImageUrl = "/uploads/products/" + uniqueFileName,
                        AltText = Path.GetFileNameWithoutExtension(image.FileName),
                        IsMainImage = sortOrder == 0, // 第一張設為主圖
                        SortOrder = ++sortOrder,
                        ImageType = "產品圖片",
                        CreatedAt = DateTime.Now
                    };

                    _context.ProductImages.Add(productImage);
                }
            }

            await _context.SaveChangesAsync();
        }

        private void DeleteImageFile(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
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