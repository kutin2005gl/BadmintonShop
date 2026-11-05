using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public sealed class FrmNhapSanPhamExcel : Form
{
    private readonly DataGridView grid=Sv4Ui.Luoi();
    private readonly Label status=Sv4Ui.Nhan("Chỉ thêm mới sản phẩm; tồn ban đầu bằng 0. File có lỗi sẽ không ghi dòng nào.");
    private DataTable? categories,existing;
    private XemTruocNhap? preview;
    private readonly Button save;
    public bool DaNhap { get; private set; }
    public FrmNhapSanPhamExcel()
    {
        Text="Nhập sản phẩm từ Excel";Size=new Size(1150,700);MinimumSize=new Size(950,550);StartPosition=FormStartPosition.CenterParent;
        save=Sv4Ui.Nut("Xác nhận nhập",Luu);save.Enabled=false;
        var bar=Sv4Ui.ThanhCongCu(110);bar.Controls.AddRange([Sv4Ui.Nut("Tải mẫu",Mau),Sv4Ui.Nut("Chọn file / xem trước",Chon),save,status]);
        Controls.Add(grid);Controls.Add(bar);ShopTheme.Apply(this);
    }
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);if(!QuyenForm.KiemTraKhiMo(this,ChucNang.SanPhamDanhMucKhuyenMai))return;
        await ShopTheme.Load(this,()=> (new DanhMucRepository().LayDanhSachHienThi(),new SanPhamRepository().LayDanhSachHienThi()),
            data=> { categories=data.Item1;existing=data.Item2; });
    }
    private void Mau()=>Sv4Ui.Chay(this,ChucNang.SanPhamDanhMucKhuyenMai,()=>
    {
        if(categories==null)throw new InvalidOperationException("Hãy đợi tải danh mục.");
        using var dialog=new SaveFileDialog { Filter="Excel|*.xlsx",FileName="MauNhapSanPham.xlsx" };
        if(dialog.ShowDialog(this)==DialogResult.OK)NhapSanPhamExcel.TaoMau(dialog.FileName,categories);
    });
    private void Chon()=>Sv4Ui.Chay(this,ChucNang.SanPhamDanhMucKhuyenMai,()=>
    {
        if(categories==null || existing==null)throw new InvalidOperationException("Hãy đợi tải danh mục và sản phẩm.");
        using var dialog=new OpenFileDialog { Filter="Excel|*.xlsx" };
        if(dialog.ShowDialog(this)==DialogResult.OK)Doc(dialog.FileName);
    });
    private async void Doc(string path)
    {
        preview=null;save.Enabled=false;grid.DataSource=null;
        await ShopTheme.Load(this,()=>NhapSanPhamExcel.Doc(path,categories!,existing!),result=>
        {
            preview=result;grid.DataSource=result.Bang;
            status.Text=$"{result.Bang.Rows.Count} dòng; {result.Loi.Count} lỗi. "+(result.Loi.Count==0?"Kiểm tra bảng rồi xác nhận nhập.":"Sửa các ô trong file và chọn lại; chưa ghi dữ liệu.");
        });
        save.Enabled=preview is { Loi.Count:0 };
    }
    private void Luu()=>Sv4Ui.Chay(this,ChucNang.SanPhamDanhMucKhuyenMai,()=>
    {
        if(preview==null || preview.Loi.Count>0)throw new ArgumentException("Hãy chọn file hợp lệ và xem trước.");
        if(Sv4Ui.XacNhan(this,$"Thêm mới {preview.SanPham.Count} sản phẩm? Không cập nhật sản phẩm đã có; tồn kho bằng 0."))Ghi(preview);
    });
    private async void Ghi(XemTruocNhap selected)
    {
        var success=await ShopTheme.Load(this,()=>NhapSanPhamExcel.Luu(selected),count=>
        { DaNhap=true;status.Text=$"Đã thêm {count} sản phẩm. Đóng cửa sổ để làm mới danh sách.";preview=null; });
        save.Enabled=!success && preview is { Loi.Count:0 };
    }
}
