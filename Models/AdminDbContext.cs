using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BearToyWebsiteBack.Models;

public partial class AdminDbContext : DbContext
{
    public AdminDbContext()
    {
    }

    public AdminDbContext(DbContextOptions<AdminDbContext> options)
        : base(options)
    {
    }

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