using System.Data;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

// SV4: nguồn truy cập hóa đơn dùng chung cho SV3; trigger chịu trách nhiệm tồn kho/tổng tiền.
public class HoaDonRepository
{
    public bool HoTroPOS() => Convert.ToInt32(DbHelper.ExecuteScalar(@"SELECT CASE WHEN
        COL_LENGTH('dbo.HoaDon','MaYeuCauPOS') IS NOT NULL AND COL_LENGTH('dbo.HoaDon','TienKhachDua') IS NOT NULL
        THEN 1 ELSE 0 END")) == 1;
    // SV4: danh sách tìm mã đơn/tên/điện thoại/email khách, ngày cuối bao gồm trọn ngày.
    public DataTable LayDanhSachHienThi(DateTime tu, DateTime den, string? trangThai = null, string? loaiBan = null, string? tuKhoa = null)
    {
        if (tu.Date > den.Date) throw new ArgumentException("Ngày bắt đầu phải trước ngày kết thúc.");
        // Older group databases can still be viewed without running a schema migration.
        // Never discard a cancellation reason on writes: ChuyenTrangThai keeps its original requirements.
        var hasReason = Convert.ToInt32(DbHelper.ExecuteScalar("SELECT CASE WHEN COL_LENGTH('dbo.HoaDon','LyDoHuy') IS NULL THEN 0 ELSE 1 END")) == 1;
        var reasonColumn = hasReason ? "h.LyDoHuy" : "CAST(NULL AS nvarchar(500)) AS LyDoHuy";
        var result = DbHelper.ExecuteQuery($@"
            SELECT h.MaHD,h.NgayLap,COALESCE(k.HoTen,N'Khách vãng lai') AS KhachHang,k.SDT,k.Email,
                   h.LoaiBan,h.HinhThucTT,h.TienGiamGia,h.TongTien,h.TrangThai,{reasonColumn}
            FROM HoaDon h LEFT JOIN KhachHang k ON h.MaKH=k.MaKH
            WHERE h.NgayLap>=@tu AND h.NgayLap<DATEADD(day,1,@den)
              AND (@tt IS NULL OR h.TrangThai=@tt) AND (@loai IS NULL OR h.LoaiBan=@loai)
              AND (@tk IS NULL OR CONVERT(nvarchar(20),h.MaHD)=@tk OR k.HoTen LIKE N'%'+@tk+N'%'
                   OR k.SDT LIKE N'%'+@tk+N'%' OR k.Email LIKE N'%'+@tk+N'%')
            ORDER BY h.NgayLap DESC,h.MaHD DESC",
            new SqlParameter("@tu",tu.Date),new SqlParameter("@den",den.Date),
            new SqlParameter("@tt",DbHelper.GiaTriHoacNull(trangThai)),new SqlParameter("@loai",DbHelper.GiaTriHoacNull(loaiBan)),
            new SqlParameter("@tk",DbHelper.GiaTriHoacNull(string.IsNullOrWhiteSpace(tuKhoa)?null:tuKhoa.Trim())));
        result.ExtendedProperties["HoTroLyDoHuy"] = hasReason;
        return result;
    }

    // SV4: hóa đơn đã lưu để xem/xuất lại; MaKH null luôn hợp lệ.
    public HoaDon? LayTheoMa(int ma)
    {
        var dt = DbHelper.ExecuteQuery("SELECT * FROM HoaDon WHERE MaHD=@ma",new SqlParameter("@ma",ma));
        if (dt.Rows.Count==0) return null;
        var r=dt.Rows[0];
        return new HoaDon { MaHD=ma,MaKH=r.Field<int?>("MaKH"),MaNV=r.Field<int?>("MaNV"),
            NgayLap=r.Field<DateTime>("NgayLap"),LoaiBan=r.Field<string>("LoaiBan")!,HinhThucTT=r.Field<string>("HinhThucTT")!,
            DiaChiGiao=r.Field<string?>("DiaChiGiao"),MaCTKM=r.Field<int?>("MaCTKM"),TienGiamGia=r.Field<decimal>("TienGiamGia"),
            TongTien=r.Field<decimal>("TongTien"),TrangThai=r.Field<string>("TrangThai")!,LyDoHuy=dt.Columns.Contains("LyDoHuy")?r.Field<string?>("LyDoHuy"):null,
            MaYeuCauPOS=dt.Columns.Contains("MaYeuCauPOS")?r.Field<Guid?>("MaYeuCauPOS"):null,TienKhachDua=dt.Columns.Contains("TienKhachDua")?r.Field<decimal?>("TienKhachDua"):null };
    }

    // SV4: giá/giảm giá lấy từ chi tiết lịch sử, không dùng giá bán hiện tại.
    public DataTable LayChiTietHienThi(int ma) => DbHelper.ExecuteQuery(@"
        SELECT ct.MaSP,sp.TenSP,ct.SoLuong,ct.DonGia,ct.GiamGia,ct.ThanhTien
        FROM ChiTietHoaDon ct JOIN SanPham sp ON ct.MaSP=sp.MaSP WHERE ct.MaHD=@ma ORDER BY ct.MaSP",new SqlParameter("@ma",ma));

    // SV4: so sánh trạng thái kỳ vọng trong cùng UPDATE; trigger hoàn kho đúng một lần.
    public void ChuyenTrangThai(int ma, string hienTai, string moi, string? lyDo = null)
    {
        if (!QuyTacBanHang.DuocChuyen(hienTai,moi)) throw new ArgumentException("Không được chuyển trạng thái này. Chỉ hủy trước khi giao.");
        if (moi=="Đã hủy") lyDo=QuyTacBanHang.BatBuoc(lyDo,500,"Lý do hủy");
        DbHelper.ExecuteNonQuery(@"
            UPDATE HoaDon SET TrangThai=@moi,LyDoHuy=CASE WHEN @moi=N'Đã hủy' THEN @lydo ELSE LyDoHuy END
            WHERE MaHD=@ma AND TrangThai=@cu AND LoaiBan=N'Online';
            IF @@ROWCOUNT=0 THROW 50001,N'Đơn đã thay đổi hoặc không phải đơn Online. Hãy tải lại.',1;",
            new SqlParameter("@ma",ma),new SqlParameter("@cu",hienTai),new SqlParameter("@moi",moi),
            new SqlParameter("@lydo",DbHelper.GiaTriHoacNull(lyDo)));
    }

    // SV4: POS atomic + chống lặp bằng token bền vững; kiểm giá/tồn dưới khóa, không tự trừ tồn.
    public int ThanhToanPOS(int maNV, IReadOnlyList<ChiTietHoaDon> gio, decimal tienKhachDua, Guid yeuCau)
    {
        if (maNV<=0 || yeuCau==Guid.Empty || gio.Count==0) throw new ArgumentException("Cần nhân viên, mã yêu cầu và ít nhất một sản phẩm.");
        QuyTacBanHang.KiemTraTien(tienKhachDua);
        if (gio.Any(x=>x.MaSP<=0 || x.SoLuong<=0 || x.GiamGia!=0) || gio.GroupBy(x=>x.MaSP).Any(g=>g.Count()>1))
            throw new ArgumentException("Giỏ có số lượng không hợp lệ, sản phẩm trùng hoặc khuyến mãi chưa hỗ trợ.");
        foreach(var ct in gio) QuyTacBanHang.KiemTraTien(ct.DonGia);
        return DbHelper.ExecuteTransaction((c,t)=>
        {
            using var lap=new SqlCommand("SELECT MaHD,MaNV,TienKhachDua FROM HoaDon WITH(UPDLOCK,HOLDLOCK) WHERE MaYeuCauPOS=@id",c,t);
            lap.Parameters.AddWithValue("@id",yeuCau);
            int? daLuu=null;
            using(var r=lap.ExecuteReader())
            {
                if(r.Read())
                {
                    daLuu=r.GetInt32(0);
                    if(r.GetInt32(1)!=maNV || r.GetDecimal(2)!=tienKhachDua) throw new InvalidOperationException("Mã yêu cầu đã dùng cho giao dịch khác. Hãy tải lại hóa đơn.");
                }
            }
            if(daLuu.HasValue)
            {
                using var check=new SqlCommand("SELECT MaSP,SoLuong,DonGia FROM ChiTietHoaDon WHERE MaHD=@ma",c,t);
                check.Parameters.AddWithValue("@ma",daLuu.Value);
                using var r=check.ExecuteReader();
                var count=0;
                while(r.Read())
                {
                    count++;
                    if(!gio.Any(x=>x.MaSP==r.GetInt32(0) && x.SoLuong==r.GetInt32(1) && x.DonGia==r.GetDecimal(2)))
                        throw new InvalidOperationException("Yêu cầu đã được lưu với giỏ khác. Xem lại hóa đơn trước khi bán tiếp.");
                }
                if(count!=gio.Count) throw new InvalidOperationException("Giỏ đã thay đổi so với hóa đơn được lưu.");
                return daLuu.Value;
            }
            decimal tong=0;
            foreach(var ct in gio.OrderBy(x=>x.MaSP))
            {
                using var sp=new SqlCommand("SELECT DonGiaBan,SoLuongTon,TrangThai FROM SanPham WITH(UPDLOCK,HOLDLOCK) WHERE MaSP=@ma",c,t);
                sp.Parameters.AddWithValue("@ma",ct.MaSP);
                using var r=sp.ExecuteReader();
                if(!r.Read() || r.GetString(2)!="Đang bán") throw new InvalidOperationException($"Sản phẩm {ct.MaSP} không còn bán. Hãy tải lại.");
                if(r.GetInt32(1)<ct.SoLuong) throw new InvalidOperationException($"Sản phẩm {ct.MaSP} không đủ tồn kho. Hãy giảm số lượng.");
                if(r.GetDecimal(0)!=ct.DonGia) throw new InvalidOperationException($"Giá sản phẩm {ct.MaSP} đã thay đổi. Hãy tải lại sản phẩm và chọn lại giỏ.");
                tong+=ct.DonGia*ct.SoLuong;
            }
            QuyTacBanHang.KiemTraTien(tong);
            if(tienKhachDua<tong) throw new ArgumentException("Tiền khách đưa chưa đủ thanh toán.");
            using var hd=new SqlCommand(@"INSERT HoaDon(MaNV,LoaiBan,HinhThucTT,TrangThai,MaYeuCauPOS,TienKhachDua)
                VALUES(@nv,N'Tại quầy',N'Tiền mặt',N'Hoàn thành',@id,@tien); SELECT CAST(SCOPE_IDENTITY() AS int);",c,t);
            hd.Parameters.AddWithValue("@nv",maNV);hd.Parameters.AddWithValue("@id",yeuCau);hd.Parameters.AddWithValue("@tien",tienKhachDua);
            var ma=(int)hd.ExecuteScalar()!;
            foreach(var ct in gio.OrderBy(x=>x.MaSP))
            {
                using var dong=new SqlCommand("INSERT ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia,GiamGia) VALUES(@hd,@sp,@sl,@gia,0)",c,t);
                dong.Parameters.AddWithValue("@hd",ma);dong.Parameters.AddWithValue("@sp",ct.MaSP);
                dong.Parameters.AddWithValue("@sl",ct.SoLuong);dong.Parameters.AddWithValue("@gia",ct.DonGia);
                dong.ExecuteNonQuery();
            }
            return ma;
        });
    }
}
