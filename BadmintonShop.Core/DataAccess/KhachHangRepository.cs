using System.Data;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Truy xuat bang KhachHang - SV1 lam phan dang ky / dang nhap (co che xac thuc Web),
/// SV2 them cac ham ho so, quan ly khach hang... vao CHINH file nay.
/// Email luon luu dang chu thuong, da cat khoang trang. Mat khau luon bam BCrypt.
/// </summary>
public class KhachHangRepository
{
    private const string Cot = "MaKH, HoTen, SDT, Email, MatKhau, DiaChi, NgayDangKy, TrangThai";

    // TV1: lay 1 khach hang theo ma, khong co tra ve null
    public KhachHang? LayTheoMa(int maKH)
        => LayMot($"SELECT {Cot} FROM KhachHang WHERE MaKH = @gt", maKH);

    // TV1: lay 1 khach hang theo email (khong phan biet hoa thuong), khong co tra ve null
    public KhachHang? LayTheoEmail(string email)
        => LayMot($"SELECT {Cot} FROM KhachHang WHERE Email = @gt", ChuanHoaEmail(email));

    // TV1: kiem tra email da duoc dang ky chua
    public bool EmailDaTonTai(string email)
        => (int)DbHelper.ExecuteScalar("SELECT COUNT(*) FROM KhachHang WHERE Email = @email",
                                       new SqlParameter("@email", ChuanHoaEmail(email)))! > 0;

    // TV1: dang ky khach hang moi (bam mat khau), tra ve MaKH. Goi EmailDaTonTai truoc de bao loi dep
    public int DangKy(KhachHang kh, string matKhauTho)
    {
        const string sql = @"INSERT INTO KhachHang (HoTen, SDT, Email, MatKhau, DiaChi)
                             VALUES (@ten, @sdt, @email, @mk, @dc);
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";
        return (int)DbHelper.ExecuteScalar(sql,
            new SqlParameter("@ten",   kh.HoTen.Trim()),
            new SqlParameter("@sdt",   DbHelper.GiaTriHoacNull(kh.SDT)),
            new SqlParameter("@email", ChuanHoaEmail(kh.Email)),
            new SqlParameter("@mk",    MatKhauHelper.BamMatKhau(matKhauTho)),
            new SqlParameter("@dc",    DbHelper.GiaTriHoacNull(kh.DiaChi)))!;
    }

    // TV1: dang nhap Web bang email + mat khau, chan khach bi khoa; thanh cong tra khach hang qua 'kh'
    public KetQuaDangNhap DangNhap(string email, string matKhau, out KhachHang? kh)
    {
        kh = null;
        var tim = LayTheoEmail(email);
        // kiem mat khau truoc, de nguoi sai mat khau khong biet duoc TK co bi khoa hay khong
        if (tim == null || !MatKhauHelper.KiemTra(matKhau, tim.MatKhau)) return KetQuaDangNhap.SaiThongTin;
        if (!tim.TrangThai) return KetQuaDangNhap.BiKhoa;
        kh = tim;
        return KetQuaDangNhap.ThanhCong;
    }

    // TV1: dat mat khau moi cho khach hang (doi / quen mat khau), bam lai bang BCrypt
    public bool DatMatKhau(int maKH, string matKhauMoiTho)
        => DbHelper.ExecuteNonQuery("UPDATE KhachHang SET MatKhau = @mk WHERE MaKH = @ma",
                                    new SqlParameter("@mk", MatKhauHelper.BamMatKhau(matKhauMoiTho)),
                                    new SqlParameter("@ma", maKH)) > 0;

    // TV1: chuan hoa email: cat khoang trang, doi ve chu thuong

    // SV2: Cap nhat thong tin ho so khach hang
    public bool CapNhatHoSo(int maKH, string hoTen, string? sdt, string? diaChi)
    {
        const string sql = @"
            UPDATE KhachHang
            SET HoTen = @hoTen,
                SDT = @sdt,
                DiaChi = @diaChi
            WHERE MaKH = @maKH
              AND TrangThai = 1";

        return DbHelper.ExecuteNonQuery(
            sql,
            new SqlParameter("@hoTen", hoTen.Trim()),
            new SqlParameter("@sdt", DbHelper.GiaTriHoacNull(sdt)),
            new SqlParameter("@diaChi", DbHelper.GiaTriHoacNull(diaChi)),
            new SqlParameter("@maKH", maKH)
        ) > 0;
    }

    /// <summary>Duoi email gia cho khach vang lai (.invalid = ten mien du phong, khong bao gio gui duoc thu).</summary>
    public const string DuoiEmailVangLai = "@khach.invalid";

