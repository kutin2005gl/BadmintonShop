using BadmintonShop.Core.Services;

namespace BadmintonShop.Desktop;

/// <summary>
/// Chan mo form khi nguoi dang dang nhap khong du quyen (SV1 - ROADMAP 2.1).
/// An menu chua du: form van co the bi mo tu cho khac, nen moi form tu kiem lai trong OnLoad.
/// </summary>
internal static class QuyenForm
{
    // TV1: kiem tra quyen dung nhom chuc nang, khong du quyen thi bao va tra ve false
    public static bool DuQuyen(ChucNang chucNang)
    {
        if (PhienDangNhap.CoQuyen(chucNang)) return true;
        MessageBox.Show("Bạn không có quyền sử dụng chức năng này.", "Phân quyền",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return false;
    }

    // TV1: dung trong OnLoad cua form - khong du quyen thi bao va dong form, tra ve false
    public static bool KiemTraKhiMo(Form form, ChucNang chucNang)
    {
        if (DuQuyen(chucNang)) return true;
        form.BeginInvoke(form.Close);
        return false;
    }
}
