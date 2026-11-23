using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public class FrmDonHang : Form
{
    private DataGridView grid=null!;private DateTimePicker tu=null!,den=null!;
    private ComboBox tt=null!,loai=null!;private TextBox tim=null!;private Label thongTin=null!;
    private SplitContainer split=null!;private Panel detailRoot=null!,detailBody=null!;private Label emptyState=null!,maHd=null!,badge=null!,cardsTitle=null!;
    private TableLayoutPanel info=null!;private FlowLayoutPanel cards=null!;private readonly Dictionary<string,Label> values=new();
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
        grid=Sv4Ui.Luoi();
        // TV1: lưới đơn hàng cuộn ngang khi thiếu chỗ, cột vừa nội dung để không cắt chữ.
        grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.AllCells;grid.ScrollBars=ScrollBars.Both;
        grid.DataBindingComplete+=(_,_)=>grid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        split=new SplitContainer { Dock=DockStyle.Fill,Orientation=Orientation.Vertical,FixedPanel=FixedPanel.None,SplitterWidth=6,BackColor=ShopTheme.Line };
        split.Panel1.Controls.Add(grid);split.Panel1.BackColor=Color.White;split.Panel2.BackColor=Color.White;
        XayChiTiet(split.Panel2);
        Controls.Add(split);Controls.Add(bar);ShopTheme.Apply(this);grid.CurrentCellChanged+=(_,_)=> { if(!binding)ChiTiet(); };
        HienTrong("Chọn một đơn hàng ở danh sách bên trái để xem chi tiết.");
    }
    // TV1: đặt tỉ lệ mặc định 70% trái / 30% phải; người dùng vẫn kéo thanh chia được.
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        // MinSize chỉ đặt sau khi SplitContainer có kích thước thật, nếu không WinForms ném lỗi.
        try
        {
            int min1=Math.Min(420,split.Width/2),min2=Math.Min(330,split.Width/3);
            split.Panel1MinSize=min1;split.Panel2MinSize=min2;
            split.SplitterDistance=Math.Clamp((int)(split.Width*0.7),min1,Math.Max(min1,split.Width-min2-split.SplitterWidth));
        }
        catch(InvalidOperationException) { }
    }
    // TV1: dựng panel chi tiết bên phải: tiêu đề + badge, thông tin label/value, danh sách thẻ sản phẩm.
    private void XayChiTiet(Control host)
    {
        var small=new Font("Segoe UI",9);
        detailRoot=new Panel { Dock=DockStyle.Fill,AutoScroll=true,AutoScrollMinSize=new Size(0,560),Padding=new Padding(14,12,14,12),BackColor=Color.White };
        emptyState=new Label { Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleCenter,ForeColor=ShopTheme.Muted,Padding=new Padding(20) };
        detailBody=new Panel { Dock=DockStyle.Fill,Visible=false };

        maHd=new Label { AutoSize=true,Font=new Font("Segoe UI",15,FontStyle.Bold),ForeColor=ShopTheme.Green,Margin=new Padding(0,2,8,2) };
        badge=new Label { AutoSize=true,Font=new Font("Segoe UI",9,FontStyle.Bold),Padding=new Padding(10,4,10,4),Anchor=AnchorStyles.Right,Margin=new Padding(0) };
        var header=new TableLayoutPanel { Dock=DockStyle.Top,AutoSize=true,ColumnCount=2,Padding=new Padding(0,0,0,6) };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.Controls.Add(maHd,0,0);header.Controls.Add(badge,1,0);

        thongTin=new Label { Dock=DockStyle.Top,AutoSize=false,Height=0,Visible=false,ForeColor=ShopTheme.Muted,Font=small };

        info=new TableLayoutPanel { Dock=DockStyle.Top,AutoSize=true,ColumnCount=2,Padding=new Padding(0,4,0,10) };
        info.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,128));info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
        foreach(var (key,label) in new[]{("KhachHang","Khách hàng"),("SDT","Điện thoại"),("Email","Email"),("DiaChi","Địa chỉ"),
            ("Giam","Giảm HĐ / voucher"),("Tong","Tổng thanh toán"),("LyDo","Lý do hủy")})
        {
            bool total=key=="Tong";
            var name=new Label { Text=label,AutoSize=true,ForeColor=ShopTheme.Muted,Font=small,Margin=new Padding(0,5,6,5) };
            var value=new Label { AutoSize=true,Margin=new Padding(0,total?3:5,0,total?3:5),
                Font=total?new Font("Segoe UI",13,FontStyle.Bold):Font,ForeColor=total?ShopTheme.Green:ShopTheme.Ink };
            int row=info.RowCount++;info.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            info.Controls.Add(name,0,row);info.Controls.Add(value,1,row);values[key]=value;
        }
        info.SizeChanged+=(_,_)=>
        {
            int w=Math.Max(80,info.ClientSize.Width-info.Padding.Horizontal-128-4);
            foreach(var v in values.Values)v.MaximumSize=new Size(w,0);
            CapNhatCuon();
        };
        header.SizeChanged+=(_,_)=>CapNhatCuon();

        cardsTitle=new Label { Dock=DockStyle.Top,Text="Sản phẩm trong đơn",AutoSize=false,Height=30,TextAlign=ContentAlignment.BottomLeft,
            Font=new Font("Segoe UI",11,FontStyle.Bold),ForeColor=ShopTheme.Green };
        cards=new FlowLayoutPanel { Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,BackColor=Color.White,Padding=new Padding(0,6,0,0) };
        cards.SizeChanged+=(_,_)=>XepThe();

        // Thứ tự thêm: Fill trước, các Top sau (Top thêm sau nằm cao hơn).
        detailBody.Controls.Add(cards);detailBody.Controls.Add(cardsTitle);detailBody.Controls.Add(info);detailBody.Controls.Add(thongTin);detailBody.Controls.Add(header);
        detailRoot.Controls.Add(detailBody);detailRoot.Controls.Add(emptyState);
        host.Controls.Add(detailRoot);
    }
    // TV1: panel chi tiết tự cuộn khi cao không đủ: luôn chừa tối thiểu ~240px cho danh sách sản phẩm.
    private void CapNhatCuon()
    {
        if(info==null || cards==null)return;
        int need=detailBody.Controls.Cast<Control>().Where(c=>c!=cards).Sum(c=>c.Height)+240+detailRoot.Padding.Vertical;
        detailRoot.AutoScrollMinSize=new Size(0,need);
    }
    // TV1: trạng thái trống của panel chi tiết (chưa chọn đơn / không có đơn).
    private void HienTrong(string text)
    {
        emptyState.Text=text;emptyState.Visible=true;detailBody.Visible=false;XoaThe();
    }
    // TV1: badge trạng thái đơn, cùng bảng màu với cột Trạng thái của lưới.
    private void DatBadge(string status)
    {
        badge.Text=status;
        var (back,fore)=status.Contains("hủy")?(Color.FromArgb(255,235,232),Color.Firebrick):
            status.Contains("Chờ")?(Color.FromArgb(255,243,214),Color.FromArgb(135,82,6)):(Color.FromArgb(219,239,228),ShopTheme.Green);
        badge.BackColor=back;badge.ForeColor=fore;
    }
    private void XoaThe()
    {
        cards.SuspendLayout();
        foreach(Control c in cards.Controls.Cast<Control>().ToArray()) { cards.Controls.Remove(c);c.Dispose(); }
        cards.ResumeLayout();
    }
    // TV1: mỗi dòng chi tiết thành một thẻ: tên, mã · SL × đơn giá, giảm %, thành tiền.
    private void DungThe(DataTable detail)
    {
        cards.SuspendLayout();XoaThe();
        foreach(DataRow row in detail.Rows)
        {
            decimal discount=row["GiamGia"] is DBNull?0:Convert.ToDecimal(row["GiamGia"]);
            var table=new TableLayoutPanel { Dock=DockStyle.Top,AutoSize=true,ColumnCount=2 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            var name=new Label { Text=Convert.ToString(row["TenSP"]),AutoSize=true,Font=new Font("Segoe UI",10,FontStyle.Bold),Margin=new Padding(0,0,0,2),Name="TenSP" };
            var line=new Label { Text=$"Mã {row["MaSP"]}  ·  SL {row["SoLuong"]} × {Convert.ToDecimal(row["DonGia"]):#,##0} đ",AutoSize=true,ForeColor=ShopTheme.Muted,Font=new Font("Segoe UI",9),Margin=new Padding(0,0,0,4) };
            var disc=new Label { Text=discount>0?$"Giảm {discount:0.##}%":"Không giảm",AutoSize=true,ForeColor=discount>0?Color.FromArgb(135,82,6):ShopTheme.Muted,Font=new Font("Segoe UI",9),Anchor=AnchorStyles.Left,Margin=new Padding(0,6,8,0) };
            var money=new Label { Text=$"{Convert.ToDecimal(row["ThanhTien"]):#,##0} đ",AutoSize=true,Font=new Font("Segoe UI",11,FontStyle.Bold),ForeColor=ShopTheme.Green,Anchor=AnchorStyles.Right,Margin=new Padding(0) };
            table.Controls.Add(name,0,0);table.SetColumnSpan(name,2);
            table.Controls.Add(line,0,1);table.SetColumnSpan(line,2);
            table.Controls.Add(disc,0,2);table.Controls.Add(money,1,2);
            var card=new Panel { Name="TheSanPham",Tag=row,BorderStyle=BorderStyle.FixedSingle,BackColor=Color.FromArgb(248,251,249),Padding=new Padding(10,8,10,8),Margin=new Padding(0,0,0,8) };
            card.Controls.Add(table);cards.Controls.Add(card);
        }
        cards.ResumeLayout();XepThe();
    }
    // TV1: thẻ luôn rộng bằng vùng cuộn; chữ tên sản phẩm tự xuống dòng thay vì bị cắt.
    private void XepThe()
    {
        if(cards.Controls.Count==0)return;
        cards.SuspendLayout();
        int w=Math.Max(120,cards.ClientSize.Width-cards.Padding.Horizontal);
        foreach(Panel card in cards.Controls.OfType<Panel>())
        {
            var table=(TableLayoutPanel)card.Controls[0];int inner=w-card.Padding.Horizontal-2;
            foreach(var l in table.Controls.OfType<Label>())l.MaximumSize=new Size(inner,0);
            card.Width=w;table.PerformLayout();card.Height=table.PreferredSize.Height+card.Padding.Vertical+2;
        }
        cards.ResumeLayout();
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
            // CO-04: lưới chỉ giữ mã đơn + tên/SĐT khách ghép từ KhachHang; email, địa chỉ hồ sơ xem ở dòng thông tin khi chọn đơn.
            foreach(var cot in new[]{"Email","DiaChiKhachHang"})if(grid.Columns.Contains(cot))grid.Columns[cot]!.Visible=false;
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
        const string noSupport="CSDL chưa hỗ trợ cập nhật trạng thái SV4; hiện chỉ xem / xuất đơn.";
        if(Sv4Ui.MaDong(grid,"MaHD") is not int ma) { HienTrong("Không có đơn hàng trong khoảng lọc."+(!supported?"\n"+noSupport:""));return; }
        var r=((DataRowView)grid.CurrentRow!.DataBoundItem).Row;
        string Or(object? v){ var s=Convert.ToString(v);return string.IsNullOrWhiteSpace(s)?"—":s; }
        emptyState.Visible=false;detailBody.Visible=true;
        maHd.Text=$"HĐ {ma}";DatBadge(Convert.ToString(r["TrangThai"])??"");
        values["KhachHang"].Text=Or(r["KhachHang"]);values["SDT"].Text=Or(r["SDT"]);values["Email"].Text=Or(r["Email"]);values["DiaChi"].Text=Or(r["DiaChiKhachHang"]);
        values["Giam"].Text=$"{r["TienGiamGia"]:N0} đ";values["Tong"].Text=$"{r["TongTien"]:N0} đ";values["LyDo"].Text=Or(r["LyDoHuy"]);
        XoaThe();ThongBao("Đang tải chi tiết…");
        var scope=$"{DbHelper.ConnectionString}|{PhienDangNhap.MaTK}|{PhienDangNhap.VaiTro}|{DbHelper.DauHieuDuLieu}";
        try
        {
            var detail=await Task.Run(()=>details.Lay(ma,scope,()=>new HoaDonRepository().LayChiTietHienThi(ma)));
            if(IsDisposed || version!=detailVersion)return;
            DungThe(detail);ThongBao(grid.DataSource is DataTable table && table.ExtendedProperties["HoTroLyDoHuy"] is false?noSupport:"");
        }
        catch(Exception ex) { if(!IsDisposed && version==detailVersion)ThongBao(LoiCsdl.ThongBao(ex)); }
    }
    // TV1: dòng thông báo nhỏ trong panel chi tiết (đang tải / lỗi / CSDL chưa hỗ trợ); rỗng thì ẩn.
    private void ThongBao(string text)
    {
        thongTin.Text=text;thongTin.Visible=text.Length>0;
        thongTin.Height=text.Length==0?0:TextRenderer.MeasureText(text,thongTin.Font,new Size(Math.Max(120,thongTin.Width),0),TextFormatFlags.WordBreak).Height+8;
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
