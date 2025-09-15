using BearToyWebsiteBack.Models;
using BearToyWebsiteBack.Models.ViewModels;
using BearToyWebsiteBack.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BearToyWebsiteBack.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly BearToyDbContext _context;

        public OrdersController(BearToyDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        [AdminPermission(AdminPermissions.OrderView)]
        public async Task<IActionResult> Index(string? status = null, string? paymentStatus = null, string? search = null, int page = 1)
        {
            var pageSize = 20;
            
            var ordersQuery = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .AsQueryable();

            // 篩選條件
            if (!string.IsNullOrEmpty(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }

            if (!string.IsNullOrEmpty(paymentStatus))
            {
                ordersQuery = ordersQuery.Where(o => o.PaymentStatus == paymentStatus);
            }

            if (!string.IsNullOrEmpty(search))
            {
                ordersQuery = ordersQuery.Where(o => 
                    o.OrderNumber!.Contains(search) ||
                    o.RecipientName!.Contains(search) ||
                    o.ContactPhone!.Contains(search) ||
                    o.Email!.Contains(search));
            }

            var totalCount = await ordersQuery.CountAsync();
            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var orderViewModels = orders.Select(o => new OrderViewModel
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber ?? $"ORD-{o.Id:D6}",
                UserId = o.UserId,
                UserName = "會員用戶",
                UserEmail = "",
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                StatusDisplayName = OrderStatusHelper.GetStatusDisplayName(o.Status, OrderStatusHelper.OrderStatuses),
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                ShippingMethod = o.ShippingMethod,
                ShippingStatus = o.ShippingStatus,
                TrackingNumber = o.TrackingNumber,
                RecipientName = o.RecipientName ?? "",
                ContactPhone = o.ContactPhone ?? "",
                ShippingAddress = o.ShippingAddress,
                ItemCount = o.OrderItems.Sum(oi => oi.Quantity),
                PaymentConfirmationDate = o.PaymentConfirmationDate,
                ShippingDate = o.ShippingDate,
                CompletionDate = o.CompletionDate,
                Notes = o.Notes
            }).ToList();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentPaymentStatus = paymentStatus;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            return View(orderViewModels);
        }

        // GET: Orders/Details/5
        [AdminPermission(AdminPermissions.OrderView)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImages)
                .Include(o => o.PaymentNotifications)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var orderViewModel = new OrderViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber ?? $"ORD-{order.Id:D6}",
                UserId = order.UserId,
                UserName = "會員用戶",
                UserEmail = "",
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                StatusDisplayName = OrderStatusHelper.GetStatusDisplayName(order.Status, OrderStatusHelper.OrderStatuses),
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                ShippingMethod = order.ShippingMethod,
                ShippingStatus = order.ShippingStatus,
                TrackingNumber = order.TrackingNumber,
                RecipientName = order.RecipientName ?? "",
                ContactPhone = order.ContactPhone ?? "",
                ShippingAddress = order.ShippingAddress,
                PaymentConfirmationDate = order.PaymentConfirmationDate,
                ShippingDate = order.ShippingDate,
                CompletionDate = order.CompletionDate,
                Notes = order.Notes,
                OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductSku = oi.Product.Sku,
                    ProductImageUrl = oi.Product.ProductImages.FirstOrDefault(pi => pi.IsMainImage)?.ImageUrl
                                   ?? oi.Product.ProductImages.FirstOrDefault()?.ImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    VariantDescription = oi.VariantDescription,
                    IsGift = oi.IsGift
                }).ToList()
            };

            return View(orderViewModel);
        }

        // GET: Orders/UpdateStatus/5
        [AdminPermission(AdminPermissions.OrderEdit)]
        public async Task<IActionResult> UpdateStatus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderStatusUpdateViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber ?? $"ORD-{order.Id:D6}",
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                ShippingStatus = order.ShippingStatus,
                TrackingNumber = order.TrackingNumber,
                Notes = order.Notes,
                PaymentConfirmationDate = order.PaymentConfirmationDate,
                ShippingDate = order.ShippingDate,
                CompletionDate = order.CompletionDate
            };

            return View(viewModel);
        }

        // POST: Orders/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminPermission(AdminPermissions.OrderEdit)]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatusUpdateViewModel viewModel)
        {
            if (id != viewModel.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var order = await _context.Orders.FindAsync(id);
                    if (order == null)
                    {
                        return NotFound();
                    }

                    // 更新訂單狀態
                    order.Status = viewModel.Status;
                    order.PaymentStatus = viewModel.PaymentStatus;
                    order.ShippingStatus = viewModel.ShippingStatus;
                    order.TrackingNumber = viewModel.TrackingNumber;
                    order.Notes = viewModel.Notes;

                    // 根據狀態自動設定日期
                    if (viewModel.PaymentStatus == "paid" && order.PaymentConfirmationDate == null)
                    {
                        order.PaymentConfirmationDate = DateTime.Now;
                    }

                    if ((viewModel.Status == "shipped" || viewModel.ShippingStatus == "shipped") && order.ShippingDate == null)
                    {
                        order.ShippingDate = DateTime.Now;
                    }

                    if ((viewModel.Status == "completed" || viewModel.Status == "delivered") && order.CompletionDate == null)
                    {
                        order.CompletionDate = DateTime.Now;
                    }

                    // 允許手動設定日期
                    if (viewModel.PaymentConfirmationDate.HasValue)
                    {
                        order.PaymentConfirmationDate = viewModel.PaymentConfirmationDate;
                    }

                    if (viewModel.ShippingDate.HasValue)
                    {
                        order.ShippingDate = viewModel.ShippingDate;
                    }

                    if (viewModel.CompletionDate.HasValue)
                    {
                        order.CompletionDate = viewModel.CompletionDate;
                    }

                    _context.Update(order);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "訂單狀態已成功更新";
                    return RedirectToAction(nameof(Details), new { id = order.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(viewModel.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(viewModel);
        }

        // POST: Orders/QuickUpdateStatus
        [HttpPost]
        [AdminPermission(AdminPermissions.OrderEdit)]
        public async Task<IActionResult> QuickUpdateStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "訂單不存在" });
            }

            try
            {
                order.Status = status;

                // 自動更新相關狀態
                switch (status)
                {
                    case "confirmed":
                        if (order.PaymentStatus == "pending")
                            order.PaymentStatus = "paid";
                        if (order.PaymentConfirmationDate == null)
                            order.PaymentConfirmationDate = DateTime.Now;
                        break;

                    case "shipped":
                        order.ShippingStatus = "shipped";
                        if (order.ShippingDate == null)
                            order.ShippingDate = DateTime.Now;
                        break;

                    case "completed":
                        order.ShippingStatus = "delivered";
                        if (order.CompletionDate == null)
                            order.CompletionDate = DateTime.Now;
                        break;

                    case "cancelled":
                        if (order.PaymentStatus == "paid")
                            order.PaymentStatus = "refunded";
                        break;
                }

                _context.Update(order);
                await _context.SaveChangesAsync();

                var statusDisplayName = OrderStatusHelper.GetStatusDisplayName(status, OrderStatusHelper.OrderStatuses);
                
                return Json(new { 
                    success = true, 
                    message = $"訂單狀態已更新為：{statusDisplayName}",
                    newStatus = status,
                    newStatusDisplay = statusDisplayName
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "更新失敗：" + ex.Message });
            }
        }

        // GET: Orders/Statistics
        public async Task<IActionResult> GetStatistics()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            
            var stats = new
            {
                TotalOrders = await _context.Orders.CountAsync(),
                TodayOrders = await _context.Orders.CountAsync(o => o.OrderDate >= today),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == "pending"),
                ProcessingOrders = await _context.Orders.CountAsync(o => o.Status == "processing" || o.Status == "confirmed"),
                ShippedOrders = await _context.Orders.CountAsync(o => o.Status == "shipped"),
                CompletedOrders = await _context.Orders.CountAsync(o => o.Status == "completed"),
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status == "completed" || o.Status == "delivered")
                    .SumAsync(o => o.TotalAmount),
                MonthlyRevenue = await _context.Orders
                    .Where(o => (o.Status == "completed" || o.Status == "delivered") && o.OrderDate >= thisMonth)
                    .SumAsync(o => o.TotalAmount),
                PendingPayments = await _context.Orders.CountAsync(o => o.PaymentStatus == "pending"),
                UnshippedOrders = await _context.Orders.CountAsync(o => o.ShippingStatus == "not_shipped" && o.Status != "cancelled")
            };

            return Json(stats);
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}