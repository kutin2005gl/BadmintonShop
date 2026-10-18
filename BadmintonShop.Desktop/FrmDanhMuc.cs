using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class FrmDanhMuc : Form
{
    private DataGridView grid=null!;private TextBox ten=null!,moTa=null!;private Label maNhan=null!;private int ma;
    private const ChucNang Quyen=ChucNang.SanPhamDanhMucKhuyenMai;
    public FrmDanhMuc() { InitializeComponent(); }

    // SV4: chỉ dựng giao diện trong constructor, dữ liệu/quyền xử lý OnLoad.
    private void InitializeComponent()
    {
        Text="Danh mục sản phẩm";Size=new Size(900,650);MinimumSize=new Size(700,500);AutoScaleMode=AutoScaleMode.Font;
        grid=Sv4Ui.Luoi();ten=new TextBox { Width=300,MaxLength=100 };moTa=new TextBox { Width=300,MaxLength=255 };
        maNhan=Sv4Ui.Nhan("Danh mục mới");var edit=Sv4Ui.ThanhCongCu(140);
        edit.Controls.AddRange([maNhan,Sv4Ui.Nhan("Tên"),ten,Sv4Ui.Nhan("Mô tả"),moTa,
            Sv4Ui.Nut("Mới",Moi),Sv4Ui.Nut("Lưu",()=>Chay(Luu)),Sv4Ui.Nut("Xóa",()=>Chay(Xoa)),Sv4Ui.Nut("Tải lại",()=>Chay(Nap))]);
        Controls.Add(grid);Controls.Add(edit);
        grid.SelectionChanged+=(_,_)=> { if(grid.CurrentRow is { Selected:true,DataBoundItem:DanhMuc d }) { ma=d.MaDM;maNhan.Text=$"Mã: {ma}";ten.Text=d.TenDanhMuc;moTa.Text=d.MoTa; } };
    }
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);if(DesignMode || !QuyenForm.KiemTraKhiMo(this,Quyen))return;Chay(Nap);
    }
    // SV4: tải lại sau ghi để phản ánh tên đã chuẩn hóa.
    private void Nap() { grid.DataSource=new DanhMucRepository().LayTatCa();Moi(); }
    private void Moi() { ma=0;maNhan.Text="Danh mục mới";ten.Clear();moTa.Clear();grid.ClearSelection();ten.Focus(); }
    private void Chay(Action a)=>Sv4Ui.Chay(this,Quyen,a);
    private void Luu()
    {
        var d=new DanhMuc { MaDM=ma,TenDanhMuc=ten.Text,MoTa=moTa.Text };
        QuyTacBanHang.BatBuoc(d.TenDanhMuc,100,"Tên danh mục");
        if(!Sv4Ui.XacNhan(this,"Lưu danh mục này?"))return;new DanhMucRepository().Luu(d);Nap();
    }
    private void Xoa()
    {
        if(ma==0)throw new ArgumentException("Hãy chọn danh mục.");
        if(!Sv4Ui.XacNhan(this,$"Xóa danh mục {ten.Text}?"))return;new DanhMucRepository().Xoa(ma);Nap();
    }
}
