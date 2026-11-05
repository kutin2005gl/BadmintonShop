using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class FrmThongKe : Form
{
    private DateTimePicker tu=null!,den=null!;private ComboBox loai=null!;private Label tong=null!;private TabControl tabs=null!;
    private KetQuaBaoCao? ketQua;private Label revenue=null!,count=null!,sold=null!;
    private const ChucNang Quyen=ChucNang.ThongKeBinhLuan;
    private readonly BoNhoPhien<(DateTime,DateTime,string?),KetQuaBaoCao> boNho=new(BoNhoPhien<int,int>.ThoiHanMacDinh);
    public FrmThongKe() { InitializeComponent(); }
    private void InitializeComponent()
    {
        Text="Thống kê — báo cáo";Size=new Size(1200,820);MinimumSize=new Size(950,650);AutoScaleMode=AutoScaleMode.Font;
        tu=new DateTimePicker { Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy",Width=130,Value=new DateTime(DateTime.Today.Year,1,1) };
        den=new DateTimePicker { Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy",Width=130 };loai=Sv4Ui.Chon("Tất cả loại bán","Online","Tại quầy");
        tong=new Label { AutoSize=true,Margin=new Padding(8),Font=new Font("Segoe UI",11,FontStyle.Bold) };
        var nam=new NumericUpDown { Minimum=1753,Maximum=9998,Value=DateTime.Today.Year,Width=75 };
        var bar=Sv4Ui.ThanhCongCu(130);bar.Controls.AddRange([Sv4Ui.Nhan("Từ"),tu,Sv4Ui.Nhan("đến"),den,loai,
            Sv4Ui.Nut("Xem báo cáo",()=>Chay(Nap)),Sv4Ui.Nut("Làm mới",()=>Chay(()=> { boNho.Xoa();Nap(); })),Sv4Ui.Nut("Xuất Excel",()=>Chay(()=>Xuat(false))),Sv4Ui.Nut("Xuất PDF",()=>Chay(()=>Xuat(true))),tong,
            Sv4Ui.Nhan("Năm"),nam,Sv4Ui.Nut("Xem cả năm",()=>Chay(()=>
            { tu.Value=new DateTime((int)nam.Value,1,1);den.Value=new DateTime((int)nam.Value,12,31);Nap(); })),
            Sv4Ui.Nhan("Đơn Hoàn thành; ngày lập trong khoảng lọc; đơn vị: đồng. Thu–chi (tiền nhập hàng): chọn Tất cả loại bán.")]);
        tabs=new TabControl { Dock=DockStyle.Fill };
        var cards=new TableLayoutPanel { Dock=DockStyle.Top,Height=100,ColumnCount=3,Padding=new Padding(8) };
        revenue=ShopTheme.Title("—",18);count=ShopTheme.Title("—",18);sold=ShopTheme.Title("—",18);
        var fields=new[]{("DOANH THU HOÀN THÀNH",revenue),("TỔNG SỐ ĐƠN",count),("SỐ SẢN PHẨM ĐÃ BÁN",sold)};
        for(int i=0;i<3;i++) { cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.33f));var card=new Panel { Dock=DockStyle.Fill,BackColor=Color.White,Padding=new Padding(10),Margin=new Padding(4) };fields[i].Item2.Dock=DockStyle.Fill;card.Controls.Add(fields[i].Item2);card.Controls.Add(new Label { Text=fields[i].Item1,Dock=DockStyle.Top,Height=25,ForeColor=ShopTheme.Muted });cards.Controls.Add(card,i,0); }
        Controls.Add(tabs);Controls.Add(cards);Controls.Add(bar);ShopTheme.Apply(this);
    }
    protected override void OnLoad(EventArgs e)
    { base.OnLoad(e);if(DesignMode || !QuyenForm.KiemTraKhiMo(this,Quyen))return;Chay(Nap); }
    private void Chay(Action a)=>Sv4Ui.Chay(this,Quyen,a);
    // SV4: nạp thành công mới thay kết quả, bảng và biểu đồ cùng dùng snapshot này.
    private async void Nap()
    {
        var from=tu.Value.Date;var to=den.Value.Date;var saleType=Sv4Ui.Loc(loai);
        var scope=$"{DbHelper.ConnectionString}|{PhienDangNhap.MaTK}|{PhienDangNhap.VaiTro}|{DbHelper.DauHieuDuLieu}";
        await ShopTheme.Load(this,()=>boNho.Lay((from,to,saleType),scope,
            ()=>BaoCaoService.Tao(new BaoCaoRepository().LayDuLieuBaoCao(from,to,saleType),from,to,saleType)),kq=>
        {
        revenue.Text=$"{kq.TongDoanhThu:N0} đ";
        count.Text=kq.Bang["Trạng thái đơn"].AsEnumerable().Sum(r=>r.Field<int>("Số đơn")).ToString("N0");
        sold.Text=kq.Bang["Bán chạy"].AsEnumerable().Sum(r=>r.Field<long>("Số lượng")).ToString("N0");
        ketQua=kq;while(tabs.TabPages.Count>0) { var old=tabs.TabPages[0];tabs.TabPages.RemoveAt(0);old.Dispose(); }
        foreach(var (ten,dt) in kq.Bang)
        {
            if(ten.StartsWith("Thu–chi ",StringComparison.Ordinal))
            {
                // Các chỉ tiêu thu, chi, chênh lệch không được cộng thành một tổng trên biểu đồ.
                var chiPage=new TabPage(ten);var chiGrid=Sv4Ui.Luoi();chiGrid.DataSource=dt;
                chiPage.Controls.Add(chiGrid);tabs.TabPages.Add(chiPage);continue;
            }
            var page=new TabPage(ten);var split=new SplitContainer { Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,Size=new Size(1100,600),SplitterDistance=290 };
            var grid=Sv4Ui.Luoi();grid.DataSource=dt;split.Panel1.Controls.Add(grid);split.Panel2.Controls.Add(new BieuDoCot { DuLieu=dt });
            page.Controls.Add(split);tabs.TabPages.Add(page);
        }
        tong.Text=$"{kq.TuNgay:dd/MM/yyyy}–{kq.DenNgay:dd/MM/yyyy} | {kq.LoaiBan} | Doanh thu: {kq.TongDoanhThu:N0} đ";
        },kq=>kq.Bang["Theo ngày"].Rows.Count==0 && !kq.Bang
            .Where(b=>b.Key.StartsWith("Thu–chi ",StringComparison.Ordinal))
            .Any(b=>b.Value.Rows[1].Field<decimal>("Tổng")!=0));
    }
    private void Xuat(bool pdf)
    {
        var kq=ketQua??throw new InvalidOperationException("Hãy xem báo cáo trước.");
        using var d=new SaveFileDialog { Filter=pdf?"PDF|*.pdf":"Excel|*.xlsx",FileName=$"BaoCao_{kq.TuNgay:yyyyMMdd}_{kq.DenNgay:yyyyMMdd}."+(pdf?"pdf":"xlsx") };
        if(d.ShowDialog(this)!=DialogResult.OK)return;
        GhiBaoCao(d.FileName,kq,pdf);
    }
    private async void GhiBaoCao(string path,KetQuaBaoCao kq,bool pdf)
    {
        var success=await ShopTheme.Load(this,()=> { if(pdf)XuatTaiLieu.BaoCaoPdf(path,kq);else XuatTaiLieu.Excel(path,kq);return true; },_=>{});
        if(success)MessageBox.Show(this,"Đã xuất kết quả đang hiển thị, gồm bộ lọc và quy tắc thống kê.");
    }
}
