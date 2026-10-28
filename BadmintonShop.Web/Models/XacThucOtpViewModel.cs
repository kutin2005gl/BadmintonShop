
using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Web.Models;

public class XacThucOtpViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
    [RegularExpression(@"^\d{6}$",
        ErrorMessage = "OTP phải gồm đúng 6 chữ số")]
    [Display(Name = "Mã OTP")]
    public string MaOtp { get; set; } = string.Empty;
}
