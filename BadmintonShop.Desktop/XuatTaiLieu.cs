using System.Data;
using System.Globalization;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using ClosedXML.Excel;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace BadmintonShop.Desktop;

public static class XuatTaiLieu
{
    private const string QuyTac="Doanh thu và số bán: chỉ đơn Hoàn thành. Số đơn: tất cả trạng thái. Giảm cấp hóa đơn phân bổ theo tỷ trọng thành tiền, phần dư lớn nhất; tổng khớp đến đồng.";

    // SV4: XLSX OpenXML thật, số giữ kiểu numeric; dữ liệu lấy cùng kết quả đang hiển thị.
    public static void Excel(string path,KetQuaBaoCao kq)
    {
        using var wb=new XLWorkbook();
        foreach(var (ten,dt) in kq.Bang)
        {
            var ws=wb.Worksheets.Add(ten);
            ws.Cell(1,1).Value="BADMINTONSHOP — "+ten;ws.Cell(1,1).Style.Font.Bold=true;
            ws.Cell(2,1).Value=$"{kq.TuNgay:dd/MM/yyyy} – {kq.DenNgay:dd/MM/yyyy}; loại bán: {kq.LoaiBan}";
            ws.Cell(3,1).Value="Tổng doanh thu";ws.Cell(3,2).Value=kq.TongDoanhThu;ws.Cell(3,2).Style.NumberFormat.Format="#,##0";
            ws.Cell(4,1).Value=QuyTac;
            for(int c=0;c<dt.Columns.Count;c++)
            {
                ws.Cell(6,c+1).Value=dt.Columns[c].ColumnName;
                for(int r=0;r<dt.Rows.Count;r++)ws.Cell(r+7,c+1).Value=XLCellValue.FromObject(dt.Rows[r][c]);
                if(dt.Columns[c].DataType!=typeof(string))ws.Column(c+1).Style.NumberFormat.Format="#,##0";
            }
            ws.Range(6,1,6,dt.Columns.Count).Style.Fill.BackgroundColor=XLColor.LightGreen;
            ws.Range(6,1,6,dt.Columns.Count).Style.Font.Bold=true;ws.SheetView.FreezeRows(6);
            ws.Columns(1,dt.Columns.Count).Width=28;ws.Column(1).Width=32;
            ws.Range(4,1,4,Math.Max(2,dt.Columns.Count)).Merge().Style.Alignment.WrapText=true;ws.Row(4).Height=45;
            if(dt.Rows.Count==0)ws.Cell(7,1).Value="Không có dữ liệu trong khoảng lọc.";
        }
        wb.SaveAs(path);
    }

    // SV4: PDF có font Unicode, phân trang và lặp tiêu đề bảng nhờ MigraDoc GDI trên Windows.
    public static void BaoCaoPdf(string path,KetQuaBaoCao kq)
    {
        var doc=TaoDocument();
        foreach(var (ten,dt) in kq.Bang)
        {
            var s=doc.AddSection();CauHinhTrang(s);
            s.AddParagraph("BADMINTONSHOP — "+ten,"Heading1");
            s.AddParagraph($"{kq.TuNgay:dd/MM/yyyy} – {kq.DenNgay:dd/MM/yyyy}; loại bán: {kq.LoaiBan}");
            s.AddParagraph($"Tổng doanh thu: {kq.TongDoanhThu:N0} đồng");s.AddParagraph(QuyTac);ThemBang(s,dt);
        }
        LuuPdf(doc,path);
    }

