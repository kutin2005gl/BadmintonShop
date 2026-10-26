using System.Reflection;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Services;
using BadmintonShop.Desktop;

internal static partial class Program
{
    private sealed class LifecyclePage : Form { }
    private static Task<bool> LoadProbe(Form form,Func<int> query,Action<int> bind)
    {
        var theme=typeof(FrmMain).Assembly.GetType("BadmintonShop.Desktop.ShopTheme")!;
        return (Task<bool>)theme.GetMethod("Load")!.MakeGenericMethod(typeof(int)).Invoke(null,[form,query,bind,null])!;
    }
    private static void AwaitUi(Func<bool> condition)
    {
        var until=DateTime.UtcNow.AddSeconds(20);
        while(!condition()) { Application.DoEvents();if(DateTime.UtcNow>until)throw new TimeoutException("Lifecycle operation did not complete.");Thread.Sleep(5); }
        Application.DoEvents();
    }
    private static FrmMain OpenLifecycleMain()
    {
        PhienDangNhap.BatDau(1,1,"sv4test","Lifecycle test",VaiTro.Admin);
        var main=new FrmMain { WindowState=FormWindowState.Normal,Size=new Size(1366,768) };main.Show();Pump(main);return main;
    }
    private static void ShowLifecyclePage(FrmMain main,Form page)=>typeof(FrmMain).GetMethod("HienTrang",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(main,[typeof(LifecyclePage),(Func<Form>)(()=>page)]);
    private static void LifecycleChecks()
    {
        var connection=Environment.GetEnvironmentVariable("ConnectionStrings__BadmintonShop")??"";
        if(!new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connection).InitialCatalog.StartsWith("BadmintonShop_SV4_Test_",StringComparison.Ordinal))throw new InvalidOperationException("Isolated database required.");
        DbHelper.ConnectionString=connection;Control.CheckForIllegalCrossThreadCalls=true;
        Environment.SetEnvironmentVariable("BadmintonShop__LifecycleTraceDirectory",output);
        var diagnostics=typeof(FrmMain).Assembly.GetType("BadmintonShop.Desktop.LifecycleDiagnostics")!;
        diagnostics.GetMethod("Install")!.Invoke(null,null);
        Test("L01 closed cache entry/current page removed and recreated",()=>
        {
            using var main=OpenLifecycleMain();var page=new LifecyclePage();ShowLifecyclePage(main,page);page.Close();
            True(page.IsDisposed);True(typeof(FrmMain).GetField("currentPage",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(main)==null);
            var pages=(System.Collections.IDictionary)typeof(FrmMain).GetField("pages",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(main)!;
            True(!pages.Contains(typeof(LifecyclePage)));var next=new LifecyclePage();ShowLifecyclePage(main,next);True(next.Visible && !ReferenceEquals(page,next));main.Close();
        });
        Test("L02 close page with an in-flight load: no bind after close",()=>
        {
            using var gate=new ManualResetEventSlim();using var entered=new ManualResetEventSlim();using var page=new LifecyclePage();page.Show();int calls=0;
            var load=LoadProbe(page,()=> { entered.Set();gate.Wait();return 1; },_=>calls++);
            try { AwaitUi(()=>entered.IsSet);page.Close();True(page.IsDisposed); }
            finally { gate.Set(); }
            AwaitUi(()=>load.IsCompleted);True(!load.GetAwaiter().GetResult());Eq(0,calls);
        });
        Test("L03 forced logout during load: old owner/page disposed, old callback skipped",()=>
        {
            using var gate=new ManualResetEventSlim();using var entered=new ManualResetEventSlim();using var main=OpenLifecycleMain();var page=new LifecyclePage();ShowLifecyclePage(main,page);int calls=0;
            var load=LoadProbe(page,()=> { entered.Set();gate.Wait();return 1; },_=>calls++);
            try { AwaitUi(()=>entered.IsSet);main.DangXuatBatBuoc();True(main.IsDisposed);True(page.IsDisposed); }
            finally { gate.Set(); }
            AwaitUi(()=>load.IsCompleted);True(!load.GetAwaiter().GetResult());Eq(0,calls);True(!PhienDangNhap.DaDangNhap);
        });
        Test("L04 hide during load retains page; callback runs on original UI thread",()=>
        {
            using var gate=new ManualResetEventSlim();using var page=new LifecyclePage();page.Show();var thread=Environment.CurrentManagedThreadId;int boundOn=0;
            var load=LoadProbe(page,()=> { gate.Wait();return 42; },n=> { Eq(42,n);boundOn=Environment.CurrentManagedThreadId; });
            page.Hide();gate.Set();AwaitUi(()=>load.IsCompleted);True(load.GetAwaiter().GetResult());Eq(thread,boundOn);page.Show();True(!page.IsDisposed);page.Close();
        });
        Test("L05 disposing hidden page with pending query releases callback",()=>
        {
            using var gate=new ManualResetEventSlim();var page=new LifecyclePage();page.Show();int calls=0;
            var load=LoadProbe(page,()=> { gate.Wait();return 1; },_=>calls++);page.Hide();page.Dispose();gate.Set();
            AwaitUi(()=>load.IsCompleted);True(!load.GetAwaiter().GetResult());Eq(0,calls);
        });
        Test("L06 diagnostic probe captures full first-chance stack (synthetic, not the reported failure)",()=>
        {
            const string probe="LIFECYCLE DIAGNOSTIC PROBE CreateHandle; not an application failure";
            bool caught=false;
            try { throw new InvalidOperationException(probe); }catch(InvalidOperationException ex)when(ex.Message==probe) { caught=true; }
            True(caught);var path=(string)diagnostics.GetProperty("LogPath")!.GetValue(null)!;var text=File.ReadAllText(path);
            True(text.Contains(probe) && text.Contains("THROW STACK") && text.Contains(nameof(LifecycleChecks)));
        });
        UiReadOnly(true);
    }
}
