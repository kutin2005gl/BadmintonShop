using System.Data;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

public sealed record SanPhamNhap(string TenSP,int MaDM,string? ThuongHieu,decimal DonGiaBan,string DonViTinh,string TrangThai);

public sealed class NhapSanPhamRepository
{
    // New products only, all-or-nothing. Recheck references and duplicates under locks at commit.
    public int ThemMoi(IReadOnlyList<SanPhamNhap> rows)
    {
        if(rows.Count is <1 or >1000)throw new ArgumentException("Nhập từ 1 đến 1000 sản phẩm mỗi lần.");
        return DbHelper.ExecuteTransaction((connection,transaction)=>
        {
            foreach(var row in rows)
            {
                var name=QuyTacBanHang.BatBuoc(row.TenSP,150,"Tên sản phẩm");
                var unit=QuyTacBanHang.BatBuoc(row.DonViTinh,20,"Đơn vị tính");
                QuyTacBanHang.KiemTraTien(row.DonGiaBan);
                if(row.MaDM<=0 || row.ThuongHieu?.Length>50 || row.TrangThai is not ("Đang bán" or "Ngừng bán"))throw new ArgumentException("Danh mục, thương hiệu hoặc trạng thái không hợp lệ.");
                using var cmd=new SqlCommand(@"
                    IF NOT EXISTS(SELECT 1 FROM DanhMuc WITH(UPDLOCK,HOLDLOCK) WHERE MaDM=@dm)
                        THROW 50001,N'Danh mục đã thay đổi. Hãy xem trước lại.',1;
                    IF EXISTS(SELECT 1 FROM SanPham WITH(UPDLOCK,HOLDLOCK) WHERE TenSP=@ten AND MaDM=@dm AND ISNULL(ThuongHieu,N'')=ISNULL(@th,N''))
                        THROW 50001,N'Sản phẩm trùng tên, danh mục và thương hiệu. Không ghi đè; hãy xem trước lại.',1;
                    INSERT SanPham(TenSP,MaDM,ThuongHieu,DonGiaBan,DonViTinh,TrangThai,SoLuongTon)
                    VALUES(@ten,@dm,@th,@gia,@dv,@tt,0);",connection,transaction);
                cmd.Parameters.AddWithValue("@ten",name);cmd.Parameters.AddWithValue("@dm",row.MaDM);
                cmd.Parameters.AddWithValue("@th",DbHelper.GiaTriHoacNull(row.ThuongHieu));cmd.Parameters.AddWithValue("@gia",row.DonGiaBan);
                cmd.Parameters.AddWithValue("@dv",unit);cmd.Parameters.AddWithValue("@tt",row.TrangThai);cmd.ExecuteNonQuery();
            }
            return rows.Count;
        },IsolationLevel.Serializable);
    }
}