    // SV4: xuất lại theo giá/giảm lịch sử lưu CSDL; không tính lại khuyến mãi hoặc giảm lần hai.
    public static void HoaDonPdf(string path,HoaDon hd,DataTable chiTiet)
    {
        var doc=TaoDocument();var s=doc.AddSection();CauHinhTrang(s);
        s.AddParagraph($"BADMINTONSHOP — HÓA ĐƠN #{hd.MaHD}","Heading1");
        s.AddParagraph($"Ngày lập: {hd.NgayLap:dd/MM/yyyy HH:mm} | {hd.LoaiBan} | {hd.HinhThucTT} | {hd.TrangThai}");
        s.AddParagraph($"Khách hàng: {(hd.MaKH.HasValue?$"Mã {hd.MaKH}":"Khách vãng lai")} | Nhân viên: {hd.MaNV}");
        if(!string.IsNullOrEmpty(hd.DiaChiGiao))s.AddParagraph("Địa chỉ giao: "+hd.DiaChiGiao);
        if(!string.IsNullOrEmpty(hd.LyDoHuy))s.AddParagraph("Lý do hủy: "+hd.LyDoHuy);
        s.AddParagraph("Giảm giá dòng là phần trăm (%). Thành tiền dòng đã trừ giảm sản phẩm.");
        ThemBang(s,chiTiet);
        var after=chiTiet.AsEnumerable().Sum(r=>r.Field<decimal>("ThanhTien"));
        if(chiTiet.Columns.Contains("DonGia") && chiTiet.Columns.Contains("SoLuong"))
        {
            var before=chiTiet.AsEnumerable().Sum(r=>r.Field<decimal>("DonGia")*r.Field<int>("SoLuong"));
            s.AddParagraph($"Tiền hàng trước giảm: {before:N0} đồng | Giảm sản phẩm: {before-after:N0} đồng");
        }
        s.AddParagraph($"Tiền hàng sau giảm sản phẩm: {after:N0} đồng");
        s.AddParagraph($"Giảm hóa đơn / voucher: {hd.TienGiamGia:N0} đồng | Mã CTKM đã lưu: {hd.MaCTKM}");
        s.AddParagraph($"Tổng thanh toán: {hd.TongTien:N0} đồng","Heading2");
        if(hd.TienKhachDua is decimal tien)s.AddParagraph($"Khách đưa: {tien:N0} đồng | Tiền thừa: {tien-hd.TongTien:N0} đồng");
        s.AddParagraph("Bản in giao dịch phục vụ đồ án; không thay thế hóa đơn điện tử thuế.");LuuPdf(doc,path);
    }
    private static Document TaoDocument()
    {
        var d=new Document();d.Styles["Normal"]!.Font.Name="Arial";d.Styles["Normal"]!.Font.Size=10;
        d.Styles["Normal"]!.ParagraphFormat.SpaceAfter=6;return d;
    }
    private static void CauHinhTrang(Section s)
    {
        s.PageSetup.PageFormat=PageFormat.A4;s.PageSetup.Orientation=MigraDoc.DocumentObjectModel.Orientation.Landscape;
        s.PageSetup.TopMargin=Unit.FromCentimeter(1.5);s.PageSetup.BottomMargin=Unit.FromCentimeter(2);
        s.PageSetup.FooterDistance=Unit.FromCentimeter(0.8);
        s.PageSetup.LeftMargin=Unit.FromCentimeter(1.5);s.PageSetup.RightMargin=Unit.FromCentimeter(1.5);
        var p=s.Footers.Primary.AddParagraph();p.AddText("BadmintonShop | Trang ");p.AddPageField();
    }
    private static void ThemBang(Section s,DataTable dt)
    {
        if(dt.Rows.Count==0) { s.AddParagraph("Không có dữ liệu trong khoảng lọc.");return; }
        var table=s.AddTable();table.Borders.Width=0.4;
        foreach(DataColumn _ in dt.Columns)table.AddColumn(Unit.FromCentimeter(26.5/dt.Columns.Count));
        var head=table.AddRow();head.HeadingFormat=true;head.Shading.Color=Colors.LightGray;head.Format.Font.Bold=true;
        var tenCot=new Dictionary<string,string> { ["MaSP"]="Mã SP",["TenSP"]="Sản phẩm",["SoLuong"]="Số lượng",["DonGia"]="Đơn giá (đ)",["GiamGia"]="Giảm (%)",["ThanhTien"]="Thành tiền (đ)" };
        for(int i=0;i<dt.Columns.Count;i++)head.Cells[i].AddParagraph(tenCot.GetValueOrDefault(dt.Columns[i].ColumnName,dt.Columns[i].ColumnName));
        foreach(DataRow r in dt.Rows)
        {
            var row=table.AddRow();
            for(int i=0;i<dt.Columns.Count;i++)row.Cells[i].AddParagraph(r[i] switch
            { decimal n=>n.ToString(dt.Columns[i].ColumnName=="GiamGia"?"0.##":"N0",CultureInfo.GetCultureInfo("vi-VN")),_=>Convert.ToString(r[i],CultureInfo.GetCultureInfo("vi-VN"))??"" });
        }
    }
    private static void LuuPdf(Document d,string path) { var r=new PdfDocumentRenderer { Document=d };r.RenderDocument();r.PdfDocument.Save(path); }
}
