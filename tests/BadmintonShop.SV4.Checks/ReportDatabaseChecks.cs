using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;
using Microsoft.Data.SqlClient;

internal static partial class Program
{
    private static void ReportDatabaseChecks()
    {
        // Never seed an existing database, even if this method is accidentally called separately.
        var catalog=new SqlConnectionStringBuilder(DbHelper.ConnectionString).InitialCatalog;
        if(!catalog.StartsWith("BadmintonShop_SV4_Test_",StringComparison.Ordinal))
            throw new InvalidOperationException("Report fixture requires an isolated generated test database.");
        var fixture=ReportProgressFixture();
        int nv2=ScalarInt("INSERT NhanVien(HoTen,ChucVu) VALUES(N'Nhân viên 2',N'Kho');SELECT CAST(SCOPE_IDENTITY() AS int)");
        int kh1=ScalarInt("INSERT KhachHang(HoTen,Email,MatKhau) VALUES(N'Khách 1','report1@example.invalid','test-only');SELECT CAST(SCOPE_IDENTITY() AS int)");
        int kh2=ScalarInt("INSERT KhachHang(HoTen,Email,MatKhau) VALUES(N'Khách 2','report2@example.invalid','test-only');SELECT CAST(SCOPE_IDENTITY() AS int)");
        int sp3=ScalarInt($"INSERT SanPham(TenSP,MaDM,DonGiaBan) VALUES(N'Sản phẩm 3',{dm1},1);SELECT CAST(SCOPE_IDENTITY() AS int)");
        int Product(int id)=>id==1?sp1:id==2?sp2:sp3;
        foreach(DataRow r in fixture.NhapHang.Rows)
        {
            int pn=Convert.ToInt32(DbHelper.ExecuteScalar("INSERT PhieuNhap(NgayNhap,MaNCC,MaNV) VALUES(@date,@ncc,@nv);SELECT CAST(SCOPE_IDENTITY() AS int)",
                new SqlParameter("@date",r["NgayNhap"]),new SqlParameter("@ncc",ncc),new SqlParameter("@nv",nv)));
            int id=r.Field<int>("MaPN");int qty=id<=2?100:(int)r.Field<decimal>("TongTien");
            DbHelper.ExecuteNonQuery("INSERT ChiTietPhieuNhap(MaPN,MaSP,SoLuong,DonGia) VALUES(@pn,@sp,@qty,@price)",
                new SqlParameter("@pn",pn),new SqlParameter("@sp",Product(id<=2?id:3)),new SqlParameter("@qty",qty),new SqlParameter("@price",r.Field<decimal>("TongTien")/qty));
        }
        foreach(var group in fixture.BanHang.AsEnumerable().GroupBy(r=>r.Field<int>("MaHD")))
        {
            var r=group.First();
            int hd=Convert.ToInt32(DbHelper.ExecuteScalar(@"INSERT HoaDon(NgayLap,LoaiBan,TrangThai,TienGiamGia,MaNV,MaKH)
                VALUES(@date,@sale,N'Chờ xác nhận',@discount,@nv,@kh);SELECT CAST(SCOPE_IDENTITY() AS int)",
                new SqlParameter("@date",r["NgayLap"]),new SqlParameter("@sale",r["LoaiBan"]),
                new SqlParameter("@discount",group.Sum(x=>x.Field<decimal>("ThanhTien"))-r.Field<decimal>("TongTien")),
                new SqlParameter("@nv",r.IsNull("MaNV")?DBNull.Value:r.Field<int>("MaNV")==1?nv:nv2),
                new SqlParameter("@kh",r.IsNull("MaKH")?DBNull.Value:r.Field<int>("MaKH")==1?kh1:kh2)));
            foreach(var line in group)
                DbHelper.ExecuteNonQuery("INSERT ChiTietHoaDon(MaHD,MaSP,SoLuong,DonGia) VALUES(@hd,@sp,1,@price)",
                    new SqlParameter("@hd",hd),new SqlParameter("@sp",Product(line.Field<int>("MaSP"))),new SqlParameter("@price",line["ThanhTien"]));
            DbHelper.ExecuteNonQuery("UPDATE HoaDon SET TrangThai=@state WHERE MaHD=@hd",new SqlParameter("@state",r["TrangThai"]),new SqlParameter("@hd",hd));
        }
        var repo=new BaoCaoRepository();var from=new DateTime(2025,1,1);var to=new DateTime(2026,12,31);
        Test("DB-R01 SQL joins / trigger totals / independent two-year expected values",()=>
        {
            var raw=repo.LayDuLieuBaoCao(from,to,null);
            Eq(10,raw.BanHang.Rows.Count);Eq(3,raw.NhapHang.Rows.Count);
            var k=BaoCaoService.Tao(raw,from,to,null);Eq(511m,k.TongDoanhThu);
            Eq(411m,k.Bang["Sản phẩm × quý 2025"].Rows[^1].Field<decimal>("Tổng"));
            Eq(100m,k.Bang["Sản phẩm × quý 2026"].Rows[^1].Field<decimal>("Tổng"));
            foreach(var name in new[]{"Nhân viên × năm","Khách hàng × năm"})Eq(511m,k.Bang[name].Rows[^1].Field<decimal>("Tổng"));
            Eq(-89m,k.Bang["Thu–chi 2025"].Rows[2].Field<decimal>("Tổng"));
            Eq(-50m,k.Bang["Thu–chi 2026"].Rows[2].Field<decimal>("Tổng"));
            Eq(120m,k.Bang["Nhân viên × năm"].AsEnumerable().Single(r=>r.Field<string>("Tên")=="Chưa gán nhân viên").Field<decimal>("Tổng"));
            Eq(90m,k.Bang["Khách hàng × năm"].AsEnumerable().Single(r=>r.Field<string>("Tên")=="Khách vãng lai").Field<decimal>("Tổng"));
            XuatTaiLieu.Excel(Path.Combine(output,"demo-report.xlsx"),k);XuatTaiLieu.BaoCaoPdf(Path.Combine(output,"demo-report.pdf"),k);
            ExportExpected("demo-report",k);
        });
        Test("DB-R02 SQL end-day inclusion / quarter boundary / purchase-only period / channel",()=>
        {
            KetQuaBaoCao Read(DateTime a,DateTime b,string? sale=null)=>BaoCaoService.Tao(repo.LayDuLieuBaoCao(a,b,sale),a,b,sale);
            Eq(60m,Read(new(2025,3,31),new(2025,3,31)).TongDoanhThu);
            var q=Read(new(2025,4,1),new(2025,6,30));Eq(90m,q.TongDoanhThu);Eq(300m,q.Bang["Thu–chi 2025"].Rows[1].Field<decimal>("Tổng"));
            var empty=Read(new(2026,12,1),to);Eq(0m,empty.TongDoanhThu);Eq(150m,empty.Bang["Thu–chi 2026"].Rows[1].Field<decimal>("Tổng"));
            Eq(451m,Read(from,to,"Online").TongDoanhThu);
            Eq(0,repo.LayDuLieuBaoCao(from,to,"Online").NhapHang.Rows.Count);
            Eq(0m,Read(new(2024,1,1),new(2024,12,31)).TongDoanhThu);
        });
        Ghi("Report fixture verified: "+catalog+"; --report-demo-only retains this fixture without subsequent mutation checks.");
    }
}
