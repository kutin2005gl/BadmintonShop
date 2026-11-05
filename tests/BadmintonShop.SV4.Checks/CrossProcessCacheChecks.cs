using System.Diagnostics;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;
using Microsoft.Data.SqlClient;

internal static partial class Program
{
    private static int CacheSignalChild()
    {
        DbHelper.ConnectionString=Environment.GetEnvironmentVariable("BadmintonShop__SignalTestConnection")!;
        if(!new SqlConnectionStringBuilder(DbHelper.ConnectionString).InitialCatalog.StartsWith("BadmintonShop_SV4_Test_",StringComparison.Ordinal))return 2;
        DbHelper.ExecuteNonQuery(@"UPDATE SanPham SET TenSP=N'Sửa từ tiến trình khác' WHERE MaSP=@id;
            UPDATE HoaDon SET TienGiamGia=TienGiamGia+1 WHERE MaHD=(SELECT MIN(MaHD) FROM HoaDon
                WHERE TrangThai=N'Hoàn thành' AND NgayLap>='20260101' AND NgayLap<'20261101');",
            new SqlParameter("@id",int.Parse(Environment.GetEnvironmentVariable("BadmintonShop__SignalTestProduct")!)));
        return 0;
    }
    private static void CrossProcessCacheChecks()
    {
        Test("DB-C05 separate process commit invalidates Desktop and Web / read and rollback keep revision",()=>
        {
            var previous=Environment.GetEnvironmentVariable("BadmintonShop__CacheSignalRoot");
            Environment.SetEnvironmentVariable("BadmintonShop__CacheSignalRoot",Path.Combine(output,"signals"));
            PhienDangNhap.BatDau(1,nv,"signal-check","Demo",VaiTro.Admin);
            try
            {
                var catalog=new BadmintonShop.Web.Services.DanhMucCongKhai();catalog.Lay();
                using var form=new FrmSanPham();form.Show();WaitUi(form);
                using var reportForm=new FrmThongKe();reportForm.Show();WaitUi(reportForm);
                var dates=Controls(reportForm).OfType<DateTimePicker>().ToArray();dates[0].Value=new DateTime(2026,1,1);dates[1].Value=new DateTime(2026,12,31);
                var view=Controls(reportForm).OfType<Button>().Single(b=>b.Text=="Xem báo cáo");view.PerformClick();WaitUi(reportForm);
                var resultField=typeof(FrmThongKe).GetField("ketQua",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
                var original=(KetQuaBaoCao)resultField.GetValue(reportForm)!;
                long before=DbHelper.PhienBanDuLieu;var token=DbHelper.DauHieuDuLieu;
                DbHelper.ExecuteScalar("SELECT COUNT(*) FROM SanPham");Eq(token,DbHelper.DauHieuDuLieu);
                Reject(()=>DbHelper.ExecuteTransaction<int>((c,t)=>throw new InvalidOperationException("rollback")));
                Eq(token,DbHelper.DauHieuDuLieu);
                var start=new ProcessStartInfo(Path.ChangeExtension(typeof(Program).Assembly.Location,".exe"))
                { UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,RedirectStandardOutput=true,RedirectStandardError=true };
                start.ArgumentList.Add("--cache-signal-child");start.Environment["BadmintonShop__SignalTestConnection"]=DbHelper.ConnectionString;
                start.Environment["BadmintonShop__SignalTestProduct"]=sp1.ToString();
                using(var child=Process.Start(start)!)
                {
                    if(!child.WaitForExit(30000)) { child.Kill();throw new TimeoutException("Signal writer timed out"); }
                    Eq(0,child.ExitCode);
                }
                Eq(before,DbHelper.PhienBanDuLieu);True(token!=DbHelper.DauHieuDuLieu);
                using var counter=new SqlCounter();
                var search=Controls(form).OfType<TextBox>().Single();search.Text="Sửa từ tiến trình khác";
                Controls(form).OfType<Button>().Single(b=>b.Text=="Lọc").PerformClick();WaitUi(form);
                var grid=Controls(form).OfType<DataGridView>().Single();Eq(1,grid.Rows.Count);Eq(sp1,Convert.ToInt32(grid.Rows[0].Cells["MaSP"].Value));Eq(1,counter.Count);
                True(catalog.Lay().Products.Any(p=>p.MaSP==sp1 && p.TenSP=="Sửa từ tiến trình khác"));Eq(3,counter.Count);
                view.PerformClick();WaitUi(reportForm);Eq(5,counter.Count);
                var updated=(KetQuaBaoCao)resultField.GetValue(reportForm)!;Eq(original.TongDoanhThu-1,updated.TongDoanhThu);
                Ghi("SQL cross-process: local revision unchanged; marker changed; Desktop reload=1; Web catalog reload=2; report reload=2 and revenue decreased by 1");form.Close();reportForm.Close();
                var unavailable=Path.Combine(output,"signal-root-is-file");File.WriteAllText(unavailable,"not a directory");
                Environment.SetEnvironmentVariable("BadmintonShop__CacheSignalRoot",unavailable);
                Eq(1,DbHelper.ExecuteNonQuery("UPDATE SanPham SET DonViTinh=DonViTinh WHERE MaSP=@id",new SqlParameter("@id",sp1)));
                Ghi("PASS signal storage failure: committed SQL still returns success; manual refresh/TTL fallback required");
            }
            finally { Environment.SetEnvironmentVariable("BadmintonShop__CacheSignalRoot",previous);PhienDangNhap.DangXuat(); }
        });
    }
}
