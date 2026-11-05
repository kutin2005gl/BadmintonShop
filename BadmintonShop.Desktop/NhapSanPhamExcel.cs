using System.Data;
using System.Globalization;
using System.IO.Compression;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using ClosedXML.Excel;

namespace BadmintonShop.Desktop;

public sealed record XemTruocNhap(IReadOnlyList<SanPhamNhap> SanPham,DataTable Bang,IReadOnlyList<string> Loi);

public static class NhapSanPhamExcel
{
    private static readonly string[] Headers=["MaSP","TenSP","MaDM","ThuongHieu","DonGiaBan","DonViTinh","TrangThai"];
    private static void Quyen()
    { if(!PhienDangNhap.CoQuyen(ChucNang.SanPhamDanhMucKhuyenMai))throw new UnauthorizedAccessException("Chỉ quản trị viên / quản lý được nhập sản phẩm."); }
    public static void TaoMau(string path,DataTable categories)
    {
        Quyen();using var book=new XLWorkbook();var sheet=book.AddWorksheet("SanPham");
        for(int i=0;i<Headers.Length;i++)sheet.Cell(1,i+1).Value=Headers[i];
        sheet.Row(1).Style.Font.Bold=true;sheet.Columns(1,7).Width=24;sheet.SheetView.FreezeRows(1);
        var help=book.AddWorksheet("HuongDan");
        help.Cell(1,1).Value="Chỉ thêm mới; MaSP để trống, mã tự sinh. Không nhập tồn kho. Tối đa 1000 dòng.";
        help.Cell(2,1).Value="TenSP, MaDM, DonGiaBan (số nguyên đồng), DonViTinh, TrangThai là bắt buộc. Không dùng công thức.";
        help.Cell(3,1).Value="TrangThai: Đang bán / Ngừng bán. Trùng tên + danh mục + thương hiệu: báo lỗi, không ghi đè.";
        help.Cell(4,1).Value="Lỗi bất kỳ: không ghi dòng nào. Danh mục hợp lệ ở bảng dưới; có thể thay đổi trước lúc lưu.";
        help.Cell(6,1).InsertTable(categories);help.Column(1).Width=110;help.Column(2).Width=35;
        book.SaveAs(path);
    }
    public static XemTruocNhap Doc(string path,DataTable categories,DataTable existing)
    {
        Quyen();var info=new FileInfo(path);
        if(!info.Extension.Equals(".xlsx",StringComparison.OrdinalIgnoreCase) || info.Length>5*1024*1024)throw new ArgumentException("Chọn file .xlsx không quá 5 MB.");
        using(var archive=ZipFile.OpenRead(path))
            if(archive.Entries.Sum(e=>e.Length)>30*1024*1024)throw new ArgumentException("Nội dung Excel giải nén quá lớn.");
        using var book=new XLWorkbook(path);
        if(!book.Worksheets.TryGetWorksheet("SanPham",out var sheet))throw new ArgumentException("Thiếu trang SanPham. Hãy tải mẫu.");
        for(int c=0;c<Headers.Length;c++)if(sheet.Cell(1,c+1).GetString()!=Headers[c])throw new ArgumentException($"Cột {c+1} phải là {Headers[c]}.");
        int end=sheet.LastRowUsed()?.RowNumber()??1;
        if(end>1001 || (sheet.LastColumnUsed()?.ColumnNumber()??0)>Headers.Length)throw new ArgumentException("Tối đa 1000 dòng và đúng 7 cột mẫu; không nhập cột tồn kho.");
        var errors=new List<string>();var rows=new List<SanPhamNhap>();var preview=new DataTable();preview.Columns.Add("Dòng",typeof(int));
        foreach(var name in Headers)preview.Columns.Add(name);preview.Columns.Add("Lỗi");
        var categoryIds=categories.AsEnumerable().Select(r=>r.Field<int>("MaDM")).ToHashSet();
        var keys=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string Key(string name,int category,string? brand)=>$"{category}|{name.Trim()}|{brand?.Trim()}";
        foreach(DataRow row in existing.Rows)keys.Add(Key(row.Field<string>("TenSP")!,row.Field<int>("MaDM"),row.Field<string?>("ThuongHieu")));
        for(int r=2;r<=end;r++)
        {
            var values=Enumerable.Range(1,7).Select(c=>sheet.Cell(r,c).GetString().Trim()).ToArray();
            if(values.All(string.IsNullOrEmpty))continue;
            var lineErrors=new List<string>();
            void Error(string column,string message)=>lineErrors.Add($"Dòng {r}, {column}: {message}");
            for(int c=1;c<=7;c++)if(sheet.Cell(r,c).HasFormula)Error(Headers[c-1],"không chấp nhận công thức");
            if(values[0].Length!=0)Error("MaSP","phải để trống; chỉ thêm mới, không ghi đè mã đã có");
            if(values[1].Length is <1 or >150)Error("TenSP","bắt buộc, tối đa 150 ký tự");
            if(!int.TryParse(values[2],out int category) || !categoryIds.Contains(category))Error("MaDM","mã danh mục không tồn tại");
            if(values[3].Length>50)Error("ThuongHieu","tối đa 50 ký tự");
            decimal price=0;
            var priceCell=sheet.Cell(r,5);
            bool validPrice=priceCell.DataType==XLDataType.Number?priceCell.TryGetValue(out price):
                decimal.TryParse(values[4],NumberStyles.Number,CultureInfo.GetCultureInfo("vi-VN"),out price);
            if(!validPrice || price<0 || price!=decimal.Truncate(price) || price>999999999999999999m)Error("DonGiaBan","phải là số nguyên đồng không âm, tối đa 18 chữ số");
            if(values[5].Length is <1 or >20)Error("DonViTinh","bắt buộc, tối đa 20 ký tự");
            if(values[6] is not ("Đang bán" or "Ngừng bán"))Error("TrangThai","chọn Đang bán hoặc Ngừng bán");
            if(!keys.Add(Key(values[1],category,values[3])))Error("TenSP/MaDM/ThuongHieu","trùng trong file hoặc danh sách hiện có");
            errors.AddRange(lineErrors);
            preview.Rows.Add(new object[]{r}.Concat(values.Cast<object>()).Append(string.Join("; ",lineErrors)).ToArray());
            if(lineErrors.Count==0)rows.Add(new(values[1],category,string.IsNullOrEmpty(values[3])?null:values[3],price,values[5],values[6]));
        }
        if(preview.Rows.Count==0)errors.Add("File chưa có sản phẩm.");
        return new(rows.AsReadOnly(),preview,errors.AsReadOnly());
    }
    public static int Luu(XemTruocNhap preview)
    {
        Quyen();if(preview.Loi.Count>0)throw new ArgumentException("File còn lỗi; không ghi dữ liệu.");
        return new NhapSanPhamRepository().ThemMoi(preview.SanPham);
    }
}
