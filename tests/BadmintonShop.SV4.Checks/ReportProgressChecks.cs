using System.Data;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using PdfSharp.Pdf.IO;

internal static partial class Program
{
    private static DuLieuBaoCao ReportProgressFixture()
    {
        var t=Source();
        void Dong(int hd,DateTime ngay,decimal net,int sp,decimal tien,string trangThai="Hoàn thành",string loai="Online",int? nv=null,int? kh=null)
            =>t.Rows.Add(hd,ngay,loai,trangThai,net,sp,$"Sản phẩm {sp}",sp==2?2:1,sp==2?"Cầu":"Vợt",1,tien,
                (object?)nv??DBNull.Value,nv.HasValue?$"Nhân viên {nv}":DBNull.Value,
                (object?)kh??DBNull.Value,kh.HasValue?$"Khách {kh}":DBNull.Value);
        // Đáp án ghi tay: HD1 chia 101 theo 100:100 -> SP1=51, SP2=50.
        Dong(1,new(2025,1,1),101,1,100,nv:1,kh:1);
        Dong(1,new(2025,1,1),101,2,100,nv:1,kh:1);
        Dong(2,new(2025,3,31,23,59,59),60,1,60,loai:"Tại quầy",nv:1);
        Dong(3,new(2025,4,1),90,2,90,kh:1);
        Dong(4,new(2025,7,1),120,1,120,nv:2,kh:2);
        Dong(5,new(2025,12,31,23,59,59),40,2,40,nv:2,kh:2);
        Dong(6,new(2026,1,1),70,1,70,nv:1,kh:1);
        Dong(7,new(2026,7,1),30,2,30);
        Dong(8,new(2026,4,1),999,3,999,"Đã hủy");
        Dong(9,new(2026,4,1),888,3,888,"Chờ xác nhận");
        Dong(10,new(2027,1,1),500,1,500);
        var chi=new DataTable();chi.Columns.Add("MaPN",typeof(int));chi.Columns.Add("NgayNhap",typeof(DateTime));chi.Columns.Add("TongTien",typeof(decimal));
        chi.Rows.Add(1,new DateTime(2025,3,31,23,59,59),200m);
        chi.Rows.Add(2,new DateTime(2025,4,1),300m);
        chi.Rows.Add(3,new DateTime(2026,12,31,23,59,59),150m);
        chi.Rows.Add(4,new DateTime(2027,1,1),999m);
        return new DuLieuBaoCao(t,chi);
    }

