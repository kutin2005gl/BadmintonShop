using System.Data;
using BadmintonShop.Core.Models;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Truy xuat PhieuNhap + ChiTietPhieuNhap (SV1).
/// Ton kho va TongTien do trigger trg_CTPN_TangTon tu cap nhat - code KHONG tu cong.
/// </summary>
public class PhieuNhapRepository
{
    private const string Cot = "MaPN, NgayNhap, MaNCC, MaNV, TongTien";

    // TV1: lay danh sach phieu nhap, loc theo khoang ngay va/hoac nha cung cap (null = bo qua)
    public List<PhieuNhap> LayTatCa(DateTime? tuNgay = null, DateTime? denNgay = null, int? maNCC = null)
    {
        var sql = $@"SELECT {Cot} FROM PhieuNhap
                     WHERE (@tu IS NULL OR NgayNhap >= @tu)
                       AND (@den IS NULL OR NgayNhap < DATEADD(DAY, 1, @den))
                       AND (@ncc IS NULL OR MaNCC = @ncc)
                     ORDER BY NgayNhap DESC";
        var dt = DbHelper.ExecuteQuery(sql,
            new SqlParameter("@tu",  SqlDbType.DateTime) { Value = DbHelper.GiaTriHoacNull(tuNgay?.Date) },
            new SqlParameter("@den", SqlDbType.DateTime) { Value = DbHelper.GiaTriHoacNull(denNgay?.Date) },
            new SqlParameter("@ncc", SqlDbType.Int)      { Value = DbHelper.GiaTriHoacNull(maNCC) });
        return dt.AsEnumerable().Select(Map).ToList();
    }

    // TV1: lay danh sach phieu nhap kem ten NCC, ten nhan vien lap (de do len DataGridView)
    public DataTable LayDanhSachHienThi(DateTime? tuNgay = null, DateTime? denNgay = null, int? maNCC = null)
    {
        const string sql = @"SELECT pn.MaPN, pn.NgayNhap, ncc.TenNCC, nv.HoTen AS NguoiLap, pn.TongTien
                             FROM PhieuNhap pn
                             JOIN NhaCungCap ncc ON ncc.MaNCC = pn.MaNCC
                             JOIN NhanVien   nv  ON nv.MaNV   = pn.MaNV
                             WHERE (@tu IS NULL OR pn.NgayNhap >= @tu)
                               AND (@den IS NULL OR pn.NgayNhap < DATEADD(DAY, 1, @den))
                               AND (@ncc IS NULL OR pn.MaNCC = @ncc)
                             ORDER BY pn.NgayNhap DESC";
        return DbHelper.ExecuteQuery(sql,
            new SqlParameter("@tu",  SqlDbType.DateTime) { Value = DbHelper.GiaTriHoacNull(tuNgay?.Date) },
            new SqlParameter("@den", SqlDbType.DateTime) { Value = DbHelper.GiaTriHoacNull(denNgay?.Date) },
            new SqlParameter("@ncc", SqlDbType.Int)      { Value = DbHelper.GiaTriHoacNull(maNCC) });
    }

    // TV1: lay 1 phieu nhap theo ma, khong co tra ve null
    public PhieuNhap? LayTheoMa(int maPN)
    {
        var dt = DbHelper.ExecuteQuery($"SELECT {Cot} FROM PhieuNhap WHERE MaPN = @ma",
                                       new SqlParameter("@ma", maPN));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    // TV1: lay cac dong chi tiet cua 1 phieu nhap
    public List<ChiTietPhieuNhap> LayChiTiet(int maPN)
    {
        var dt = DbHelper.ExecuteQuery(
            "SELECT MaPN, MaSP, SoLuong, DonGia, ThanhTien FROM ChiTietPhieuNhap WHERE MaPN = @ma",
            new SqlParameter("@ma", maPN));
        return dt.AsEnumerable().Select(MapChiTiet).ToList();
    }

    // TV1: lay chi tiet phieu nhap kem ten san pham (de do len DataGridView)
    public DataTable LayChiTietHienThi(int maPN)
        => DbHelper.ExecuteQuery(@"SELECT ct.MaSP, sp.TenSP, sp.DonViTinh, ct.SoLuong, ct.DonGia, ct.ThanhTien
                                   FROM ChiTietPhieuNhap ct JOIN SanPham sp ON sp.MaSP = ct.MaSP
                                   WHERE ct.MaPN = @ma ORDER BY sp.TenSP",
                                 new SqlParameter("@ma", maPN));

    // TV1: luu phieu nhap + chi tiet trong 1 transaction (loi la rollback het), tra ve MaPN moi
    public int Them(PhieuNhap pn, IReadOnlyList<ChiTietPhieuNhap> chiTiet)
    {
        if (chiTiet.Count == 0)
            throw new ArgumentException("Phieu nhap phai co it nhat 1 san pham.", nameof(chiTiet));
        if (chiTiet.GroupBy(c => c.MaSP).Any(g => g.Count() > 1))
            throw new ArgumentException("Mot san pham chi duoc xuat hien 1 lan trong phieu nhap.", nameof(chiTiet));

        return DbHelper.ExecuteTransaction((conn, tran) =>
        {
            using var cmdPN = new SqlCommand(
                @"INSERT INTO PhieuNhap (NgayNhap, MaNCC, MaNV) VALUES (@ngay, @ncc, @nv);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tran);
            cmdPN.Parameters.AddWithValue("@ngay", pn.NgayNhap == default ? DateTime.Now : pn.NgayNhap);
            cmdPN.Parameters.AddWithValue("@ncc", pn.MaNCC);
            cmdPN.Parameters.AddWithValue("@nv", pn.MaNV);
            var maPN = (int)cmdPN.ExecuteScalar()!;

            foreach (var ct in chiTiet)
            {
                using var cmdCT = new SqlCommand(
                    "INSERT INTO ChiTietPhieuNhap (MaPN, MaSP, SoLuong, DonGia) VALUES (@pn, @sp, @sl, @gia)",
                    conn, tran);
                cmdCT.Parameters.AddWithValue("@pn", maPN);
                cmdCT.Parameters.AddWithValue("@sp", ct.MaSP);
                cmdCT.Parameters.AddWithValue("@sl", ct.SoLuong);
                cmdCT.Parameters.AddWithValue("@gia", ct.DonGia);
                cmdCT.ExecuteNonQuery();   // trigger tu tang ton + tinh TongTien
            }
            return maPN;
        });
    }

    // TV1: chuyen 1 dong DataRow thanh doi tuong PhieuNhap
    private static PhieuNhap Map(DataRow r) => new()
    {
        MaPN     = r.Field<int>("MaPN"),
        NgayNhap = r.Field<DateTime>("NgayNhap"),
        MaNCC    = r.Field<int>("MaNCC"),
        MaNV     = r.Field<int>("MaNV"),
        TongTien = r.Field<decimal>("TongTien"),
    };

    // TV1: chuyen 1 dong DataRow thanh doi tuong ChiTietPhieuNhap
    private static ChiTietPhieuNhap MapChiTiet(DataRow r) => new()
    {
        MaPN      = r.Field<int>("MaPN"),
        MaSP      = r.Field<int>("MaSP"),
        SoLuong   = r.Field<int>("SoLuong"),
        DonGia    = r.Field<decimal>("DonGia"),
        ThanhTien = r.Field<decimal>("ThanhTien"),
    };
}
