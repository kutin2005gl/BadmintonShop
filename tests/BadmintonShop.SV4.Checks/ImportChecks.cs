using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;
using ClosedXML.Excel;

internal static partial class Program
{
    private static void ImportChecks()
    {
        PhienDangNhap.BatDau(1,nv,"import-check","Demo",VaiTro.Admin);
        var categories=new DanhMucRepository().LayDanhSachHienThi();var existing=new SanPhamRepository().LayDanhSachHienThi();
        string path=Path.Combine(output,"import-template.xlsx");NhapSanPhamExcel.TaoMau(path,categories);
        Test("DB-X01 template / preview valid / commit / stock zero / invalidate / stale duplicate",()=>
        {
            using(var book=new XLWorkbook(path))
            {
                var sheet=book.Worksheet("SanPham");
                for(int i=0;i<2;i++)
                {
                    sheet.Cell(i+2,2).Value="Excel mới "+i;sheet.Cell(i+2,3).Value=dm1;sheet.Cell(i+2,4).Value="Demo";
                    sheet.Cell(i+2,5).Value=1000000+i;sheet.Cell(i+2,6).Value="Cây";sheet.Cell(i+2,7).Value="Đang bán";
                }
                book.SaveAs(Path.Combine(output,"import-valid.xlsx"));
            }
            var preview=NhapSanPhamExcel.Doc(Path.Combine(output,"import-valid.xlsx"),categories,existing);
            Eq(0,preview.Loi.Count);Eq(2,preview.SanPham.Count);
            int before=ScalarInt("SELECT COUNT(*) FROM SanPham");long revision=DbHelper.PhienBanDuLieu;
            Eq(2,NhapSanPhamExcel.Luu(preview));Eq(before+2,ScalarInt("SELECT COUNT(*) FROM SanPham"));True(DbHelper.PhienBanDuLieu>revision);
            Eq(0,ScalarInt("SELECT SUM(SoLuongTon) FROM SanPham WHERE TenSP LIKE N'Excel mới %'"));
            Reject(()=>NhapSanPhamExcel.Luu(preview));Eq(before+2,ScalarInt("SELECT COUNT(*) FROM SanPham"));
            var duplicate=NhapSanPhamExcel.Doc(Path.Combine(output,"import-valid.xlsx"),categories,new SanPhamRepository().LayDanhSachHienThi());
            True(duplicate.Loi.Any(x=>x.Contains("trùng")));Reject(()=>NhapSanPhamExcel.Luu(duplicate));
        });
        Test("DB-X02 row/column errors / formulas / invalid reference / ID / no partial commit / permission",()=>
        {
            using(var book=new XLWorkbook(Path.Combine(output,"import-valid.xlsx")))
            {
                var sheet=book.Worksheet("SanPham");sheet.Cell(2,1).Value=sp1;sheet.Cell(2,3).Value=999999;
                sheet.Cell(2,5).Value=-1;sheet.Cell(3,2).FormulaA1="\"formula\"";
                book.SaveAs(Path.Combine(output,"import-invalid.xlsx"));
            }
            var bad=NhapSanPhamExcel.Doc(Path.Combine(output,"import-invalid.xlsx"),categories,existing);
            foreach(var column in new[]{"MaSP","MaDM","DonGiaBan","TenSP"})True(bad.Loi.Any(x=>x.Contains(column)));
            int before=ScalarInt("SELECT COUNT(*) FROM SanPham");Reject(()=>NhapSanPhamExcel.Luu(bad));
            Reject(()=>new NhapSanPhamRepository().ThemMoi([
                new("Rollback Excel",dm1,null,10,"Cây","Đang bán"),new("Bad category",999999,null,10,"Cây","Đang bán")]));
            Eq(before,ScalarInt("SELECT COUNT(*) FROM SanPham"));
            PhienDangNhap.BatDau(2,nv,"no-import","Employee",VaiTro.NhanVien);
            try { NhapSanPhamExcel.Luu(bad);throw new Exception("Expected permission rejection"); }catch(UnauthorizedAccessException) { }
            try { NhapSanPhamExcel.Doc(path,categories,existing);throw new Exception("Expected permission rejection"); }catch(UnauthorizedAccessException) { }
            PhienDangNhap.BatDau(1,nv,"import-check","Demo",VaiTro.Admin);
            File.WriteAllLines(Path.Combine(output,"import-errors.txt"),bad.Loi);
        });
        Test("DB-X03 import preview form / invalid file disables confirm / valid file enables confirm",()=>
        {
            using var form=new FrmNhapSanPhamExcel();form.Show();WaitUi(form);
            var read=typeof(FrmNhapSanPhamExcel).GetMethod("Doc",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!;
            var save=Controls(form).OfType<Button>().Single(b=>b.Text=="Xác nhận nhập");True(!save.Enabled);
            read.Invoke(form,[Path.Combine(output,"import-invalid.xlsx")]);WaitUi(form);True(!save.Enabled);
            var grid=Controls(form).OfType<DataGridView>().Single();Eq(2,grid.Rows.Count);True(grid.Columns.Contains("Lỗi"));
            using(var book=new XLWorkbook(Path.Combine(output,"import-valid.xlsx")))
            { book.Worksheet("SanPham").Cell(2,2).Value="Chưa ghi xem trước 1";book.Worksheet("SanPham").Cell(3,2).Value="Chưa ghi xem trước 2";book.SaveAs(Path.Combine(output,"import-preview-only.xlsx")); }
            read.Invoke(form,[Path.Combine(output,"import-preview-only.xlsx")]);WaitUi(form);True(save.Enabled);
            Eq(0,ScalarInt("SELECT COUNT(*) FROM SanPham WHERE TenSP LIKE N'Chưa ghi xem trước%'"));
            using var bitmap=new Bitmap(form.Width,form.Height);form.DrawToBitmap(bitmap,form.ClientRectangle);bitmap.Save(Path.Combine(output,"import-preview.png"));form.Close();
        });
        PhienDangNhap.DangXuat();
    }
}
