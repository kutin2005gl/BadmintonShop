
using System.Data;
using BadmintonShop.Core.Models;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

public class DanhGiaRepository
{
    // Lấy các đánh giá đã duyệt của một sản phẩm
    public List<DanhGia> LayTheoSanPham(int maSP)
    {
        const string sql = @"
            SELECT MaDG, MaSP, MaKH, SoSao,
                   NoiDung, NgayDanhGia, TrangThai
            FROM dbo.DanhGiaSanPham
            WHERE MaSP = @maSP
              AND TrangThai = N'Đã duyệt'
            ORDER BY NgayDanhGia DESC, MaDG DESC";

        var dt = DbHelper.ExecuteQuery(
            sql,
            new SqlParameter("@maSP", maSP)
        );

        return dt.AsEnumerable()
                 .Select(Map)
                 .ToList();
    }

    // Khách hàng gửi đánh giá mới
    public void ThemDanhGia(
        int maSP,
        int maKH,
        int soSao,
        string? noiDung)
    {
        if (maSP <= 0 || maKH <= 0)
            throw new ArgumentException(
                "Thông tin sản phẩm hoặc khách hàng không hợp lệ."
            );

        if (soSao < 1 || soSao > 5)
            throw new ArgumentException(
                "Số sao phải từ 1 đến 5."
            );

        noiDung = noiDung?.Trim();

        if (noiDung?.Length > 1000)
            throw new ArgumentException(
                "Nội dung đánh giá không được quá 1000 ký tự."
            );

        const string sql = @"
            INSERT INTO dbo.DanhGiaSanPham
                (MaSP, MaKH, SoSao, NoiDung, TrangThai)
            VALUES
                (@maSP, @maKH, @soSao, @noiDung, N'Chờ duyệt')";

        DbHelper.ExecuteNonQuery(
            sql,
            new SqlParameter("@maSP", maSP),
            new SqlParameter("@maKH", maKH),
            new SqlParameter("@soSao", soSao),
            new SqlParameter("@noiDung",
                DbHelper.GiaTriHoacNull(noiDung))
        );
    }

    // Lấy toàn bộ đánh giá để quản trị viên duyệt
    public List<DanhGia> LayTatCa()
    {
        const string sql = @"
            SELECT MaDG, MaSP, MaKH, SoSao,
                   NoiDung, NgayDanhGia, TrangThai
            FROM dbo.DanhGiaSanPham
            ORDER BY NgayDanhGia DESC, MaDG DESC";

        var dt = DbHelper.ExecuteQuery(sql);

        return dt.AsEnumerable()
                 .Select(Map)
                 .ToList();
    }

    // Quản trị viên duyệt hoặc từ chối đánh giá
    public void CapNhatTrangThai(int maDG, string trangThai)
    {
        if (trangThai != "Đã duyệt" &&
            trangThai != "Từ chối")
        {
            throw new ArgumentException(
                "Trạng thái đánh giá không hợp lệ."
            );
        }

        const string sql = @"
            UPDATE dbo.DanhGiaSanPham
            SET TrangThai = @trangThai
            WHERE MaDG = @maDG";

        int soDong = DbHelper.ExecuteNonQuery(
            sql,
            new SqlParameter("@trangThai", trangThai),
            new SqlParameter("@maDG", maDG)
        );

        if (soDong == 0)
            throw new InvalidOperationException(
                "Không tìm thấy đánh giá."
            );
    }

    // Chuyển dữ liệu SQL thành đối tượng DanhGia
    private static DanhGia Map(DataRow r)
    {
        return new DanhGia
        {
            MaDG = r.Field<int>("MaDG"),
            MaSP = r.Field<int>("MaSP"),
            MaKH = r.Field<int>("MaKH"),
            SoSao = r.Field<int>("SoSao"),
            NoiDung = r.Field<string?>("NoiDung"),
            NgayDanhGia = r.Field<DateTime>("NgayDanhGia"),
            TrangThai = r.Field<string>("TrangThai")!
        };
    }
}
