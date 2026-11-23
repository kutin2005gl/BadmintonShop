using BadmintonShop.Web.Models;
using BadmintonShop.Web.Infrastructure;
using BadmintonShop.Web.Services;
using Microsoft.AspNetCore.Authorization;
using BadmintonShop.Core.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace BadmintonShop.Web.Controllers;

public class SanPhamController : Controller
{
    private readonly BadmintonShop.Web.Services.DanhMucCongKhai catalog;
    public SanPhamController(BadmintonShop.Web.Services.DanhMucCongKhai catalog) => this.catalog = catalog;
    private readonly SanPhamRepository _sanPhamRepository = new();

    // Repository lấy danh mục từ SQL Server
    private readonly DanhMucRepository _danhMucRepository = new();
    private readonly DanhGiaRepository _danhGiaRepository = new();
    // Giá khách thực sự thấy trên thẻ sản phẩm (đã trừ khuyến mãi nếu có)
    private static decimal GiaHienThi(BadmintonShop.Core.Models.SanPham sp)
        => KhuyenMaiHienThi.GiaSauGiam(sp.DonGiaBan, KhuyenMaiHienThi.LayPhanTram(sp.MaSP));

    // GET: /SanPham
    [HttpGet]
    public IActionResult Index(
        string? tuKhoa,
        int? maDM,
        string? sapXep,
        string? thuongHieu,
        string? mucGia,
        int trang = 1, bool lamMoi = false)
    {
        const int soSanPhamMoiTrang = 8;

        // 1. Lấy danh mục từ SQL Server
        var snapshot = catalog.Lay(lamMoi);
        var danhMuc = snapshot.Categories;

        // 2. Lấy sản phẩm theo từ khóa và danh mục
        // Khoảng giá sinh từ giá thực tế của sản phẩm đang bán (xem MucGia.Tao)
        var dsMucGia = MucGia.Tao(snapshot.Products
            .Where(sp => sp.TrangThai == "Đang bán").Select(GiaHienThi));
        var muc = MucGia.DocKhoa(mucGia);
        mucGia = muc?.Khoa;

        var keyword = tuKhoa?.Trim();
        var danhSach = snapshot.Products.Where(sp =>
            (string.IsNullOrEmpty(keyword) || $"{sp.TenSP} {sp.ThuongHieu}".Contains(keyword, StringComparison.CurrentCultureIgnoreCase)) &&
            (!maDM.HasValue || sp.MaDM == maDM) &&
            (string.IsNullOrEmpty(thuongHieu) || string.Equals(sp.ThuongHieu, thuongHieu, StringComparison.CurrentCultureIgnoreCase)) &&
            // Lọc theo giá khách thực sự thấy (đã trừ khuyến mãi), khoảng [Tu, Den)
            (muc == null || muc.ChuaGia(GiaHienThi(sp)))).ToList();

        // 3. Chỉ hiển thị sản phẩm đang bán
        danhSach = danhSach
            .Where(sp => sp.TrangThai == "Đang bán")
            .ToList();

        // 4. Sắp xếp sản phẩm
        danhSach = sapXep switch
        {
            "gia-tang" => danhSach
                .OrderBy(sp => sp.DonGiaBan)
                .ThenBy(sp => sp.MaSP)
                .ToList(),

            "gia-giam" => danhSach
                .OrderByDescending(sp => sp.DonGiaBan)
                .ThenBy(sp => sp.MaSP)
                .ToList(),

            "ten-z-a" => danhSach
                .OrderByDescending(sp => sp.TenSP)
                .ThenBy(sp => sp.MaSP)
                .ToList(),

            // Mới nhất: mã sản phẩm lớn hơn = thêm vào sau
            "moi-nhat" => danhSach
                .OrderByDescending(sp => sp.MaSP)
                .ToList(),

            _ => danhSach
                .OrderBy(sp => sp.TenSP)
                .ThenBy(sp => sp.MaSP)
                .ToList()
        };

        // 5. Tính tổng số sản phẩm và tổng số trang
        int tongSanPham = danhSach.Count;

        int tongSoTrang = Math.Max(
            1,
            (int)Math.Ceiling(
                tongSanPham / (double)soSanPhamMoiTrang
            )
        );

        // 6. Đảm bảo trang hợp lệ
        trang = Math.Clamp(trang, 1, tongSoTrang);

        // 7. Phân trang: 8 sản phẩm mỗi trang
        var sanPhamPhanTrang = danhSach
            .Skip((trang - 1) * soSanPhamMoiTrang)
            .Take(soSanPhamMoiTrang)
            .ToList();

        // 8. Truyền dữ liệu sang giao diện
        ViewBag.DanhMuc = danhMuc;

        ViewBag.TuKhoa = tuKhoa;
        ViewBag.MaDM = maDM;
        ViewBag.SapXep = sapXep;
        ViewBag.ThuongHieu = thuongHieu;
        ViewBag.MucGia = mucGia;
        ViewBag.DanhSachMucGia = dsMucGia;

        // Danh sách thương hiệu cho ô lọc (chỉ sản phẩm đang bán)
        ViewBag.DanhSachThuongHieu = snapshot.Products
            .Where(sp => sp.TrangThai == "Đang bán" && !string.IsNullOrWhiteSpace(sp.ThuongHieu))
            .Select(sp => sp.ThuongHieu!.Trim())
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        ViewBag.TrangHienTai = trang;
        ViewBag.TongSoTrang = tongSoTrang;
        ViewBag.TongSanPham = tongSanPham;

        return View(sanPhamPhanTrang);
    }

