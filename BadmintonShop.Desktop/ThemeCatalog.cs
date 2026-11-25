namespace BadmintonShop.Desktop;

// TV1: bảng màu và kiểu của giao diện (màu, mật độ bảng, phông tiêu đề).
internal sealed record ThemeDef(
    string TitleFont, int RowHeight,
    Color Bg, Color Surface, Color SurfaceAlt, Color Field, Color Ink, Color Muted, Color Line,
    Color Accent, Color OnAccent, Color AccentHover, Color AccentDown, Color AccentSoft,
    Color Danger, Color DangerBg, Color Warn, Color WarnBg, Color Highlight,
    Color TopBg, Color TopInk, Color NavBg, Color NavInk, Color NavMuted, Color NavHover, Color NavActive, Color NavActiveInk,
    Color HeadBg, Color HeadInk);

internal static class ThemeCatalog
{
    // TV1: viết tắt tạo màu RGB cho bảng màu
    private static Color C(int r, int g, int b) => Color.FromArgb(r, g, b);

    // Giấy: nền giấy ấm, mực xanh navy, thoáng như sổ sách; thanh bên sáng.
    public static readonly ThemeDef Giay = new("Georgia", 46,
        Bg: C(245,241,233), Surface: C(255,253,248), SurfaceAlt: C(250,246,238), Field: C(255,253,248),
        Ink: C(28,34,51), Muted: C(94,92,102), Line: C(226,219,205),
        Accent: C(30,50,92), OnAccent: C(255,253,248), AccentHover: C(48,74,128), AccentDown: C(18,32,66), AccentSoft: C(226,231,242),
        Danger: C(176,40,40), DangerBg: C(251,232,228), Warn: C(133,77,4), WarnBg: C(252,238,205), Highlight: C(205,70,35),
        TopBg: C(245,241,233), TopInk: C(28,34,51),
        NavBg: C(236,230,217), NavInk: C(28,34,51), NavMuted: C(88,84,78), NavHover: C(227,220,204), NavActive: C(255,253,248), NavActiveInk: C(28,34,51),
        HeadBg: C(236,230,217), HeadInk: C(28,34,51));
}
