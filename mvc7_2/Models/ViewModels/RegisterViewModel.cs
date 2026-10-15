using System.ComponentModel.DataAnnotations;

namespace mvc7_2.Models.ViewModels;

public class RegisterViewModel
{
    [Display(Name = "Tên đăng nhập")]
    [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
    [StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Họ và tên không được để trống")]
    [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(8, ErrorMessage = "Mật khẩu phải có tối thiểu 8 ký tự")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Xác nhận mật khẩu")]
    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
