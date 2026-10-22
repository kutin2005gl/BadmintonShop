namespace BadmintonShop.Core.Services;

// SV4: quy tắc dùng chung để desktop và web không chuyển trạng thái tùy ý.
public static class QuyTacBanHang
{
    public const int NguongSapHet = 5;
    public static readonly string[] TrangThaiDon = ["Chờ xác nhận", "Đã xác nhận", "Đang giao", "Hoàn thành", "Đã hủy"];

    // SV4: chỉ hủy trước khi giao; đơn tại quầy hoàn thành không được hủy bằng luồng giao hàng.
    public static bool DuocChuyen(string hienTai, string moi) => (hienTai, moi) switch
    {
        ("Chờ xác nhận", "Đã xác nhận") or ("Đã xác nhận", "Đang giao") or
        ("Đang giao", "Hoàn thành") or ("Chờ xác nhận", "Đã hủy") or ("Đã xác nhận", "Đã hủy") => true,
        _ => false
    };

    // SV4: kiểm tra trước truy cập CSDL, dùng lại trong kiểm thử.
    public static string BatBuoc(string? value, int max, string ten)
    {
        var s = value?.Trim() ?? "";
        if (s.Length == 0 || s.Length > max) throw new ArgumentException($"{ten} bắt buộc và tối đa {max} ký tự.");
        return s;
    }

    // SV4: khớp DECIMAL(18,0), không làm tròn giá âm/thập phân âm thầm.
    public static void KiemTraTien(decimal tien)
    {
        if (tien < 0 || tien > 999999999999999999m || decimal.Truncate(tien) != tien)
            throw new ArgumentException("Số tiền phải là số nguyên từ 0 đến 999999999999999999 đồng.");
    }
}
