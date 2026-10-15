using Microsoft.Data.SqlClient;

namespace BadmintonShop.Core.Services;

/// <summary>
/// Doi loi CSDL (SqlException, loi trigger...) thanh thong bao tieng Viet de hien cho nguoi dung - SV1.
/// Dung chung cho Desktop (MessageBox) va Web (ModelState / TempData), vd. SV3 bat loi dat hang:
///   catch (SqlException ex) { ModelState.AddModelError("", LoiCsdl.ThongBao(ex)); }
/// </summary>
public static class LoiCsdl
{
    // ma loi ket noi thuong gap: khong tim thay server/instance (LocalDB = 50), het thoi gian, dang nhap that bai, mat ket noi
    private static readonly int[] LoiKetNoi = { -1, 2, 26, 40, 50, 53, 64, 121, 233, 1225, 4060, 10054, 10060, 11001, 18456 };

    // TV1: tra ve thong bao tieng Viet de hieu cho 1 loi xay ra khi lam viec voi CSDL
    public static string ThongBao(Exception ex)
    {
        if (ex is not SqlException sql) return "Có lỗi xảy ra:\n" + ex.Message;

        // loi do trigger trong 02_Triggers.sql RAISERROR (tieng Viet khong dau)
        if (CoNoiDung(sql, "khong du ton kho"))
            return "Sản phẩm không đủ tồn kho. Vui lòng giảm số lượng hoặc nhập thêm hàng.";
        if (CoNoiDung(sql, "khong the mo lai don hang da huy"))
            return "Không thể mở lại đơn hàng đã hủy.";

        return sql.Number switch
        {
            2627 or 2601 => "Dữ liệu bị trùng với bản ghi đã có (ví dụ tên đăng nhập, email đã tồn tại).",
            547          => "Dữ liệu không hợp lệ hoặc đang được dùng ở nơi khác nên không thể lưu / xóa.",
            -2           => "Cơ sở dữ liệu phản hồi quá lâu. Vui lòng thử lại.",
            _ when LoiKetNoi.Contains(sql.Number) || sql.Class >= 20   // muc 20+ = loi nghiem trong / mat ket noi
                         => "Không kết nối được cơ sở dữ liệu. Kiểm tra SQL Server đã chạy và chuỗi kết nối.",
            _            => "Có lỗi khi làm việc với cơ sở dữ liệu:\n" + sql.Message,
        };
    }

    // TV1: kiem tra trong cac loi cua SqlException co loi nao chua doan noi dung can tim
    private static bool CoNoiDung(SqlException ex, string noiDung)
        => ex.Errors.Cast<SqlError>().Any(e => e.Message.Contains(noiDung, StringComparison.OrdinalIgnoreCase));
}
