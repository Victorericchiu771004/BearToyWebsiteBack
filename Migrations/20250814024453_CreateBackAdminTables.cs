using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BearToyWebsiteBack.Migrations
{
    /// <inheritdoc />
    public partial class CreateBackAdminTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackAdminRoles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackAdminRoles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "BackAdmins",
                columns: table => new
                {
                    AdminId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    LoginAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackAdmins", x => x.AdminId);
                });

            migrationBuilder.CreateTable(
                name: "BackPermissions",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PermissionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackPermissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "BackAdminRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackAdminRoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BackAdminRoleAssignments_BackAdminRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "BackAdminRoles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BackAdminRoleAssignments_BackAdmins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "BackAdmins",
                        principalColumn: "AdminId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BackAdminRolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackAdminRolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BackAdminRolePermissions_BackAdminRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "BackAdminRoles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BackAdminRolePermissions_BackPermissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "BackPermissions",
                        principalColumn: "PermissionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackAdminRoleAssignments_AdminId_RoleId",
                table: "BackAdminRoleAssignments",
                columns: new[] { "AdminId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BackAdminRoleAssignments_RoleId",
                table: "BackAdminRoleAssignments",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_BackAdminRolePermissions_PermissionId",
                table: "BackAdminRolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_BackAdminRolePermissions_RoleId_PermissionId",
                table: "BackAdminRolePermissions",
                columns: new[] { "RoleId", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BackAdmins_Username",
                table: "BackAdmins",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BackPermissions_PermissionCode",
                table: "BackPermissions",
                column: "PermissionCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackAdminRoleAssignments");

            migrationBuilder.DropTable(
                name: "BackAdminRolePermissions");

            migrationBuilder.DropTable(
                name: "BackAdmins");

            migrationBuilder.DropTable(
                name: "BackAdminRoles");

            migrationBuilder.DropTable(
                name: "BackPermissions");
        }
    }
}
