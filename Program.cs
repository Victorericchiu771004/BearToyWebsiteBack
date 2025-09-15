using BearToyWebsiteBack.Data;
using BearToyWebsiteBack.Models;
using BearToyWebsiteBack.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BearToyWebsiteBack
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("BearToyConnection") ?? throw new InvalidOperationException("Connection string 'BearToyConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            
            // 加入 BearToyDbContext
            var bearToyConnectionString = builder.Configuration.GetConnectionString("BearToyConnection") ?? throw new InvalidOperationException("Connection string 'BearToyConnection' not found.");
            builder.Services.AddDbContext<BearToyDbContext>(options =>
                options.UseSqlServer(bearToyConnectionString));
            
            // 加入 AdminDbContext（只用於後台管理員功能）
            builder.Services.AddDbContext<AdminDbContext>(options =>
                options.UseSqlServer(bearToyConnectionString));
            
            builder.Services.AddControllersWithViews()
                .AddDataAnnotationsLocalization();
            
            // 設定語系支援
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[] { "zh-TW" };
                options.SetDefaultCulture(supportedCultures[0])
                       .AddSupportedCultures(supportedCultures)
                       .AddSupportedUICultures(supportedCultures);
            });

            // 註冊管理員認證服務
            builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();
            builder.Services.AddScoped<IAdminInitializeService, AdminInitializeService>();

            // 設定管理員Cookie認證
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/AdminAuth/Login";
                    options.LogoutPath = "/AdminAuth/Logout";
                    options.AccessDeniedPath = "/AdminAuth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                    options.Cookie.Name = "BearToyAdmin";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            
            app.UseRequestLocalization();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=AdminAuth}/{action=Login}/{id?}");
            app.MapRazorPages();

            // 初始化管理員系統
            Task.Run(async () =>
            {
                using var scope = app.Services.CreateScope();
                var initService = scope.ServiceProvider.GetRequiredService<IAdminInitializeService>();
                await initService.InitializeAsync();
            });

            app.Run();
        }
    }
}
