namespace BadmintonShop.Core.Models;

/// <summary>Bang HoaDon</summary>
public class HoaDon
{
    public int MaHD { get; set; }
    public int? MaKH { get; set; }
    public int? MaNV { get; set; }
    public DateTime NgayLap { get; set; }
    public string LoaiBan { get; set; } = "";  // Online / Tại quầy
    public string HinhThucTT { get; set; } = "";  // COD / Online / Tiền mặt
    public string? DiaChiGiao { get; set; }
    public int? MaCTKM { get; set; }
    public decimal TienGiamGia { get; set; }
    public decimal TongTien { get; set; }  // trigger tu tinh
    public string TrangThai { get; set; } = "";
    public string? LyDoHuy { get; set; }
    public string? GhiChu { get; set; }   // SV3: ghi chu cua khach khi dat don online (NULL = khong co)
    public Guid? MaYeuCauPOS { get; set; }
    public decimal? TienKhachDua { get; set; }
}
