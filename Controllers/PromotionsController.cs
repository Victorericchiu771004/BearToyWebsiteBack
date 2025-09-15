using BearToyWebsiteBack.Models;
using BearToyWebsiteBack.Models.ViewModels;
using BearToyWebsiteBack.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BearToyWebsiteBack.Controllers
{
    [Authorize]
    public class PromotionsController : Controller
    {
        private readonly BearToyDbContext _context;

        public PromotionsController(BearToyDbContext context)
        {
            _context = context;
        }

        // GET: Promotions
        [AdminPermission(AdminPermissions.PromotionView)]
        public async Task<IActionResult> Index(string? filter = null, string? status = null, string? type = null, int page = 1)
        {
            var pageSize = 20;
            
            var promotionsQuery = _context.Promotions.AsQueryable();

            // 篩選條件
            if (!string.IsNullOrEmpty(filter))
            {
                promotionsQuery = promotionsQuery.Where(p => 
                    p.Name.Contains(filter) ||
                    p.Description.Contains(filter) ||
                    p.PromoCode.Contains(filter));
            }

            if (!string.IsNullOrEmpty(status))
            {
                switch (status)
                {
                    case "active":
                        promotionsQuery = promotionsQuery.Where(p => p.IsActive);
                        break;
                    case "inactive":
                        promotionsQuery = promotionsQuery.Where(p => !p.IsActive);
                        break;
                    case "expired":
                        promotionsQuery = promotionsQuery.Where(p => p.EndDate < DateTime.Now);
                        break;
                    case "upcoming":
                        promotionsQuery = promotionsQuery.Where(p => p.StartDate > DateTime.Now);
                        break;
                    case "ongoing":
                        promotionsQuery = promotionsQuery.Where(p => 
                            p.IsActive && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(type))
            {
                promotionsQuery = promotionsQuery.Where(p => p.PromotionType == type);
            }

            var totalCount = await promotionsQuery.CountAsync();
            var promotions = await promotionsQuery
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var promotionViewModels = promotions.Select(p => new PromotionViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                PromotionType = p.PromotionType,
                DiscountType = p.DiscountType,
                DiscountValue = p.DiscountValue,
                MinOrderAmount = p.MinOrderAmount,
                MaxDiscountAmount = p.MaxDiscountAmount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                IsActive = p.IsActive,
                UsageLimit = p.UsageLimit,
                UsedCount = p.UsedCount,
                BadgeColor = p.BadgeColor,
                BadgeText = p.BadgeText,
                PromoCode = p.PromoCode,
                RequireCode = p.RequireCode,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            var viewModel = new PromotionListViewModel
            {
                Promotions = promotionViewModels,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                TotalCount = totalCount,
                CurrentFilter = filter,
                CurrentStatus = status,
                CurrentType = type
            };

            return View(viewModel);
        }

        // GET: Promotions/Details/5
        [AdminPermission(AdminPermissions.PromotionView)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _context.Promotions
                .Include(p => p.ProductPromotions)
                .ThenInclude(pp => pp.Product)
                .Include(p => p.PromotionExclusions)
                .ThenInclude(pe => pe.Product)
                .Include(p => p.PromotionExclusions)
                .ThenInclude(pe => pe.Category)
                .Include(p => p.PromotionGifts)
                .ThenInclude(pg => pg.GiftProduct)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (promotion == null)
            {
                return NotFound();
            }

            var viewModel = new PromotionViewModel
            {
                Id = promotion.Id,
                Name = promotion.Name,
                Description = promotion.Description,
                PromotionType = promotion.PromotionType,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                MinOrderAmount = promotion.MinOrderAmount,
                MaxDiscountAmount = promotion.MaxDiscountAmount,
                MaxOrderDiscountAmount = promotion.MaxOrderDiscountAmount,
                MinOrderQuantity = promotion.MinOrderQuantity,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                IsActive = promotion.IsActive,
                UsageLimit = promotion.UsageLimit,
                UsedCount = promotion.UsedCount,
                BadgeColor = promotion.BadgeColor,
                BadgeText = promotion.BadgeText,
                SortOrder = promotion.SortOrder,
                CanStackWithOthers = promotion.CanStackWithOthers,
                ExclusionGroup = promotion.ExclusionGroup,
                Priority = promotion.Priority,
                PromoCode = promotion.PromoCode,
                RequireCode = promotion.RequireCode,
                CreatedAt = promotion.CreatedAt,
                UpdatedAt = promotion.UpdatedAt,
                SelectedProductIds = promotion.ProductPromotions.Select(pp => pp.ProductId).ToList(),
                ExcludedProductIds = promotion.PromotionExclusions.Where(pe => pe.ProductId.HasValue).Select(pe => pe.ProductId.Value).ToList(),
                ExcludedCategoryIds = promotion.PromotionExclusions.Where(pe => pe.CategoryId.HasValue).Select(pe => pe.CategoryId.Value).ToList()
            };

            return View(viewModel);
        }

        // GET: Promotions/Create
        [AdminPermission(AdminPermissions.PromotionCreate)]
        public IActionResult Create()
        {
            var viewModel = new PromotionViewModel
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                IsActive = true,
                BadgeColor = "#FF6B6B"
            };

            ViewBag.Products = _context.Products.Select(p => new { p.Id, p.Name }).ToList();
            ViewBag.Categories = _context.Categories.Select(c => new { c.Id, c.Name }).ToList();

            return View(viewModel);
        }

        // POST: Promotions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.PromotionCreate)]
        public async Task<IActionResult> Create(PromotionViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var promotion = new Promotion
                {
                    Name = viewModel.Name,
                    Description = viewModel.Description,
                    PromotionType = viewModel.PromotionType,
                    DiscountType = viewModel.DiscountType,
                    DiscountValue = viewModel.DiscountValue,
                    MinOrderAmount = viewModel.MinOrderAmount,
                    MaxDiscountAmount = viewModel.MaxDiscountAmount,
                    MaxOrderDiscountAmount = viewModel.MaxOrderDiscountAmount,
                    MinOrderQuantity = viewModel.MinOrderQuantity,
                    StartDate = viewModel.StartDate,
                    EndDate = viewModel.EndDate,
                    IsActive = viewModel.IsActive,
                    UsageLimit = viewModel.UsageLimit,
                    UsedCount = 0,
                    BadgeColor = viewModel.BadgeColor,
                    BadgeText = viewModel.BadgeText,
                    SortOrder = viewModel.SortOrder,
                    CanStackWithOthers = viewModel.CanStackWithOthers,
                    ExclusionGroup = viewModel.ExclusionGroup,
                    Priority = viewModel.Priority,
                    PromoCode = viewModel.PromoCode,
                    RequireCode = viewModel.RequireCode,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Add(promotion);
                await _context.SaveChangesAsync();

                // 新增產品關聯
                foreach (var productId in viewModel.SelectedProductIds)
                {
                    _context.ProductPromotions.Add(new ProductPromotion
                    {
                        PromotionId = promotion.Id,
                        ProductId = productId,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });
                }

                // 新增排除產品
                foreach (var productId in viewModel.ExcludedProductIds)
                {
                    _context.PromotionExclusions.Add(new PromotionExclusion
                    {
                        PromotionId = promotion.Id,
                        ProductId = productId,
                        CreatedAt = DateTime.Now
                    });
                }

                // 新增排除類別
                foreach (var categoryId in viewModel.ExcludedCategoryIds)
                {
                    _context.PromotionExclusions.Add(new PromotionExclusion
                    {
                        PromotionId = promotion.Id,
                        CategoryId = categoryId,
                        CreatedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "行銷活動建立成功！";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Products = _context.Products.Select(p => new { p.Id, p.Name }).ToList();
            ViewBag.Categories = _context.Categories.Select(c => new { c.Id, c.Name }).ToList();
            return View(viewModel);
        }

        // GET: Promotions/Edit/5
        [AdminPermission(AdminPermissions.PromotionEdit)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _context.Promotions
                .Include(p => p.ProductPromotions)
                .Include(p => p.PromotionExclusions)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (promotion == null)
            {
                return NotFound();
            }

            var viewModel = new PromotionViewModel
            {
                Id = promotion.Id,
                Name = promotion.Name,
                Description = promotion.Description,
                PromotionType = promotion.PromotionType,
                DiscountType = promotion.DiscountType,
                DiscountValue = promotion.DiscountValue,
                MinOrderAmount = promotion.MinOrderAmount,
                MaxDiscountAmount = promotion.MaxDiscountAmount,
                MaxOrderDiscountAmount = promotion.MaxOrderDiscountAmount,
                MinOrderQuantity = promotion.MinOrderQuantity,
                StartDate = promotion.StartDate,
                EndDate = promotion.EndDate,
                IsActive = promotion.IsActive,
                UsageLimit = promotion.UsageLimit,
                UsedCount = promotion.UsedCount,
                BadgeColor = promotion.BadgeColor,
                BadgeText = promotion.BadgeText,
                SortOrder = promotion.SortOrder,
                CanStackWithOthers = promotion.CanStackWithOthers,
                ExclusionGroup = promotion.ExclusionGroup,
                Priority = promotion.Priority,
                PromoCode = promotion.PromoCode,
                RequireCode = promotion.RequireCode,
                SelectedProductIds = promotion.ProductPromotions.Select(pp => pp.ProductId).ToList(),
                ExcludedProductIds = promotion.PromotionExclusions.Where(pe => pe.ProductId.HasValue).Select(pe => pe.ProductId.Value).ToList(),
                ExcludedCategoryIds = promotion.PromotionExclusions.Where(pe => pe.CategoryId.HasValue).Select(pe => pe.CategoryId.Value).ToList()
            };

            ViewBag.Products = _context.Products.Select(p => new { p.Id, p.Name }).ToList();
            ViewBag.Categories = _context.Categories.Select(c => new { c.Id, c.Name }).ToList();

            return View(viewModel);
        }

        // POST: Promotions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.PromotionEdit)]
        public async Task<IActionResult> Edit(int id, PromotionViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var promotion = await _context.Promotions
                        .Include(p => p.ProductPromotions)
                        .Include(p => p.PromotionExclusions)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (promotion == null)
                    {
                        return NotFound();
                    }

                    // 更新促銷資料
                    promotion.Name = viewModel.Name;
                    promotion.Description = viewModel.Description;
                    promotion.PromotionType = viewModel.PromotionType;
                    promotion.DiscountType = viewModel.DiscountType;
                    promotion.DiscountValue = viewModel.DiscountValue;
                    promotion.MinOrderAmount = viewModel.MinOrderAmount;
                    promotion.MaxDiscountAmount = viewModel.MaxDiscountAmount;
                    promotion.MaxOrderDiscountAmount = viewModel.MaxOrderDiscountAmount;
                    promotion.MinOrderQuantity = viewModel.MinOrderQuantity;
                    promotion.StartDate = viewModel.StartDate;
                    promotion.EndDate = viewModel.EndDate;
                    promotion.IsActive = viewModel.IsActive;
                    promotion.UsageLimit = viewModel.UsageLimit;
                    promotion.BadgeColor = viewModel.BadgeColor;
                    promotion.BadgeText = viewModel.BadgeText;
                    promotion.SortOrder = viewModel.SortOrder;
                    promotion.CanStackWithOthers = viewModel.CanStackWithOthers;
                    promotion.ExclusionGroup = viewModel.ExclusionGroup;
                    promotion.Priority = viewModel.Priority;
                    promotion.PromoCode = viewModel.PromoCode;
                    promotion.RequireCode = viewModel.RequireCode;
                    promotion.UpdatedAt = DateTime.Now;

                    // 更新產品關聯
                    _context.ProductPromotions.RemoveRange(promotion.ProductPromotions);
                    foreach (var productId in viewModel.SelectedProductIds)
                    {
                        _context.ProductPromotions.Add(new ProductPromotion
                        {
                            PromotionId = promotion.Id,
                            ProductId = productId,
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        });
                    }

                    // 更新排除規則
                    _context.PromotionExclusions.RemoveRange(promotion.PromotionExclusions);
                    foreach (var productId in viewModel.ExcludedProductIds)
                    {
                        _context.PromotionExclusions.Add(new PromotionExclusion
                        {
                            PromotionId = promotion.Id,
                            ProductId = productId,
                            CreatedAt = DateTime.Now
                        });
                    }

                    foreach (var categoryId in viewModel.ExcludedCategoryIds)
                    {
                        _context.PromotionExclusions.Add(new PromotionExclusion
                        {
                            PromotionId = promotion.Id,
                            CategoryId = categoryId,
                            CreatedAt = DateTime.Now
                        });
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "行銷活動更新成功！";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromotionExists(viewModel.Id))
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

            ViewBag.Products = _context.Products.Select(p => new { p.Id, p.Name }).ToList();
            ViewBag.Categories = _context.Categories.Select(c => new { c.Id, c.Name }).ToList();
            return View(viewModel);
        }

        // POST: Promotions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.PromotionDelete)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion != null)
            {
                _context.Promotions.Remove(promotion);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "行銷活動刪除成功！";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Promotions/Analytics/5
        [AdminPermission(AdminPermissions.PromotionAnalytics)]
        public async Task<IActionResult> Analytics(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return NotFound();
            }

            // 計算分析資料
            var usageData = await _context.UserPromotionUsages
                .Where(u => u.PromotionId == id)
                .ToListAsync();

            var orderData = await _context.Orders
                .Where(o => _context.UserPromotionUsages
                    .Where(u => u.PromotionId == id)
                    .Select(u => u.OrderId)
                    .Contains(o.Id))
                .ToListAsync();

            var viewModel = new PromotionAnalyticsViewModel
            {
                PromotionId = promotion.Id,
                PromotionName = promotion.Name,
                TotalUsage = usageData.Count,
                TotalDiscountAmount = usageData.Sum(u => u.DiscountAmount),
                TotalOrderAmount = orderData.Sum(o => o.TotalAmount),
                UniqueUsers = usageData.Select(u => u.UserId).Distinct().Count(),
                DailyUsageData = usageData
                    .GroupBy(u => u.UsedAt.Date)
                    .Select(g => new DailyUsageData
                    {
                        Date = g.Key,
                        Usage = g.Count(),
                        Revenue = orderData.Where(o => g.Any(u => u.OrderId == o.Id)).Sum(o => o.TotalAmount)
                    })
                    .OrderBy(d => d.Date)
                    .ToList()
            };

            return View(viewModel);
        }

        private bool PromotionExists(int id)
        {
            return _context.Promotions.Any(e => e.Id == id);
        }
    }
}