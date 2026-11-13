using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using BadmintonShop.Core.DataAccess;

namespace BadmintonShop.Core.Services;

/// <summary>Cau hinh SMTP (doc tu muc "Email" trong cau hinh, mat khau dat bang user-secrets - KHONG commit).</summary>
public class EmailCauHinh
{
    public bool Bat { get; set; }
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string TaiKhoan { get; set; } = "";        // vd. shop@gmail.com
    public string MatKhau { get; set; } = "";         // Gmail: "Mat khau ung dung" (App Password) 16 ky tu
    public string TenNguoiGui { get; set; } = "BadmintonShop";
}

/// <summary>Gui email xac nhan don hang cho khach (SV3). Loi gui mail KHONG lam hong don hang: chi tra ve false.</summary>
public static class EmailService
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    public static EmailCauHinh CauHinh { get; set; } = new();

    public static bool DaCauHinh
        => CauHinh.Bat && !string.IsNullOrWhiteSpace(CauHinh.TaiKhoan) && !string.IsNullOrWhiteSpace(CauHinh.MatKhau);

    // TV3: gui email xac nhan don; chua cau hinh / khach khong co email / loi SMTP -> false, khong nem loi
    public static async Task<bool> GuiXacNhanDonAsync(int maHD, int maKH)
    {
        if (!DaCauHinh) return false;
        try
        {
            var khach = new KhachHangRepository().LayTheoMa(maKH);
            var donRepo = new DonHangOnlineRepository();
            var don = donRepo.LayDonCuaKhach(maHD, maKH);
            if (khach == null || don == null || string.IsNullOrWhiteSpace(khach.Email)
                || KhachHangRepository.LaEmailVangLai(khach.Email)) return false;
            var dong = donRepo.LayChiTietHienThi(maHD, maKH);

            using var mail = new MailMessage
            {
                From = new MailAddress(CauHinh.TaiKhoan, CauHinh.TenNguoiGui, Encoding.UTF8),
                Subject = $"Xác nhận đơn hàng #{maHD} - {CauHinh.TenNguoiGui}",
                Body = XayDungNoiDung(khach.HoTen, maHD, don.HinhThucTT, don.DiaChiGiao, don.TienGiamGia, don.TongTien, dong),
                IsBodyHtml = true,
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
            };
            mail.To.Add(khach.Email);

            using var smtp = new SmtpClient(CauHinh.Host, CauHinh.Port)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(CauHinh.TaiKhoan, CauHinh.MatKhau),
                Timeout = 20000,
            };
            await smtp.SendMailAsync(mail);
            return true;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("[Email] Khong gui duoc email don #" + maHD + ": " + ex.Message);
            return false;
        }
    }

    // TV3: noi dung HTML (da ma hoa ky tu dac biet de tranh chen HTML tu ten / dia chi)
    private static string XayDungNoiDung(string hoTen, int maHD, string hinhThucTT, string? diaChi,
                                         decimal tienGiam, decimal tongTien, DataTable dong)
    {
        string E(string? s) => WebUtility.HtmlEncode(s ?? "");
        string Tien(decimal x) => x.ToString("N0", Vi) + " ₫";

        var sb = new StringBuilder();
        sb.Append("<div style=\"font-family:Segoe UI,Arial,sans-serif;max-width:560px\">");
        sb.Append($"<h2 style=\"color:#13533f\">Cảm ơn bạn đã đặt hàng!</h2>");
        sb.Append($"<p>Xin chào <b>{E(hoTen)}</b>, đơn hàng <b>#{maHD}</b> của bạn đã được ghi nhận và đang chờ cửa hàng xác nhận.</p>");
        sb.Append("<table style=\"width:100%;border-collapse:collapse\">");
        sb.Append("<tr style=\"background:#f4f7f6\"><th align=\"left\">Sản phẩm</th><th>SL</th><th align=\"right\">Thành tiền</th></tr>");
        foreach (DataRow r in dong.Rows)
            sb.Append($"<tr><td>{E(Convert.ToString(r["TenSP"]))}</td><td align=\"center\">{r["SoLuong"]}</td>" +
                      $"<td align=\"right\">{Tien(Convert.ToDecimal(r["ThanhTien"]))}</td></tr>");
        sb.Append("</table><hr/>");
        if (tienGiam > 0) sb.Append($"<p>Giảm giá đơn hàng: <b>-{Tien(tienGiam)}</b></p>");
        sb.Append($"<p style=\"font-size:18px\">Tổng thanh toán: <b>{Tien(tongTien)}</b></p>");
        sb.Append($"<p>Giao tới: {E(diaChi)}<br/>Thanh toán: {(hinhThucTT == "COD" ? "khi nhận hàng (COD)" : "online")}</p>");
        sb.Append("<p style=\"color:#64756e;font-size:12px\">Bạn có thể theo dõi trạng thái đơn trong mục “Đơn hàng của tôi” trên website.</p>");
        sb.Append("</div>");
        return sb.ToString();
    }
}
