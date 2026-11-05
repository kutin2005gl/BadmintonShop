using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.Services;

// Shared revision marker avoids SQL polling between Web/Desktop processes on one machine.
// It contains only a random token. Different Windows users/hosts can configure a shared root.
internal static class TinHieuDuLieu
{
    private static string PathFor(string connection)
    {
        var builder=new SqlConnectionStringBuilder(connection);
        string database=$"{builder.DataSource.Trim().ToUpperInvariant()}|{builder.InitialCatalog.Trim().ToUpperInvariant()}";
        string name=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(database)));
        var root=Environment.GetEnvironmentVariable("BadmintonShop__CacheSignalRoot")??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"BadmintonShop","CacheSignals");
        return Path.Combine(root,name+".revision");
    }
    public static string Doc(string connection)
    {
        try { return File.ReadAllText(PathFor(connection)); }
        catch(FileNotFoundException) { return "initial"; }
        catch(DirectoryNotFoundException) { return "initial"; }
        catch(Exception ex) when(ex is IOException or UnauthorizedAccessException or ArgumentException)
        { return Guid.NewGuid().ToString("N"); } // If the marker cannot be read, bypass cache.
    }
    public static void Ghi(string connection)
    {
        string? temp=null;
        try
        {
            var path=PathFor(connection);Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
            File.WriteAllText(temp,Guid.NewGuid().ToString("N"));File.Move(temp,path,true);
        }
        catch(Exception ex) when(ex is IOException or UnauthorizedAccessException or ArgumentException)
        { System.Diagnostics.Trace.WriteLine("Cache signal unavailable; use refresh/expiry: "+ex.GetType().Name); }
        finally
        {
            if(temp!=null)try { File.Delete(temp); }catch(Exception ex) when(ex is IOException or UnauthorizedAccessException or ArgumentException) { }
        }
    }
}
