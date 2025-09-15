using System.ComponentModel.DataAnnotations;

namespace BearToyWebsiteBack.Models.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDisplayName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string ShippingMethod { get; set; } = string.Empty;
        public string ShippingStatus { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();
        public DateTime? PaymentConfirmationDate { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? Notes { get; set; }
    }

    public class OrderItemViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSku { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
        public string VariantDescription { get; set; } = string.Empty;
        public bool IsGift { get; set; }
    }

    public class OrderStatusUpdateViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        
        [Required]
        public string Status { get; set; } = string.Empty;
        
        [Required]
        public string PaymentStatus { get; set; } = string.Empty;
        
        [Required]
        public string ShippingStatus { get; set; } = string.Empty;
        
        public string? TrackingNumber { get; set; }
        public string? Notes { get; set; }
        
        public DateTime? PaymentConfirmationDate { get; set; }
        public DateTime? ShippingDate { get; set; }
        public DateTime? CompletionDate { get; set; }
    }

    public static class OrderStatusHelper
    {
        public static readonly Dictionary<string, string> OrderStatuses = new()
        {
            { "pending", "待處理" },
            { "confirmed", "已確認" },
            { "processing", "處理中" },
            { "shipped", "已出貨" },
            { "delivered", "已送達" },
            { "completed", "已完成" },
            { "cancelled", "已取消" },
            { "returned", "已退貨" }
        };

        public static readonly Dictionary<string, string> PaymentStatuses = new()
        {
            { "pending", "待付款" },
            { "paid", "已付款" },
            { "failed", "付款失敗" },
            { "refunded", "已退款" },
            { "partial_refunded", "部分退款" }
        };

        public static readonly Dictionary<string, string> ShippingStatuses = new()
        {
            { "not_shipped", "未出貨" },
            { "preparing", "備貨中" },
            { "shipped", "已出貨" },
            { "in_transit", "運送中" },
            { "delivered", "已送達" },
            { "returned", "已退回" }
        };

        public static string GetStatusDisplayName(string status, Dictionary<string, string> statusMap)
        {
            return statusMap.TryGetValue(status, out var displayName) ? displayName : status;
        }

        public static string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "pending" => "bg-warning text-dark",
                "confirmed" => "bg-info",
                "processing" => "bg-primary",
                "shipped" => "bg-success",
                "delivered" => "bg-success",
                "completed" => "bg-success",
                "cancelled" => "bg-danger",
                "returned" => "bg-secondary",
                _ => "bg-secondary"
            };
        }

        public static string GetPaymentStatusBadgeClass(string paymentStatus)
        {
            return paymentStatus switch
            {
                "pending" => "bg-warning text-dark",
                "paid" => "bg-success",
                "failed" => "bg-danger",
                "refunded" => "bg-info",
                "partial_refunded" => "bg-info",
                _ => "bg-secondary"
            };
        }

        public static string GetShippingStatusBadgeClass(string shippingStatus)
        {
            return shippingStatus switch
            {
                "not_shipped" => "bg-secondary",
                "preparing" => "bg-warning text-dark",
                "shipped" => "bg-primary",
                "in_transit" => "bg-info",
                "delivered" => "bg-success",
                "returned" => "bg-danger",
                _ => "bg-secondary"
            };
        }
    }
}