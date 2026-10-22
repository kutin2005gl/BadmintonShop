using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class FrmThongKe : Form
{
    private DateTimePicker tu=null!,den=null!;private ComboBox loai=null!;private Label tong=null!;private TabControl tabs=null!;
    private KetQuaBaoCao? ketQua;
    private const ChucNang Quyen=ChucNang.ThongKeBinhLuan;
    public FrmThongKe() { InitializeComponent(); }
    private void InitializeComponent()
    {
        Text="Thống kê — báo cáo";Size=new Size(1200,820);MinimumSize=new Size(950,650);AutoScaleMode=AutoScaleMode.Font;
        tu=new DateTimePicker { Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy",Width=130,Value=new DateTime(DateTime.Today.Year,1,1) };
        den=new DateTimePicker { Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy",Width=130 };loai=Sv4Ui.Chon("Tất cả loại bán","Online","Tại quầy");
        tong=new Label { AutoSize=true,Margin=new Padding(8),Font=new Font("Segoe UI",11,FontStyle.Bold) };
        var bar=Sv4Ui.ThanhCongCu(130);bar.Controls.AddRange([Sv4Ui.Nhan("Từ"),tu,Sv4Ui.Nhan("đến"),den,loai,
            Sv4Ui.Nut("Xem báo cáo",()=>Chay(Nap)),Sv4Ui.Nut("Xuất Excel",()=>Chay(()=>Xuat(false))),Sv4Ui.Nut("Xuất PDF",()=>Chay(()=>Xuat(true))),tong,
            Sv4Ui.Nhan("Chỉ đơn Hoàn thành tính doanh thu/số bán. Số đơn đếm mọi trạng thái. Sau đổi bộ lọc, bấm Xem báo cáo.")]);
        tabs=new TabControl { Dock=DockStyle.Fill };Controls.Add(tabs);Controls.Add(bar);
    }
    protected override void OnLoad(EventArgs e)
    { base.OnLoad(e);if(DesignMode || !QuyenForm.KiemTraKhiMo(this,Quyen))return;Chay(Nap); }
    private void Chay(Action a)=>Sv4Ui.Chay(this,Quyen,a);
    // SV4: nạp thành công mới thay kết quả, bảng và biểu đồ cùng dùng snapshot này.
    private void Nap()
    {
        var kq=BaoCaoService.Tao(new BaoCaoRepository().LayDuLieu(tu.Value,den.Value,Sv4Ui.Loc(loai)),tu.Value,den.Value,Sv4Ui.Loc(loai));
        ketQua=kq;while(tabs.TabPages.Count>0) { var old=tabs.TabPages[0];tabs.TabPages.RemoveAt(0);old.Dispose(); }
        foreach(var (ten,dt) in kq.Bang)
        {
            var page=new TabPage(ten);var split=new SplitContainer { Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,Size=new Size(1100,600),SplitterDistance=290 };
            var grid=Sv4Ui.Luoi();grid.DataSource=dt;split.Panel1.Controls.Add(grid);split.Panel2.Controls.Add(new BieuDoCot { DuLieu=dt });
            page.Controls.Add(split);tabs.TabPages.Add(page);
        }
        tong.Text=$"{kq.TuNgay:dd/MM/yyyy}–{kq.DenNgay:dd/MM/yyyy} | {kq.LoaiBan} | Doanh thu: {kq.TongDoanhThu:N0} đ";
    }
    private void Xuat(bool pdf)
    {
        var kq=ketQua??throw new InvalidOperationException("Hãy xem báo cáo trước.");
        using var d=new SaveFileDialog { Filter=pdf?"PDF|*.pdf":"Excel|*.xlsx",FileName=$"BaoCao_{kq.TuNgay:yyyyMMdd}_{kq.DenNgay:yyyyMMdd}."+(pdf?"pdf":"xlsx") };
        if(d.ShowDialog(this)!=DialogResult.OK)return;
        if(pdf)XuatTaiLieu.BaoCaoPdf(d.FileName,kq);else XuatTaiLieu.Excel(d.FileName,kq);
        MessageBox.Show(this,"Đã xuất kết quả đang hiển thị, gồm bộ lọc và quy tắc thống kê.");
    }
}
