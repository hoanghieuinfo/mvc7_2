using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace mvc7_2.Models;

[Table("tblProducts")]
public class Product
{
    [Key]
    [Display(Name = "Mã SP")]
    public int ProductId { get; set; }

    [Display(Name = "Tên sản phẩm")]
    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [StringLength(150, ErrorMessage = "Tên sản phẩm tối đa 150 ký tự")]
    public string ProductName { get; set; } = string.Empty;

    [Display(Name = "Đơn giá")]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue, ErrorMessage = "Đơn giá không được âm")]
    public decimal Price { get; set; }

    [Display(Name = "Số lượng")]
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
    public int Quantity { get; set; }

    // Khóa ngoại trỏ về tblUsers(UserId) - người tạo sản phẩm
    [Display(Name = "Người tạo")]
    public int CreatedByUserId { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public User? CreatedByUser { get; set; }
}