    public static bool LaEmailVangLai(string? email)
        => email != null && email.EndsWith(DuoiEmailVangLai, StringComparison.OrdinalIgnoreCase);

    /// <summary>Chuan hoa SDT ve dang 0xxxxxxxxx (bo khoang trang, dau cham, +84/84). Khong hop le tra ve null.</summary>
    public static string? ChuanHoaSDT(string? sdt)
    {
        if (string.IsNullOrWhiteSpace(sdt)) return null;
        var so = new string(sdt.Where(char.IsDigit).ToArray());
        if (so.StartsWith("84") && so.Length is 11 or 12) so = "0" + so[2..];
        return so.Length is >= 10 and <= 11 && so[0] == '0' ? so : null;
    }

    /// <summary>
    /// Dat hang khong can dang nhap: tim khach theo SDT, chua co thi tao khach moi (email gia, mat khau ngau nhien
    /// nen khong dang nhap duoc cho toi khi khach tu dang ky/quen mat khau). Khong ghi de ho so khach da co.
    /// Tra ve MaKH. Loi nhap lieu nem ArgumentException, khach bi khoa nem InvalidOperationException.
    /// </summary>
    public int LayHoacTaoKhachVangLai(string? hoTen, string? sdt, string? diaChi)
    {
        hoTen = (hoTen ?? "").Trim();
        if (hoTen.Length == 0 || hoTen.Length > 100) throw new ArgumentException("Vui lòng nhập họ tên (tối đa 100 ký tự).");
        var so = ChuanHoaSDT(sdt) ?? throw new ArgumentException("Số điện thoại không hợp lệ.");

        int? Tim()
        {
            // SDT co the da luu o dang +84..., 84..., co dau cach/cham
            var dt = DbHelper.ExecuteQuery(@"
                SELECT TOP 1 MaKH, TrangThai FROM KhachHang
                WHERE REPLACE(REPLACE(REPLACE(REPLACE(SDT,' ',''),'-',''),'.',''),'+','') IN (@a, @b)
                ORDER BY TrangThai DESC, MaKH",
                new SqlParameter("@a", so), new SqlParameter("@b", "84" + so[1..]));
            if (dt.Rows.Count == 0) return null;
            if (!dt.Rows[0].Field<bool>("TrangThai"))
                throw new InvalidOperationException("Số điện thoại này thuộc tài khoản đang bị khóa. Vui lòng liên hệ cửa hàng.");
            return dt.Rows[0].Field<int>("MaKH");
        }

        var co = Tim();
        if (co != null) return co.Value;

        try
        {
            return (int)DbHelper.ExecuteScalar(@"INSERT INTO KhachHang (HoTen, SDT, Email, MatKhau, DiaChi)
                                                 VALUES (@ten, @sdt, @email, @mk, @dc);
                                                 SELECT CAST(SCOPE_IDENTITY() AS INT);",
                new SqlParameter("@ten", hoTen),
                new SqlParameter("@sdt", so),
                new SqlParameter("@email", "khach" + so + DuoiEmailVangLai),
                new SqlParameter("@mk", MatKhauHelper.BamMatKhau(Guid.NewGuid().ToString("N"))),
                new SqlParameter("@dc", DbHelper.GiaTriHoacNull(diaChi)))!;
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)   // hai don dong thoi cung SDT moi: lay khach vua tao
        {
            var vuaTao = Tim();
            if (vuaTao == null) throw;
            return vuaTao.Value;
        }
    }

    private static string ChuanHoaEmail(string email) => email.Trim().ToLowerInvariant();

    // TV1: chay cau SELECT tra ve toi da 1 khach hang theo 1 tham so @gt
    private static KhachHang? LayMot(string sql, object giaTri)
    {
        var dt = DbHelper.ExecuteQuery(sql, new SqlParameter("@gt", giaTri));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    // TV1: chuyen 1 dong DataRow thanh doi tuong KhachHang
    private static KhachHang Map(DataRow r) => new()
    {
        MaKH       = r.Field<int>("MaKH"),
        HoTen      = r.Field<string>("HoTen")!,
        SDT        = r.Field<string?>("SDT"),
        Email      = r.Field<string>("Email")!,
        MatKhau    = r.Field<string>("MatKhau")!,
        DiaChi     = r.Field<string?>("DiaChi"),
        NgayDangKy = r.Field<DateTime>("NgayDangKy"),
        TrangThai  = r.Field<bool>("TrangThai"),
    };
}
