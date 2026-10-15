using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc7_2.Models;

[Table("tblUserRoles")]
public class UserRole
{
    [Key]
    public int RoleId { get; set; }

    // Khóa ngoại trỏ về tblUsers(UserId)
    public int UserId { get; set; }

    // Chỉ nhận 1 trong 3 giá trị: NORMAL < STAFF < MANAGER
    [Required, StringLength(50)]
    public string RoleName { get; set; } = Roles.Normal;

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
