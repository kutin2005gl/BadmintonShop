using System.Data;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

// SV4: nguồn truy cập hóa đơn dùng chung cho SV3; trigger chịu trách nhiệm tồn kho/tổng tiền.
public class HoaDonRepository
{
    public bool HoTroPOS() => Convert.ToInt32(DbHelper.ExecuteScalar(@"SELECT CASE WHEN
        COL_LENGTH('dbo.HoaDon','MaYeuCauPOS') IS NOT NULL AND COL_LENGTH('dbo.HoaDon','TienKhachDua') IS NOT NULL AND COL_LENGTH('dbo.HoaDon','DauVanTayPOS') IS NOT NULL
        AND EXISTS(SELECT 1 FROM sys.indexes i JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
          JOIN sys.columns c ON c.object_id=ic.object_id AND c.column_id=ic.column_id
          WHERE i.object_id=OBJECT_ID('dbo.HoaDon') AND i.is_unique=1 AND i.is_disabled=0
          AND i.has_filter=1 AND i.filter_definition=N'([MaYeuCauPOS] IS NOT NULL)'
          AND c.name='MaYeuCauPOS' AND ic.key_ordinal=1
          AND NOT EXISTS(SELECT 1 FROM sys.index_columns extra WHERE extra.object_id=i.object_id AND extra.index_id=i.index_id AND extra.key_ordinal>1))
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
        return Map(dt.Rows[0]);
    }
    internal static HoaDon Map(DataRow r)
    {
        var dt=r.Table;var ma=r.Field<int>("MaHD");
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

    // SV4: compatibility overload; callers with promotions use the confirmed shared cart snapshot overload.
    public int ThanhToanPOS(int maNV,IReadOnlyList<ChiTietHoaDon> rows,decimal cash,Guid token)
    {
        var quote=new KetQuaGioHang();
        foreach(var row in rows)quote.Dong.Add(new DongGioHang { MaSP=row.MaSP,SoLuong=row.SoLuong,DonGia=row.DonGia,
            PhanTramGiam=row.GiamGia,ThanhTien=TinhKhuyenMaiService.TinhThanhTien(row.SoLuong,row.DonGia,row.GiamGia) });
        quote.TongTienHang=quote.Dong.Sum(d=>d.ThanhTien);
        return ThanhToanPOS(maNV,quote,cash,token);
    }

    // SV4: reprice under locks and compare the confirmation before consuming a voucher or inserting any rows.
    public int ThanhToanPOS(int maNV,KetQuaGioHang quote,decimal tienKhachDua,Guid yeuCau)
    {
        if(maNV<=0 || yeuCau==Guid.Empty)throw new ArgumentException("Cần nhân viên và mã yêu cầu POS.");
        QuyTacBanHang.KiemTraTien(tienKhachDua);GiaoDichBanHang.KiemTraGio(quote);
        var fingerprint=XacNhanGia.DauVanTayPOS(maNV,tienKhachDua,quote);
        return DbHelper.ExecuteTransaction((c,t)=>
        {
            // Serialize only equal tokens. A range lock on an absent unique-index key can deadlock
            // with a Web order holding product locks while inserting its own invoice into that gap.
            using(var gate=new SqlCommand("DECLARE @r int; EXEC @r=sys.sp_getapplock @Resource=@key,@LockMode='Exclusive',@LockOwner='Transaction',@LockTimeout=15000; SELECT @r;",c,t))
            {
                gate.Parameters.AddWithValue("@key","BadmintonShop.POS."+yeuCau.ToString("N"));
                if(Convert.ToInt32(gate.ExecuteScalar())<0)throw new InvalidOperationException("Yêu cầu POS đang được xử lý. Hãy thử lại cùng giỏ và mã yêu cầu.");
            }
            int? existing=null;string? savedFingerprint=null;bool legacy=false;
            using(var cmd=new SqlCommand("SELECT MaHD,MaNV,TienKhachDua,DauVanTayPOS,MaCTKM,TienGiamGia FROM HoaDon WITH(READCOMMITTED) WHERE MaYeuCauPOS=@id",c,t))
            {
                cmd.Parameters.AddWithValue("@id",yeuCau);using var reader=cmd.ExecuteReader();
                if(reader.Read())
                {
                    existing=reader.GetInt32(0);savedFingerprint=reader.IsDBNull(3)?null:reader.GetString(3);
                    if(reader.GetInt32(1)!=maNV || reader.GetDecimal(2)!=tienKhachDua)
                        throw new InvalidOperationException("Mã yêu cầu đã dùng cho giao dịch khác. Hãy xem hóa đơn đã lưu.");
                    legacy=reader.IsDBNull(4) && reader.GetDecimal(5)==0;
                }
            }
            if(existing.HasValue)
            {
                if(savedFingerprint!=null)
                {
                    if(savedFingerprint!=fingerprint)throw new InvalidOperationException("Giỏ, voucher hoặc báo giá đã thay đổi so với yêu cầu được lưu. Hãy xem hóa đơn trước khi bán tiếp.");
                }
                else
                {
                    // Before integration only undiscounted POS requests existed. Never infer an old voucher input.
                    if(!legacy || quote.VoucherYeuCau!=null || quote.MaVoucher!=null || quote.TienGiamHoaDon!=0)
                        throw new InvalidOperationException("Hóa đơn POS cũ không có dấu vết voucher. Hãy xem lại hóa đơn đã lưu.");
                    using var cmd=new SqlCommand("SELECT MaSP,SoLuong,DonGia,GiamGia FROM ChiTietHoaDon WHERE MaHD=@ma",c,t);
                    cmd.Parameters.AddWithValue("@ma",existing.Value);using var reader=cmd.ExecuteReader();int count=0;
                    while(reader.Read())
                    {
                        count++;
                        if(!quote.Dong.Any(d=>d.MaSP==reader.GetInt32(0) && d.SoLuong==reader.GetInt32(1) && d.DonGia==reader.GetDecimal(2) && d.PhanTramGiam==reader.GetDecimal(3)))
                            throw new InvalidOperationException("Giỏ đã thay đổi so với hóa đơn được lưu.");
                    }
                    if(count!=quote.Dong.Count)throw new InvalidOperationException("Giỏ đã thay đổi so với hóa đơn được lưu.");
                }
                return existing.Value; // No repricing, voucher increment, or stock mutation on successful retry.
            }
            var ngay=DateTime.Now;
            var fresh=GiaoDichBanHang.TinhLai(c,t,quote,ngay);
            if(tienKhachDua<fresh.TongThanhToan)throw new ArgumentException("Tiền khách đưa chưa đủ thanh toán.");
            GiaoDichBanHang.DungVoucher(c,t,fresh,ngay);
            using var hd=new SqlCommand(@"INSERT HoaDon(MaNV,LoaiBan,HinhThucTT,TrangThai,MaYeuCauPOS,TienKhachDua,MaCTKM,TienGiamGia,DauVanTayPOS)
                VALUES(@nv,N'Tại quầy',N'Tiền mặt',N'Hoàn thành',@id,@cash,@km,@discount,@hash); SELECT CAST(SCOPE_IDENTITY() AS int);",c,t);
            hd.Parameters.AddWithValue("@nv",maNV);hd.Parameters.AddWithValue("@id",yeuCau);hd.Parameters.AddWithValue("@cash",tienKhachDua);
            hd.Parameters.AddWithValue("@km",DbHelper.GiaTriHoacNull(fresh.MaCTKM));hd.Parameters.AddWithValue("@discount",fresh.TienGiamHoaDon);hd.Parameters.AddWithValue("@hash",fingerprint);
            int ma=(int)hd.ExecuteScalar()!;
            GiaoDichBanHang.GhiChiTiet(c,t,ma,fresh);
            return ma;
        },IsolationLevel.Serializable);
    }
}
