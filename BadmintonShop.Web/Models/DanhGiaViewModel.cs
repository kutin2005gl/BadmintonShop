
using System.ComponentModel.DataAnnotations;

namespace BadmintonShop.Web.Models;

public class DanhGiaViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Sản phẩm không hợp lệ.")]
    public int MaSP { get; set; }

    [Range(1, 5, ErrorMessage = "Vui lòng chọn từ 1 đến 5 sao.")]
    public int SoSao { get; set; } = 5;

    [StringLength(1000,
        ErrorMessage = "Nhận xét không được vượt quá 1000 ký tự.")]
    public string? NoiDung { get; set; }
}
