using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;
using Microsoft.Data.SqlClient;

internal static partial class Program
{
    // This mode only reads the configured store. It never submits save/payment/status actions.
    private static void UiReadOnly(bool useCurrentDatabase=false)
    {
        var connection = useCurrentDatabase?DbHelper.ConnectionString:Environment.GetEnvironmentVariable("ConnectionStrings__BadmintonShop")
            ?? XDocument.Load(Path.Combine(root,"BadmintonShop.Desktop/App.config")).Descendants("add")
                .Single(x => (string?)x.Attribute("name") == "BadmintonShop").Attribute("connectionString")!.Value;
        var builder = new SqlConnectionStringBuilder(connection) { ConnectTimeout = 5 };
        DbHelper.ConnectionString = builder.ConnectionString;
        Test("GUI read-only SQL connection", () => { using var c = DbHelper.TaoKetNoi(); c.Open(); });
        if (failed > 0) return;
        PhienDangNhap.BatDau(1,1,"ui-readonly","Kiểm tra giao diện",VaiTro.Admin);
        using var main = new FrmMain();
        main.WindowState = FormWindowState.Normal; main.Size = new Size(1366, 768); main.Show(); Pump(main);
        var screens = new[] { ("Tổng quan",typeof(FrmTongQuan)),("Sản phẩm",typeof(FrmSanPham)),("Danh mục",typeof(FrmDanhMuc)),("Đơn hàng",typeof(FrmDonHang)),("Bán tại quầy",typeof(FrmPOS)),("Thống kê",typeof(FrmThongKe)) };
        foreach (var (title,type) in screens) Test("GUI open/read " + title, () =>
        {
            Controls(main).OfType<Button>().Single(b => b.Text.Trim().EndsWith(title)).PerformClick(); Pump(main);
            var page = Controls(main).OfType<Form>().Single(f => f.GetType()==type);
            True(page.Visible);
            var error=Controls(page).OfType<Label>().FirstOrDefault(l => l.Text.StartsWith("Lỗi:"));
            if(error!=null) throw new Exception(error.Text);
            foreach (var size in new[] { new Size(1366,768),new Size(1920,1080) })
            {
                main.Size=size;Application.DoEvents();
                // Record the size Windows actually permits; never label a capped bitmap as 1920x1080.
                if(main.Size!=size)Ghi($"LIMIT {type.Name}: requested {size.Width}x{size.Height}, actual {main.Width}x{main.Height}; target size UNVERIFIED");
                File.AppendAllText(Path.Combine(output,"render-sizes.tsv"),$"{type.Name}\t{size.Width}x{size.Height}\t{main.Width}x{main.Height}\n");
                using var bitmap=new Bitmap(main.Width,main.Height);main.DrawToBitmap(bitmap,new Rectangle(Point.Empty,bitmap.Size));
                bitmap.Save(Path.Combine(output,$"{type.Name}-{main.Width}x{main.Height}.png"));
            }
            main.Size=new Size(1366,768);
        });
        Test("GUI single instance / POS cart survives navigation",()=>
        {
            Button Link(string name)=>Controls(main).OfType<Button>().Single(b=>b.Text.Trim().EndsWith(name));
            Link("Bán tại quầy").PerformClick();Pump(main);
            var pos=Controls(main).OfType<FrmPOS>().Single();
            var grids=Controls(pos).OfType<DataGridView>().ToList();
            var product=grids.Single(g=>g.DataSource is not BindingList<DongPOS>);
            if(product.Rows.Count==0) { Ghi("POS cart check skipped: no products in store.");return; }
            var available=product.Rows.Cast<DataGridViewRow>().FirstOrDefault(r=>Convert.ToInt32(r.Cells["SoLuongTon"].Value)>0);
            if(available==null) { Ghi("POS cart check skipped: no stock.");return; }
            product.CurrentCell=available.Cells["TenSP"];
            Controls(pos).OfType<Button>().Single(b=>b.Text=="Thêm vào giỏ").PerformClick();
            Pump(main);
            var cart=(BindingList<DongPOS>)grids.Single(g=>g.DataSource is BindingList<DongPOS>).DataSource!;
            Eq(1,cart.Count);Link("Sản phẩm").PerformClick();Link("Bán tại quầy").PerformClick();Eq(1,cart.Count);
            Eq(1,Controls(main).OfType<FrmPOS>().Count());
            Controls(pos).OfType<Button>().Single(b=>b.Text=="Bỏ dòng giỏ").PerformClick();Pump(main);Eq(0,cart.Count);
        });
        Test("GUI category draft survives navigation",()=>
        {
            var link=Controls(main).OfType<Button>().Single(b=>b.Text.Trim().EndsWith("Danh mục"));link.PerformClick();Pump(main);
            var category=Controls(main).OfType<FrmDanhMuc>().Single();
            var name=Controls(category).OfType<TextBox>().Single(t=>t.MaxLength==100);
            var previous=name.Text;name.Text="Bản nháp kiểm tra - không lưu";
            Controls(main).OfType<Button>().Single(b=>b.Text.Trim().EndsWith("Tổng quan")).PerformClick();link.PerformClick();
            Eq("Bản nháp kiểm tra - không lưu",name.Text);name.Text=previous;
        });
        Test("GUI product editor read-only",()=>
        {
            using var editor=new FrmSanPhamSua();editor.Show();Pump(editor);
            True(!Controls(editor).OfType<Label>().Any(l=>l.Text.StartsWith("Lỗi:")));
            using var bmp=new Bitmap(editor.Width,editor.Height);editor.DrawToBitmap(bmp,new Rectangle(Point.Empty,bmp.Size));
            bmp.Save(Path.Combine(output,"FrmSanPhamSua.png"));editor.Close();
        });
        Test("GUI manager sidebar permissions",()=>
        {
            PhienDangNhap.BatDau(1,1,"ui-manager","Kiểm tra quản lý",VaiTro.QuanLy);
            using var manager=new FrmMain();manager.WindowState=FormWindowState.Normal;manager.Show();Pump(manager);
            var buttons=Controls(manager).OfType<Button>().Select(b=>b.Text.Trim()).ToList();
            foreach(var name in new[]{"Sản phẩm","Danh mục","Đơn hàng","Bán tại quầy","Thống kê","Phiếu nhập"})True(buttons.Any(t=>t.EndsWith(name)));
            True(!buttons.Any(t=>t.EndsWith("Nhân viên, tài khoản") || t.EndsWith("Sao lưu, phục hồi")));
            manager.Close();PhienDangNhap.BatDau(1,1,"ui-readonly","Kiểm tra giao diện",VaiTro.Admin);
        });
        // Font/layout scaling is a simulation; actual per-monitor DPI still requires a Windows setting check.
        Test("GUI simulated scale 125/150 percent",()=>
        {
            foreach(float factor in new[]{1.25f,1.5f})
            {
                main.Scale(new SizeF(factor,factor));main.Size=new Size(1366,768);Application.DoEvents();
                using var bmp=new Bitmap(main.Width,main.Height);main.DrawToBitmap(bmp,new Rectangle(Point.Empty,bmp.Size));
                bmp.Save(Path.Combine(output,$"Scale-simulation-{factor:0.00}.png"));
                main.Scale(new SizeF(1/factor,1/factor));
            }
        });
        main.Close();PhienDangNhap.DangXuat();
        Test("GUI employee sidebar permissions",()=>
        {
            PhienDangNhap.BatDau(1,1,"ui-readonly","Kiểm tra phân quyền",VaiTro.NhanVien);
            using var employee=new FrmMain();employee.WindowState=FormWindowState.Normal;employee.Show();Pump(employee);
            var buttons=Controls(employee).OfType<Button>().Select(b=>b.Text.Trim()).ToList();
            True(buttons.Any(t=>t.EndsWith("Bán tại quầy")));True(buttons.Any(t=>t.EndsWith("Đơn hàng")));
            True(!buttons.Any(t=>t.EndsWith("Sản phẩm") || t.EndsWith("Thống kê") || t.EndsWith("Nhân viên, tài khoản")));
            employee.Close();PhienDangNhap.DangXuat();
        });
    }
    private static void Pump(Form main)
    {
        var until=DateTime.UtcNow.AddSeconds(40);
        var theme=typeof(FrmMain).Assembly.GetType("BadmintonShop.Desktop.ShopTheme")!;
        var busy=theme.GetMethod("IsBusy",BindingFlags.Public|BindingFlags.Static)!;
        do
        {
            Application.DoEvents();Thread.Sleep(25);
            if(!Controls(main).OfType<Form>().Append(main).Any(f=>(bool)busy.Invoke(null,[f])!))
            { Application.DoEvents();Thread.Sleep(150);Application.DoEvents();if(!Controls(main).OfType<Form>().Append(main).Any(f=>(bool)busy.Invoke(null,[f])!))return; }
        }while(DateTime.UtcNow<until);
        throw new TimeoutException("UI load exceeded 40 seconds.");
    }
}