    // GET: /SanPham/ChiTiet/5
    [HttpGet]
    public IActionResult ChiTiet(int id)
    {
        var sanPham = _sanPhamRepository.LayTheoMa(id);

        if (sanPham == null || sanPham.TrangThai != "Đang bán")
        {
            return NotFound();
        }

        // Lấy các đánh giá đã được duyệt
        var danhSachDanhGia = _danhGiaRepository.LayTheoSanPham(id);

        ViewBag.DanhSachDanhGia = danhSachDanhGia;
        ViewBag.TongDanhGia = danhSachDanhGia.Count;

        ViewBag.SaoTrungBinh = danhSachDanhGia.Count > 0
            ? danhSachDanhGia.Average(dg => dg.SoSao)
            : 0.0;

        return View(sanPham);
    }


    // POST: /SanPham/GuiDanhGia
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult GuiDanhGia(DanhGiaViewModel model)
    {
        var maKH = User.LayMaKH();

        if (maKH == null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            TempData["LoiDanhGia"] =
                "Vui lòng kiểm tra số sao và nội dung đánh giá.";

            return RedirectToAction(
                "ChiTiet",
                new { id = model.MaSP }
            );
        }

        var sanPham = _sanPhamRepository.LayTheoMa(model.MaSP);

        if (!_danhGiaRepository.KhachDaMuaSanPham(maKH.Value, model.MaSP))
        {
            TempData["LoiDanhGia"] =
                "Chỉ khách đã mua sản phẩm này (đơn đã hoàn thành) mới được đánh giá.";

            return RedirectToAction("ChiTiet", new { id = model.MaSP });
        }

        if (sanPham == null || sanPham.TrangThai != "Đang bán")
        {
            return NotFound();
        }

        try
        {
            _danhGiaRepository.ThemDanhGia(
                model.MaSP,
                maKH.Value,
                model.SoSao,
                model.NoiDung
            );

            TempData["ThanhCongDanhGia"] =
                "Gửi đánh giá thành công! " +
                "Đánh giá của bạn đang chờ quản trị viên duyệt.";
        }
        catch (ArgumentException ex)
        {
            TempData["LoiDanhGia"] = ex.Message;
        }

        return RedirectToAction(
            "ChiTiet",
            new { id = model.MaSP }
        );
    }

}