    private static void ReportProgressChecks()
    {
        var tu=new DateTime(2025,1,1);var den=new DateTime(2026,12,31);
        Test("R01 product quarters / two years / empty quarters / end date / canceled",()=>
        {
            var k=BaoCaoService.Tao(ReportProgressFixture(),tu,den,null);Eq(511m,k.TongDoanhThu);
            var a=k.Bang["Sản phẩm × quý 2025"];var b=k.Bang["Sản phẩm × quý 2026"];
            Eq(511m,k.Bang["Theo quý"].AsEnumerable().Sum(r=>r.Field<decimal>("Doanh thu")));
            var sp1=a.AsEnumerable().Single(r=>r.Field<int?>("Mã SP")==1);
            Eq(111m,sp1.Field<decimal>("Q1"));Eq(0m,sp1.Field<decimal>("Q2"));Eq(120m,sp1.Field<decimal>("Q3"));Eq(231m,sp1.Field<decimal>("Tổng"));
            var sp2=a.AsEnumerable().Single(r=>r.Field<int?>("Mã SP")==2);
            Eq(50m,sp2.Field<decimal>("Q1"));Eq(90m,sp2.Field<decimal>("Q2"));Eq(40m,sp2.Field<decimal>("Q4"));Eq(180m,sp2.Field<decimal>("Tổng"));
            Eq(411m,a.Rows[^1].Field<decimal>("Tổng"));Eq(100m,b.Rows[^1].Field<decimal>("Tổng"));
            Eq(0m,b.AsEnumerable().Single(r=>r.Field<int?>("Mã SP")==3).Field<decimal>("Tổng"));
            foreach(var dt in new[]{a,b})foreach(DataRow row in dt.Rows)
                Eq(row.Field<decimal>("Tổng"),Enumerable.Range(1,4).Sum(q=>row.Field<decimal>($"Q{q}")));
        });
        Test("R02 employee / customer years / unassigned groups / no duplicated invoice",()=>
        {
            var k=BaoCaoService.Tao(ReportProgressFixture(),tu,den,null);
            var nv=k.Bang["Nhân viên × năm"];var kh=k.Bang["Khách hàng × năm"];
            Eq(231m,nv.AsEnumerable().Single(r=>r.Field<int?>("Mã")==1).Field<decimal>("Tổng"));
            Eq(120m,nv.AsEnumerable().Single(r=>r.Field<string>("Tên")=="Chưa gán nhân viên").Field<decimal>("Tổng"));
            Eq(90m,kh.AsEnumerable().Single(r=>r.Field<string>("Tên")=="Khách vãng lai").Field<decimal>("Tổng"));
            foreach(var dt in new[]{nv,kh})
            { Eq(411m,dt.Rows[^1].Field<decimal>("2025"));Eq(100m,dt.Rows[^1].Field<decimal>("2026"));Eq(511m,dt.Rows[^1].Field<decimal>("Tổng")); }
        });
        Test("R03 purchase cash out / separate datasets / negative difference / channel scope",()=>
        {
            var source=ReportProgressFixture();var k=BaoCaoService.Tao(source,tu,den,null);
            var a=k.Bang["Thu–chi 2025"];var b=k.Bang["Thu–chi 2026"];
            Eq(411m,a.Rows[0].Field<decimal>("Tổng"));Eq(500m,a.Rows[1].Field<decimal>("Tổng"));Eq(-89m,a.Rows[2].Field<decimal>("Tổng"));
            Eq(200m,a.Rows[1].Field<decimal>("Q1"));Eq(300m,a.Rows[1].Field<decimal>("Q2"));
            Eq(150m,b.Rows[1].Field<decimal>("Q4"));Eq(-50m,b.Rows[2].Field<decimal>("Tổng"));
            var online=BaoCaoService.Tao(source,tu,den,"Online");Eq(451m,online.TongDoanhThu);
            True(!online.Bang.Keys.Any(n=>n.StartsWith("Thu–chi ")));
        });
        Test("R04 date intersection / reversed range / empty year",()=>
        {
            var source=ReportProgressFixture();var from=new DateTime(2025,4,1);var to=new DateTime(2025,6,30);
            var k=BaoCaoService.Tao(source,from,to,null);Eq(90m,k.TongDoanhThu);
            var a=k.Bang["Sản phẩm × quý 2025"];Eq(0m,a.Rows[^1].Field<decimal>("Q1"));Eq(90m,a.Rows[^1].Field<decimal>("Q2"));
            Reject(()=>BaoCaoService.Tao(source,to,from,null));
            var empty=BaoCaoService.Tao(source,new DateTime(2024,1,1),new DateTime(2024,12,31),null);
            Eq(0m,empty.TongDoanhThu);Eq(0m,empty.Bang["Sản phẩm × quý 2024"].Rows[^1].Field<decimal>("Tổng"));
            Eq(0m,empty.Bang["Thu–chi 2024"].Rows[1].Field<decimal>("Tổng"));
            var chiOnly=BaoCaoService.Tao(source,new DateTime(2026,12,1),new DateTime(2026,12,31),null);
            Eq(0m,chiOnly.TongDoanhThu);Eq(-150m,chiOnly.Bang["Thu–chi 2026"].Rows[2].Field<decimal>("Tổng"));
        });
        Test("R05 allocation shared across products and categories",()=>
        {
            // 3 dòng bằng nhau, 2 danh mục: phân bổ SP (1,0,0), không chia lần nữa theo DM (0,1).
            var t=Source();
            for(int sp=1;sp<=3;sp++)t.Rows.Add(1,tu,"Online","Hoàn thành",1m,sp,$"SP{sp}",sp==1?1:2,$"DM{(sp==1?1:2)}",1,1m);
            var k=BaoCaoService.Tao(t,tu,tu,null);
            Eq(1m,k.Bang["Theo danh mục"].AsEnumerable().Single(r=>r.Field<int>("Mã DM")==1).Field<decimal>("Doanh thu"));
            Eq(1m,k.Bang["Sản phẩm × quý 2025"].AsEnumerable().Single(r=>r.Field<int?>("Mã SP")==1).Field<decimal>("Q1"));
        });
        Test("R06 new reports XLSX numeric values and PDF reopen",()=>
        {
            var k=BaoCaoService.Tao(ReportProgressFixture(),tu,den,null);
            var xlsx=Path.Combine(output,"progress-reports.xlsx");XuatTaiLieu.Excel(xlsx,k);
            using(var wb=new XLWorkbook(xlsx))
            {
                Eq(k.Bang.Count,wb.Worksheets.Count);
                foreach(var (name,dt) in k.Bang)
                    for(int r=0;r<dt.Rows.Count;r++)for(int c=0;c<dt.Columns.Count;c++)
                        if(dt.Rows[r][c] is decimal expected)Eq(expected,wb.Worksheet(name).Cell(r+7,c+1).GetValue<decimal>());
            }
            using(var doc=SpreadsheetDocument.Open(xlsx,false))True(!new OpenXmlValidator().Validate(doc).Any());
            var pdf=Path.Combine(output,"progress-reports.pdf");XuatTaiLieu.BaoCaoPdf(pdf,k);
            using var p=PdfReader.Open(pdf,PdfDocumentOpenMode.Import);True(p.PageCount>=k.Bang.Count);
        });
        Test("R07 chart includes unassigned group and handles empty total row",()=>
        {
            var k=BaoCaoService.Tao(ReportProgressFixture(),tu,den,null);
            foreach(var name in new[]{"Nhân viên × năm","Khách hàng × năm","Sản phẩm × quý 2025"})
            { using var chart=new BieuDoCot { DuLieu=k.Bang[name],Size=new Size(1000,300) };using var bitmap=new Bitmap(1000,300);chart.DrawToBitmap(bitmap,chart.ClientRectangle); }
            var empty=BaoCaoService.Tao(Source(),tu,tu,null);
            using var c=new BieuDoCot { DuLieu=empty.Bang["Sản phẩm × quý 2025"],Size=new Size(1000,300) };
            using var b=new Bitmap(1000,300);c.DrawToBitmap(b,c.ClientRectangle);
        });
    }
}
