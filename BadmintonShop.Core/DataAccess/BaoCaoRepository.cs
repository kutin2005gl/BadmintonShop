using System.Data;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

public class BaoCaoRepository
{
    // SV4: đọc một tập dữ liệu nhất quán, không truy vấn lại khi đổi biểu đồ/xuất file.
    public DataTable LayDuLieu(DateTime tu, DateTime den, string? loaiBan)
    {
        if(tu.Date>den.Date) throw new ArgumentException("Ngày bắt đầu phải trước ngày kết thúc.");
        return DbHelper.ExecuteTransaction((c,t)=>
        {
            using var cmd=new SqlCommand(@"
                SELECT h.MaHD,h.NgayLap,h.LoaiBan,h.TrangThai,h.TongTien,ct.MaSP,sp.TenSP,
                       sp.MaDM,dm.TenDanhMuc,ct.SoLuong,ct.ThanhTien
                FROM HoaDon h LEFT JOIN ChiTietHoaDon ct ON h.MaHD=ct.MaHD
                LEFT JOIN SanPham sp ON ct.MaSP=sp.MaSP LEFT JOIN DanhMuc dm ON sp.MaDM=dm.MaDM
                WHERE h.NgayLap>=@tu AND h.NgayLap<DATEADD(day,1,@den) AND (@loai IS NULL OR h.LoaiBan=@loai)
                ORDER BY h.MaHD,ct.MaSP",c,t);
            cmd.Parameters.AddWithValue("@tu",tu.Date);cmd.Parameters.AddWithValue("@den",den.Date);
            cmd.Parameters.AddWithValue("@loai",DbHelper.GiaTriHoacNull(loaiBan));
            using var da=new SqlDataAdapter(cmd);
            var table=new DataTable();da.Fill(table);return table;
        },IsolationLevel.Serializable);
    }
}
