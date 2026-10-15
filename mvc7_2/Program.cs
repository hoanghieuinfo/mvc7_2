using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using mvc7_2.Authorization;
using mvc7_2.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Kết nối CSDL (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Xác thực bằng Cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".MVC7_2.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = context =>
        {
            // Trang quản lý sản phẩm: người chưa đăng nhập cũng bị chuyển thẳng về
            // /Account/AccessDenied (HTTP 403) giống người dùng NORMAL.
            if (context.Request.Path.StartsWithSegments("/Products"))
            {
                var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
                context.Response.Redirect(context.Request.PathBase + options.AccessDeniedPath +
                                          "?returnUrl=" + Uri.EscapeDataString(returnUrl));
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };
    });

// Phân quyền
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.CanDeleteProduct, policy =>
        policy.Requirements.Add(new ProductDeleteRequirement()));
});
builder.Services.AddSingleton<IAuthorizationHandler, ProductDeleteHandler>();

var app = builder.Build();

// Tự động tạo CSDL, các bảng và điền dữ liệu mẫu
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
