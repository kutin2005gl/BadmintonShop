using System.Text;
using System.Text.RegularExpressions;
using BadmintonShop.Core.Services;

// Cach dung:
//   dotnet run --project tools/HashTool -- Admin@123 Khach@123        -> in chuoi bam
//   dotnet run --project tools/HashTool -- --patch database/03_SampleData.sql
//        -> thay moi <HASH_matkhau> trong file bang chuoi bam BCrypt that
if (args.Length == 2 && args[0] == "--patch")
{
    var duongDan = args[1];
    var noiDung = File.ReadAllText(duongDan);
    var dem = 0;
    noiDung = Regex.Replace(noiDung, "<HASH_(.+?)>", m => { dem++; return MatKhauHelper.BamMatKhau(m.Groups[1].Value); });
    File.WriteAllText(duongDan, noiDung, new UTF8Encoding(true));
    Console.WriteLine($"Da thay {dem} chuoi bam trong {duongDan}");
    return;
}

var dsMatKhau = args.Length > 0 ? args : new[] { "Admin@123", "Quanly@123", "Nhanvien@123", "Khach@123" };
foreach (var mk in dsMatKhau)
    Console.WriteLine($"{mk,-15} {MatKhauHelper.BamMatKhau(mk)}");
