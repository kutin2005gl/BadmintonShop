using System.Data;
using BadmintonShop.Core.DataAccess;
using BadmintonShop.Core.Models;
using BadmintonShop.Core.Services;
using Microsoft.Data.SqlClient;
using ChiTietKM = BadmintonShop.Core.DataAccess.KhuyenMaiQuanLyRepository.ChiTietKhuyenMai;

// Muc 1.1 / 1.2: mot chuong trinh co ca ba nhom uu dai, luu trong mot giao dich; chon uu dai hoa don va ghi dung ma voucher.
internal static partial class Program
{
    private static CTKM Chuong(string ten = "Chương trình nhiều loại", int id = 0)
        => new() { MaCTKM = id, TenCTKM = ten, NgayBatDau = DateTime.Today.AddDays(-2), NgayKetThuc = DateTime.Today.AddDays(5), TrangThai = true };
    private static int Count(string sql, params SqlParameter[] p) => Convert.ToInt32(DbHelper.ExecuteScalar(sql, p));
    private static string Ma(string tag) => tag + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

    private static void MultiPromotionChecks()
    {
        if (!new SqlConnectionStringBuilder(DbHelper.ConnectionString).InitialCatalog.StartsWith("BadmintonShop_SV4_Test_", StringComparison.Ordinal))
            throw new InvalidOperationException("Test database required.");
        var qly = new KhuyenMaiQuanLyRepository();
        var pos = new HoaDonRepository(); var web = new DonHangOnlineRepository(); var km = new KhuyenMaiRepository();

        Test("M01 one program holds product + invoice + voucher under one MaCTKM", () =>
        {
            ResetOffers(); int p = Product(); string vc = Ma("M01");
            var ct = new ChiTietKM(); ct.SanPham.Rows.Add(p, 10m); ct.HoaDon.Rows.Add(500000m, 5m); ct.Voucher.Rows.Add(vc, 15m, 7, 0);
            int id = qly.Luu(Chuong(), ct);
            var back = qly.LayChiTietTatCa(id);
            Eq(1, back.SanPham.Rows.Count); Eq(1, back.HoaDon.Rows.Count); Eq(1, back.Voucher.Rows.Count);
            Eq(id, Count("SELECT MaCTKM FROM KM_TheoVoucher WHERE MaVoucher=@v", new SqlParameter("@v", vc)));
            Eq("HONHOP", qly.LayCTKM(id)!.LoaiKM);
            var row = qly.LayDanhSach().Rows.Cast<DataRow>().Single(r => r.Field<int>("MaCTKM") == id);
            Eq("Sản phẩm + Hóa đơn + Voucher", row.Field<string>("Loai")); Eq(3, row.Field<int>("SoMuc"));
        });

        Test("M02 invalid data in one tab rolls back the whole save", () =>
        {
            ResetOffers(); int p = Product(); string dup = Ma("M02");
            var first = new ChiTietKM(); first.Voucher.Rows.Add(dup, 10m, 3, 0); qly.Luu(Chuong("Giữ mã"), first);
            int programs = Count("SELECT COUNT(*) FROM CTKM"), spRows = Count("SELECT COUNT(*) FROM KM_TheoSP"), hdRows = Count("SELECT COUNT(*) FROM KM_TheoHD");
            var ct = new ChiTietKM(); ct.SanPham.Rows.Add(p, 10m); ct.HoaDon.Rows.Add(1000000m, 5m); ct.Voucher.Rows.Add(dup, 20m, 3, 0);
            Reject(() => qly.Luu(Chuong("Trùng mã"), ct));                       // ma voucher da thuoc chuong trinh khac
            Eq(programs, Count("SELECT COUNT(*) FROM CTKM")); Eq(spRows, Count("SELECT COUNT(*) FROM KM_TheoSP")); Eq(hdRows, Count("SELECT COUNT(*) FROM KM_TheoHD"));
            var sai = new ChiTietKM(); sai.SanPham.Rows.Add(p, 10m); sai.HoaDon.Rows.Add(1000000m, 150m);
            try { qly.Luu(Chuong("Sai %"), sai); throw new Exception("Expected rejection"); }
            catch (ArgumentException ex) { True(ex.Message.StartsWith("Tab Hóa đơn:")); }
            Eq(programs, Count("SELECT COUNT(*) FROM CTKM"));
            Reject(() => qly.Luu(Chuong("Rỗng"), new ChiTietKM()));              // it nhat mot nhom hop le
        });

        Test("M03 adding groups to an old single-type program", () =>
        {
            ResetOffers(); int p = Product(); int old = Offer("SANPHAM", 10, p);
            var ct = qly.LayChiTietTatCa(old); Eq(1, ct.SanPham.Rows.Count); Eq(0, ct.HoaDon.Rows.Count);
            ct.HoaDon.Rows.Add(300000m, 5m); string vc = Ma("M03"); ct.Voucher.Rows.Add(vc, 10m, 2, 0);
            Eq(old, qly.Luu(Chuong("Integration test", old), ct));
            var back = qly.LayChiTietTatCa(old); Eq(1, back.SanPham.Rows.Count); Eq(1, back.HoaDon.Rows.Count); Eq(1, back.Voucher.Rows.Count);
            Eq("HONHOP", qly.LayCTKM(old)!.LoaiKM);
            Eq(10m, new TinhKhuyenMaiService().LayPhanTramGiamSanPham(p, DateTime.Now));     // nhom san pham cua chuong trinh hon hop van duoc xet
            var q = Quote(p, 4, vc); Eq(vc, q.MaVoucher); Eq(old, q.MaCTKM!.Value);
        });

        Test("M04 threshold 2M/5%: voucher vs threshold, ties, boundary and below-threshold", () =>
        {
            ResetOffers(); int p = Product(2_500_000m), under = Product(1_500_000m), edge = Product(2_100_000m);
            string v10 = Ma("A"), v3 = Ma("B"), v5 = Ma("C");
            var ct = new ChiTietKM(); ct.SanPham.Rows.Add(p, 20m); ct.SanPham.Rows.Add(edge, 10m); ct.HoaDon.Rows.Add(2_000_000m, 5m);
            ct.Voucher.Rows.Add(v10, 10m, 50, 0); ct.Voucher.Rows.Add(v3, 3m, 50, 0); ct.Voucher.Rows.Add(v5, 5m, 50, 0);
            qly.Luu(Chuong("Ngưỡng 2 triệu"), ct);

            var big = Quote(p, 1, v10);                       // sau giam SP = 2.000.000 (dung bang nguong) -> dat nguong
            Eq(2_000_000m, big.TongTienHang); Eq(200_000m, big.TienGiamHoaDon); Eq(v10, big.MaVoucher); True(!big.VoucherHopLeNhungThua); Eq(1_800_000m, big.TongThanhToan);
            Eq(2_000_000m, big.TienHangTruocGiam - big.TienGiamSanPham);
            var small = Quote(p, 1, v3);                      // voucher nho hon -> chon giam nguong
            Eq(100_000m, small.TienGiamHoaDon); True(small.MaVoucher == null); True(small.VoucherHopLeNhungThua); True(small.LoiVoucher == null); Eq(v3, small.VoucherYeuCau);
            var tie = Quote(p, 1, v5);                        // bang nhau -> uu tien voucher
            Eq(100_000m, tie.TienGiamHoaDon); Eq(v5, tie.MaVoucher); True(!tie.VoucherHopLeNhungThua);
            var none = Quote(p, 1); Eq(100_000m, none.TienGiamHoaDon); True(none.MaVoucher == null); True(!none.VoucherHopLeNhungThua);

            var low = Quote(under, 1, v3);                    // 1,5 trieu: chua dat nguong -> chi dung duoc voucher
            Eq(45_000m, low.TienGiamHoaDon); Eq(v3, low.MaVoucher);
            Eq(0m, Quote(under, 1).TienGiamHoaDon);
            var miss = Quote(edge, 1);                        // 2,1 trieu giam SP con 1,89 trieu -> khong dat nguong
            Eq(1_890_000m, miss.TongTienHang); Eq(0m, miss.TienGiamHoaDon);
            Eq(94_500m, Quote(edge, 1, v5).TienGiamHoaDon);   // nhung voucher van dung duoc
        });

        Test("M05 only the winning code is recorded and charged", () =>
        {
            ResetOffers(); int p = Product(2_500_000m, 20); string win = Ma("W"), lose = Ma("L");
            var ct = new ChiTietKM(); ct.SanPham.Rows.Add(p, 20m); ct.HoaDon.Rows.Add(2_000_000m, 5m);
            ct.Voucher.Rows.Add(win, 10m, 5, 0); ct.Voucher.Rows.Add(lose, 3m, 5, 0);
            qly.Luu(Chuong("Ghi mã thắng"), ct);

            var loser = Quote(p, 1, lose); True(loser.MaVoucher == null);              // thua giam nguong
            int hdLose = web.TaoDon(customer, "Test address", "COD", loser);
            Eq(0, Used(lose)); True(km.LayVoucherCuaHoaDon(hdLose) == null);
            True(!km.KhachDaDungVoucher(customer, lose)); True(!km.KhachDaDungVoucher(customer, win));   // don chi dung giam nguong khong tinh la da dung voucher
            SameSaved(hdLose, loser);

            var winner = Quote(p, 1, win);
            int hdWin = web.TaoDon(customer, "Test address", "COD", winner);
            Eq(1, Used(win)); Eq(win, km.LayVoucherCuaHoaDon(hdWin)); True(km.KhachDaDungVoucher(customer, win)); Eq(0, Used(lose));
            SameSaved(hdWin, winner);
            Reject(() => web.TaoDon(customer, "Test address", "COD", Quote(p, 1, win)));      // cung khach khong dung lai ma
            Eq(1, Used(win));

            int posId = pos.ThanhToanPOS(nv, Quote(p, 1, win), 2_000_000, Guid.NewGuid());   // POS (khong theo khach) van ghi ma thang
            Eq(2, Used(win)); Eq(win, km.LayVoucherCuaHoaDon(posId));

            DbHelper.ExecuteNonQuery("UPDATE HoaDon SET TrangThai=N'Đã hủy', LyDoHuy=N'test' WHERE MaHD=@id", new SqlParameter("@id", hdWin));
            True(!km.KhachDaDungVoucher(customer, win));                                      // huy don thi duoc dung lai ma
        });

        Test("M06 voucher uses are decremented atomically and exhaustion aborts the order", () =>
        {
            ResetOffers(); int p = Product(100, 20); string vc = Ma("X");
            var ct = new ChiTietKM(); ct.Voucher.Rows.Add(vc, 10m, 1, 0); qly.Luu(Chuong("Một lượt"), ct);
            var q1 = Quote(p, 1, vc); var q2 = Quote(p, 1, vc);
            pos.ThanhToanPOS(nv, q1, 100, Guid.NewGuid());
            int orders = Orders(), stock = Stock(p);
            Reject(() => pos.ThanhToanPOS(nv, q2, 100, Guid.NewGuid()));
            Eq(orders, Orders()); Eq(stock, Stock(p)); Eq(1, Used(vc));
            Eq(1, Count("SELECT COUNT(*) FROM HoaDon_Voucher WHERE MaVoucher=@v", new SqlParameter("@v", vc)));
        });
    }
}
