using System.Data;
using System.Text.Json;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;
using ClosedXML.Excel;
using PdfSharp.Pdf.IO;

internal static partial class Program
{
    private static void ExportExpected(string name,KetQuaBaoCao report)
    {
        var tables=report.Bang.Select(pair=>new { Name=pair.Key,
            Columns=pair.Value.Columns.Cast<DataColumn>().Select(c=>c.ColumnName).ToArray(),
            Rows=pair.Value.Rows.Cast<DataRow>().Select(r=>r.ItemArray.Select(v=>v==DBNull.Value?null:v).ToArray()).ToArray() });
        File.WriteAllText(Path.Combine(output,name+"-expected.json"),JsonSerializer.Serialize(tables));
    }
    private static void ExportProgressChecks()
    {
        Test("X03 wide year pivots split into readable sections / XLSX all years retained",()=>
        {
            var table=Source();
            for(int year=2005;year<=2026;year++)
                table.Rows.Add(year,new DateTime(year,1,1),"Online","Hoàn thành",123456789012m,1,"Vợt thử",1,"Vợt",1,123456789012m,
                    1,"Nhân viên tên dài Nguyễn Thị Minh Anh kiểm tra xuống dòng",1,"Khách hàng tên dài Trần Hoàng Nam kiểm tra xuống dòng");
            var report=BaoCaoService.Tao(table,new DateTime(2005,1,1),new DateTime(2026,12,31),null);
            report=report with { Bang=report.Bang.Where(pair=>pair.Key is "Nhân viên × năm" or "Khách hàng × năm").ToDictionary() };
            var prefix=Path.Combine(output,"wide-pivots");XuatTaiLieu.Excel(prefix+".xlsx",report);XuatTaiLieu.BaoCaoPdf(prefix+".pdf",report);
            using(var pdf=PdfReader.Open(prefix+".pdf",PdfDocumentOpenMode.Import))Eq(16,pdf.PageCount);
            using(var book=new XLWorkbook(prefix+".xlsx"))foreach(var sheet in book.Worksheets)Eq(25,sheet.LastColumnUsed()!.ColumnNumber());
            ExportExpected("wide-pivots",report);
        });
    }
}
