using System.Data;
using System.Globalization;
using BadmintonShop.Core.DataAccess;

namespace BadmintonShop.Core.Services;

// TV1: doanh thu va so don cho xac nhan cua mot ngay trong ky dang xem
public sealed record DiemNgay(DateTime Ngay, decimal DoanhThu, int DonCho);

public sealed record TongQuanSnapshot(DateTime Ngay, decimal DoanhThu, decimal DoanhThuHomQua, int ChoXacNhan, int ChoMoiHomNay,
    int DangBan, int TongSanPham, DataTable SapHet, DataTable DonGanDay, IReadOnlyList<DiemNgay> TheoNgay,
    decimal TongKy, decimal TongKyTruoc, IReadOnlyDictionary<string, int> TrangThaiDon);

public static class TongQuanService
{
    // Read-only composition of the existing repositories; never writes store data.
    // Pending orders cover all dates. Revenue follows BaoCaoService (completed orders by NgayLap).
    // TV1: gom so lieu trang tong quan cho ky soNgay ngay gan nhat, kem ky lien truoc de so sanh
    public static TongQuanSnapshot Tai(DateTime ngay, bool baoCao, bool sanPham, int soNgay = 7)
    {
        if (soNgay < 1) throw new ArgumentOutOfRangeException(nameof(soNgay));
        var homNay = ngay.Date;
        var dauKy = homNay.AddDays(1 - soNgay);
        var repo = new HoaDonRepository();
        var orders = repo.LayDanhSachHienThi(homNay.AddDays(-Math.Max(30, soNgay) + 1), homNay);
        var pending = repo.LayDanhSachHienThi(new DateTime(1753, 1, 1), homNay, "Chờ xác nhận").Rows.Cast<DataRow>().ToList();
        var recent = orders.Clone();
        foreach (DataRow row in orders.Rows.Cast<DataRow>().Take(5)) recent.ImportRow(row);

        var trangThai = orders.AsEnumerable().Where(r => r.Field<DateTime>("NgayLap").Date >= dauKy)
            .GroupBy(r => r.Field<string>("TrangThai")!).ToDictionary(g => g.Key, g => g.Count());

        var low = new DataTable(); int active = 0, total = 0;
        if (sanPham)
        {
            var products = new SanPhamRepository().LayDanhSachHienThi(); low = products.Clone(); total = products.Rows.Count;
            var sapHet = new List<DataRow>();
            foreach (DataRow row in products.Rows)
            {
                if (row.Field<string>("TrangThai") != "Đang bán") continue;
                active++;
                if (row.Field<int>("SoLuongTon") <= QuyTacBanHang.NguongSapHet) sapHet.Add(row);
            }
            foreach (var row in sapHet.OrderBy(r => r.Field<int>("SoLuongTon"))) low.ImportRow(row);
        }

        var doanhThu = new Dictionary<DateTime, decimal>();
        if (baoCao)
        {
            // Lay mot lan ca ky truoc va it nhat tu hom qua, roi cat theo tung khoang
            var tu = homNay.AddDays(1 - soNgay * 2); if (tu > homNay.AddDays(-1)) tu = homNay.AddDays(-1);
            var report = BaoCaoService.Tao(new BaoCaoRepository().LayDuLieu(tu, homNay, null), tu, homNay, null);
            foreach (DataRow r in report.Bang["Theo ngày"].Rows)
                doanhThu[DateTime.ParseExact(r.Field<string>("Kỳ")!, "yyyy-MM-dd", CultureInfo.InvariantCulture)] = r.Field<decimal>("Doanh thu");
        }

        var theoNgay = Enumerable.Range(0, soNgay).Select(i => dauKy.AddDays(i))
            .Select(d => new DiemNgay(d, doanhThu.GetValueOrDefault(d), pending.Count(r => r.Field<DateTime>("NgayLap").Date == d))).ToList();
        var tongKyTruoc = doanhThu.Where(x => x.Key >= dauKy.AddDays(-soNgay) && x.Key < dauKy).Sum(x => x.Value);

        return new(homNay, doanhThu.GetValueOrDefault(homNay), doanhThu.GetValueOrDefault(homNay.AddDays(-1)),
            pending.Count, pending.Count(r => r.Field<DateTime>("NgayLap").Date == homNay), active, total, low, recent,
            theoNgay, theoNgay.Sum(x => x.DoanhThu), tongKyTruoc, trangThai);
    }
}
