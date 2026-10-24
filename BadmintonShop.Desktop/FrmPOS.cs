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

public class FrmPOS : Form, IUnsavedWork
{
    private DataGridView sanPham=null!,gioLuoi=null!;private TextBox tim=null!;
    private NumericUpDown sl=null!,tien=null!;private Label tong=null!,ketQua=null!;private Button luu=null!;
    private readonly BindingList<DongPOS> gio=new();private Guid yeuCau=Guid.NewGuid();private bool hoTroPOS;private bool dangLuu;private int? maDaLuu;
    private const ChucNang Quyen=ChucNang.DonHangPOS;
    public bool HasUnsavedWork => gio.Count > 0 || tien.Value > 0;
    public FrmPOS() { InitializeComponent(); }
    private void InitializeComponent()
    {
        Text="POS — bán tại quầy / khách vãng lai";Size=new Size(1200,820);MinimumSize=new Size(1000,700);AutoScaleMode=AutoScaleMode.Font;
        sanPham=Sv4Ui.Luoi();gioLuoi=Sv4Ui.Luoi();gioLuoi.DataSource=gio;
        void CartColumns()
        {
            if(gioLuoi.Columns.Count==0)return;
            gioLuoi.Columns["MaSP"].Visible=false;
            gioLuoi.Columns["TenSP"].MinimumWidth=150;
            gioLuoi.Columns["SoLuong"].MinimumWidth=75;
            gioLuoi.Columns["DonGia"].MinimumWidth=110;
            gioLuoi.Columns["ThanhTien"].MinimumWidth=120;
        }
        gioLuoi.DataBindingComplete+=(_,_)=>CartColumns();CartColumns();
        tim=new TextBox { Width=200,PlaceholderText="Tên hoặc thương hiệu" };sl=new NumericUpDown { Minimum=1,Maximum=1000000,Value=1,Width=90 };
        tien=new NumericUpDown { Maximum=999999999999999999m,ThousandsSeparator=true,Width=180 };
        tong=new Label { AutoSize=true,Font=new Font("Segoe UI",12,FontStyle.Bold),Margin=new Padding(6) };
        ketQua=new Label { Dock=DockStyle.Bottom,Height=55,Padding=new Padding(5) };
        var bar=Sv4Ui.ThanhCongCu(110);bar.Controls.AddRange([tim,Sv4Ui.Nut("Tìm / tải giá mới",()=>Chay(Nap)),Sv4Ui.Nhan("Số lượng"),sl,
            Sv4Ui.Nut("Thêm vào giỏ",()=>Chay(Them)),Sv4Ui.Nut("Sửa số lượng giỏ",()=>Chay(Sua)),Sv4Ui.Nut("Bỏ dòng giỏ",()=>Chay(Bo)),
            Sv4Ui.Nhan("Khuyến mãi SV3 chưa sẵn sàng — POS bán theo giá thường, không áp voucher.")]);
        luu=Sv4Ui.Nut("Thanh toán tiền mặt",()=>Chay(ThanhToan));
        luu.Font=new Font("Segoe UI",12,FontStyle.Bold);luu.Width=280;luu.Height=48;
        var payment=Sv4Ui.ThanhCongCu(215);payment.Dock=DockStyle.Bottom;
        tong.Font=new Font("Segoe UI",17,FontStyle.Bold);tong.ForeColor=ShopTheme.Green;tong.MaximumSize=new Size(430,0);
        payment.Controls.AddRange([tong,Sv4Ui.Nhan("Tiền khách đưa (VNĐ)"),tien,luu,
            Sv4Ui.Nut("Xuất lại PDF",()=>Chay(Xuat)),Sv4Ui.Nut("Giỏ mới",()=>Chay(Moi))]);
        var left=new Panel { Dock=DockStyle.Fill,Padding=new Padding(8),BackColor=Color.White };
        left.Controls.Add(sanPham);left.Controls.Add(bar);
        var right=new Panel { Dock=DockStyle.Fill,Padding=new Padding(8),BackColor=Color.White,Margin=new Padding(10,0,0,0) };
        right.Controls.Add(gioLuoi);right.Controls.Add(payment);
        right.Controls.Add(new Label { Text="GIỎ HÀNG  /  KHÁCH VÃNG LAI",Dock=DockStyle.Top,Height=35,ForeColor=ShopTheme.Green,Font=new Font("Segoe UI",11,FontStyle.Bold) });
        var split=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(12) };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,53));split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,47));
        split.Controls.Add(left,0,0);split.Controls.Add(right,1,0);
        Controls.Add(split);Controls.Add(ketQua);ShopTheme.Apply(this);MinimumSize=new Size(1000,600);
        bar.Height=170;bar.MinimumSize=new Size(0,170);
        FormClosing+=(_,e)=> { if(ShopTheme.IsBusy(this))e.Cancel=true;else if(HasUnsavedWork && !Sv4Ui.XacNhan(this,"Giỏ chưa thanh toán. Bỏ giỏ và đóng?"))e.Cancel=true; };

        tien.ValueChanged+=(_,_)=>Tinh();gio.ListChanged+=(_,_)=>Tinh();Tinh();
    }
    protected override void OnLoad(EventArgs e)
    { base.OnLoad(e);if(DesignMode || !QuyenForm.KiemTraKhiMo(this,Quyen))return;Chay(Nap); }
    private void Chay(Action a)=>Sv4Ui.Chay(this,Quyen,a);
    private async void Nap()
    {
        var keyword=tim.Text;
        await ShopTheme.Load(this,()=> (Rows:new SanPhamRepository().LayTatCa(keyword).Where(x=>x.TrangThai=="Đang bán").ToList(),Ready:new HoaDonRepository().HoTroPOS()),result=>
        {
            sanPham.DataSource=result.Rows;hoTroPOS=result.Ready;luu.Enabled=hoTroPOS;
            if(!hoTroPOS)ketQua.Text="Chưa thể thanh toán: CSDL thiếu trường POS theo bản SV4. Có thể xem sản phẩm và chuẩn bị giỏ; cần người quản trị cập nhật CSDL trước khi thu tiền.";
            foreach(DataGridViewColumn col in sanPham.Columns)col.Visible=col.Name is "MaSP" or "TenSP" or "DonGiaBan" or "SoLuongTon";
        },result=>result.Rows.Count==0);
    }
    private void Them()
    {
        if(sanPham.CurrentRow?.DataBoundItem is not SanPham s)throw new ArgumentException("Chọn sản phẩm ở bảng bên trái.");
        var cu=gio.FirstOrDefault(x=>x.MaSP==s.MaSP);int so=checked((cu?.SoLuong??0)+(int)sl.Value);
        if(so>s.SoLuongTon)throw new ArgumentException("Không đủ tồn kho theo lần tải gần nhất.");
        if(cu==null)gio.Add(new DongPOS { MaSP=s.MaSP,TenSP=s.TenSP,SoLuong=so,DonGia=s.DonGiaBan });
        else { cu.SoLuong=so;gio.ResetBindings(); }
    }
    private void Sua()
    {
        if(gioLuoi.CurrentRow?.DataBoundItem is not DongPOS d)throw new ArgumentException("Chọn sản phẩm trong giỏ bên phải.");
        d.SoLuong=(int)sl.Value;gio.ResetBindings();
    }
    private void Bo() { if(gioLuoi.CurrentRow?.DataBoundItem is DongPOS d)gio.Remove(d); }
    private void Tinh()
    {
        var total=gio.Sum(x=>x.ThanhTien);tong.Text=$"Tổng thanh toán: {total:N0} đ\n"+(tien.Value>=total?$"Thừa: {tien.Value-total:N0} đ":$"Thiếu: {total-tien.Value:N0} đ");
    }
    // SV4: chỉ thay token sau giao dịch chắc chắn thành công, retry cùng token không tạo thêm hóa đơn.
    private async void ThanhToan()
    {
        try
        {
        if(dangLuu)return;
        if(!hoTroPOS)throw new InvalidOperationException("CSDL chưa hỗ trợ lưu POS an toàn. Hãy liên hệ quản trị viên.");
        if(gio.Count==0)throw new ArgumentException("Giỏ hàng đang trống.");
        if(tien.Value<gio.Sum(x=>x.ThanhTien))throw new ArgumentException("Khách chưa đưa đủ tiền.");
        if(!Sv4Ui.XacNhan(this,$"Lưu hóa đơn và thu {gio.Sum(x=>x.ThanhTien):N0} đồng?"))return;
        dangLuu=true;luu.Enabled=false;
        try
        {
            var rows=gio.Select(x=>new ChiTietHoaDon { MaSP=x.MaSP,SoLuong=x.SoLuong,DonGia=x.DonGia }).ToList();
            var employee=PhienDangNhap.MaNV;var cash=tien.Value;var token=yeuCau;
            var success=await ShopTheme.Load(this,()=>new HoaDonRepository().ThanhToanPOS(employee,rows,cash,token),ma=>
            {
            maDaLuu=ma;
            ketQua.Text=$"Đã lưu hóa đơn {maDaLuu} — Hoàn thành. Có thể xuất PDF hoặc xuất lại từ Đơn hàng.";
            yeuCau=Guid.NewGuid();gio.Clear();tien.Value=0;
            });
            if(success)Nap();
        }
        finally { dangLuu=false;luu.Enabled=hoTroPOS; }
        }
        catch(Exception ex) { MessageBox.Show(this,LoiCsdl.ThongBao(ex),"Không thể thanh toán",MessageBoxButtons.OK,MessageBoxIcon.Warning); }
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
        GhiPdf(d.FileName,ma);
    }
    private async void GhiPdf(string path,int ma)
    {
        var success=await ShopTheme.Load(this,()=> { var r=new HoaDonRepository();XuatTaiLieu.HoaDonPdf(path,r.LayTheoMa(ma)!,r.LayChiTietHienThi(ma));return true; },_=>{});
        if(success)MessageBox.Show(this,"Đã xuất PDF.");
    }
}
