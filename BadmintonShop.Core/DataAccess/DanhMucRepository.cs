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
