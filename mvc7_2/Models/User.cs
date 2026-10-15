using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc7_2.Models;

[Table("tblUsers")]
public class User
{
    [Key]
    public int UserId { get; set; }

    [Required, StringLength(50)]
    public string Username { get; set; } = string.Empty;

    // Chuỗi mật khẩu đã băm (PBKDF2-SHA256 + salt), không bao giờ lưu plaintext
    [Required, StringLength(256)]
    public string PasswordHashed { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Email { get; set; }

    // Số lần đăng nhập sai liên tiếp
    public int AccessFailedCount { get; set; } = 0;

    // Thời điểm hết hạn khóa tài khoản (null nếu không bị khóa)
    public DateTime? LockoutEnd { get; set; }

    // Trạng thái khóa tài khoản
    public bool IsLocked { get; set; } = false;

    // 1 User - N UserRoles
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // 1 User - N Products (người tạo)
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
