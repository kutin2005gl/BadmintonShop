namespace BadmintonShop.Core.Models;

public class DongGioHang
{
    public int MaSP { get; set; }
    public string TenSP { get; set; } = "";
    public string? HinhAnh { get; set; }
    public int SoLuong { get; set; }
    public int TonKho { get; set; }
    public decimal DonGia { get; set; }          // gia ban hien tai (chua giam)
    public decimal PhanTramGiam { get; set; }    // % giam theo san pham (KM_TheoSP)
    public decimal DonGiaSauGiam { get; set; }
    public decimal ThanhTien { get; set; }       // = SoLuong * DonGia * (1 - %/100), lam tron dong
    public bool DuHang => SoLuong <= TonKho;
}

/// <summary>Ket qua tinh gio hang: cac dong, tong tien, khuyen mai cap hoa don, canh bao (SV3).</summary>
public class KetQuaGioHang
{
    public List<DongGioHang> Dong { get; } = new();
    public string? VoucherYeuCau { get; set; } // normalized input, including a valid voucher losing to an invoice offer
    public decimal TienHangTruocGiam => Dong.Sum(d=>d.SoLuong*d.DonGia);
    public decimal TienGiamSanPham => TienHangTruocGiam-TongTienHang;
    public decimal TongTienHang { get; set; }    // tong ThanhTien (da giam theo SP)
    public decimal TienGiamHoaDon { get; set; }  // giam theo bac hoa don hoac voucher
    public int? MaCTKM { get; set; }
    public string? MaVoucher { get; set; }       // chi co khi voucher thuc su duoc ap dung
    public string? LoiVoucher { get; set; }      // co khi khach nhap voucher nhung khong hop le
    public List<string> CanhBao { get; } = new();
    public decimal TongThanhToan => Math.Max(0, TongTienHang - TienGiamHoaDon);
    public int TongSoLuong => Dong.Sum(d => d.SoLuong);
    public bool CoTheDatHang => Dong.Count > 0 && Dong.All(d => d.DuHang);
}