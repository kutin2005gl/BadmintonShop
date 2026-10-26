using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace BadmintonShop.Desktop;

// Capture the first throw before a subsequent using/finally Dispose can mask its cause.
// Diagnostics observe events only: they never Close/Dispose, pump messages, or suppress an exception.
internal static class LifecycleDiagnostics
{
    private static readonly object Sync = new();
    private static readonly Queue<string> Recent = new();
    private static readonly ConditionalWeakTable<Form, object> Tracked = new();
    private static int installed;
    [ThreadStatic] private static bool writing;
    public static string LogPath { get; } = Path.Combine(
        Environment.GetEnvironmentVariable("BadmintonShop__LifecycleTraceDirectory")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BadmintonShop","Diagnostics"),
        $"lifecycle-{DateTime.Now:yyyyMMdd-HHmmss}-{Environment.ProcessId}.log");

    public static void Install()
    {
        if(Interlocked.Exchange(ref installed,1)!=0)return;
        AppDomain.CurrentDomain.FirstChanceException+=FirstChance;
        AppDomain.CurrentDomain.UnhandledException+=(_,e)=>Capture("UNHANDLED",e.ExceptionObject as Exception);
        Record($"START runtime={Environment.Version} module={typeof(FrmMain).Assembly.ManifestModule.ModuleVersionId}");
    }
    public static void Record(string message)
    {
        lock(Sync)
        {
            Recent.Enqueue($"{DateTime.Now:O} thread={Environment.CurrentManagedThreadId} {message}");
            while(Recent.Count>120)Recent.Dequeue();
        }
    }
    public static void Track(Form form)
    {
        if(Tracked.TryGetValue(form,out _))return;
        Tracked.Add(form,new object());
        void Event(string name)=>Record($"{form.GetType().Name} {name} handle={form.IsHandleCreated} disposed={form.IsDisposed} disposing={form.Disposing}");
        form.HandleCreated+=(_,_)=>Event("HandleCreated");
        form.HandleDestroyed+=(_,_)=>Event("HandleDestroyed");
        form.Load+=(_,_)=>Event("Load");form.Shown+=(_,_)=>Event("Shown");
        form.FormClosing+=(_,e)=>Event($"FormClosing reason={e.CloseReason} cancel={e.Cancel}");
        form.FormClosed+=(_,_)=>Event("FormClosed");form.Disposed+=(_,_)=>Event("Disposed");
        Event("Tracked");
    }
    private static void FirstChance(object? sender,FirstChanceExceptionEventArgs e)
    {
        if(e.Exception is InvalidOperationException &&
            (e.Exception.Message.Contains("CreateHandle",StringComparison.OrdinalIgnoreCase)
             || Environment.GetEnvironmentVariable("BadmintonShop__LifecycleTrace")=="1"))
            Capture("FIRST CHANCE",e.Exception);
    }
    private static void Capture(string kind,Exception? error)
    {
        if(writing)return;
        writing=true;
        try
        {
            lock(Sync)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(LogPath,$"\n{DateTime.Now:O} {kind} thread={Environment.CurrentManagedThreadId}\n"+
                    string.Join(Environment.NewLine,Recent)+$"\nEXCEPTION\n{error}\nTHROW STACK\n{Environment.StackTrace}\n");
            }
        }
        catch(Exception ex) when(ex is IOException or UnauthorizedAccessException)
        { Debug.WriteLine("Cannot write lifecycle diagnostics: "+ex); }
        finally { writing=false; }
    }
}
