using System.Data;
using BadmintonShop.Core.DataAccess;

namespace BadmintonShop.Core.Services;

public sealed record TongQuanSnapshot(DateTime Ngay, decimal DoanhThu, int ChoXacNhan, int DangBan,
    DataTable SapHet, DataTable DonGanDay, DataTable DoanhThuNgay);

public static class TongQuanService
{
    // Read-only composition of the existing repositories; never writes store data.
    // Pending orders cover all dates. Revenue follows BaoCaoService (completed orders by NgayLap).
    public static TongQuanSnapshot Tai(DateTime ngay, bool baoCao, bool sanPham)
    {
        var repo = new HoaDonRepository();
        var orders = repo.LayDanhSachHienThi(ngay.Date.AddDays(-29), ngay.Date);
        var pending = repo.LayDanhSachHienThi(new DateTime(1753, 1, 1), ngay.Date, "Chờ xác nhận").Rows.Count;
        var recent = orders.Clone();
        foreach (DataRow row in orders.Rows.Cast<DataRow>().Take(8)) recent.ImportRow(row);
        var low = new DataTable(); int active = 0;
        if (sanPham)
        {
            var products = new SanPhamRepository().LayDanhSachHienThi(); low = products.Clone();
            foreach (DataRow row in products.Rows)
            {
                if (row.Field<string>("TrangThai") != "Đang bán") continue;
                active++;
                if (row.Field<int>("SoLuongTon") <= QuyTacBanHang.NguongSapHet) low.ImportRow(row);
            }
        }
        decimal revenue = 0; var chart = new DataTable();
        if (baoCao)
        {
            var report = BaoCaoService.Tao(new BaoCaoRepository().LayDuLieu(ngay.Date.AddDays(-6), ngay.Date, null), ngay.AddDays(-6), ngay, null);
            chart = report.Bang["Theo ngày"];
            revenue = chart.AsEnumerable().Where(r => r.Field<string>("Kỳ") == ngay.ToString("yyyy-MM-dd")).Sum(r => r.Field<decimal>("Doanh thu"));
        }
        return new(ngay.Date, revenue, pending, active, low, recent, chart);
    }
}
