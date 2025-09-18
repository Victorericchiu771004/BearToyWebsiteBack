using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BearToyWebsiteBack.Models;

public partial class BearToyDbContext : DbContext
{
    public BearToyDbContext()
    {
    }

    public BearToyDbContext(DbContextOptions<BearToyDbContext> options)
        : base(options)
    {
    }


    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ConvenienceStore> ConvenienceStores { get; set; }

    public virtual DbSet<DeliveryInfo> DeliveryInfos { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<PaymentNotification> PaymentNotifications { get; set; }

    public virtual DbSet<PostalCode> PostalCodes { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductPromotion> ProductPromotions { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<PromotionExclusion> PromotionExclusions { get; set; }

    public virtual DbSet<PromotionGift> PromotionGifts { get; set; }

    public virtual DbSet<SubCategory> SubCategories { get; set; }

    public virtual DbSet<UserPromotionUsage> UserPromotionUsages { get; set; }

    // 管理員相關表
    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<AdminRole> AdminRoles { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<AdminRolePermission> AdminRolePermissions { get; set; }

    public virtual DbSet<AdminRoleAssignment> AdminRoleAssignments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // 連線字串將從 appsettings.json 或 Program.cs 中配置
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasIndex(e => e.VariantId, "IX_CartItems_VariantId");

            entity.Property(e => e.VariantDescription).HasMaxLength(500);

            entity.HasOne(d => d.Product).WithMany(p => p.CartItems).HasForeignKey(d => d.ProductId);

            entity.HasOne(d => d.Variant).WithMany(p => p.CartItems).HasForeignKey(d => d.VariantId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<ConvenienceStore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Convenie__3214EC07B7C18159");

            entity.HasIndex(e => new { e.City, e.District }, "IX_ConvenienceStores_City_District");

            entity.HasIndex(e => e.StoreId, "IX_ConvenienceStores_StoreId").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.StoreId).HasMaxLength(50);
            entity.Property(e => e.StoreName).HasMaxLength(100);
            entity.Property(e => e.StoreType).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<DeliveryInfo>(entity =>
        {
            entity.HasIndex(e => e.ProductId, "IX_DeliveryInfos_ProductId").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.DeliveryDescription).HasMaxLength(500);
            entity.Property(e => e.EstimatedDeliveryTime).HasMaxLength(100);
            entity.Property(e => e.PreOrderDescription).HasMaxLength(500);
            entity.Property(e => e.StockStatus).HasDefaultValue(1);
            entity.Property(e => e.SuggestedQuantity).HasDefaultValue(1);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Product).WithOne(p => p.DeliveryInfo).HasForeignKey<DeliveryInfo>(d => d.ProductId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(e => e.OrderDate, "IX_Orders_OrderDate").IsDescending();

            entity.HasIndex(e => e.OrderNumber, "IX_Orders_OrderNumber");

            entity.HasIndex(e => e.PaymentStatus, "IX_Orders_PaymentStatus");

            entity.HasIndex(e => e.Status, "IX_Orders_Status");

            entity.HasIndex(e => e.UserId, "IX_Orders_UserId");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CompletionDate).HasColumnType("datetime");
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.OrderNumber).HasMaxLength(100);
            entity.Property(e => e.PaymentConfirmationDate).HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasDefaultValue("BankTransfer");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.PostalCode).HasMaxLength(10);
            entity.Property(e => e.RecipientName).HasMaxLength(100);
            entity.Property(e => e.ShippingAddress).HasMaxLength(100);
            entity.Property(e => e.ShippingDate).HasColumnType("datetime");
            entity.Property(e => e.ShippingMethod)
                .HasMaxLength(50)
                .HasDefaultValue("HomeDelivery");
            entity.Property(e => e.ShippingStatus)
                .HasMaxLength(50)
                .HasDefaultValue("Preparing");
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.StoreAddress).HasMaxLength(200);
            entity.Property(e => e.StoreId).HasMaxLength(50);
            entity.Property(e => e.StoreName).HasMaxLength(100);
            entity.Property(e => e.StorePickupCode).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);

