
using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Web.Models;

public class DatLaiMatKhauViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "Mật khẩu phải có từ 8 đến 100 ký tự")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string MatKhauMoi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
    [Compare(nameof(MatKhauMoi),
        ErrorMessage = "Xác nhận mật khẩu không khớp")]
    [DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu mới")]
    public string XacNhanMatKhau { get; set; } = string.Empty;
}
