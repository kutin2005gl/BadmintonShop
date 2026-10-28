using System.Data;
using System.Text.RegularExpressions;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Quan ly chuong trinh khuyen mai cho Desktop (SV3): danh sach, them / sua (kem chi tiet theo loai), bat/tat, xoa.
/// Tach rieng khoi KhuyenMaiRepository (phan TINH khuyen mai cho Web va POS) de khong xung dot.
/// Chi tiet theo loai: SANPHAM (MaSP, PhanTramGG), HOADON (TongTienToiThieu, PhanTramGG), VOUCHER (MaVoucher, PhanTramGG, SoLuong, DaDung).
/// </summary>
public class KhuyenMaiQuanLyRepository
{
    public static readonly string[] CacLoai = { "SANPHAM", "HOADON", "VOUCHER" };
    private static readonly Regex MaVoucherHopLe = new("^[A-Z0-9_-]{3,30}$", RegexOptions.Compiled);

    // TV3: danh sach chuong trinh kem trang thai hien tai (dang ap dung / sap dien ra / ngung)
    public DataTable LayDanhSach() => DbHelper.ExecuteQuery(@"
        SELECT k.MaCTKM, k.TenCTKM,
               CASE k.LoaiKM WHEN 'SANPHAM' THEN N'Theo sản phẩm' WHEN 'HOADON' THEN N'Theo hóa đơn' ELSE N'Voucher' END AS Loai,
               CONVERT(char(10), k.NgayBatDau, 103) AS TuNgay,
               CONVERT(char(10), k.NgayKetThuc, 103) AS DenNgay,
               (SELECT COUNT(*) FROM KM_TheoSP WHERE MaCTKM = k.MaCTKM)
             + (SELECT COUNT(*) FROM KM_TheoHD WHERE MaCTKM = k.MaCTKM)
             + (SELECT COUNT(*) FROM KM_TheoVoucher WHERE MaCTKM = k.MaCTKM) AS SoMuc,
               CASE WHEN k.TrangThai = 0 THEN N'Ngừng áp dụng'
                    WHEN CAST(GETDATE() AS DATE) > k.NgayKetThuc THEN N'Ngừng (hết hạn)'
                    WHEN CAST(GETDATE() AS DATE) < k.NgayBatDau THEN N'Sắp diễn ra'
                    ELSE N'Đang áp dụng' END AS TrangThai
        FROM CTKM k ORDER BY k.MaCTKM DESC");

    // TV3: lay 1 chuong trinh de sua
    public CTKM? LayCTKM(int ma)
    {
        var dt = DbHelper.ExecuteQuery("SELECT MaCTKM, TenCTKM, NgayBatDau, NgayKetThuc, LoaiKM, TrangThai FROM CTKM WHERE MaCTKM = @ma",
            new SqlParameter("@ma", ma));
        if (dt.Rows.Count == 0) return null;
        var r = dt.Rows[0];
        return new CTKM
        {
            MaCTKM = r.Field<int>("MaCTKM"),
            TenCTKM = r.Field<string>("TenCTKM")!,
            NgayBatDau = r.Field<DateTime>("NgayBatDau"),
            NgayKetThuc = r.Field<DateTime>("NgayKetThuc"),
            LoaiKM = r.Field<string>("LoaiKM")!,
            TrangThai = r.Field<bool>("TrangThai"),
        };
    }

    // TV3: bang chi tiet RONG dung cot theo loai (de them moi tren luoi)
    public static DataTable TaoBangChiTiet(string loai)
    {
        var t = new DataTable();
        switch (loai)
        {
            case "SANPHAM":
                t.Columns.Add("MaSP", typeof(int));
                t.Columns.Add("PhanTramGG", typeof(decimal));
                break;
            case "HOADON":
                t.Columns.Add("TongTienToiThieu", typeof(decimal));
                t.Columns.Add("PhanTramGG", typeof(decimal));
                break;
            case "VOUCHER":
                t.Columns.Add("MaVoucher", typeof(string));
                t.Columns.Add("PhanTramGG", typeof(decimal));
                t.Columns.Add("SoLuong", typeof(int));
                t.Columns.Add("DaDung", typeof(int));
                break;
            default: throw new ArgumentException("Loại khuyến mãi không hợp lệ.");
        }
        return t;
    }

    // TV3: chi tiet cua 1 chuong trinh (cung cot voi TaoBangChiTiet)
    public DataTable LayChiTiet(int ma, string loai)
    {
        var sql = loai switch
        {
            "SANPHAM" => "SELECT MaSP, PhanTramGG FROM KM_TheoSP WHERE MaCTKM = @ma ORDER BY MaSP",
            "HOADON" => "SELECT TongTienToiThieu, PhanTramGG FROM KM_TheoHD WHERE MaCTKM = @ma ORDER BY TongTienToiThieu",
            "VOUCHER" => "SELECT MaVoucher, PhanTramGG, SoLuong, DaDung FROM KM_TheoVoucher WHERE MaCTKM = @ma ORDER BY MaVoucher",
            _ => throw new ArgumentException("Loại khuyến mãi không hợp lệ."),
        };
        return DbHelper.ExecuteQuery(sql, new SqlParameter("@ma", ma));
    }

    // kiem tra du lieu (goi truoc khi luu, nem ArgumentException co thong bao tieng Viet)
    public static void KiemTra(CTKM k, DataTable ct)
    {
        k.TenCTKM = QuyTacBanHang.BatBuoc(k.TenCTKM, 150, "Tên chương trình");
        if (!CacLoai.Contains(k.LoaiKM)) throw new ArgumentException("Loại khuyến mãi không hợp lệ.");
        if (k.NgayKetThuc.Date < k.NgayBatDau.Date) throw new ArgumentException("Ngày kết thúc phải từ ngày bắt đầu trở đi.");

        var rows = Dong(ct);
        if (rows.Count == 0) throw new ArgumentException("Hãy thêm ít nhất một dòng chi tiết cho chương trình.");

        var daCo = new HashSet<string>();
        foreach (var r in rows)
        {
            var pt = So(r, "PhanTramGG", "Phần trăm giảm");
            if (pt <= 0 || pt > 100) throw new ArgumentException("Phần trăm giảm phải lớn hơn 0 và không quá 100.");

            switch (k.LoaiKM)
            {
                case "SANPHAM":
                    var maSP = (int)So(r, "MaSP", "Sản phẩm");
                    if (!daCo.Add(maSP.ToString())) throw new ArgumentException("Một sản phẩm bị chọn nhiều lần trong cùng chương trình.");
                    break;
                case "HOADON":
                    var tong = So(r, "TongTienToiThieu", "Từ tổng tiền");
                    if (tong < 0 || tong != Math.Round(tong)) throw new ArgumentException("Mốc tổng tiền phải là số đồng không âm.");
                    if (!daCo.Add(tong.ToString("0"))) throw new ArgumentException("Có hai bậc trùng mốc tổng tiền.");
                    break;
                case "VOUCHER":
                    var ma = MaVC(r);
                    if (!MaVoucherHopLe.IsMatch(ma)) throw new ArgumentException($"Mã voucher “{ma}” không hợp lệ (3–30 ký tự: chữ A-Z, số, gạch ngang/dưới).");
                    if (!daCo.Add(ma)) throw new ArgumentException($"Mã voucher “{ma}” bị trùng trong danh sách.");
                    var sl = (int)So(r, "SoLuong", "Số lượt");
                    if (sl < 1) throw new ArgumentException($"Voucher {ma}: số lượt dùng phải từ 1 trở lên.");
                    if (!r.IsNull("DaDung") && sl < Convert.ToInt32(r["DaDung"]))
                        throw new ArgumentException($"Voucher {ma}: số lượt không được nhỏ hơn số lượt đã dùng ({r["DaDung"]}).");
                    break;
            }
        }
    }

    // TV3: luu chuong trinh (ma = 0 la them moi) cung chi tiet trong 1 transaction, tra ve MaCTKM
    public int Luu(CTKM k, DataTable ct)
    {
        KiemTra(k, ct);
        var rows = Dong(ct);

        return DbHelper.ExecuteTransaction((c, t) =>
        {
            var ma = k.MaCTKM;
            if (ma == 0)
            {
                using var them = Lenh(c, t, @"INSERT INTO CTKM (TenCTKM, NgayBatDau, NgayKetThuc, LoaiKM, TrangThai)
                                              VALUES (@ten, @bd, @kt, @loai, @tt);
                                              SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    ("@ten", k.TenCTKM), ("@bd", k.NgayBatDau.Date), ("@kt", k.NgayKetThuc.Date), ("@loai", k.LoaiKM), ("@tt", k.TrangThai));
                ma = (int)them.ExecuteScalar()!;
            }
            else
            {
                using var loai = Lenh(c, t, "SELECT LoaiKM FROM CTKM WHERE MaCTKM = @ma", ("@ma", ma));
                var loaiCu = loai.ExecuteScalar() as string
                             ?? throw new InvalidOperationException("Chương trình không còn tồn tại. Hãy tải lại.");
                if (loaiCu != k.LoaiKM) throw new InvalidOperationException("Không thể đổi loại của chương trình đã tạo.");
                using var sua = Lenh(c, t, @"UPDATE CTKM SET TenCTKM = @ten, NgayBatDau = @bd, NgayKetThuc = @kt, TrangThai = @tt
                                             WHERE MaCTKM = @ma",
                    ("@ten", k.TenCTKM), ("@bd", k.NgayBatDau.Date), ("@kt", k.NgayKetThuc.Date), ("@tt", k.TrangThai), ("@ma", ma));
                sua.ExecuteNonQuery();
            }

            switch (k.LoaiKM)
            {
                case "SANPHAM":
                    Lenh(c, t, "DELETE FROM KM_TheoSP WHERE MaCTKM = @ma", ("@ma", ma)).ExecuteNonQuery();
                    foreach (var r in rows)
                        Lenh(c, t, "INSERT INTO KM_TheoSP (MaCTKM, MaSP, PhanTramGG) VALUES (@ma, @sp, @pt)",
                            ("@ma", ma), ("@sp", Convert.ToInt32(r["MaSP"])), ("@pt", Convert.ToDecimal(r["PhanTramGG"]))).ExecuteNonQuery();
                    break;

                case "HOADON":
                    Lenh(c, t, "DELETE FROM KM_TheoHD WHERE MaCTKM = @ma", ("@ma", ma)).ExecuteNonQuery();
                    foreach (var r in rows)
                        Lenh(c, t, "INSERT INTO KM_TheoHD (MaCTKM, TongTienToiThieu, PhanTramGG) VALUES (@ma, @tong, @pt)",
                            ("@ma", ma), ("@tong", Convert.ToDecimal(r["TongTienToiThieu"])), ("@pt", Convert.ToDecimal(r["PhanTramGG"]))).ExecuteNonQuery();
                    break;

                case "VOUCHER":
                    LuuVoucher(c, t, ma, rows);
                    break;
            }
            return ma;
        });
    }

    // Voucher: giu nguyen DaDung cua ma da co; chi xoa ma chua ai dung
    private static void LuuVoucher(SqlConnection c, SqlTransaction t, int ma, List<DataRow> rows)
    {
        var daCo = new Dictionary<string, int>();
        using (var q = Lenh(c, t, "SELECT MaVoucher, DaDung FROM KM_TheoVoucher WHERE MaCTKM = @ma", ("@ma", ma)))
        using (var rd = q.ExecuteReader())
            while (rd.Read()) daCo[rd.GetString(0)] = rd.GetInt32(1);

        var moi = rows.Select(MaVC).ToHashSet();
        foreach (var (maVC, dung) in daCo)
        {
            if (moi.Contains(maVC)) continue;
            if (dung > 0)
                throw new InvalidOperationException($"Voucher {maVC} đã được dùng {dung} lượt nên không thể xóa. Hãy ngừng áp dụng chương trình thay vì xóa mã.");
            Lenh(c, t, "DELETE FROM KM_TheoVoucher WHERE MaVoucher = @v", ("@v", maVC)).ExecuteNonQuery();
        }

        foreach (var r in rows)
        {
            var maVC = MaVC(r);
            var pt = Convert.ToDecimal(r["PhanTramGG"]);
            var sl = Convert.ToInt32(r["SoLuong"]);
            var capNhat = Lenh(c, t, @"UPDATE KM_TheoVoucher SET PhanTramGG = @pt, SoLuong = @sl
                                       WHERE MaVoucher = @v AND MaCTKM = @ma",
                ("@pt", pt), ("@sl", sl), ("@v", maVC), ("@ma", ma)).ExecuteNonQuery();
            if (capNhat > 0) continue;

            var trung = Convert.ToInt32(Lenh(c, t, "SELECT COUNT(*) FROM KM_TheoVoucher WHERE MaVoucher = @v", ("@v", maVC)).ExecuteScalar());
            if (trung > 0) throw new InvalidOperationException($"Mã voucher {maVC} đã thuộc chương trình khác.");
            Lenh(c, t, "INSERT INTO KM_TheoVoucher (MaCTKM, MaVoucher, PhanTramGG, SoLuong, DaDung) VALUES (@ma, @v, @pt, @sl, 0)",
                ("@ma", ma), ("@v", maVC), ("@pt", pt), ("@sl", sl)).ExecuteNonQuery();
        }
    }

    // TV3: bat / tat chuong trinh (khong xoa du lieu)
    public void DatTrangThai(int ma, bool bat)
    {
        var n = DbHelper.ExecuteNonQuery("UPDATE CTKM SET TrangThai = @tt WHERE MaCTKM = @ma",
            new SqlParameter("@tt", bat), new SqlParameter("@ma", ma));
        if (n == 0) throw new InvalidOperationException("Chương trình không còn tồn tại. Hãy tải lại.");
    }

    // TV3: xoa chuong trinh; da ap dung cho hoa don thi khong cho xoa (chi ngung ap dung)
    public void Xoa(int ma)
    {
        var dung = Convert.ToInt32(DbHelper.ExecuteScalar("SELECT COUNT(*) FROM HoaDon WHERE MaCTKM = @ma", new SqlParameter("@ma", ma)));
        if (dung > 0)
            throw new InvalidOperationException($"Chương trình đã được áp dụng cho {dung} hóa đơn nên không thể xóa. Hãy chọn “Bật/Tắt” để ngừng áp dụng.");
        DbHelper.ExecuteNonQuery("DELETE FROM CTKM WHERE MaCTKM = @ma", new SqlParameter("@ma", ma));   // chi tiet xoa theo (ON DELETE CASCADE)
    }

    // ham phu
    private static List<DataRow> Dong(DataTable ct)
        => ct.Rows.Cast<DataRow>().Where(r => r.RowState != DataRowState.Deleted).ToList();

    private static decimal So(DataRow r, string cot, string ten)
    {
        if (r.IsNull(cot)) throw new ArgumentException($"Thiếu giá trị: {ten}.");
        return Convert.ToDecimal(r[cot]);
    }

    private static string MaVC(DataRow r)
        => (r.IsNull("MaVoucher") ? "" : Convert.ToString(r["MaVoucher"])!).Trim().ToUpperInvariant();

    private static SqlCommand Lenh(SqlConnection c, SqlTransaction t, string sql, params (string Ten, object? GiaTri)[] thamSo)
    {
        var cmd = new SqlCommand(sql, c, t);
        foreach (var (ten, giaTri) in thamSo) cmd.Parameters.AddWithValue(ten, giaTri ?? DBNull.Value);
        return cmd;
    }
}
