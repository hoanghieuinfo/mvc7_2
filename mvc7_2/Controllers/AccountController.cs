using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc7_2.Data;
using mvc7_2.Models;
using mvc7_2.Models.ViewModels;
using mvc7_2.Services;

namespace mvc7_2.Controllers;

public class AccountController : Controller
{
    private const int MaxFailedAttempts = 3;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly AppDbContext _db;

    public AccountController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.Username = model.Username?.Trim() ?? string.Empty;

        if (ModelState.IsValid && await _db.Users.AnyAsync(u => u.Username == model.Username))
        {
            ModelState.AddModelError(nameof(model.Username), "Tên đăng nhập đã tồn tại");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new User
        {
            Username = model.Username,
            FullName = model.FullName.Trim(),
            PasswordHashed = PasswordHasher.Hash(model.Password),
            // Tài khoản đăng ký mới mặc định có vai trò NORMAL
            UserRoles = { new UserRole { RoleName = Roles.Normal } }
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        TempData["Message"] = "Đăng ký thành công! Vui lòng đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(TestLogin));
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var username = model.Username.Trim();
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
            return View(model);
        }

        var now = DateTime.Now;

        // Tài khoản đang bị khóa
        if (user.IsLocked && user.LockoutEnd.HasValue && user.LockoutEnd.Value > now)
        {
            var remaining = user.LockoutEnd.Value - now;
            ModelState.AddModelError(string.Empty,
                $"Tài khoản đã bị khóa do đăng nhập sai quá {MaxFailedAttempts} lần. " +
                $"Vui lòng thử lại sau {Math.Ceiling(remaining.TotalMinutes)} phút " +
                $"(mở khóa lúc {user.LockoutEnd.Value:HH:mm:ss dd/MM/yyyy}).");
            return View(model);
        }

        // Hết thời gian khóa -> mở khóa
        if (user.IsLocked)
        {
            user.IsLocked = false;
            user.LockoutEnd = null;
            user.AccessFailedCount = 0;
        }

        if (!PasswordHasher.Verify(model.Password, user.PasswordHashed))
        {
            user.AccessFailedCount++;

            if (user.AccessFailedCount >= MaxFailedAttempts)
            {
                user.IsLocked = true;
                user.LockoutEnd = now.Add(LockoutDuration);
                ModelState.AddModelError(string.Empty,
                    $"Bạn đã đăng nhập sai {MaxFailedAttempts} lần. Tài khoản bị khóa đăng nhập trong " +
                    $"{LockoutDuration.TotalMinutes} phút (đến {user.LockoutEnd.Value:HH:mm:ss dd/MM/yyyy}).");
            }
            else
            {
                ModelState.AddModelError(string.Empty,
                    $"Tên đăng nhập hoặc mật khẩu không đúng. Bạn còn " +
                    $"{MaxFailedAttempts - user.AccessFailedCount} lần thử.");
            }

            await _db.SaveChangesAsync();
            return View(model);
        }

        // Đăng nhập thành công -> reset bộ đếm
        user.AccessFailedCount = 0;
        user.IsLocked = false;
        user.LockoutEnd = null;
        await _db.SaveChangesAsync();

        await SignInAsync(user);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction(nameof(TestLogin));
    }

    // GET: /Account/TestLogin - chỉ truy cập được khi đã đăng nhập
    [HttpGet]
    [Authorize]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult TestLogin()
    {
        return View();
    }

    // GET: /Account/AccessDenied - trang báo lỗi không có quyền truy cập (HTTP 403)
    [HttpGet]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // GET: /Account/Logout
    public async Task<IActionResult> Logout()
    {
        // Xóa cookie đăng nhập
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        TempData["Message"] = "Bạn đã đăng xuất.";
        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Tạo cookie đăng nhập chứa thông tin người dùng và các vai trò.
    /// Theo thứ tự phân cấp NORMAL &lt; STAFF &lt; MANAGER, vai trò cao bao gồm quyền của vai trò thấp hơn,
    /// nên người dùng được gắn claim cho vai trò cao nhất và mọi vai trò bên dưới.
    /// </summary>
    private async Task SignInAsync(User user)
    {
        var highestRole = Roles.Highest(user.UserRoles.Select(r => r.RoleName));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Username),
            new("FullName", user.FullName),
            new("HighestRole", highestRole)
        };
        claims.AddRange(Roles.All
            .Take(Roles.Level(highestRole) + 1)
            .Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
