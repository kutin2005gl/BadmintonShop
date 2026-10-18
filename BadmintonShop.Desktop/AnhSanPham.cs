using System.Drawing.Imaging;
using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

public static class AnhSanPham
{
    // SV4: chỉ nhận ảnh giải mã được, giới hạn dung lượng và diện tích trước khi sao bitmap.
    public static Bitmap Doc(string path)
    {
        if(new FileInfo(path).Length>10*1024*1024)throw new ArgumentException("Ảnh tối đa 10 MB.");
        Image image;
        try { image=Image.FromFile(path); }
        catch(Exception ex) when(ex is ArgumentException or OutOfMemoryException)
        { throw new ArgumentException("Ảnh không hợp lệ hoặc không thể giải mã.",nameof(path),ex); }
        using var decoded=image;
        if((long)image.Width*image.Height>30000000)throw new ArgumentException("Ảnh tối đa 30 triệu điểm ảnh.");
        return new Bitmap(image);
    }
    // SV4: tên sinh tự động, mã hóa JPEG; không đưa tên/đường dẫn gốc của người dùng vào CSDL.
    public static string Luu(Bitmap image)
    {
        var khoa=$"products/{Guid.NewGuid():N}.jpg";var path=KhoAnhSanPham.DuongDanDayDu(khoa)!;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);image.Save(path,ImageFormat.Jpeg);return khoa;
    }
}
