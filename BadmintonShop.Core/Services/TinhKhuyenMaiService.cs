namespace BadmintonShop.Core.Services;

/// <summary>
/// Tinh tien giam gia cho gio hang / hoa don - dung chung cho Web (SV3) va POS Desktop (SV4).
/// SV1 chi tao KHUNG (ten ham + tham so); SV3 viet ruot (doc CTKM con hieu luc qua Repository).
/// Quy uoc: % giam theo SP ghi vao ChiTietHoaDon.GiamGia; giam theo HD/voucher ghi vao HoaDon.TienGiamGia + MaCTKM.
/// </summary>
public class TinhKhuyenMaiService
{
    // TV1: (khung cho SV3) tra ve % giam dang ap dung cho 1 san pham vao ngay 'ngay', khong co tra ve 0
    public decimal LayPhanTramGiamSanPham(int maSP, DateTime ngay)
        => throw new NotImplementedException("SV3: doc KM_TheoSP cua CTKM con hieu luc");

    // TV1: (khung cho SV3) tra ve (MaCTKM, so tien giam) theo bac tong tien hoa don cao nhat dat duoc
    public (int? MaCTKM, decimal TienGiam) TinhGiamTheoHoaDon(decimal tongTienHang, DateTime ngay)
        => throw new NotImplementedException("SV3: doc KM_TheoHD, chon bac TongTienToiThieu lon nhat <= tongTienHang");

    // TV1: (khung cho SV3) kiem tra ma voucher va tra ve (MaCTKM, so tien giam); voucher sai/het luot tra ve null
    public (int MaCTKM, decimal TienGiam)? TinhGiamTheoVoucher(string maVoucher, decimal tongTienHang, DateTime ngay)
        => throw new NotImplementedException("SV3: doc KM_TheoVoucher, kiem tra DaDung < SoLuong");
}
