using System.Data;
using BadmintonShop.Core.Models;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Truy xuat bang SanPham - phan DOC dung chung cho ca nhom (SV1 tao).
/// SV2/SV4 bo sung ham ghi (Them/Sua/Xoa...) vao file nay.
/// </summary>
public class SanPhamRepository
{
    // SV4: danh sách JOIN phục vụ lưới và bộ lọc; giữ nguyên các API đọc của SV1.
    public DataTable LayDanhSachHienThi() => DbHelper.ExecuteQuery(@"
        SELECT sp.MaSP,sp.TenSP,dm.TenDanhMuc,sp.ThuongHieu,sp.DonGiaBan,sp.DonViTinh,
               sp.SoLuongTon,sp.TrangThai,sp.MaDM,
               CASE WHEN sp.SoLuongTon=0 THEN N'Hết hàng' WHEN sp.SoLuongTon<=5 THEN N'Sắp hết (≤5)' ELSE N'Còn hàng' END AS CanhBao
        FROM SanPham sp JOIN DanhMuc dm ON dm.MaDM=sp.MaDM ORDER BY sp.TenSP");

    // SV4: thêm tồn = 0; sửa tuyệt đối không ghi SoLuongTon, tồn do phiếu nhập/trigger quản lý.
    public int Luu(SanPham sp)
    {
        sp.TenSP = Services.QuyTacBanHang.BatBuoc(sp.TenSP, 150, "Tên sản phẩm");
        sp.DonViTinh = Services.QuyTacBanHang.BatBuoc(sp.DonViTinh, 20, "Đơn vị tính");
        if (sp.ThuongHieu?.Length > 50 || sp.MaDM <= 0) throw new ArgumentException("Thương hiệu tối đa 50 ký tự, phải chọn danh mục.");
        if (sp.TrangThai is not ("Đang bán" or "Ngừng bán")) throw new ArgumentException("Trạng thái sản phẩm không hợp lệ.");
        Services.QuyTacBanHang.KiemTraTien(sp.DonGiaBan);
        Services.KhoAnhSanPham.DuongDanDayDu(sp.HinhAnh);
        const string sql = @"
            IF @ma=0 BEGIN
                INSERT SanPham(TenSP,MaDM,ThuongHieu,DonGiaBan,DonViTinh,HinhAnh,MoTa,TrangThai,SoLuongTon)
                VALUES(@ten,@dm,@th,@gia,@dv,@anh,@mt,@tt,0);
                SELECT CAST(SCOPE_IDENTITY() AS int);
            END ELSE BEGIN
                UPDATE SanPham SET TenSP=@ten,MaDM=@dm,ThuongHieu=@th,DonGiaBan=@gia,DonViTinh=@dv,
                    HinhAnh=@anh,MoTa=@mt,TrangThai=@tt WHERE MaSP=@ma;
                IF @@ROWCOUNT=0 THROW 50001, N'Sản phẩm đã bị xóa. Hãy tải lại.', 1;
                SELECT @ma;
            END";
        return (int)DbHelper.ExecuteScalar(sql, new SqlParameter("@ma",sp.MaSP),new SqlParameter("@ten",sp.TenSP),
            new SqlParameter("@dm",sp.MaDM),new SqlParameter("@th",DbHelper.GiaTriHoacNull(sp.ThuongHieu)),
            new SqlParameter("@gia",sp.DonGiaBan),new SqlParameter("@dv",sp.DonViTinh),
            new SqlParameter("@anh",DbHelper.GiaTriHoacNull(sp.HinhAnh)),new SqlParameter("@mt",DbHelper.GiaTriHoacNull(sp.MoTa)),
            new SqlParameter("@tt",sp.TrangThai))!;
    }

    // SV4: luôn ngừng bán thay vì xóa sản phẩm để giữ lịch sử giao dịch.
    public void NgungBan(int maSP)
    {
        DbHelper.ExecuteNonQuery("UPDATE SanPham SET TrangThai=N'Ngừng bán' WHERE MaSP=@ma; IF @@ROWCOUNT=0 THROW 50001,N'Sản phẩm không tồn tại.',1;",
            new SqlParameter("@ma", maSP));
    }
    private const string Cot = "MaSP, TenSP, MaDM, ThuongHieu, DonGiaBan, SoLuongTon, DonViTinh, HinhAnh, MoTa, TrangThai";

    // TV1: lay danh sach san pham, loc theo tu khoa (ten / thuong hieu) va danh muc neu co
    public List<SanPham> LayTatCa(string? tuKhoa = null, int? maDM = null)
    {
        var sql = $@"SELECT {Cot} FROM SanPham
                     WHERE (@tk IS NULL OR TenSP LIKE N'%' + @tk + N'%' OR ThuongHieu LIKE N'%' + @tk + N'%')
                       AND (@dm IS NULL OR MaDM = @dm)
                     ORDER BY TenSP";
        var dt = DbHelper.ExecuteQuery(sql,
            new SqlParameter("@tk", SqlDbType.NVarChar, 150) { Value = DbHelper.GiaTriHoacNull(tuKhoa) },
            new SqlParameter("@dm", SqlDbType.Int) { Value = DbHelper.GiaTriHoacNull(maDM) });
        return dt.AsEnumerable().Select(Map).ToList();
    }

    // TV1: lay 1 san pham theo ma, khong co tra ve null
    public SanPham? LayTheoMa(int maSP)
    {
        var dt = DbHelper.ExecuteQuery($"SELECT {Cot} FROM SanPham WHERE MaSP = @ma",
                                       new SqlParameter("@ma", maSP));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    // TV1: chuyen 1 dong DataRow thanh doi tuong SanPham
    private static SanPham Map(DataRow r) => new()
    {
        MaSP       = r.Field<int>("MaSP"),
        TenSP      = r.Field<string>("TenSP")!,
        MaDM       = r.Field<int>("MaDM"),
        ThuongHieu = r.Field<string?>("ThuongHieu"),
        DonGiaBan  = r.Field<decimal>("DonGiaBan"),
        SoLuongTon = r.Field<int>("SoLuongTon"),
        DonViTinh  = r.Field<string>("DonViTinh")!,
        HinhAnh    = r.Field<string?>("HinhAnh"),
        MoTa       = r.Field<string?>("MoTa"),
        TrangThai  = r.Field<string>("TrangThai")!,
    };
}
