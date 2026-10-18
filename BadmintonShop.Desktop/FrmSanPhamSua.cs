using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class FrmSanPhamSua : Form
{
    public int MaSanPham { get; set; }
    private TextBox ten=null!,th=null!,dv=null!,mt=null!;private NumericUpDown gia=null!;private ComboBox dm=null!,tt=null!;
    private Label ton=null!,anhNhan=null!;private PictureBox preview=null!;private string? khoaAnh;private Bitmap? anhMoi;
    private const ChucNang Quyen=ChucNang.SanPhamDanhMucKhuyenMai;
    public FrmSanPhamSua() { InitializeComponent(); }
    private void InitializeComponent()
    {
        Text="Thông tin sản phẩm";Size=new Size(720,820);MinimumSize=new Size(620,650);AutoScaleMode=AutoScaleMode.Font;
        ten=new TextBox { Width=400,MaxLength=150 };th=new TextBox { MaxLength=50 };dv=new TextBox { MaxLength=20,Text="Cái" };
        mt=new TextBox { Multiline=true,Height=70 };gia=new NumericUpDown { Maximum=999999999999999999m,ThousandsSeparator=true,Width=300 };
        dm=Sv4Ui.Chon();dm.Width=400;tt=Sv4Ui.Chon("Đang bán","Ngừng bán");ton=Sv4Ui.Nhan("0 — chỉ tăng qua phiếu nhập");
        anhNhan=Sv4Ui.Nhan("Chưa có ảnh");preview=new PictureBox { Width=320,Height=200,SizeMode=PictureBoxSizeMode.Zoom,BorderStyle=BorderStyle.FixedSingle };
        var p=Sv4Ui.BangNhap();
        Sv4Ui.Truong(p,"Tên sản phẩm *",ten);Sv4Ui.Truong(p,"Danh mục *",dm);Sv4Ui.Truong(p,"Thương hiệu",th);
        Sv4Ui.Truong(p,"Giá bán (đồng) *",gia);Sv4Ui.Truong(p,"Đơn vị tính *",dv);Sv4Ui.Truong(p,"Trạng thái",tt);
        Sv4Ui.Truong(p,"Tồn kho (chỉ xem)",ton);Sv4Ui.Truong(p,"Mô tả",mt);Sv4Ui.Truong(p,"Ảnh",preview);Sv4Ui.Truong(p,"Khóa ảnh",anhNhan);
        var buttons=new FlowLayoutPanel { AutoSize=true };
        buttons.Controls.AddRange([Sv4Ui.Nut("Chọn ảnh",()=>Chay(ChonAnh)),Sv4Ui.Nut("Bỏ ảnh",()=> { khoaAnh=null;anhMoi?.Dispose();anhMoi=null;HienAnh(null);anhNhan.Text="Chưa có ảnh"; }),
            Sv4Ui.Nut("Lưu",()=>Chay(Luu)),Sv4Ui.Nut("Đóng",Close)]);Sv4Ui.Truong(p,"",buttons);Controls.Add(p);
    }
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);if(DesignMode || !QuyenForm.KiemTraKhiMo(this,Quyen))return;
        Chay(()=>
        {
            dm.DataSource=new DanhMucRepository().LayTatCa();dm.DisplayMember="TenDanhMuc";dm.ValueMember="MaDM";
            if(MaSanPham==0)return;
            var s=new SanPhamRepository().LayTheoMa(MaSanPham)??throw new InvalidOperationException("Sản phẩm đã bị xóa.");
            ten.Text=s.TenSP;th.Text=s.ThuongHieu;dv.Text=s.DonViTinh;mt.Text=s.MoTa;gia.Value=s.DonGiaBan;
            dm.SelectedValue=s.MaDM;tt.SelectedItem=s.TrangThai;ton.Text=$"{s.SoLuongTon:N0} — chỉ xem";khoaAnh=s.HinhAnh;anhNhan.Text=khoaAnh??"Chưa có ảnh";
            try { var path=KhoAnhSanPham.DuongDanDayDu(khoaAnh);if(path!=null) { using var image=Image.FromFile(path);HienAnh(image); } }
            catch(Exception ex) when(ex is ArgumentException or IOException or OutOfMemoryException) { anhNhan.Text="Ảnh thiếu/không hợp lệ — hãy chọn ảnh khác hoặc bỏ ảnh"; }
        });
    }
    private void Chay(Action a)=>Sv4Ui.Chay(this,Quyen,a);
    // SV4: giải mã và mã hóa lại JPEG, không sao chép nội dung tùy ý vào vùng ảnh dùng chung.
    private void ChonAnh()
    {
        using var dialog=new OpenFileDialog { Filter="Ảnh|*.jpg;*.jpeg;*.png;*.bmp",CheckFileExists=true };
        if(dialog.ShowDialog(this)!=DialogResult.OK)return;
        var image=AnhSanPham.Doc(dialog.FileName);
        anhMoi?.Dispose();anhMoi=image;HienAnh(anhMoi);anhNhan.Text="Ảnh mới (sẽ lưu khi bấm Lưu)";
    }
    private void HienAnh(Image? image) { var old=preview.Image;preview.Image=image==null?null:new Bitmap(image);old?.Dispose(); }
    private void Luu()
    {
        if(dm.SelectedValue is not int maDM)throw new ArgumentException("Hãy tạo/chọn danh mục trước.");
        QuyTacBanHang.BatBuoc(ten.Text,150,"Tên sản phẩm");QuyTacBanHang.BatBuoc(dv.Text,20,"Đơn vị tính");
        if(!Sv4Ui.XacNhan(this,"Lưu thông tin sản phẩm? Tồn kho không bị thay đổi."))return;
        string? moi=null;
        try
        {
            if(anhMoi!=null)
            {
                moi=AnhSanPham.Luu(anhMoi);
            }
            new SanPhamRepository().Luu(new SanPham { MaSP=MaSanPham,TenSP=ten.Text,MaDM=maDM,ThuongHieu=th.Text,
                DonGiaBan=gia.Value,DonViTinh=dv.Text,MoTa=mt.Text,TrangThai=tt.Text,HinhAnh=moi??khoaAnh });
        }
        catch { if(moi!=null) { try { File.Delete(KhoAnhSanPham.DuongDanDayDu(moi)!); } catch(IOException) { /* tệp mồ côi có thể dọn sau */ } }throw; }
        DialogResult=DialogResult.OK;Close();
    }
    protected override void Dispose(bool disposing) { if(disposing) { anhMoi?.Dispose();preview?.Image?.Dispose(); }base.Dispose(disposing); }
}
