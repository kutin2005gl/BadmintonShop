using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class FrmDanhMuc : Form, IUnsavedWork
{
    private DataGridView grid=null!;private TextBox ten=null!,moTa=null!;private Label maNhan=null!;private int ma;
    private string original="\n";private TextBox search=null!;private List<DanhMuc> all=[];
    public bool HasUnsavedWork => $"{ten.Text}\n{moTa.Text}"!=original;
    private const ChucNang Quyen=ChucNang.SanPhamDanhMucKhuyenMai;
    public FrmDanhMuc() { InitializeComponent(); }

    // SV4: chỉ dựng giao diện trong constructor, dữ liệu/quyền xử lý OnLoad.
    private void InitializeComponent()
    {
        Text="Danh mục sản phẩm";Size=new Size(900,650);MinimumSize=new Size(700,500);AutoScaleMode=AutoScaleMode.Font;
        grid=Sv4Ui.Luoi();ten=new TextBox { Width=300,MaxLength=100 };moTa=new TextBox { Width=300,MaxLength=255 };
        maNhan=Sv4Ui.Nhan("Danh mục mới");var edit=Sv4Ui.ThanhCongCu(140);
        edit.Controls.AddRange([maNhan,Sv4Ui.Nhan("Tên"),ten,Sv4Ui.Nhan("Mô tả"),moTa,
            Sv4Ui.Nut("Mới",()=> { if(!HasUnsavedWork || Sv4Ui.XacNhan(this,"Bỏ thay đổi để tạo danh mục mới?"))Moi(); }),Sv4Ui.Nut("Lưu",()=>Chay(Luu)),Sv4Ui.Nut("Xóa",()=>Chay(Xoa)),Sv4Ui.Nut("Tải lại",()=>Chay(Nap))]);
        search=new TextBox { Width=260,PlaceholderText="Tìm tên hoặc mô tả danh mục" };
        var filter=Sv4Ui.ThanhCongCu(62);filter.Controls.AddRange([search,Sv4Ui.Nut("Tìm kiếm",Loc)]);
        Controls.Add(grid);Controls.Add(edit);Controls.Add(filter);ShopTheme.Apply(this);
        FormClosing+=(_,e)=> { if(HasUnsavedWork && !Sv4Ui.XacNhan(this,"Bỏ thay đổi danh mục chưa lưu?"))e.Cancel=true; };
        grid.RowValidating+=(_,e)=> { if(HasUnsavedWork && !Sv4Ui.XacNhan(this,"Bỏ thay đổi danh mục trước khi chọn dòng khác?"))e.Cancel=true; };
        grid.SelectionChanged+=(_,_)=> { if(grid.CurrentRow is { Selected:true,DataBoundItem:DanhMuc d }) { ma=d.MaDM;maNhan.Text=$"Mã: {ma}";ten.Text=d.TenDanhMuc;moTa.Text=d.MoTa;original=$"{ten.Text}\n{moTa.Text}"; } };
    }
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);if(DesignMode || !QuyenForm.KiemTraKhiMo(this,Quyen))return;Chay(Nap);
    }
    // SV4: tải lại sau ghi để phản ánh tên đã chuẩn hóa.
    private async void Nap()
    {
        if(HasUnsavedWork && !Sv4Ui.XacNhan(this,"Tải lại và bỏ thay đổi chưa lưu?"))return;
        await ShopTheme.Load(this,()=>new DanhMucRepository().LayTatCa(),rows=> { all=rows;ApDungLoc();Moi(); },rows=>rows.Count==0);
    }
    private void Loc() { if(HasUnsavedWork && !Sv4Ui.XacNhan(this,"Lọc lại và bỏ thay đổi danh mục chưa lưu?"))return;ApDungLoc(); }
    private void ApDungLoc() { var keyword=search.Text.Trim();grid.DataSource=all.Where(d=>$"{d.TenDanhMuc} {d.MoTa}".Contains(keyword,StringComparison.CurrentCultureIgnoreCase)).ToList(); }
    private void Moi() { ma=0;maNhan.Text="Danh mục mới";ten.Clear();moTa.Clear();original="\n";grid.ClearSelection();ten.Focus(); }
    private void Chay(Action a)=>Sv4Ui.Chay(this,Quyen,a);
    private void Luu()
    {
        var d=new DanhMuc { MaDM=ma,TenDanhMuc=ten.Text,MoTa=moTa.Text };
        QuyTacBanHang.BatBuoc(d.TenDanhMuc,100,"Tên danh mục");
        if(!Sv4Ui.XacNhan(this,"Lưu danh mục này?"))return;Ghi(()=>new DanhMucRepository().Luu(d));
    }
    private async void Ghi(Action action)
    {
        var success=await ShopTheme.Load(this,()=> { action();return true; },_=> { original=$"{ten.Text}\n{moTa.Text}"; });if(success)Nap();
    }
    private void Xoa()
    {
        if(ma==0)throw new ArgumentException("Hãy chọn danh mục.");
        if(!Sv4Ui.XacNhan(this,$"Xóa danh mục {ten.Text}?"))return;var id=ma;Ghi(()=>new DanhMucRepository().Xoa(id));
    }
}
