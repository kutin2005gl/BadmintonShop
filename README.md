# BadmintonShop – Hệ thống bán đồ thể thao cầu lông

## Phần SV4 (desktop)

Sau khi có CSDL nền, áp dụng **`database/04_SV4_Migration.sql`** (bổ sung, chạy lại được); sau đó `database/06_SV4_POS_Request.sql`; tùy chọn `database/05_SV4_Demo.sql` chỉ trên CSDL test/demo riêng 12 tháng. Không chạy lại script 01/file tổng trên CSDL cần giữ.

- [Hướng dẫn chạy và sử dụng SV4](docs/bao-cao/huong-dan-su-dung-sv4.md)
- [Đặc tả](docs/bao-cao/dac-ta-sv4.md), [quy ước tích hợp](docs/bao-cao/tich-hop-sv4.md), [sơ đồ lớp](docs/diagrams/06-lop-core-sv4.md)
- [Kế hoạch/test case](docs/bao-cao/kiem-thu-sv4.md), [kết quả thực tế](docs/bao-cao/ket-qua-kiem-thu-sv4.md), [tổng kết](docs/bao-cao/tong-ket-sv4.md)

POS đã tích hợp khuyến mãi dùng chung SV3 (sản phẩm, hóa đơn, voucher), kiểm lại trong transaction. Cần migration `04_SV4_Migration.sql` và `06_SV4_POS_Request.sql`; không tự chạy trên CSDL cửa hàng. Xem [tích hợp SV3–SV4 và demo](docs/bao-cao/tich-hop-sv3-sv4.md). Ảnh cần cấu hình `BadmintonShop__MediaRoot` dùng chung cho web/desktop; CSDL chỉ lưu `products/<uuid>.jpg`.

Đồ án môn Công nghệ phần mềm – ĐH Sài Gòn, HK1 2026–2027 (nhóm 4 người).

