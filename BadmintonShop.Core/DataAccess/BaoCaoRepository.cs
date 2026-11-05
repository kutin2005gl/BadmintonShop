using System.Data;
using Microsoft.Data.SqlClient;
using BadmintonShop.Core.Models;

namespace BadmintonShop.Core.DataAccess;

public class BaoCaoRepository
{
    // SV4: đọc một tập dữ liệu nhất quán, không truy vấn lại khi đổi biểu đồ/xuất file.
    public DataTable LayDuLieu(DateTime tu, DateTime den, string? loaiBan)
        => DocDuLieu(tu,den,loaiBan,false).BanHang;

    public DuLieuBaoCao LayDuLieuBaoCao(DateTime tu, DateTime den, string? loaiBan)
        => DocDuLieu(tu,den,loaiBan,loaiBan==null);

    private DuLieuBaoCao DocDuLieu(DateTime tu, DateTime den, string? loaiBan,bool baoGomChi)
    {
        if(tu.Date>den.Date) throw new ArgumentException("Ngày bắt đầu phải trước ngày kết thúc.");
        return DbHelper.ExecuteTransaction((c,t)=>
        {
            using var cmd=new SqlCommand(@"
                SELECT h.MaHD,h.NgayLap,h.LoaiBan,h.TrangThai,h.TongTien,ct.MaSP,sp.TenSP,
                       sp.MaDM,dm.TenDanhMuc,ct.SoLuong,ct.ThanhTien,
                       h.MaNV,nv.HoTen AS TenNhanVien,h.MaKH,kh.HoTen AS TenKhachHang
                FROM HoaDon h LEFT JOIN ChiTietHoaDon ct ON h.MaHD=ct.MaHD
                LEFT JOIN SanPham sp ON ct.MaSP=sp.MaSP LEFT JOIN DanhMuc dm ON sp.MaDM=dm.MaDM
                LEFT JOIN NhanVien nv ON h.MaNV=nv.MaNV LEFT JOIN KhachHang kh ON h.MaKH=kh.MaKH
                WHERE h.NgayLap>=@tu AND h.NgayLap<DATEADD(day,1,@den) AND (@loai IS NULL OR h.LoaiBan=@loai)
                ORDER BY h.MaHD,ct.MaSP",c,t);
            cmd.Parameters.AddWithValue("@tu",tu.Date);cmd.Parameters.AddWithValue("@den",den.Date);
            cmd.Parameters.AddWithValue("@loai",DbHelper.GiaTriHoacNull(loaiBan));
            using var da=new SqlDataAdapter(cmd);
            var table=new DataTable();da.Fill(table);
            if(!baoGomChi)return new DuLieuBaoCao(table,new DataTable());
            using var chiCmd=new SqlCommand(@"
                SELECT MaPN,NgayNhap,TongTien FROM PhieuNhap
                WHERE NgayNhap>=@tu AND NgayNhap<DATEADD(day,1,@den)
                ORDER BY MaPN",c,t);
            chiCmd.Parameters.AddWithValue("@tu",tu.Date);chiCmd.Parameters.AddWithValue("@den",den.Date);
            using var chiDa=new SqlDataAdapter(chiCmd);
            var chi=new DataTable();chiDa.Fill(chi);
            return new DuLieuBaoCao(table,chi);
        },IsolationLevel.Serializable,thayDoiDuLieu:false);
    }
}
