using System.Data;
using System.Drawing.Drawing2D;

namespace BadmintonShop.Desktop;

// SV4: biểu đồ GDI WinForms .NET 8; >12 dòng gộp phần còn lại, tổng vẫn khớp bảng.
public sealed class BieuDoCot : Control
{
    public DataTable? DuLieu { get; set; }
    public BieuDoCot() { DoubleBuffered=true;BackColor=Color.White;Dock=DockStyle.Fill; }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;
        if(DuLieu==null || DuLieu.Rows.Count==0) { g.DrawString("Không có dữ liệu trong khoảng lọc.",Font,Brushes.DimGray,20,20);return; }
        var dt=DuLieu;int label=dt.Columns.Count>2?1:0;
        // Bảng chéo có hàng tổng với mã NULL; không vẽ lại tổng như một sản phẩm.
        var values=dt.AsEnumerable().Where(r=>!(dt.Columns.Count>2 && r.IsNull(0) && Convert.ToString(r[1])=="Tất cả"))
            .Select(r=>(Ten:Convert.ToString(r[label])??"",GiaTri:Convert.ToDecimal(r[dt.Columns.Count-1]))).ToList();
        if(values.Count==0) { g.DrawString("Không có dữ liệu trong khoảng lọc.",Font,Brushes.DimGray,20,20);return; }
        decimal total=values.Sum(x=>x.GiaTri);
        int capacity=Math.Clamp((Height-50)/24,1,12);
        if(values.Count>capacity)values=[..values.Take(capacity-1),("Các dòng còn lại",values.Skip(capacity-1).Sum(x=>x.GiaTri))];
        g.DrawString($"{dt.Columns[^1].ColumnName} — Tổng: {total:N0} (cùng dữ liệu với bảng)",Font,Brushes.Black,10,8);
        if(Height<65)return;
        decimal max=Math.Max(1,values.Max(x=>x.GiaTri));float row=Math.Max(12,Math.Min(30,(Height-50f)/values.Count));
        float chartWidth=Math.Max(30,Width-340);using var brush=new SolidBrush(ShopTheme.Green);
        for(int i=0;i<values.Count;i++)
        {
            float y=35+i*row;var x=values[i];
            using var format=new StringFormat { Trimming=StringTrimming.EllipsisCharacter,FormatFlags=StringFormatFlags.NoWrap };
            g.DrawString(x.Ten,Font,Brushes.Black,new RectangleF(8,y,190,row),format);
            float width=(float)(x.GiaTri/max)*chartWidth;g.FillRectangle(brush,205,y+2,width,Math.Max(5,row-7));
            g.DrawString(x.GiaTri.ToString("N0"),Font,Brushes.Black,210+width,y);
        }
    }
}