- **BadmintonShop.Core** – Class Library dùng chung: Models, DataAccess (ADO.NET), Services (SV1)
- **BadmintonShop.Web** – ASP.NET Core MVC cho khách hàng (SV2, SV3)
- **BadmintonShop.Desktop** – WinForms cho nhân viên / quản lý (SV1, SV4)
- **database/** – `BadmintonShop.sql` (file bàn giao, chạy 1 lần trên máy trống) = nối `01_CreateDatabase.sql` → `02_Triggers.sql` → `03_SampleData.sql` → migration `04` → `06` → `07`
- **docs/** – ERD, phân công; **[hướng dẫn cài đặt](docs/bao-cao/huong-dan-cai-dat.md)** (bản mới nhất), các chương báo cáo trong `docs/bao-cao/`

## Chạy nhanh

1. SSMS: mở và chạy `database/BadmintonShop.sql` (hoặc chạy lần lượt 01 → 02 → 03 → 04 → 06 → 07 trên CSDL mới).
   Ai sửa 01/02/03/04/06/07 thì ghép lại file tổng (Git Bash): `cd database && { cat 01_CreateDatabase.sql; sed '1s/^\xEF\xBB\xBF//' 02_Triggers.sql; sed '1s/^\xEF\xBB\xBF//' 03_SampleData.sql; cat 04_SV4_Migration.sql; cat 06_SV4_POS_Request.sql; cat 07_SV2_DanhGiaSanPham.sql; } > BadmintonShop.sql`
   (hoặc nhắn SV1 ghép).
2. Sửa `Server=` trong `BadmintonShop.Web/appsettings.json` và `BadmintonShop.Desktop/App.config` cho khớp máy mình (`localhost` hoặc `.\SQLEXPRESS`). **Không commit** phần sửa này nếu có mật khẩu.
3. Mở `BadmintonShop.sln` → Build → chạy Web hoặc Desktop.

Tài khoản test: `admin / Admin@123`, `quanly / Quanly@123`, `nhanvien / Nhanvien@123` (desktop).

## Quy tắc Git

- `main`: bản ổn định để nộp/demo. `develop`: nhánh tích hợp. Mỗi người làm trên `feature/svX-ten-chuc-nang` tách từ `develop`.
- Mọi thay đổi vào `develop` qua **Pull Request**; SV1 duyệt (SV4 duyệt thay khi SV1 bận).
- Commit nhỏ, rõ: `feat(sv3): ap ma giam gia theo voucher`, `fix(sv1): ...`, `docs(sv2): ...`.
- **Không viết SQL trong Controller/Form** – chỉ viết trong Repository của Core.
- Sau tuần 1: chỉ được **thêm** bảng/cột, không đổi tên hay xóa cột đã có. Lược đồ chốt: [`docs/ERD.md`](docs/ERD.md).

## Quy tắc viết Repository (Core/DataAccess)

- **Mỗi người tự tạo `XxxRepository.cs` cho bảng mình phụ trách**, copy khuôn `NhaCungCapRepository.cs`:
  hằng `Cot`, SQL có tham số `@ten` (không cộng chuỗi), 1 hàm `Map(DataRow)` riêng.
- Đã có sẵn (SV1): `NhaCungCap`, `NhanVien`, `TaiKhoan`, `PhieuNhap` (+ chi tiết, lưu bằng transaction),
  và phần **đọc** dùng chung `SanPhamRepository.LayTatCa/LayTheoMa`, `DanhMucRepository.LayTatCa/LayTheoMa`.
  SV2/SV4 **thêm hàm ghi** (Them/Sua/Xoa) vào chính 2 file này, không tạo file trùng.
- Ghi nhiều bảng cùng lúc (HoaDon + ChiTietHoaDon) → dùng `DbHelper.ExecuteTransaction`, xem mẫu `PhieuNhapRepository.Them`.
- Tồn kho và `TongTien` do **trigger** cập nhật – code không tự cộng/trừ. Không đủ tồn → trigger ném lỗi, bắt `SqlException` rồi hiện
  `LoiCsdl.ThongBao(ex)` (Core/Services) → ra câu tiếng Việt dễ hiểu ("Sản phẩm không đủ tồn kho...", trùng dữ liệu, mất kết nối...).
- Mỗi hàm có **1 dòng comment ghi người làm + chức năng**, ví dụ: `// TV2: lay san pham theo thuong hieu`.
- Cần DataGridView có cột tên (JOIN)? Viết thêm hàm `LayDanhSachHienThi()` trả `DataTable`, giữ Model đúng 1–1 với bảng.

## Đăng nhập & phân quyền Desktop (SV1)

- Chạy app → `FrmDangNhap` → `FrmMain`. Đăng xuất quay lại `FrmDangNhap`. (`nhanvien2` là tài khoản bị khóa để demo.)
- Người đang đăng nhập: `PhienDangNhap.MaNV`, `.HoTen`, `.VaiTro` (vd. phiếu nhập/hóa đơn lấy `MaNV` từ đây).
- Kiểm quyền: `PhienDangNhap.CoQuyen(ChucNang.Xxx)` – bảng quyền nằm trong `Core/Services/PhanQuyen.cs`, không so chuỗi vai trò tay.
- `FrmMain` đã có đủ menu (ẩn/hiện theo vai trò). Làm xong form của mình: trong `FrmMain.Designer.cs` đổi
  `mnuXxx.Click += mnuChuaCo_Click;` thành handler mở form, vd `MoForm<FrmSanPham>(ChucNang.SanPhamDanhMucKhuyenMai);`
  (MoForm tự chặn nếu không đủ quyền). SV4 chỉnh giao diện `FrmMain` thoải mái, giữ hàm `ApDungPhanQuyen()`.
- Quy tắc Form (để VS Designer mở được): constructor chỉ gọi `InitializeComponent()`, **nạp dữ liệu trong `OnLoad`**, mẫu:
  ```csharp
  protected override void OnLoad(EventArgs e)
  {
      base.OnLoad(e);
      if (DesignMode || !QuyenForm.KiemTraKhiMo(this, ChucNang.SanPhamDanhMucKhuyenMai)) return;
      NapLuoi();
  }
  ```

## Đăng nhập Web khách hàng (SV1 làm cơ chế – SV2 làm giao diện)

Cookie Authentication đã bật trong `Program.cs`. Chưa đăng nhập mà vào trang `[Authorize]` → tự chuyển về `/TaiKhoan/DangNhap?ReturnUrl=...`.
Khách bị khóa (`TrangThai = 0`) không đăng nhập được, đang đăng nhập mà bị khóa thì bị đăng xuất ở request kế tiếp.

- **Core:** `KhachHangRepository` – `EmailDaTonTai`, `DangKy(kh, matKhauTho)` (tự băm MK), `DangNhap(email, mk, out kh)` → `KetQuaDangNhap.ThanhCong / SaiThongTin / BiKhoa`, `DatMatKhau`. SV2 thêm hàm hồ sơ vào chính file này.
- **Web:** `Infrastructure/XacThucKhachHang.cs` – `HttpContext.DangNhapKhachHangAsync(kh, ghiNho)`, `HttpContext.DangXuatKhachHangAsync()`, `User.LayMaKH()`, `User.LayHoTen()`, `User.LayEmail()`.

**SV2** – `TaiKhoanController` (tên controller/action phải đúng `TaiKhoan/DangNhap`, `TaiKhoan/DangXuat`):
```csharp
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> DangNhap(string email, string matKhau, bool ghiNho, string? returnUrl)
{
    switch (new KhachHangRepository().DangNhap(email, matKhau, out var kh))
    {
        case KetQuaDangNhap.BiKhoa:    ModelState.AddModelError("", "Tài khoản đã bị khóa."); return View();
        case KetQuaDangNhap.SaiThongTin: ModelState.AddModelError("", "Sai email hoặc mật khẩu."); return View();
    }
    await HttpContext.DangNhapKhachHangAsync(kh!, ghiNho);
    return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
}
```
Đăng ký: kiểm `EmailDaTonTai` + `MatKhauHelper.LaMatKhauManh` trước, rồi `DangKy(...)` và (tuỳ) đăng nhập luôn.

**SV2/SV3** – trang cần đăng nhập (giỏ hàng, đặt hàng, hồ sơ, lịch sử đơn): thêm `[Authorize]` lên controller/action, lấy khách bằng `var maKH = User.LayMaKH()!.Value;`.
Trong View/Layout: `@if (User.Identity!.IsAuthenticated) { <span>Xin chào @User.LayHoTen()</span> }`.

## Chuỗi kết nối trên máy mình

Không muốn sửa file config? Đặt biến môi trường **User** `ConnectionStrings__BadmintonShop`
(vd `Server=(localdb)\MSSQLLocalDB;Database=BadmintonShop;Trusted_Connection=True;TrustServerCertificate=True;`),
mở lại Visual Studio – cả Web và Desktop đều ưu tiên biến này.
Chạy script bằng `sqlcmd` nhớ thêm `-I` (bật QUOTED_IDENTIFIER), vd `sqlcmd -S . -E -I -f 65001 -i database\BadmintonShop.sql`.

## Bàn giao SV4 26/10/2026

[Kết quả kiểm thử và giới hạn mới nhất](docs/final-integration/README.md), [kịch bản demo 5–7 phút](docs/bao-cao/huong-dan-su-dung-sv4.md), [checklist SV2 kiểm chéo](docs/bao-cao/checklist-sv2-desktop.md). `--schema-readonly` chỉ kiểm metadata CSDL cấu hình, không tự migration. CSDL máy kiểm tra còn thiếu 04/06 nên không nhận POS cửa hàng đã hoạt động trước khi được SV1 cập nhật.

## Đối chiếu yêu cầu môn học — 05/11/2026

Xem [báo cáo rà soát](docs/bao-cao/ra-soat-yeu-cau-2026-11-05.md). Các kết quả kiểm thử cũ chỉ chứng minh phạm vi đã kiểm, chưa chứng minh đủ 7 nhóm yêu cầu của giảng viên. Những phần bắt buộc còn thiếu cần được bổ sung trước khi coi là hoàn tất đồ án.

### Tiến độ yêu cầu môn học — đợt 2 (05/11/2026)

Đã bổ sung DB demo riêng, bộ nhớ/làm mới và tín hiệu giữa tiến trình, main–sub Desktop, tìm nâng cao, nhập Excel có xem trước/rollback, PDF nhiều năm chia phần. Phạm vi nghiệm thu, giả định và log kiểm tra nằm trong [báo cáo đợt 2](docs/bao-cao/quy-tac-va-tien-do-dot-2.md) và [checklist](docs/bao-cao/checklist-tien-do-4-thanh-vien.md). Không chạy lại script xóa/tạo DB hiện có để thử chức năng; dùng chế độ tạo DB thử có tên mới được hướng dẫn trong báo cáo.
