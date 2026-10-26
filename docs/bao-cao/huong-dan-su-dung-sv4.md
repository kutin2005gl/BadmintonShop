# Hướng dẫn sử dụng SV4 — 26/10/2026

## Mở trong Visual Studio và thiết lập

Mở `D:\CodexWorktrees\BadmintonShop-sv4\BadmintonShop.sln` trên nhánh `feature/sv4-final-integration`, đặt `BadmintonShop.Desktop` làm Startup Project, Build rồi F5. Cần Windows, .NET 8 Desktop Runtime/SDK và SQL Server. Mở đúng worktree ổ D, không sửa/build bản dự phòng ổ C. Kết quả/commit trước PR và lệnh chạy demo hiện có: [rà soát cuối](../final-integration/pr-review/README.md).

Cả web và desktop ưu tiên biến môi trường `ConnectionStrings__BadmintonShop`; nếu không có thì đọc appsettings.json/App.config tương ứng. Giữ cấu hình cá nhân ở máy. Có thể kiểm schema mà không ghi dữ liệu:

```powershell
dotnet run --project tests/BadmintonShop.SV4.Checks -- --schema-readonly
```

Ngày 26/10, cấu hình hiện tại trỏ `localhost / BadmintonShop`; kết nối thực tế `DESKTOP-FGE20L2`. Thiếu `HoaDon.LyDoHuy`, `MaYeuCauPOS`, `TienKhachDua`, `DauVanTayPOS` và index `UX_HoaDon_MaYeuCauPOS`. Ba trigger vẫn bật. Công cụ chỉ SELECT metadata, không sửa/seed CSDL; xem [kết quả](../final-integration/schema-readonly.txt). Máy khác cần chạy lại kiểm tra.

Với CSDL hiện có cần giữ: SV1 sao lưu/review rồi áp dụng **04 → 06** bằng SSMS hoặc sqlcmd. Hai migration có `USE BadmintonShop`; phải kiểm tên đích trước Execute. Không chạy script 01 hoặc SQL tổng trên CSDL đang có dữ liệu. Không chạy demo trên CSDL cửa hàng.

Cài máy trống: script tổng đã gồm schema nền, trigger, dữ liệu mẫu, 04 và 06; có đoạn DROP/CREATE nên chỉ dùng cho môi trường bỏ được. Bộ kiểm tra cài mới tạo CSDL `BadmintonShop_SV4_Test_Install_*`, bỏ hoàn toàn đoạn DROP/CREATE và đổi `USE` sang tên test.

Kho ảnh: đặt `BadmintonShop__MediaRoot` tới cùng thư mục bền vững cho web/desktop, ví dụ `D:\BadmintonShopMedia`; desktop cần ghi, web cần đọc và mount `/media/products`. CSDL chỉ lưu `products/<uuid>.jpg`. Chưa mount/fallback ảnh trên web là việc SV2 cần xác minh.

## Thao tác chính

- **Danh mục/Sản phẩm:** tìm/lọc, thêm/sửa, chọn ảnh, ngừng bán. Tồn 0 khi tạo; dùng Nhập hàng → Phiếu nhập của SV1 để tăng tồn, không sửa tồn trực tiếp. Cảnh báo hết = 0; sắp hết = 1–5.
- **Đơn hàng:** lọc ngày/trạng thái/loại/mã hoặc thông tin khách. Chọn đơn để xem giá và giảm đã lưu; xác nhận → giao → hoàn thành. Nhân viên hủy trước giao phải nhập lý do. Đơn đã thay đổi cần tải lại. Xuất PDF từ đơn đã lưu.
- **POS:** sản phẩm bên trái, giỏ bên phải. Chọn số lượng → Thêm vào giỏ; sửa/bỏ dòng trước thanh toán. Nhập voucher rồi Áp dụng hoặc Bỏ mã. Sau giảm SP, chọn bậc hóa đơn hoặc voucher giảm nhiều hơn; bằng nhau ưu tiên voucher; không cộng hai ưu đãi cấp hóa đơn. Voucher thua bậc hóa đơn không bị trừ lượt.
- **Thanh toán:** kiểm tiền hàng, giảm SP, giảm hóa đơn/voucher, tổng, tiền khách đưa và tiền thiếu/thừa. Nút khóa khi đang xử lý hoặc chưa đủ điều kiện. Giá/ưu đãi thay đổi: xem báo giá mới rồi xác nhận lại, không cần bỏ giỏ. Chỉ báo thành công sau transaction. Khi lỗi kết nối chưa rõ kết quả: giữ nguyên giỏ/tiền/voucher/token để retry hoặc tra Đơn hàng; không bấm Giỏ mới rồi bán lại ngay.
- **Báo cáo:** chọn khoảng/loại bán → Xem báo cáo. Chỉ Hoàn thành tạo doanh thu và số lượng; trạng thái đếm cả đơn hủy. Phân kỳ theo NgayLap. Excel/PDF dùng đúng kết quả đang hiển thị; sửa bộ lọc chưa tải không làm đổi file. PDF hóa đơn dùng giá/giảm/tổng đã lưu.
- **Điều hướng:** chuyển mục giữ giỏ và bản nháp. Đóng/đăng xuất cảnh báo dữ liệu chưa lưu và chặn khi có tác vụ chạy. ADMIN có mọi mục; QUANLY trừ nhân viên/tài khoản và sao lưu; NHANVIEN chỉ đơn hàng/POS (ngoài các mục phiên cá nhân).

