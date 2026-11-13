using System.Data;
using BadmintonShop.Core.Models;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Truy xuat CTKM, KM_TheoSP, KM_TheoHD, KM_TheoVoucher (SV3).
/// Chi tinh CTKM "con hieu luc": TrangThai = 1 va ngay nam trong [NgayBatDau, NgayKetThuc].
/// </summary>
public class KhuyenMaiRepository
{
    private readonly SqlConnection? connection;
    private readonly SqlTransaction? transaction;
    public KhuyenMaiRepository() { }
    // SV4: bound repository never opens another connection during checkout.
    public KhuyenMaiRepository(SqlConnection connection, SqlTransaction transaction)
    { this.connection=connection;this.transaction=transaction; }
    private DataTable Query(string sql,params SqlParameter[] parameters)
    {
        if(connection==null)return DbHelper.ExecuteQuery(sql,parameters);
        using var cmd=new SqlCommand(sql,connection,transaction);cmd.Parameters.AddRange(parameters);
        using var adapter=new SqlDataAdapter(cmd);var result=new DataTable();adapter.Fill(result);return result;
    }
    private object? Scalar(string sql,params SqlParameter[] parameters)
    {
        if(connection==null)return DbHelper.ExecuteScalar(sql,parameters);
        using var cmd=new SqlCommand(sql,connection,transaction);cmd.Parameters.AddRange(parameters);return cmd.ExecuteScalar();
    }
    // TV3: % giam cao nhat dang ap dung cho 1 san pham vao ngay 'ngay', khong co tra ve 0
    public decimal LayPhanTramSanPham(int maSP, DateTime ngay)
    {
        const string sql = @"SELECT MAX(sp.PhanTramGG)
                             FROM KM_TheoSP sp JOIN CTKM k ON k.MaCTKM = sp.MaCTKM
                             WHERE sp.MaSP = @sp AND k.LoaiKM = 'SANPHAM' AND k.TrangThai = 1
                               AND @ngay BETWEEN k.NgayBatDau AND k.NgayKetThuc";
        var kq = Scalar(sql,
            new SqlParameter("@sp", maSP),
            new SqlParameter("@ngay", SqlDbType.Date) { Value = ngay.Date });
        return kq is decimal d ? d : 0m;
    }

    // TV3: bac giam theo hoa don cao nhat dat duoc (TongTienToiThieu lon nhat <= tongTienHang), khong co tra ve null
    public KMTheoHD? LayBacHoaDon(decimal tongTienHang, DateTime ngay)
    {
        const string sql = @"SELECT TOP 1 h.MaCTKM, h.TongTienToiThieu, h.PhanTramGG
                             FROM KM_TheoHD h JOIN CTKM k ON k.MaCTKM = h.MaCTKM
                             WHERE k.LoaiKM = 'HOADON' AND k.TrangThai = 1
                               AND @ngay BETWEEN k.NgayBatDau AND k.NgayKetThuc
                               AND h.TongTienToiThieu <= @tong
                             ORDER BY h.TongTienToiThieu DESC, h.PhanTramGG DESC, h.MaCTKM";
        var dt = Query(sql,
            new SqlParameter("@tong", tongTienHang),
            new SqlParameter("@ngay", SqlDbType.Date) { Value = ngay.Date });
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new KMTheoHD
        {
            MaCTKM = r.Field<int>("MaCTKM"),
            TongTienToiThieu = r.Field<decimal>("TongTienToiThieu"),
            PhanTramGG = r.Field<decimal>("PhanTramGG"),
        };
    }

    // TV3: lay voucher con hieu luc (CTKM dang bat, dung ngay, DaDung < SoLuong); sai / het han / het luot tra ve null
    public KMTheoVoucher? LayVoucherHopLe(string maVoucher, DateTime ngay)
    {
        if(string.IsNullOrWhiteSpace(maVoucher) || maVoucher.Trim().Length>30)return null;
        string sql = $@"SELECT v.MaCTKM, v.MaVoucher, v.PhanTramGG, v.SoLuong, v.DaDung
                             FROM KM_TheoVoucher v {(transaction!=null?"WITH (UPDLOCK,HOLDLOCK)":"")} JOIN CTKM k ON k.MaCTKM = v.MaCTKM
                             WHERE v.MaVoucher = @ma AND k.LoaiKM = 'VOUCHER' AND k.TrangThai = 1
                               AND @ngay BETWEEN k.NgayBatDau AND k.NgayKetThuc
                               AND v.DaDung < v.SoLuong";
        var dt = Query(sql,
            new SqlParameter("@ma", SqlDbType.VarChar, 30) { Value = (maVoucher ?? "").Trim().ToUpperInvariant() },
            new SqlParameter("@ngay", SqlDbType.Date) { Value = ngay.Date });
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new KMTheoVoucher
        {
            MaCTKM = r.Field<int>("MaCTKM"),
            MaVoucher = r.Field<string>("MaVoucher")!,
            PhanTramGG = r.Field<decimal>("PhanTramGG"),
            SoLuong = r.Field<int>("SoLuong"),
            DaDung = r.Field<int>("DaDung"),
        };
    }

