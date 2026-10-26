using System.Diagnostics;
using System.Reflection;
using BadmintonShop.Desktop;

internal static partial class Program
{
    // Opt-in diagnostic driver invokes the real Desktop entry point. Login fixture is restricted to a test DB.
    private static System.Windows.Forms.Timer? lifecycleTimer;
    private static EventHandler? lifecycleIdle;
    public static string InstallLifecycleDriver()
    {
        var connection=Environment.GetEnvironmentVariable("ConnectionStrings__BadmintonShop")??"";
        if(!System.Text.RegularExpressions.Regex.IsMatch(connection,@"(?:^|;)(?:Database|Initial Catalog)\s*=\s*BadmintonShop_SV4_Test_\w+(?:;|$)",System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            throw new InvalidOperationException("Lifecycle debug requires the isolated SV4 test database.");
        if(output.Length==0) { output=Path.Combine(Path.GetTempPath(),"BadmintonShop-SV4-Checks",DateTime.Now.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(output); }
        var trace=Path.Combine(output,"first-chance.txt");
        void Write(string value) { lock(log)File.AppendAllText(trace,$"{DateTime.Now:O} [thread {Environment.CurrentManagedThreadId}] {value}\n"); }
        AppDomain.CurrentDomain.FirstChanceException+=(_,e)=> {
            if(e.Exception is InvalidOperationException)Write("THROWN "+e.Exception+"\nTHROW STACK\n"+Environment.StackTrace);
        };
        var timer=lifecycleTimer=new System.Windows.Forms.Timer { Interval=150 };
        int visits=0,logins=0;
        string[] menu=["Sản phẩm","Danh mục","Đơn hàng","Bán tại quầy","Thống kê","Tổng quan","Nhà cung cấp","Phiếu nhập","Nhân viên, tài khoản"];
        timer.Tick+=(_,_)=>
        {
            if(!Debugger.IsAttached || !File.Exists(Environment.GetEnvironmentVariable("BADMINTON_DEBUG_GATE")??""))return;
            var login=Application.OpenForms.OfType<FrmDangNhap>().FirstOrDefault(f=>f.Visible);
            if(login!=null)
            {
                Write("LOGIN "+(++logins));
                Controls(login).OfType<TextBox>().Single(c=>c.Name=="txtTenDangNhap").Text="sv4test";
                Controls(login).OfType<TextBox>().Single(c=>c.Name=="txtMatKhau").Text="TestOnly@123";
                Controls(login).OfType<Button>().Single(c=>c.Name=="btnDangNhap").PerformClick();return;
            }
            var main=Application.OpenForms.OfType<FrmMain>().FirstOrDefault(f=>f.Visible);
            if(main==null)return;
            if(visits<72)
            {
                // Reopen disposed cached pages as well as retaining POS. No business write actions are invoked.
                if(visits>=menu.Length)
                {
                    var old=Controls(main).OfType<Form>().FirstOrDefault(f=>f.Visible && f is not FrmPOS);
                    if(old!=null) { Write("CLOSE PAGE "+old.GetType().Name);old.Close(); }
                }
                string name=menu[visits++%menu.Length];Write("NAV "+name);
                Controls(main).OfType<Button>().Single(b=>b.Text.Trim().EndsWith(name)).PerformClick();
                return;
            }
            var theme=typeof(FrmMain).Assembly.GetType("BadmintonShop.Desktop.ShopTheme")!;
            var busy=theme.GetMethod("IsBusy",BindingFlags.Public|BindingFlags.Static)!;
            if(Controls(main).OfType<Form>().Any(f=>(bool)busy.Invoke(null,[f])!))return;
            if(logins==1) { Write("FORCED LOGOUT");visits=0;main.DangXuatBatBuoc();return; }
            Write("CLOSE MAIN");timer.Stop();main.Close();Write("DRIVER COMPLETE");
        };
        EventHandler start=(_,_)=> { if(!timer.Enabled)timer.Start(); };
        lifecycleIdle=start;Application.Idle+=start;
        if(Application.OpenForms.Count>0)timer.Start();
        return output;
    }
    private static int LifecycleDebug()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        InstallLifecycleDriver();
        void Write(string message)=>File.AppendAllText(Path.Combine(output,"first-chance.txt"),message+"\n");
        Console.WriteLine("DEBUG DRIVER PID="+Environment.ProcessId+" OUTPUT="+output);
        try
        {
            typeof(FrmMain).Assembly.GetType("BadmintonShop.Desktop.Program")!.GetMethod("Main",BindingFlags.NonPublic|BindingFlags.Static)!.Invoke(null,null);
            Write("EXIT OK");return 0;
        }
        catch(TargetInvocationException ex) { Write("ENTRY FAILED "+ex);Console.WriteLine(ex);return 1; }
        finally { Application.Idle-=lifecycleIdle;lifecycleTimer?.Dispose(); }
    }
}
