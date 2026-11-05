using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class FrmSanPham : Form
{
    private DataGridView grid=null!;private TextBox tim=null!;private ComboBox dm=null!,thuongHieu=null!,tt=null!,ton=null!;
    private DataTable? data; private PictureBox preview=null!;private Label anhNhan=null!;private int previewVersion;
    private const ChucNang Quyen=ChucNang.SanPhamDanhMucKhuyenMai;
    private readonly BoNhoPhien<int,DataTable> boNho=new(BoNhoPhien<int,int>.ThoiHanMacDinh);
    private string PhamVi=>$"{DbHelper.ConnectionString}|{PhienDangNhap.MaTK}|{PhienDangNhap.VaiTro}|{DbHelper.DauHieuDuLieu}";
    public FrmSanPham() { InitializeComponent(); }
    private void InitializeComponent()
    {
        Text="Sản phẩm — tồn kho chỉ xem, nhập hàng qua Phiếu nhập";Size=new Size(1200,750);MinimumSize=new Size(950,600);
        AutoScaleMode=AutoScaleMode.Font;grid=Sv4Ui.Luoi();tim=new TextBox { Width=190,PlaceholderText="Tên, mã hoặc thương hiệu" };
        dm=Sv4Ui.Chon();thuongHieu=Sv4Ui.Chon();tt=Sv4Ui.Chon("Mọi trạng thái","Đang bán","Ngừng bán");
        ton=Sv4Ui.Chon("Mọi mức tồn","Hết hàng","Sắp hết (1–5)","Còn hàng (>5)");
        var bar=Sv4Ui.ThanhCongCu(120);
        bar.Controls.AddRange([tim,dm,thuongHieu,tt,ton,Sv4Ui.Nut("Lọc",Loc),Sv4Ui.Nut("Tải lại",()=>Chay(Nap)),
            Sv4Ui.Nut("Thêm",()=>Sua(0)),Sv4Ui.Nut("Nhập Excel",()=>Chay(()=> { using var form=new FrmNhapSanPhamExcel();form.ShowDialog(this);if(form.DaNhap)Nap(); })),Sv4Ui.Nut("Sửa",()=>Chay(()=>Sua(Sv4Ui.MaDong(grid,"MaSP")??throw new ArgumentException("Hãy chọn sản phẩm cần sửa.")))),
            Sv4Ui.Nut("Ngừng bán",()=>Chay(Ngung)),Sv4Ui.Nhan("Cảnh báo: hết hàng = 0; sắp hết = 1–5. Bấm tiêu đề cột để sắp xếp.")]);
        var pane=new Panel { Dock=DockStyle.Right,Width=200,Padding=new Padding(12),BackColor=Color.White };
        preview=new PictureBox { Dock=DockStyle.Top,Height=185,SizeMode=PictureBoxSizeMode.Zoom };
        anhNhan=new Label { Dock=DockStyle.Fill,Text="Chọn sản phẩm để xem ảnh",AutoEllipsis=true,Padding=new Padding(4) };
        pane.Controls.Add(anhNhan);pane.Controls.Add(preview);
        Controls.Add(grid);Controls.Add(pane);Controls.Add(bar);ShopTheme.Apply(this);
        grid.SelectionChanged+=(_,_)=>XemAnh();
        FormClosed+=(_,_)=>preview.Image?.Dispose();
    }
    protected override void OnLoad(EventArgs e)
    { base.OnLoad(e);if(DesignMode || !QuyenForm.KiemTraKhiMo(this,Quyen))return;Chay(Nap); }
    private void Chay(Action a)=>Sv4Ui.Chay(this,Quyen,a);
    // SV4: bộ lọc dùng LINQ, tránh ghép biểu thức DataView từ chuỗi người dùng.
    private async void Nap()
    {
        boNho.Xoa();var scope=PhamVi;
        await ShopTheme.Load(this,()=>boNho.Lay(0,scope,()=>new SanPhamRepository().LayDanhSachHienThi()),rows=>
        {
        data=rows;
        dm.Items.Clear();dm.Items.Add("Mọi danh mục");dm.Items.AddRange(data.AsEnumerable().Select(r=>r.Field<string>("TenDanhMuc")!).Distinct().Order().ToArray());dm.SelectedIndex=0;
        thuongHieu.Items.Clear();thuongHieu.Items.Add("Mọi thương hiệu");thuongHieu.Items.AddRange(data.AsEnumerable().Select(r=>r.Field<string?>("ThuongHieu")??"").Distinct().Order().ToArray());thuongHieu.SelectedIndex=0;
        GanBoLoc();
        },rows=>rows.Rows.Count==0);
    }
    private async void Loc()
    {
        if(data==null)return;
        var scope=PhamVi;
        await ShopTheme.Load(this,()=>boNho.Lay(0,scope,()=>new SanPhamRepository().LayDanhSachHienThi()),rows=>
        { data=rows;GanBoLoc(); });
    }
    private void GanBoLoc()
    {
        if(data==null)return;
        var view=data.Clone();var keyword=tim.Text.Trim();
        foreach(var r in data.AsEnumerable().Where(r=>
            (keyword.Length==0 || $"{r["MaSP"]} {r["TenSP"]} {r["ThuongHieu"]}".Contains(keyword,StringComparison.CurrentCultureIgnoreCase)) &&
            (dm.SelectedIndex<=0 || r.Field<string>("TenDanhMuc")==dm.Text) &&
            (thuongHieu.SelectedIndex<=0 || (r.Field<string?>("ThuongHieu")??"")==thuongHieu.Text) &&
            (tt.SelectedIndex<=0 || r.Field<string>("TrangThai")==tt.Text) &&
            (ton.SelectedIndex==0 || ton.SelectedIndex==1 && r.Field<int>("SoLuongTon")==0 ||
             ton.SelectedIndex==2 && r.Field<int>("SoLuongTon") is >0 and <=QuyTacBanHang.NguongSapHet ||
             ton.SelectedIndex==3 && r.Field<int>("SoLuongTon")>QuyTacBanHang.NguongSapHet))) view.ImportRow(r);
        grid.DataSource=view;grid.Columns["MaDM"].Visible=false;grid.Columns["HinhAnh"].Visible=false;grid.Columns["DonGiaBan"].DefaultCellStyle.Format="#,##0 'đ'";
    }
    private async void XemAnh()
    {
        var version=++previewVersion;
        preview.Image?.Dispose();preview.Image=null;
        if(Sv4Ui.MaDong(grid,"MaSP") is not int ma) { anhNhan.Text="Chưa có sản phẩm được chọn";return; }
        // Capture the selected snapshot before awaiting; selecting/filtering never re-queries SQL.
        var row=((DataRowView)grid.CurrentRow!.DataBoundItem).Row;
        var name=row.Field<string>("TenSP");var price=row.Field<decimal>("DonGiaBan");
        var stock=row.Field<int>("SoLuongTon");var imageKey=row.Field<string?>("HinhAnh");
        anhNhan.Text="Đang tải ảnh…";
        try
        {
            var result=await Task.Run(()=>
            {
                Bitmap? image=null;
                try { var path=KhoAnhSanPham.DuongDanDayDu(imageKey);if(path!=null && File.Exists(path))image=AnhSanPham.Doc(path); }
                catch(Exception ex) when(ex is ArgumentException or IOException or OutOfMemoryException) { /* Older/missing media never hides product information. */ }
                return image;
            });
            if(IsDisposed || version!=previewVersion) { result?.Dispose();return; }
            preview.Image=result;
            anhNhan.Text=$"{name}\n\n{price:N0} đ\nTồn: {stock:N0}\n\n"+(result==null?"Chưa có ảnh hợp lệ trong kho ảnh chung":"");
        }
        catch(Exception) { if(!IsDisposed && version==previewVersion)anhNhan.Text="Không tải được ảnh. Chọn lại sản phẩm để thử lại."; }
    }
    private void Sua(int ma)
    {
        Chay(()=> { using var f=new FrmSanPhamSua { MaSanPham=ma };if(f.ShowDialog(this)==DialogResult.OK)Nap(); });
    }
    private async void GhiNgung(int ma)
    {
        var success=await ShopTheme.Load(this,()=> { new SanPhamRepository().NgungBan(ma);return true; },_=>{});
        if(success)Nap();
    }
    private void Ngung()
    {
        var ma=Sv4Ui.MaDong(grid,"MaSP")??throw new ArgumentException("Hãy chọn sản phẩm.");
        if(!Sv4Ui.XacNhan(this,"Ngừng bán sản phẩm này? Lịch sử giao dịch được giữ lại."))return;
        GhiNgung(ma);
    }
}
