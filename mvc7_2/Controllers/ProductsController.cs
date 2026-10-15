using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc7_2.Authorization;
using mvc7_2.Data;
using mvc7_2.Models;

namespace mvc7_2.Controllers;

// Chỉ người dùng có vai trò từ STAFF trở lên (STAFF hoặc MANAGER) được truy cập.
// Chưa đăng nhập hoặc chỉ có vai trò NORMAL -> chuyển hướng /Account/AccessDenied (HTTP 403).
[Authorize(Roles = Roles.StaffOrAbove)]
public class ProductsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuthorizationService _authorizationService;

    public ProductsController(AppDbContext db, IAuthorizationService authorizationService)
    {
        _db = db;
        _authorizationService = authorizationService;
    }

    // GET: /Products/Manage
    public async Task<IActionResult> Manage()
    {
        var products = await _db.Products
            .Include(p => p.CreatedByUser)
            .OrderBy(p => p.ProductId)
            .ToListAsync();
        return View(products);
    }

    // GET: /Products/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Product());
    }

    // POST: /Products/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ProductName,Price,Quantity")] Product product)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        // Người tạo là tài khoản đang đăng nhập
        product.CreatedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        TempData["Message"] = $"Đã thêm sản phẩm \"{product.ProductName}\".";
        return RedirectToAction(nameof(Manage));
    }

    // POST: /Products/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // Kiểm tra quyền sở hữu trên Server: MANAGER xóa mọi SP, STAFF chỉ xóa SP do mình tạo
        var result = await _authorizationService.AuthorizeAsync(User, product, Policies.CanDeleteProduct);
        if (!result.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        TempData["Message"] = $"Đã xóa sản phẩm \"{product.ProductName}\".";
        return RedirectToAction(nameof(Manage));
    }
}
