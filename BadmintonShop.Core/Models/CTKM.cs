namespace BadmintonShop.Core.Models;

/// <summary>Bang CTKM</summary>
public class CTKM
{
    public int MaCTKM { get; set; }
    public string TenCTKM { get; set; } = "";
    public DateTime NgayBatDau { get; set; }
    public DateTime NgayKetThuc { get; set; }
    public string LoaiKM { get; set; } = "";  // SANPHAM / HOADON / VOUCHER
    public bool TrangThai { get; set; }
}
