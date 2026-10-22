using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class FrmDonHang : Form
{
    private DataGridView grid=null!,chiTiet=null!;private DateTimePicker tu=null!,den=null!;
    private ComboBox tt=null!,loai=null!;private TextBox tim=null!;private Label thongTin=null!;
    private const ChucNang Quyen=ChucNang.DonHangPOS;
    public FrmDonHang() { InitializeComponent(); }
    private void InitializeComponent()
    {
        Text="Quản lý đơn hàng";Size=new Size(1250,800);MinimumSize=new Size(1000,650);AutoScaleMode=AutoScaleMode.Font;
        tu=new DateTimePicker { Width=130,Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy",Value=DateTime.Today.AddMonths(-12) };
        den=new DateTimePicker { Width=130,Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy" };tt=Sv4Ui.Chon(["Mọi trạng thái",..QuyTacBanHang.TrangThaiDon]);
        loai=Sv4Ui.Chon("Mọi loại bán","Online","Tại quầy");tim=new TextBox { Width=200,PlaceholderText="Mã đơn / tên / SĐT / email" };
        var bar=Sv4Ui.ThanhCongCu(100);bar.Controls.AddRange([Sv4Ui.Nhan("Từ"),tu,Sv4Ui.Nhan("đến"),den,tt,loai,tim,
            Sv4Ui.Nut("Tải / lọc",()=>Chay(Nap)),Sv4Ui.Nut("Xác nhận",()=>Chuyen("Đã xác nhận")),Sv4Ui.Nut("Giao hàng",()=>Chuyen("Đang giao")),
            Sv4Ui.Nut("Hoàn thành",()=>Chuyen("Hoàn thành")),Sv4Ui.Nut("Hủy + lý do",()=>Chuyen("Đã hủy")),Sv4Ui.Nut("Xuất PDF hóa đơn",()=>Chay(Xuat))]);
        grid=Sv4Ui.Luoi();chiTiet=Sv4Ui.Luoi();thongTin=new Label { Dock=DockStyle.Top,Height=65,Padding=new Padding(6) };
        var split=new SplitContainer { Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,Size=new Size(1200,650),SplitterDistance=320 };
        split.Panel1.Controls.Add(grid);split.Panel2.Controls.Add(chiTiet);split.Panel2.Controls.Add(thongTin);
        Controls.Add(split);Controls.Add(bar);grid.SelectionChanged+=(_,_)=>Chay(ChiTiet);
    }
    protected override void OnLoad(EventArgs e)
    { base.OnLoad(e);if(DesignMode || !QuyenForm.KiemTraKhiMo(this,Quyen))return;Chay(Nap); }
    private void Chay(Action a)=>Sv4Ui.Chay(this,Quyen,a);
    private void Nap() { grid.DataSource=new HoaDonRepository().LayDanhSachHienThi(tu.Value,den.Value,Sv4Ui.Loc(tt),Sv4Ui.Loc(loai),tim.Text);ChiTiet(); }
    private int Ma()=>Sv4Ui.MaDong(grid,"MaHD")??throw new ArgumentException("Hãy chọn một hóa đơn.");
    private void ChiTiet()
    {
        if(Sv4Ui.MaDong(grid,"MaHD") is not int ma) { chiTiet.DataSource=null;thongTin.Text="Không có đơn hàng trong khoảng lọc.";return; }
        chiTiet.DataSource=new HoaDonRepository().LayChiTietHienThi(ma);
        var r=((DataRowView)grid.CurrentRow!.DataBoundItem).Row;
        thongTin.Text=$"HĐ {ma} | {r["TrangThai"]} | Giảm hóa đơn: {r["TienGiamGia"]:N0} đ | Tổng thanh toán: {r["TongTien"]:N0} đ\nLý do hủy: {r["LyDoHuy"]}";
    }
    // SV4: truyền trạng thái đang thấy, không đọc lại rồi ghi đè yêu cầu từ máy khác.
    private void Chuyen(string moi)=>Chay(()=>
    {
        int ma=Ma();string cu=(string)grid.CurrentRow!.Cells["TrangThai"].Value;
        if(!QuyTacBanHang.DuocChuyen(cu,moi))throw new InvalidOperationException("Sai luồng trạng thái. Chỉ hủy khi Chờ xác nhận hoặc Đã xác nhận.");
        string? lyDo=null;
        if(moi=="Đã hủy") { lyDo=Sv4Ui.NhapLyDo(this);if(lyDo==null)return;QuyTacBanHang.BatBuoc(lyDo,500,"Lý do hủy"); }
        if(!Sv4Ui.XacNhan(this,$"Chuyển đơn {ma}: {cu} → {moi}?"))return;
        new HoaDonRepository().ChuyenTrangThai(ma,cu,moi,lyDo);Nap();
    });
    private void Xuat()
    {
        int ma=Ma();using var d=new SaveFileDialog { Filter="PDF|*.pdf",FileName=$"HoaDon_{ma}.pdf" };
        if(d.ShowDialog(this)!=DialogResult.OK)return;
        var repo=new HoaDonRepository();var hd=repo.LayTheoMa(ma)??throw new InvalidOperationException("Không tìm thấy hóa đơn.");
        XuatTaiLieu.HoaDonPdf(d.FileName,hd,repo.LayChiTietHienThi(ma));MessageBox.Show(this,"Đã xuất hóa đơn PDF.");
    }
}