## CSDL demo và kịch bản 5–7 phút

Dùng CSDL riêng tên `BadmintonShop_SV4_Demo*` hoặc `BadmintonShop_SV4_Test_*`, có đủ schema/trigger/04/06. Có thể dùng CSDL cài mới test do lệnh dưới tạo; tài khoản mẫu được seed chỉ ở CSDL này. Chuyển chuỗi kết nối của cả hai app bằng biến môi trường phiên rồi mở lại app. Không tạo tài khoản/cửa hậu trong CSDL thật.

```powershell
dotnet run --project tests/BadmintonShop.SV4.Checks -- --install-checks --server .
# Đọc tên CSDL test vừa in; tuyệt đối không thay bằng CSDL cửa hàng:
sqlcmd -S . -E -C -I -b -f 65001 -d '<TEN_CSDL_TEST_VUA_TAO>' -i database/05_SV4_Demo.sql
$env:ConnectionStrings__BadmintonShop='Server=.;Database=<TEN_CSDL_TEST_VUA_TAO>;Integrated Security=True;TrustServerCertificate=True;'
dotnet run --project BadmintonShop.Desktop
```

Script 05 chỉ nhận CSDL có tiền tố test/demo trên, nhập kho trước bán, 12 tháng đơn, nhãn `[SV4-DEMO]`; khóa transaction và sentinel chống thêm trùng. Có giảm vợt 10%, bậc đơn từ 1 triệu giảm 5% và `SV4DEMO15` giảm 15%, hiệu lực 30 ngày kể từ lần seed đầu, 30 lượt. Chạy lại không gia hạn chương trình đã có.

| Thời gian | Thao tác và điểm cần thấy |
| --- | --- |
| 0:00–1:00 | ADMIN/QUANLY thêm sản phẩm `[DEMO]`, chọn ảnh; tồn 0. |
| 1:00–2:00 | Phiếu nhập SV1 chọn nhà cung cấp, nhập hàng vừa tạo; lưu rồi kiểm tồn tăng. |
| 2:00–3:30 | POS chọn vợt `[SV4-DEMO]`, áp `SV4DEMO15`: một vợt 500.000 → giảm SP 50.000 → voucher 67.500 → trả 382.500; đưa 400.000 thừa 17.500. Lưu và xuất PDF. |
| 3:30–5:00 | Nếu SV2 login/catalog sẵn sàng: khách đặt COD, desktop thấy cùng mã/chi tiết rồi xác nhận/giao/hoàn thành; web cập nhật trạng thái. Nếu thiếu UI SV2, dùng đơn `[SV4-DEMO]` để minh họa trạng thái và nói rõ đó là dữ liệu demo, không nhận đã đi web E2E. |
| 5:00–6:30 | Thống kê mở từ 13 tháng trước đến hôm nay; lọc Online/Tại quầy, xem biểu đồ và xuất XLSX/PDF. Đơn hoàn thành tính doanh thu; hủy không tính. |

## Kiểm tra và bằng chứng

```powershell
dotnet build BadmintonShop.sln
dotnet run --project tests/BadmintonShop.SV4.Checks -- --integration --server .
python tests/BadmintonShop.SV4.Checks/verify_integration_exports.py '<thu-muc-ket-qua>'
```

Test ghi riêng `BadmintonShop_SV4_Test_*`, không tự xóa CSDL. Kết quả và giới hạn xem [biên bản](../final-integration/README.md); checklist SV2 chưa được đánh dấu thay người kiểm. Ảnh render thực tế 1366×768 và 1556×884; 1920×1080 chưa xác minh do giới hạn cửa sổ. Scale mô phỏng không thay thế kiểm DPI Windows thật, hộp thoại, Designer hoặc in giấy.
