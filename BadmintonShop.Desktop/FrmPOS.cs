using System.ComponentModel;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class DongPOS
{
    public int MaSP { get; set; }
    public string TenSP { get; set; }="";
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien=>SoLuong*DonGia;
}

public class FrmPOS : Form
{
    private DataGridView sanPham=null!,gioLuoi=null!;private TextBox tim=null!;
    private NumericUpDown sl=null!,tien=null!;private Label tong=null!,ketQua=null!;private Button luu=null!;
    private readonly BindingList<DongPOS> gio=new();private Guid yeuCau=Guid.NewGuid();private bool dangLuu;private int? maDaLuu;
    private const ChucNang Quyen=ChucNang.DonHangPOS;
    public FrmPOS() { InitializeComponent(); }
    private void InitializeComponent()
    {
        Text="POS — bán tại quầy / khách vãng lai";Size=new Size(1200,820);MinimumSize=new Size(1000,700);AutoScaleMode=AutoScaleMode.Font;
        sanPham=Sv4Ui.Luoi();gioLuoi=Sv4Ui.Luoi();gioLuoi.DataSource=gio;
        tim=new TextBox { Width=200,PlaceholderText="Tên hoặc thương hiệu" };sl=new NumericUpDown { Minimum=1,Maximum=1000000,Value=1,Width=90 };
        tien=new NumericUpDown { Maximum=999999999999999999m,ThousandsSeparator=true,Width=180 };
        tong=new Label { AutoSize=true,Font=new Font("Segoe UI",12,FontStyle.Bold),Margin=new Padding(6) };
        ketQua=new Label { Dock=DockStyle.Bottom,Height=55,Padding=new Padding(5) };
        var bar=Sv4Ui.ThanhCongCu(110);bar.Controls.AddRange([tim,Sv4Ui.Nut("Tìm / tải giá mới",()=>Chay(Nap)),Sv4Ui.Nhan("Số lượng"),sl,
            Sv4Ui.Nut("Thêm vào giỏ",()=>Chay(Them)),Sv4Ui.Nut("Sửa số lượng giỏ",()=>Chay(Sua)),Sv4Ui.Nut("Bỏ dòng giỏ",()=>Chay(Bo)),
            Sv4Ui.Nhan("Khuyến mãi SV3 chưa sẵn sàng — POS bán theo giá thường, không áp voucher.")]);
        var bottom=Sv4Ui.ThanhCongCu(110);bottom.Dock=DockStyle.Bottom;luu=Sv4Ui.Nut("Thanh toán tiền mặt",()=>Chay(ThanhToan));
        bottom.Controls.AddRange([tong,Sv4Ui.Nhan("Khách đưa (đồng)"),tien,luu,Sv4Ui.Nut("Xuất lại PDF",()=>Chay(Xuat)),
            Sv4Ui.Nut("Giỏ mới",()=>Chay(Moi))]);
        var split=new SplitContainer { Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,Size=new Size(1100,550),SplitterDistance=270 };
        split.Panel1.Controls.Add(sanPham);split.Panel2.Controls.Add(gioLuoi);Controls.Add(split);Controls.Add(bar);Controls.Add(bottom);Controls.Add(ketQua);
        tien.ValueChanged+=(_,_)=>Tinh();gio.ListChanged+=(_,_)=>Tinh();Tinh();
    }
    protected override void OnLoad(EventArgs e)
    { base.OnLoad(e);if(DesignMode || !QuyenForm.KiemTraKhiMo(this,Quyen))return;Chay(Nap); }
    private void Chay(Action a)=>Sv4Ui.Chay(this,Quyen,a);
    private void Nap()
    {
        sanPham.DataSource=new SanPhamRepository().LayTatCa(tim.Text).Where(x=>x.TrangThai=="Đang bán").ToList();
        foreach(var cot in new[]{"MaDM","HinhAnh","MoTa"})sanPham.Columns[cot].Visible=false;
    }
    private void Them()
    {
        if(sanPham.CurrentRow?.DataBoundItem is not SanPham s)throw new ArgumentException("Chọn sản phẩm ở lưới trên.");
        var cu=gio.FirstOrDefault(x=>x.MaSP==s.MaSP);int so=checked((cu?.SoLuong??0)+(int)sl.Value);
        if(so>s.SoLuongTon)throw new ArgumentException("Không đủ tồn kho theo lần tải gần nhất.");
        if(cu==null)gio.Add(new DongPOS { MaSP=s.MaSP,TenSP=s.TenSP,SoLuong=so,DonGia=s.DonGiaBan });
        else { cu.SoLuong=so;gio.ResetBindings(); }
    }
    private void Sua()
    {
        if(gioLuoi.CurrentRow?.DataBoundItem is not DongPOS d)throw new ArgumentException("Chọn dòng giỏ ở lưới dưới.");
        d.SoLuong=(int)sl.Value;gio.ResetBindings();
    }
    private void Bo() { if(gioLuoi.CurrentRow?.DataBoundItem is DongPOS d)gio.Remove(d); }
    private void Tinh()
    {
        var total=gio.Sum(x=>x.ThanhTien);tong.Text=$"Tổng: {total:N0} đ | "+(tien.Value>=total?$"Thừa: {tien.Value-total:N0} đ":$"Thiếu: {total-tien.Value:N0} đ");
    }
    // SV4: chỉ thay token sau giao dịch chắc chắn thành công, retry cùng token không tạo thêm hóa đơn.
    private void ThanhToan()
    {
        if(dangLuu)return;
        if(gio.Count==0)throw new ArgumentException("Giỏ hàng đang trống.");
        if(tien.Value<gio.Sum(x=>x.ThanhTien))throw new ArgumentException("Khách chưa đưa đủ tiền.");
        if(!Sv4Ui.XacNhan(this,$"Lưu hóa đơn và thu {gio.Sum(x=>x.ThanhTien):N0} đồng?"))return;
        dangLuu=true;luu.Enabled=false;
        try
        {
            var rows=gio.Select(x=>new ChiTietHoaDon { MaSP=x.MaSP,SoLuong=x.SoLuong,DonGia=x.DonGia }).ToList();
            maDaLuu=new HoaDonRepository().ThanhToanPOS(PhienDangNhap.MaNV,rows,tien.Value,yeuCau);
            ketQua.Text=$"Đã lưu hóa đơn {maDaLuu} — Hoàn thành. Có thể xuất PDF hoặc xuất lại từ Đơn hàng.";
            yeuCau=Guid.NewGuid();gio.Clear();tien.Value=0;Nap();
        }
        finally { dangLuu=false;luu.Enabled=true; }
    }
    private void Moi()
    {
        if(!Sv4Ui.XacNhan(this,"Bắt đầu giỏ mới? Nếu lần thanh toán trước lỗi kết nối, kiểm tra Đơn hàng trước để tránh bán lại."))return;
        gio.Clear();yeuCau=Guid.NewGuid();tien.Value=0;Nap();
    }
    private void Xuat()
    {
        if(maDaLuu is not int ma)throw new ArgumentException("Chưa lưu hóa đơn trong phiên này. Hóa đơn cũ có thể xuất từ Đơn hàng.");
        using var d=new SaveFileDialog { Filter="PDF|*.pdf",FileName=$"HoaDon_{ma}.pdf" };if(d.ShowDialog(this)!=DialogResult.OK)return;
        var r=new HoaDonRepository();XuatTaiLieu.HoaDonPdf(d.FileName,r.LayTheoMa(ma)!,r.LayChiTietHienThi(ma));MessageBox.Show(this,"Đã xuất PDF.");
    }
}
