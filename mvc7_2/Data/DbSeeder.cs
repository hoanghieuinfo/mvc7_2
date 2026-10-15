using mvc7_2.Models;
using mvc7_2.Services;

namespace mvc7_2.Data;

/// <summary>
/// Điền sẵn dữ liệu mẫu cho tblUsers, tblUserRoles, tblProducts (chỉ chạy khi CSDL còn trống).
/// Mật khẩu mặc định của tất cả tài khoản mẫu: 12345678
/// </summary>
public static class DbSeeder
{
    public const string DefaultPassword = "12345678";

    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any())
        {
            return;
        }

        User NewUser(string username, string fullName, string email, string role) => new()
        {
            Username = username,
            FullName = fullName,
            Email = email,
            PasswordHashed = PasswordHasher.Hash(DefaultPassword),
            UserRoles = { new UserRole { RoleName = role } }
        };

        var manager = NewUser("manager", "Nguyễn Văn Quản Lý", "manager@shop.vn", Roles.Manager);
        var staff1 = NewUser("staff1", "Trần Thị Nhân Viên", "staff1@shop.vn", Roles.Staff);
        var staff2 = NewUser("staff2", "Lê Văn Nhân Viên", "staff2@shop.vn", Roles.Staff);
        var normal = NewUser("user1", "Phạm Văn Khách", "user1@shop.vn", Roles.Normal);

        db.Users.AddRange(manager, staff1, staff2, normal);

        db.Products.AddRange(
            new Product { ProductName = "Laptop Dell Inspiron 15", Price = 15_990_000, Quantity = 10, CreatedByUser = manager },
            new Product { ProductName = "Chuột Logitech M331", Price = 350_000, Quantity = 50, CreatedByUser = manager },
            new Product { ProductName = "Bàn phím cơ Akko 3087", Price = 1_290_000, Quantity = 25, CreatedByUser = staff1 },
            new Product { ProductName = "Màn hình LG 24 inch", Price = 3_490_000, Quantity = 15, CreatedByUser = staff1 },
            new Product { ProductName = "Tai nghe Sony WH-1000XM5", Price = 7_990_000, Quantity = 8, CreatedByUser = staff2 },
            new Product { ProductName = "USB Kingston 64GB", Price = 150_000, Quantity = 100, CreatedByUser = staff2 });

        db.SaveChanges();
    }
}
