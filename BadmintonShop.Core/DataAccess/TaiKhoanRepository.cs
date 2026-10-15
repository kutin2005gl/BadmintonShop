using System.Data;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Truy xuat bang TaiKhoan (tai khoan nhan vien dang nhap Desktop) - SV1.
/// Mat khau luon bam bang MatKhauHelper truoc khi luu, KHONG luu mat khau tho.
/// Ham DangNhap se them o tuan 2 (nhanh feature/sv1-auth-desktop).
/// </summary>
public class TaiKhoanRepository
{
    private const string Cot = "MaTK, TenDangNhap, MatKhau, VaiTro, MaNV, TrangThai";

    // TV1: lay tat ca tai khoan, sap xep theo ten dang nhap
    public List<TaiKhoan> LayTatCa()
        => DbHelper.ExecuteQuery($"SELECT {Cot} FROM TaiKhoan ORDER BY TenDangNhap")
                   .AsEnumerable().Select(Map).ToList();

    // TV1: lay danh sach tai khoan kem ho ten, chuc vu nhan vien (de do len DataGridView)
    public DataTable LayDanhSachHienThi()
        => DbHelper.ExecuteQuery(@"SELECT tk.MaTK, tk.TenDangNhap, tk.VaiTro, tk.TrangThai,
                                          nv.MaNV, nv.HoTen, nv.ChucVu
                                   FROM TaiKhoan tk JOIN NhanVien nv ON nv.MaNV = tk.MaNV
                                   ORDER BY tk.TenDangNhap");

    // TV1: lay 1 tai khoan theo ma, khong co tra ve null
    public TaiKhoan? LayTheoMa(int maTK)
        => LayMot($"SELECT {Cot} FROM TaiKhoan WHERE MaTK = @gt", maTK);

    // TV1: lay tai khoan theo ten dang nhap (dung cho dang nhap), khong co tra ve null
    public TaiKhoan? LayTheoTenDangNhap(string tenDangNhap)
        => LayMot($"SELECT {Cot} FROM TaiKhoan WHERE TenDangNhap = @gt", tenDangNhap);

    // TV1: lay tai khoan cua 1 nhan vien (moi NV toi da 1 TK), khong co tra ve null
    public TaiKhoan? LayTheoMaNV(int maNV)
        => LayMot($"SELECT {Cot} FROM TaiKhoan WHERE MaNV = @gt", maNV);

    // TV1: kiem tra ten dang nhap da bi dung chua
    public bool TenDangNhapDaTonTai(string tenDangNhap)
        => (int)DbHelper.ExecuteScalar("SELECT COUNT(*) FROM TaiKhoan WHERE TenDangNhap = @ten",
                                       new SqlParameter("@ten", tenDangNhap))! > 0;

    // TV1: cap tai khoan moi cho nhan vien, bam mat khau tho truoc khi luu, tra ve MaTK
    public int Them(TaiKhoan tk, string matKhauTho)
    {
        const string sql = @"INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, MaNV, TrangThai)
                             VALUES (@ten, @mk, @vt, @nv, @tt);
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";
        return (int)DbHelper.ExecuteScalar(sql,
            new SqlParameter("@ten", tk.TenDangNhap),
            new SqlParameter("@mk",  MatKhauHelper.BamMatKhau(matKhauTho)),
            new SqlParameter("@vt",  tk.VaiTro),
            new SqlParameter("@nv",  tk.MaNV),
            new SqlParameter("@tt",  tk.TrangThai))!;
    }

    // TV1: doi vai tro (ADMIN / QUANLY / NHANVIEN) cua tai khoan
    public bool DoiVaiTro(int maTK, string vaiTro)
        => DbHelper.ExecuteNonQuery("UPDATE TaiKhoan SET VaiTro = @vt WHERE MaTK = @ma",
                                    new SqlParameter("@vt", vaiTro), new SqlParameter("@ma", maTK)) > 0;

    // TV1: khoa (false) hoac mo khoa (true) tai khoan
    public bool DoiTrangThai(int maTK, bool hoatDong)
        => DbHelper.ExecuteNonQuery("UPDATE TaiKhoan SET TrangThai = @tt WHERE MaTK = @ma",
                                    new SqlParameter("@tt", hoatDong), new SqlParameter("@ma", maTK)) > 0;

    // TV1: dat mat khau moi (reset / doi mat khau), bam lai bang BCrypt
    public bool DatMatKhau(int maTK, string matKhauMoiTho)
        => DbHelper.ExecuteNonQuery("UPDATE TaiKhoan SET MatKhau = @mk WHERE MaTK = @ma",
                                    new SqlParameter("@mk", MatKhauHelper.BamMatKhau(matKhauMoiTho)),
                                    new SqlParameter("@ma", maTK)) > 0;

    // TV1: chay cau SELECT tra ve toi da 1 tai khoan theo 1 tham so @gt
    private static TaiKhoan? LayMot(string sql, object giaTri)
    {
        var dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@gt", giaTri));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    // TV1: chuyen 1 dong DataRow thanh doi tuong TaiKhoan
    private static TaiKhoan Map(DataRow r) => new()
    {
        MaTK        = r.Field<int>("MaTK"),
        TenDangNhap = r.Field<string>("TenDangNhap")!,
        MatKhau     = r.Field<string>("MatKhau")!,
        VaiTro      = r.Field<string>("VaiTro")!,
        MaNV        = r.Field<int>("MaNV"),
        TrangThai   = r.Field<bool>("TrangThai"),
    };
}
