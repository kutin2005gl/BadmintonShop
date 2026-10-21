using System.Globalization;

namespace BadmintonShop.Web.Infrastructure;

/// <summary>Dinh dang hien thi dung chung cho View cua SV3.</summary>
public static class DinhDang
{
    private static readonly CultureInfo Vi = new("vi-VN");

    // TV3: 8580000 -> "8.580.000 ₫"
    public static string Dong(this decimal so) => so.ToString("N0", Vi) + " ₫";

    // TV3: mau badge Bootstrap theo trang thai don
    public static string MauTrangThai(string trangThai) => trangThai switch
    {
        "Chờ xác nhận" => "bg-warning text-dark",
        "Đã xác nhận" => "bg-info text-dark",
        "Đang giao" => "bg-primary",
        "Hoàn thành" => "bg-success",
        "Đã hủy" => "bg-danger",
        _ => "bg-secondary",
    };
}
