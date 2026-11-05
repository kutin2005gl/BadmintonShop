using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class FrmDonHang : Form
{
    private DataGridView grid=null!,chiTiet=null!;private DateTimePicker tu=null!,den=null!;
    private ComboBox tt=null!,loai=null!;private TextBox tim=null!;private Label thongTin=null!;
    private int detailVersion;private bool binding;
    private const ChucNang Quyen=ChucNang.DonHangPOS;
    private readonly BoNhoPhien<int,DataTable> details=new(BoNhoPhien<int,int>.ThoiHanMacDinh);
    private readonly BoNhoPhien<(DateTime,DateTime),DataTable> lists=new(BoNhoPhien<int,int>.ThoiHanMacDinh);
    private DataTable? source;
    private Func<DataRow,bool>? advanced;
    private Label filterDescription=null!;
    public FrmDonHang() { InitializeComponent(); }
    private void InitializeComponent()
    {
        Text="Quản lý đơn hàng";Size=new Size(1250,800);MinimumSize=new Size(1000,650);AutoScaleMode=AutoScaleMode.Font;
        tu=new DateTimePicker { Width=130,Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy",Value=DateTime.Today.AddMonths(-12) };
        den=new DateTimePicker { Width=130,Format=DateTimePickerFormat.Custom,CustomFormat="dd/MM/yyyy" };tt=Sv4Ui.Chon(["Mọi trạng thái",..QuyTacBanHang.TrangThaiDon]);
        loai=Sv4Ui.Chon("Mọi loại bán","Online","Tại quầy");tim=new TextBox { Width=200,PlaceholderText="Mã đơn / tên / SĐT / email" };
        filterDescription=Sv4Ui.Nhan("Chưa áp dụng bộ lọc nâng cao");
        var bar=Sv4Ui.ThanhCongCu(140);bar.Controls.AddRange([Sv4Ui.Nhan("Từ"),tu,Sv4Ui.Nhan("đến"),den,tt,loai,tim,
            Sv4Ui.Nut("Tải / lọc",()=>Chay(Nap)),Sv4Ui.Nut("Xác nhận",()=>Chuyen("Đã xác nhận")),Sv4Ui.Nut("Giao hàng",()=>Chuyen("Đang giao")),
            Sv4Ui.Nut("Hoàn thành",()=>Chuyen("Hoàn thành")),Sv4Ui.Nut("Hủy + lý do",()=>Chuyen("Đã hủy")),Sv4Ui.Nut("Xuất PDF hóa đơn",()=>Chay(Xuat)),
            Sv4Ui.Nut("Làm mới",()=>Chay(()=> { lists.Xoa();details.Xoa();Nap(); })),
            Sv4Ui.Nut("Tìm nâng cao",()=>Chay(()=>
            {
                if(source==null)throw new InvalidOperationException("Hãy tải danh sách trước.");
                using var dialog=new FrmLocNangCao(source);
                if(dialog.ShowDialog(this)==DialogResult.OK) { advanced=dialog.KetQua;filterDescription.Text=dialog.MoTa;Nap(); }
            })),Sv4Ui.Nut("Bỏ lọc nâng cao",()=>Chay(()=> { advanced=null;filterDescription.Text="Chưa áp dụng bộ lọc nâng cao";Nap(); })),filterDescription]);
        grid=Sv4Ui.Luoi();chiTiet=Sv4Ui.Luoi();thongTin=new Label { Dock=DockStyle.Top,Height=90,Padding=new Padding(6) };
        var split=new SplitContainer { Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,Size=new Size(1200,650),SplitterDistance=320 };
        split.Panel1.Controls.Add(grid);split.Panel2.Controls.Add(chiTiet);split.Panel2.Controls.Add(thongTin);
        Controls.Add(split);Controls.Add(bar);ShopTheme.Apply(this);grid.CurrentCellChanged+=(_,_)=> { if(!binding)ChiTiet(); };
    }
    protected override void OnLoad(EventArgs e)
    { base.OnLoad(e);if(DesignMode || !QuyenForm.KiemTraKhiMo(this,Quyen))return;Chay(Nap); }
    private void Chay(Action a)=>Sv4Ui.Chay(this,Quyen,a);
    private async void Nap()
    {
        var from=tu.Value.Date;var to=den.Value.Date;var status=Sv4Ui.Loc(tt);var type=Sv4Ui.Loc(loai);var search=tim.Text.Trim();
        var scope=$"{DbHelper.ConnectionString}|{PhienDangNhap.MaTK}|{PhienDangNhap.VaiTro}|{DbHelper.DauHieuDuLieu}";
        await ShopTheme.Load(this,()=>lists.Lay((from,to),scope,()=>new HoaDonRepository().LayDanhSachHienThi(from,to)),rows=>
        {
            source=rows;var filtered=rows.Clone();
            foreach(var row in rows.AsEnumerable().Where(r=>
                (status==null || r.Field<string>("TrangThai")==status) && (type==null || r.Field<string>("LoaiBan")==type) &&
                (search.Length==0 || new[]{"MaHD","KhachHang","SDT","Email"}.Any(c=>Convert.ToString(r[c])!.Contains(search,StringComparison.CurrentCultureIgnoreCase))) &&
                (advanced==null || advanced(r))))filtered.ImportRow(row);
            filtered.ExtendedProperties["HoTroLyDoHuy"]=rows.ExtendedProperties["HoTroLyDoHuy"];
            binding=true;try { grid.DataSource=filtered; }finally { binding=false; }
        },rows=>rows.Rows.Count==0);
        ChiTiet();
    }
    private int Ma()=>Sv4Ui.MaDong(grid,"MaHD")??throw new ArgumentException("Hãy chọn một hóa đơn.");
    private async void ChiTiet()
    {
        if(IsDisposed || Disposing)return;
        if(InvokeRequired) { ShopTheme.OnUi(this,ChiTiet);return; }
        var version=++detailVersion;
        var transitions=new Dictionary<string,string> { ["Xác nhận"]="Đã xác nhận",["Giao hàng"]="Đang giao",["Hoàn thành"]="Hoàn thành",["Hủy + lý do"]="Đã hủy" };
        bool supported=grid.DataSource is DataTable source && source.ExtendedProperties["HoTroLyDoHuy"] is true;
        string status=Convert.ToString(grid.CurrentRow?.Cells["TrangThai"].Value)??"";
        bool online=Convert.ToString(grid.CurrentRow?.Cells["LoaiBan"].Value)=="Online";
        foreach(var button in Controls.OfType<FlowLayoutPanel>().SelectMany(p=>p.Controls.OfType<Button>()))
            if(transitions.TryGetValue(button.Text,out var target))button.Enabled=supported && online && QuyTacBanHang.DuocChuyen(status,target);
        chiTiet.DataSource=null;
        if(Sv4Ui.MaDong(grid,"MaHD") is not int ma) { thongTin.Text="Không có đơn hàng trong khoảng lọc."+(!supported?"\nCSDL chưa hỗ trợ cập nhật trạng thái SV4; hiện chỉ xem / xuất đơn.":"");return; }
        var r=((DataRowView)grid.CurrentRow!.DataBoundItem).Row;
        var description=$"HĐ {ma} | {r["TrangThai"]} | Giảm hóa đơn / voucher: {r["TienGiamGia"]:N0} đ | Tổng thanh toán: {r["TongTien"]:N0} đ\nLý do hủy: {r["LyDoHuy"]}";
        thongTin.Text="Đang tải chi tiết…";
        var scope=$"{DbHelper.ConnectionString}|{PhienDangNhap.MaTK}|{PhienDangNhap.VaiTro}|{DbHelper.DauHieuDuLieu}";
        try
        {
            var detail=await Task.Run(()=>details.Lay(ma,scope,()=>new HoaDonRepository().LayChiTietHienThi(ma)));
            if(IsDisposed || version!=detailVersion)return;
            chiTiet.DataSource=detail;thongTin.Text=description+(grid.DataSource is DataTable table && table.ExtendedProperties["HoTroLyDoHuy"] is false?"\nCSDL chưa hỗ trợ cập nhật trạng thái SV4; hiện chỉ xem / xuất đơn.":"");
        }
        catch(Exception ex) { if(!IsDisposed && version==detailVersion)thongTin.Text=LoiCsdl.ThongBao(ex); }
    }
    // SV4: truyền trạng thái đang thấy, không đọc lại rồi ghi đè yêu cầu từ máy khác.
    private void Chuyen(string moi)=>Chay(()=>
    {
        if(grid.DataSource is DataTable table && table.ExtendedProperties["HoTroLyDoHuy"] is false)
            throw new InvalidOperationException("CSDL thiếu trường lý do hủy theo bản SV4. Chỉ xem/xuất đơn; liên hệ quản trị viên trước khi cập nhật trạng thái.");
        int ma=Ma();string cu=(string)grid.CurrentRow!.Cells["TrangThai"].Value;
        if(!QuyTacBanHang.DuocChuyen(cu,moi))throw new InvalidOperationException("Sai luồng trạng thái. Chỉ hủy khi Chờ xác nhận hoặc Đã xác nhận.");
        string? lyDo=null;
        if(moi=="Đã hủy") { lyDo=Sv4Ui.NhapLyDo(this);if(lyDo==null)return;QuyTacBanHang.BatBuoc(lyDo,500,"Lý do hủy"); }
        if(!Sv4Ui.XacNhan(this,$"Chuyển đơn {ma}: {cu} → {moi}?"))return;
        GhiTrangThai(ma,cu,moi,lyDo);
    });
    private async void GhiTrangThai(int ma,string cu,string moi,string? lyDo)
    {
        var success=await ShopTheme.Load(this,()=> { new HoaDonRepository().ChuyenTrangThai(ma,cu,moi,lyDo);return true; },_=>{});
        if(success)Nap();
    }
    private void Xuat()
    {
        int ma=Ma();using var d=new SaveFileDialog { Filter="PDF|*.pdf",FileName=$"HoaDon_{ma}.pdf" };
        if(d.ShowDialog(this)!=DialogResult.OK)return;
        GhiPdf(d.FileName,ma);
    }
    private async void GhiPdf(string path,int ma)
    {
        var success=await ShopTheme.Load(this,()=> { var repo=new HoaDonRepository();var hd=repo.LayTheoMa(ma)??throw new InvalidOperationException("Không tìm thấy hóa đơn.");XuatTaiLieu.HoaDonPdf(path,hd,repo.LayChiTietHienThi(ma));return true; },_=>{});
        if(success)MessageBox.Show(this,"Đã xuất hóa đơn PDF.");
    }
}
