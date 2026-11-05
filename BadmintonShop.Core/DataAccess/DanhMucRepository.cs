using System.Data;
using BadmintonShop.Core.Models;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.DataAccess;

/// <summary>
/// Truy xuat bang DanhMuc - phan DOC dung chung cho ca nhom (SV1 tao).
/// SV2/SV4 bo sung ham ghi neu can.
/// </summary>
public class DanhMucRepository
{
    public DataTable LayDanhSachHienThi()=>DbHelper.ExecuteQuery("SELECT MaDM,TenDanhMuc FROM DanhMuc ORDER BY MaDM");
    // SV4: thêm/sửa trên repository dùng chung, kiểm tên trùng trong transaction có khóa khoảng.
    public int Luu(DanhMuc dm)
    {
        dm.TenDanhMuc = Services.QuyTacBanHang.BatBuoc(dm.TenDanhMuc, 100, "Tên danh mục");
        if (dm.MoTa?.Length > 255) throw new ArgumentException("Mô tả tối đa 255 ký tự.");
        return DbHelper.ExecuteTransaction((c, t) =>
        {
            using var cmd = new SqlCommand(@"
                IF EXISTS(SELECT 1 FROM DanhMuc WITH(UPDLOCK,HOLDLOCK) WHERE TenDanhMuc=@ten AND MaDM<>@ma)
                    THROW 50001, N'Tên danh mục đã tồn tại.', 1;
                IF @ma=0 BEGIN
                    INSERT DanhMuc(TenDanhMuc,MoTa) VALUES(@ten,@mt);
                    SELECT CAST(SCOPE_IDENTITY() AS int);
                END ELSE BEGIN
                    UPDATE DanhMuc SET TenDanhMuc=@ten,MoTa=@mt WHERE MaDM=@ma;
                    IF @@ROWCOUNT=0 THROW 50001, N'Danh mục đã bị xóa. Hãy tải lại.', 1;
                    SELECT @ma;
                END", c, t);
            cmd.Parameters.AddWithValue("@ma", dm.MaDM);
            cmd.Parameters.AddWithValue("@ten", dm.TenDanhMuc);
            cmd.Parameters.AddWithValue("@mt", DbHelper.GiaTriHoacNull(dm.MoTa));
            return (int)cmd.ExecuteScalar()!;
        });
    }

    // SV4: chặn xóa danh mục có sản phẩm, FK tiếp tục bảo vệ nếu có ghi đồng thời.
    public void Xoa(int maDM)
    {
        DbHelper.ExecuteNonQuery(@"
            IF EXISTS(SELECT 1 FROM SanPham WHERE MaDM=@ma)
                THROW 50001, N'Danh mục đang có sản phẩm. Hãy chuyển sản phẩm sang danh mục khác trước.', 1;
            DELETE DanhMuc WHERE MaDM=@ma;
            IF @@ROWCOUNT=0 THROW 50001, N'Danh mục đã bị xóa. Hãy tải lại.', 1;", new SqlParameter("@ma", maDM));
    }
    // TV1: lay tat ca danh muc, sap xep theo ten (dung cho menu web, ComboBox desktop)
    public List<DanhMuc> LayTatCa()
        => DbHelper.ExecuteQuery("SELECT MaDM, TenDanhMuc, MoTa FROM DanhMuc ORDER BY TenDanhMuc")
                   .AsEnumerable().Select(Map).ToList();

    // TV1: lay 1 danh muc theo ma, khong co tra ve null
    public DanhMuc? LayTheoMa(int maDM)
    {
        var dt = DbHelper.ExecuteQuery("SELECT MaDM, TenDanhMuc, MoTa FROM DanhMuc WHERE MaDM = @ma",
                                       new SqlParameter("@ma", maDM));
        return dt.Rows.Count == 0 ? null : Map(dt.Rows[0]);
    }

    // TV1: chuyen 1 dong DataRow thanh doi tuong DanhMuc
    private static DanhMuc Map(DataRow r) => new()
    {
        MaDM       = r.Field<int>("MaDM"),
        TenDanhMuc = r.Field<string>("TenDanhMuc")!,
        MoTa       = r.Field<string?>("MoTa"),
    };
}
