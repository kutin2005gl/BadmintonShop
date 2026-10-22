using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

// SV4: các điều khiển và ranh giới xử lý lỗi dùng chung, không truy cập CSDL trong helper giao diện.
internal static class Sv4Ui
{
    public static DataGridView Luoi()
    {
        var grid=new DataGridView { Dock=DockStyle.Fill,ReadOnly=true,AutoGenerateColumns=true,
            AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,SelectionMode=DataGridViewSelectionMode.FullRowSelect,
            MultiSelect=false,AllowUserToAddRows=false,AllowUserToDeleteRows=false,RowHeadersVisible=false,BackgroundColor=Color.White,
            ColumnHeadersHeightSizeMode=DataGridViewColumnHeadersHeightSizeMode.AutoSize };
        grid.DataBindingComplete+=(_,_)=>DinhDangLuoi(grid);return grid;
    }
    // SV4: nhãn nghiệp vụ tiếng Việt và định dạng đồng nguyên trên mọi bảng desktop.
    private static void DinhDangLuoi(DataGridView grid)
    {
        var ten=new Dictionary<string,string> { ["MaSP"]="Mã SP",["TenSP"]="Sản phẩm",["TenDanhMuc"]="Danh mục",["ThuongHieu"]="Thương hiệu",
            ["DonGiaBan"]="Giá bán (đ)",["DonViTinh"]="Đơn vị",["SoLuongTon"]="Tồn kho",["TrangThai"]="Trạng thái",["CanhBao"]="Cảnh báo",
            ["MaDM"]="Mã DM",["MoTa"]="Mô tả",["MaHD"]="Mã HĐ",["NgayLap"]="Ngày lập",["KhachHang"]="Khách hàng",["SDT"]="Điện thoại",
            ["LoaiBan"]="Loại bán",["HinhThucTT"]="Thanh toán",["TienGiamGia"]="Giảm HĐ (đ)",["TongTien"]="Tổng tiền (đ)",
            ["LyDoHuy"]="Lý do hủy",["SoLuong"]="Số lượng",["DonGia"]="Đơn giá (đ)",["GiamGia"]="Giảm (%)",["ThanhTien"]="Thành tiền (đ)" };
        foreach(DataGridViewColumn c in grid.Columns)
        {
            if(ten.TryGetValue(c.Name,out var label))c.HeaderText=label;
            if(c.Name is "TenSP" or "TenDanhMuc" or "KhachHang")c.FillWeight=180;
            if(c.Name is "MaSP" or "MaHD" or "MaDM")c.FillWeight=50;
            if(c.ValueType==typeof(decimal) || c.ValueType==typeof(long))
            { c.DefaultCellStyle.Format=c.Name=="GiamGia"?"0.##":"N0";c.DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleRight; }
            if(c.ValueType==typeof(DateTime))c.DefaultCellStyle.Format="dd/MM/yyyy HH:mm";
        }
    }
    public static FlowLayoutPanel ThanhCongCu(int height=90) => new() { Dock=DockStyle.Top,Height=height,Padding=new Padding(8),AutoScroll=true,WrapContents=true };
    public static Button Nut(string text,Action action)
    {
        var b=new Button { Text=text,AutoSize=true,Height=32,Margin=new Padding(4) };b.Click+=(_,_)=>action();return b;
    }
    public static ComboBox Chon(params string[] values)
    {
        var c=new ComboBox { DropDownStyle=ComboBoxStyle.DropDownList,Width=160,Margin=new Padding(4) };
        c.Items.AddRange(values);if(values.Length>0)c.SelectedIndex=0;return c;
    }
    public static Label Nhan(string text) => new() { Text=text,AutoSize=true,Margin=new Padding(5,9,5,3) };
    public static bool XacNhan(Form owner,string text) => MessageBox.Show(owner,text,"Xác nhận",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes;
    public static void Chay(Form f,ChucNang quyen,Action action)
    {
        if(!QuyenForm.DuQuyen(quyen)) return;
        try { action(); } catch(Exception ex) { MessageBox.Show(f,LoiCsdl.ThongBao(ex),"Không thể thực hiện",MessageBoxButtons.OK,MessageBoxIcon.Warning); }
    }
    public static int? MaDong(DataGridView grid,string cot) => grid.CurrentRow?.Cells[cot].Value is int ma?ma:null;
    public static string? Loc(ComboBox c) => c.SelectedIndex<=0?null:c.Text;
    public static TableLayoutPanel BangNhap() => new() { Dock=DockStyle.Fill,AutoScroll=true,ColumnCount=2,Padding=new Padding(12) };
    public static void Truong(TableLayoutPanel p,string nhan,Control control)
    {
        int row=p.RowCount++;p.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        p.Controls.Add(Nhan(nhan),0,row);control.Anchor=AnchorStyles.Left|AnchorStyles.Right;control.Margin=new Padding(4);p.Controls.Add(control,1,row);
    }
    public static string? NhapLyDo(Form owner)
    {
        using var f=new Form { Text="Lý do hủy (bắt buộc)",Width=520,Height=230,StartPosition=FormStartPosition.CenterParent };
        var input=new TextBox { Dock=DockStyle.Fill,Multiline=true,MaxLength=500 };
        var b=new Button { Dock=DockStyle.Bottom,Text="Xác nhận hủy",Height=40,DialogResult=DialogResult.OK };
        f.Controls.Add(input);f.Controls.Add(b);return f.ShowDialog(owner)==DialogResult.OK?input.Text:null;
    }
}
