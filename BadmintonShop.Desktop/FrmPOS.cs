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
    public decimal GiamGia { get; set; }
    public decimal ThanhTien { get; set; }
}

public class FrmPOS : Form, IUnsavedWork
{
    private DataGridView sanPham=null!,gioLuoi=null!;
    private TextBox tim=null!,voucher=null!;
    private NumericUpDown sl=null!,tien=null!;
    private Label tong=null!,chiPhi=null!,ketQua=null!,voucherInfo=null!;
    private Button luu=null!;
    private readonly BindingList<DongPOS> gio=new();
    private Guid yeuCau=Guid.NewGuid();
    private bool hoTroPOS,dangLuu,binding;
    private int? maDaLuu;
    private string? voucherYeuCau;
    private KetQuaGioHang? baoGia;
    private const ChucNang Quyen=ChucNang.DonHangPOS;
    public bool HasUnsavedWork=>gio.Count>0 || tien.Value>0 || !string.IsNullOrWhiteSpace(voucher.Text);
    public FrmPOS() { InitializeComponent(); }
    private void InitializeComponent()
    {
        Text="POS — bán tại quầy / khách vãng lai";Size=new Size(1200,820);MinimumSize=new Size(1000,650);AutoScaleMode=AutoScaleMode.Font;
        sanPham=Sv4Ui.Luoi();gioLuoi=Sv4Ui.Luoi();gioLuoi.DataSource=gio;
        void CartColumns()
        {
            if(gioLuoi.Columns.Count==0)return;
            gioLuoi.Columns["MaSP"].Visible=false;
            foreach(DataGridViewColumn column in gioLuoi.Columns)column.MinimumWidth=column.Name=="TenSP"?140:column.Name is "GiamGia" or "SoLuong"?75:115;
        }
        gioLuoi.DataBindingComplete+=(_,_)=>CartColumns();CartColumns();
        tim=new TextBox { Width=190,PlaceholderText="Tên hoặc thương hiệu" };
        sl=new NumericUpDown { Minimum=1,Maximum=1000000,Value=1,Width=80 };
        tien=new NumericUpDown { Maximum=999999999999999999m,ThousandsSeparator=true,Width=180 };
        voucher=new TextBox { Width=170,MaxLength=30,PlaceholderText="Nhập mã voucher",AccessibleName="Mã voucher" };
        tong=ShopTheme.Title("Tổng thanh toán: 0 đ",15);tong.AutoSize=false;tong.Dock=DockStyle.Top;tong.Height=64;
        chiPhi=new Label { Dock=DockStyle.Top,AutoSize=true,Padding=new Padding(4),ForeColor=ShopTheme.Muted };
        ketQua=new Label { Dock=DockStyle.Bottom,Height=54,Padding=new Padding(8),AutoEllipsis=true };
        voucherInfo=new Label { AutoSize=true,MaximumSize=new Size(450,0),Margin=new Padding(4),ForeColor=ShopTheme.Green };
        var bar=Sv4Ui.ThanhCongCu();bar.Controls.AddRange([tim,Sv4Ui.Nut("Tìm / tải giá mới",()=>Chay(Nap)),Sv4Ui.Nhan("Số lượng"),sl,
            Sv4Ui.Nut("Thêm vào giỏ",()=>Chay(Them)),Sv4Ui.Nut("Sửa số lượng giỏ",()=>Chay(Sua)),Sv4Ui.Nut("Bỏ dòng giỏ",()=>Chay(Bo))]);
        var codes=Sv4Ui.ThanhCongCu();codes.Controls.AddRange([voucher,Sv4Ui.Nut("Áp dụng",()=>Chay(()=> { voucherYeuCau=MaVoucher();TinhLai(); })),
            Sv4Ui.Nut("Bỏ mã",()=> { voucherYeuCau=null;voucher.Clear();TinhLai(); }),voucherInfo]);
        luu=Sv4Ui.Nut("Thanh toán tiền mặt",()=>Chay(ThanhToan));luu.Font=new Font("Segoe UI",11,FontStyle.Bold);luu.Height=44;
        var actions=Sv4Ui.ThanhCongCu();actions.Dock=DockStyle.Fill;
        actions.AutoScroll=true;
        actions.Controls.AddRange([Sv4Ui.Nhan("Khách đưa (VNĐ)"),tien,luu,Sv4Ui.Nut("Xuất lại PDF",()=>Chay(Xuat)),Sv4Ui.Nut("Giỏ mới",()=>Chay(Moi))]);
        var payment=new Panel { Dock=DockStyle.Bottom,Height=300,Padding=new Padding(6),BackColor=Color.White };
        payment.Controls.Add(actions);payment.Controls.Add(tong);payment.Controls.Add(chiPhi);
        var left=new Panel { Dock=DockStyle.Fill,Padding=new Padding(8),BackColor=Color.White };
        left.Controls.Add(sanPham);left.Controls.Add(bar);
        var right=new Panel { Dock=DockStyle.Fill,Padding=new Padding(8),BackColor=Color.White,Margin=new Padding(10,0,0,0) };
        right.Controls.Add(gioLuoi);right.Controls.Add(payment);right.Controls.Add(codes);
        right.Controls.Add(new Label { Text="GIỎ HÀNG / KHUYẾN MÃI",Dock=DockStyle.Top,Height=32,ForeColor=ShopTheme.Green,Font=new Font("Segoe UI",11,FontStyle.Bold) });
        var split=new TableLayoutPanel { Dock=DockStyle.Fill,ColumnCount=2,Padding=new Padding(12) };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,48));split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,52));
        split.Controls.Add(left,0,0);split.Controls.Add(right,1,0);
        Controls.Add(split);Controls.Add(ketQua);ShopTheme.Apply(this);
        FormClosing+=(_,e)=> { if(ShopTheme.IsBusy(this))e.Cancel=true;else if(HasUnsavedWork && !Sv4Ui.XacNhan(this,"Giỏ chưa thanh toán. Bỏ giỏ và đóng?"))e.Cancel=true; };
        tien.ValueChanged+=(_,_)=>HienTong();voucher.TextChanged+=(_,_)=>HienTong();
        gio.ListChanged+=(_,_)=> { if(!binding)TinhLai(); };HienTong();
    }
    protected override void OnLoad(EventArgs e)
    { base.OnLoad(e);if(DesignMode || !QuyenForm.KiemTraKhiMo(this,Quyen))return;Chay(Nap); }
    private void Chay(Action a)=>Sv4Ui.Chay(this,Quyen,a);
    private string? MaVoucher()=>string.IsNullOrWhiteSpace(voucher.Text)?null:voucher.Text.Trim().ToUpperInvariant();
    private async void Nap()
    {
        if(IsDisposed || Disposing)return;
        if(InvokeRequired) { ShopTheme.OnUi(this,Nap);return; }
        var keyword=tim.Text;
        bool success=await ShopTheme.Load(this,()=> (Rows:new SanPhamRepository().LayTatCa(keyword).Where(x=>x.TrangThai=="Đang bán").ToList(),Ready:new HoaDonRepository().HoTroPOS()),result=>
        {
            sanPham.DataSource=result.Rows;hoTroPOS=result.Ready;
            ketQua.Text=!hoTroPOS?"CSDL chưa hỗ trợ POS tích hợp. Cần migration 04 và 06 trước khi thanh toán; không tự cập nhật CSDL cửa hàng.":
                maDaLuu is int saved?$"Hóa đơn gần nhất: {saved}. Đã lưu thành công; có thể xuất PDF.":
                "Giảm sản phẩm trước; chọn ưu đãi hóa đơn hoặc voucher tốt hơn, không cộng chồng.";
            foreach(DataGridViewColumn col in sanPham.Columns)col.Visible=col.Name is "MaSP" or "TenSP" or "DonGiaBan" or "SoLuongTon";
        },result=>result.Rows.Count==0);
        if(success)TinhLai();else HienTong();
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
    private async void TinhLai()
    {
        if(IsDisposed || Disposing || binding || dangLuu)return;
        if(InvokeRequired) { ShopTheme.OnUi(this,TinhLai);return; }
        baoGia=null;HienTong();
        if(gio.Count==0) { voucherInfo.Text="Thêm sản phẩm để áp dụng khuyến mãi.";return; }
        var rows=gio.Select(d=>(d.MaSP,d.SoLuong)).ToArray();var code=voucherYeuCau;
        voucherInfo.Text="Đang tính khuyến mãi…";
        await ShopTheme.Load(this,()=>new GioHangService().Tinh(rows,code),NhanBaoGia);
        HienTong();
    }
    private void NhanBaoGia(KetQuaGioHang result)
    {
        baoGia=result;binding=true;
        try
        {
            foreach(var d in gio)
            {
                var fresh=result.Dong.FirstOrDefault(x=>x.MaSP==d.MaSP);
                if(fresh==null)continue;
                d.TenSP=fresh.TenSP;d.DonGia=fresh.DonGia;d.GiamGia=fresh.PhanTramGiam;d.ThanhTien=fresh.ThanhTien;
            }
            gio.ResetBindings();
        }
        finally { binding=false; }
        voucherInfo.Text=result.LoiVoucher??(result.MaVoucher!=null?$"Đã áp dụng {result.MaVoucher}":result.VoucherYeuCau!=null?"Ưu đãi hóa đơn tốt hơn voucher; mã không bị trừ lượt.":"Tự động áp dụng ưu đãi hiện có.");
        if(result.CanhBao.Count>0)voucherInfo.Text+="\n"+string.Join(" ",result.CanhBao);
        voucherInfo.ForeColor=result.LoiVoucher==null?ShopTheme.Green:Color.Firebrick;HienTong();
    }
    private void HienTong()
    {
        if(IsDisposed || Disposing || luu==null)return;
        if(InvokeRequired) { ShopTheme.OnUi(this,HienTong);return; }
        decimal total=baoGia?.TongThanhToan??0;
        chiPhi.Text=baoGia==null?(gio.Count==0?"Giỏ hàng trống":"Chưa có báo giá hợp lệ"):
            $"Tiền hàng trước giảm: {baoGia.TienHangTruocGiam:N0} đ\nGiảm sản phẩm: {baoGia.TienGiamSanPham:N0} đ\nGiảm hóa đơn / voucher: {baoGia.TienGiamHoaDon:N0} đ";
        tong.Text=$"Tổng thanh toán: {total:N0} đ\n"+(tien.Value>=total?$"Tiền thừa: {tien.Value-total:N0} đ":$"Còn thiếu: {total-tien.Value:N0} đ");
        bool sameCode=MaVoucher()==voucherYeuCau;
        luu.Enabled=hoTroPOS && !dangLuu && baoGia is { CoTheDatHang:true,LoiVoucher:null } && baoGia.Dong.Count==gio.Count && sameCode && tien.Value>=total;
        if(!sameCode)voucherInfo.Text="Mã đã sửa. Bấm Áp dụng hoặc Bỏ mã trước khi thanh toán.";
    }
    // SV4: retain the exact confirmed quote/token on uncertain failures; successful retry reads saved history.
    private async void ThanhToan()
    {
        try
        {
            if(dangLuu)return;
            if(!QuyenForm.DuQuyen(Quyen))return;
            if(!hoTroPOS || baoGia==null || !baoGia.CoTheDatHang || baoGia.LoiVoucher!=null || baoGia.Dong.Count!=gio.Count || MaVoucher()!=voucherYeuCau)
                throw new InvalidOperationException("Hãy áp dụng mã và xem lại báo giá hợp lệ trước khi thanh toán.");
            var quote=baoGia;var cash=tien.Value;var employee=PhienDangNhap.MaNV;var token=yeuCau;
            if(cash<quote.TongThanhToan)throw new ArgumentException("Khách chưa đưa đủ tiền.");
            if(!Sv4Ui.XacNhan(this,$"Lưu hóa đơn và thu {quote.TongThanhToan:N0} đồng?"))return;
            dangLuu=true;luu.Enabled=false;
            bool changed=false;
            var success=await ShopTheme.Load(this,()=>
            {
                try { return (Ma:(int?)new HoaDonRepository().ThanhToanPOS(employee,quote,cash,token),Moi:(KetQuaGioHang?)null); }
                catch(GiaThanhToanThayDoiException ex) { return (Ma:(int?)null,Moi:(KetQuaGioHang?)ex.GioMoi); }
            },result=>
            {
                if(result.Moi!=null)
                {
                    changed=true;NhanBaoGia(result.Moi);ketQua.Text="Báo giá đã thay đổi, chưa thu tiền. Kiểm tra tổng mới và bấm Thanh toán để xác nhận lại.";return;
                }
                maDaLuu=result.Ma;ketQua.Text=$"Đã lưu hóa đơn {maDaLuu}. Có thể xuất PDF hoặc xuất lại từ Đơn hàng.";
                binding=true;try { gio.Clear();tien.Value=0;voucher.Clear(); }finally { binding=false; }
                voucherYeuCau=null;baoGia=null;yeuCau=Guid.NewGuid();
            });
            dangLuu=false;
            if(success && !changed)Nap();
        }
        catch(Exception ex) { MessageBox.Show(this,LoiCsdl.ThongBao(ex),"Không thể thanh toán",MessageBoxButtons.OK,MessageBoxIcon.Warning); }
        finally { dangLuu=false;HienTong(); }
    }
    private void Moi()
    {
        if(!Sv4Ui.XacNhan(this,"Bắt đầu giỏ mới? Nếu thanh toán trước bị lỗi kết nối, kiểm tra Đơn hàng trước để tránh bán lại."))return;
        binding=true;try { gio.Clear();tien.Value=0;voucher.Clear(); }finally { binding=false; }
        voucherYeuCau=null;baoGia=null;yeuCau=Guid.NewGuid();Nap();HienTong();
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