            // 移除對前台 AspNetUsers 的外鍵引用
            // entity.HasOne(d => d.User).WithMany(p => p.Orders).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasIndex(e => e.ProductVariantId, "IX_OrderItems_ProductVariantId");

            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.VariantDescription)
                .HasMaxLength(500)
                .HasDefaultValue("");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasForeignKey(d => d.OrderId);

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems).HasForeignKey(d => d.ProductId);

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.OrderItems).HasForeignKey(d => d.ProductVariantId);
        });

        modelBuilder.Entity<PaymentNotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentN__3214EC078D0EA0CE");

            entity.HasIndex(e => e.OrderId, "IX_PaymentNotifications_OrderId");

            entity.HasIndex(e => e.Status, "IX_PaymentNotifications_Status");

            entity.HasIndex(e => e.UserId, "IX_PaymentNotifications_UserId");

            entity.Property(e => e.AccountName).HasMaxLength(100);
            entity.Property(e => e.AdminRemarks).HasMaxLength(500);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Last5DigitsOfAccount).HasMaxLength(5);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TransferAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TransferDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.PaymentNotifications)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentNotifications_Orders");

            // 移除對前台 AspNetUsers 的外鍵引用，改為使用字串ID
            // entity.HasOne(d => d.User).WithMany(p => p.PaymentNotifications)
            //     .HasForeignKey(d => d.UserId)
            //     .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PostalCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PostalCo__3214EC07DC3F9A13");

            entity.HasIndex(e => new { e.City, e.District }, "IX_PostalCodes_City_District");

            entity.HasIndex(e => e.PostalCode1, "IX_PostalCodes_PostalCode");

            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.PostalCode1)
                .HasMaxLength(10)
                .HasColumnName("PostalCode");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasDefaultValue("")
                .HasColumnName("SKU");

            entity.HasOne(d => d.Category).WithMany(p => p.Products).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.SubCategory).WithMany(p => p.Products).HasForeignKey(d => d.SubCategoryId);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasIndex(e => new { e.ProductId, e.SortOrder }, "IX_ProductImages_ProductId_SortOrder");
            entity.HasIndex(e => new { e.ProductId, e.ImageCategory }, "IX_ProductImages_ProductId_Category");

            entity.Property(e => e.AltText).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ImageType)
                .HasMaxLength(20)
                .HasDefaultValue("產品圖片");
            entity.Property(e => e.ImageCategory)
                .HasMaxLength(20)
                .HasDefaultValue("gallery");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages).HasForeignKey(d => d.ProductId);
        });

        modelBuilder.Entity<ProductPromotion>(entity =>
        {
            entity.HasIndex(e => new { e.ProductId, e.PromotionId }, "IX_ProductPromotions_ProductId_PromotionId").IsUnique();

            entity.HasIndex(e => e.PromotionId, "IX_ProductPromotions_PromotionId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductPromotions).HasForeignKey(d => d.ProductId);

            entity.HasOne(d => d.Promotion).WithMany(p => p.ProductPromotions).HasForeignKey(d => d.PromotionId);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(e => new { e.ProductId, e.Sku }, "IX_ProductVariants_ProductId_SKU").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants).HasForeignKey(d => d.ProductId);
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.Property(e => e.BadgeColor)
                .HasMaxLength(10)
                .HasDefaultValue("#FF6B6B");
            entity.Property(e => e.BadgeText).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DiscountType)
                .HasMaxLength(20)
                .HasDefaultValue("");
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ExclusionGroup)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaxOrderDiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MinOrderAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PromoCode)
                .HasMaxLength(50)
                .HasDefaultValue("");
            entity.Property(e => e.PromotionType).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<PromotionExclusion>(entity =>
        {
            entity.HasIndex(e => e.CategoryId, "IX_PromotionExclusions_CategoryId");

            entity.HasIndex(e => e.ProductId, "IX_PromotionExclusions_ProductId");

            entity.HasIndex(e => e.PromotionId, "IX_PromotionExclusions_PromotionId");

            entity.HasOne(d => d.Category).WithMany(p => p.PromotionExclusions).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.Product).WithMany(p => p.PromotionExclusions).HasForeignKey(d => d.ProductId);

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionExclusions).HasForeignKey(d => d.PromotionId);
        });

        modelBuilder.Entity<PromotionGift>(entity =>
        {
            entity.HasIndex(e => e.GiftProductId, "IX_PromotionGifts_GiftProductId");

            entity.HasIndex(e => e.PromotionId, "IX_PromotionGifts_PromotionId");

            entity.HasOne(d => d.GiftProduct).WithMany(p => p.PromotionGifts)
                .HasForeignKey(d => d.GiftProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionGifts).HasForeignKey(d => d.PromotionId);
        });

        modelBuilder.Entity<SubCategory>(entity =>
        {
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.SubCategories).HasForeignKey(d => d.CategoryId);
        });

        modelBuilder.Entity<UserPromotionUsage>(entity =>
        {
            entity.HasIndex(e => e.OrderId, "IX_UserPromotionUsages_OrderId");

            entity.HasIndex(e => e.PromotionId, "IX_UserPromotionUsages_PromotionId");

            entity.HasIndex(e => e.UserId, "IX_UserPromotionUsages_UserId");

            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.UserPromotionUsages)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Promotion).WithMany(p => p.UserPromotionUsages).HasForeignKey(d => d.PromotionId);

            // 移除對前台 AspNetUsers 的外鍵引用
            // entity.HasOne(d => d.User).WithMany(p => p.UserPromotionUsages)
            //     .HasForeignKey(d => d.UserId)
            //     .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // 管理員相關表配置
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.ToTable("BackAdmins");
            entity.HasKey(e => e.AdminId);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.LoginAttempts).HasDefaultValue(0);
            entity.Property(e => e.LastLoginIp).HasMaxLength(45);

        });

        modelBuilder.Entity<AdminRole>(entity =>
        {
            entity.ToTable("BackAdminRoles");
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleName).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("BackPermissions");
            entity.HasKey(e => e.PermissionId);
            entity.HasIndex(e => e.PermissionCode).IsUnique();
            entity.Property(e => e.PermissionCode).HasMaxLength(100);
            entity.Property(e => e.PermissionName).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<AdminRolePermission>(entity =>
        {
            entity.ToTable("BackAdminRolePermissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.AdminRole)
                .WithMany(p => p.AdminRolePermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.Permission)
                .WithMany(p => p.AdminRolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AdminRoleAssignment>(entity =>
        {
            entity.ToTable("BackAdminRoleAssignments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AdminId, e.RoleId }).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Admin)
                .WithMany(p => p.AdminRoleAssignments)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.AdminRole)
                .WithMany(p => p.AdminRoleAssignments)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