    // SV4: explain invalid vouchers from the same repository/transaction used by Web and POS.
    public string? LoiVoucher(string maVoucher, DateTime ngay)
    {
        if (string.IsNullOrWhiteSpace(maVoucher) || maVoucher.Trim().Length > 30)
            return "Mã voucher không hợp lệ (tối đa 30 ký tự).";
        var rows = Query(@"SELECT k.TrangThai,k.LoaiKM,k.NgayBatDau,k.NgayKetThuc,v.SoLuong,v.DaDung
            FROM KM_TheoVoucher v JOIN CTKM k ON k.MaCTKM=v.MaCTKM WHERE v.MaVoucher=@ma",
            new SqlParameter("@ma", SqlDbType.VarChar, 30) { Value = maVoucher.Trim().ToUpperInvariant() });
        if (rows.Rows.Count == 0) return "Mã voucher không tồn tại.";
        var r = rows.Rows[0];
        if (!r.Field<bool>("TrangThai") || r.Field<string>("LoaiKM") != "VOUCHER") return "Voucher đã ngừng áp dụng.";
        if (ngay.Date < r.Field<DateTime>("NgayBatDau").Date) return $"Voucher chưa đến hạn; bắt đầu {r.Field<DateTime>("NgayBatDau"):dd/MM/yyyy}.";
        if (ngay.Date > r.Field<DateTime>("NgayKetThuc").Date) return $"Voucher đã hết hạn ngày {r.Field<DateTime>("NgayKetThuc"):dd/MM/yyyy}.";
        return r.Field<int>("DaDung") >= r.Field<int>("SoLuong") ? "Voucher đã hết lượt sử dụng." : null;
    }

    // CO-02: mot khach chi duoc dung 1 lan moi ma voucher. Tinh theo don chua huy da gan voucher nay (HoaDon.MaCTKM),
    // nen huy don thi duoc dung lai. Chi ap dung voi khach co ma (POS khong chon khach -> bo qua).
    public bool KhachDaDungVoucher(int maKH, string maVoucher)
    {
        if (maKH <= 0 || string.IsNullOrWhiteSpace(maVoucher)) return false;
        var n = Scalar(@"SELECT COUNT(*) FROM HoaDon h JOIN KM_TheoVoucher v ON v.MaCTKM = h.MaCTKM
                         WHERE v.MaVoucher = @ma AND h.MaKH = @kh AND h.TrangThai <> N'Đã hủy'",
            new SqlParameter("@ma", SqlDbType.VarChar, 30) { Value = maVoucher.Trim().ToUpperInvariant() },
            new SqlParameter("@kh", maKH));
        return n is int i && i > 0;
    }

    public const string ThongBaoDaDungVoucher = "Bạn đã dùng mã này rồi, mỗi khách chỉ được dùng một mã voucher một lần.";

    // TV3/SV4: increment only within checkout transaction; false means no longer eligible.
    public bool DungVoucher(SqlConnection conn, SqlTransaction tran, string maVoucher, DateTime? ngay=null, int? maCTKM=null)
    {
        using var cmd = new SqlCommand(
            @"UPDATE v SET DaDung = v.DaDung + 1 FROM KM_TheoVoucher v JOIN CTKM k ON k.MaCTKM=v.MaCTKM
              WHERE v.MaVoucher=@ma AND v.DaDung<v.SoLuong AND k.TrangThai=1 AND k.LoaiKM='VOUCHER'
                AND @ngay BETWEEN k.NgayBatDau AND k.NgayKetThuc AND (@km IS NULL OR v.MaCTKM=@km)", conn, tran);
        cmd.Parameters.AddWithValue("@ma", maVoucher.Trim().ToUpperInvariant());
        cmd.Parameters.AddWithValue("@ngay",(ngay??DateTime.Now).Date);cmd.Parameters.AddWithValue("@km",DbHelper.GiaTriHoacNull(maCTKM));
        return cmd.ExecuteNonQuery() == 1;
    }
}
