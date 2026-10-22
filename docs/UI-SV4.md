# Bàn giao giao diện SV4 — 22/10/2026

Đã triển khai trực tiếp giao diện WinForms .NET 8, không thêm thư viện giao diện. Không tìm thấy AGENTS.md trong project hoặc các thư mục cha áp dụng. Trước khi sửa, Git chỉ có thay đổi chưa commit ở `BadmintonShop.Desktop/App.config`; thay đổi đó được giữ nguyên. Không chạy script SQL, migration, seed, cập nhật dữ liệu cửa hàng, commit, push hoặc merge.

## Mở trong Visual Studio

1. Mở `BadmintonShop.sln` bằng Visual Studio có workload **.NET desktop development** và .NET 8 SDK.
2. Dừng phiên BadmintonShop cũ đang chạy bằng **Shift+F5**. Phiên cũ/Visual Studio đang khóa DLL trong thư mục build mặc định khi kiểm tra.
3. Đặt **BadmintonShop.Desktop** làm Startup Project, chọn Debug, Build Solution và F5.
4. Đăng nhập bằng tài khoản hiện có. Màn hình đầu là **Tổng quan**; chọn các mục trong sidebar. Cuộn sidebar để đến Hệ thống nếu cửa sổ thấp.
5. Chuỗi kết nối vẫn lấy từ biến `ConnectionStrings__BadmintonShop`, nếu không có thì dùng App.config. Không có chuỗi kết nối riêng nào được ghi đè bởi lần nâng cấp này.

## Giao diện và hành vi

- Sidebar xanh đậm chia theo nghiệp vụ, có biểu tượng, hover và mục đang chọn. Thanh trên có tên màn hình, họ tên, vai trò và đăng xuất. Mục Nhân sự, Nhập hàng, Nhà cung cấp, đổi mật khẩu, sao lưu/phục hồi vẫn truy cập được theo quyền cũ. Khuyến mãi và duyệt bình luận giữ thông báo đang phát triển như code ban đầu.
- Các trang được tạo một lần rồi giữ trong vùng làm việc. Chuyển menu không bỏ giỏ POS, bộ lọc hoặc bản nháp danh mục. Thoát/đăng xuất hỏi khi có giỏ/bản nháp chưa lưu và chờ tác vụ tải/ghi đang chạy. Hộp thoại sản phẩm hỏi trước khi bỏ thay đổi.
- Bộ style dùng chung: Segoe UI, xanh đậm, nền xám nhạt, khung trắng; nút chính, nút thường và thao tác xóa/hủy phân biệt; tiêu đề bảng, hàng xen kẽ, nhãn trạng thái có chữ và màu, tiền VNĐ và ngày `dd/MM/yyyy`.
- Sản phẩm: tìm kiếm, danh mục/thương hiệu/trạng thái/tồn kho, thêm/sửa/ngừng bán, xem ảnh bên phải và cảnh báo tồn thấp. Ảnh cũ thiếu/không đúng khóa kho ảnh không làm mất thông tin sản phẩm. Không nạp ảnh tùy ý từ URL/đường dẫn không hợp lệ.
- Danh mục: tìm tên/mô tả, thêm/lưu/xóa và bảo vệ bản nháp.
- Đơn hàng: lọc ngày/trạng thái/loại bán/từ khóa; danh sách và chi tiết, tổng tiền/giảm giá/lý do hủy; nút chuyển trạng thái theo luồng hợp lệ, xuất PDF.
- POS: sản phẩm bên trái, giỏ bên phải, tổng tiền/khách đưa/tiền thừa hoặc thiếu và nút thanh toán. Giữ giá thường/giới hạn khuyến mãi có sẵn, transaction, token chống lặp và kiểm tra tồn kho tại repository. Chưa bổ sung nghiệp vụ khuyến mãi mới.
- Báo cáo: bộ lọc, thẻ doanh thu/số đơn/số lượng bán, biểu đồ co theo chiều cao, các bảng và xuất Excel/PDF từ cùng kết quả báo cáo.
- Các truy vấn/tác vụ SV4 chạy qua worker, cập nhật control trên UI thread, có trạng thái tải/trống/lỗi. Truy vấn chi tiết/ảnh cũ không ghi đè lựa chọn mới. Các màn hình cũ của thành viên khác giữ cơ chế tải của chúng.
- Dock/TableLayoutPanel/FlowLayoutPanel và vùng cuộn giữ khả năng truy cập khi thiếu diện tích. Bảng nhiều cột dùng cuộn ngang; ô dài xuống dòng. Đã bật PerMonitorV2. Hộp thoại sản phẩm giới hạn kích thước theo vùng làm việc màn hình.

## Dashboard: dữ liệu và cách tính

- Doanh thu hôm nay: tổng `TongTien` của hóa đơn **Hoàn thành**, theo **NgayLap** hôm nay, tất cả loại bán, qua BaoCaoRepository/BaoCaoService.
- Đơn chờ xác nhận: tất cả đơn có trạng thái **Chờ xác nhận**, toàn thời gian đến hết hôm nay.
- Sản phẩm đang bán: đếm sản phẩm có trạng thái **Đang bán**.
- Tồn thấp/hết hàng: sản phẩm đang bán có tồn **0–5**, theo ngưỡng QuyTacBanHang; bao gồm cả hết hàng.
- Biểu đồ: doanh thu 7 ngày gần nhất, cùng quy tắc Hoàn thành; không có doanh thu thì hiển thị trạng thái trống.
- Đơn gần đây: tối đa 8 đơn mới nhất trong 30 ngày gần nhất. Danh sách tồn thấp phản ánh lần làm mới gần nhất.
- Tài khoản không có quyền báo cáo/sản phẩm không được tải các nhóm dữ liệu đó, các thẻ tương ứng ghi **Theo quyền**. Mốc ngày dùng ngày cục bộ máy chạy ứng dụng. Những nguồn repository được đọc lần lượt, không phải một transaction snapshot chung cho toàn dashboard.

## Giới hạn CSDL phát hiện khi chạy thật

CSDL cấu hình hiện tại kết nối được nhưng thiếu các trường của SV4 đã được script `database/04_SV4_Migration.sql` trong repository yêu cầu: `LyDoHuy`, và các trường phục vụ POS. **Không chạy migration hoặc đổi schema trong lần làm này.**

Đã thêm đọc tương thích trong HoaDonRepository: danh sách/hóa đơn cũ vẫn xem và xuất được khi thiếu các trường tùy chọn, không bịa lý do hủy. Các câu lệnh transaction/ghi trạng thái/kiểm giá/kiểm kho ban đầu được giữ nguyên. Giao diện thông báo và khóa thao tác không được CSDL hiện tại hỗ trợ; đặc biệt không cho thu tiền POS nếu thiếu các trường lưu POS an toàn. Người quản trị cần xử lý sự tương thích CSDL trong một công việc riêng trước khi dùng các thao tác này.

CSDL hiện tại không có đơn trong khoảng lọc kiểm tra, vì vậy dashboard/đơn hàng/báo cáo hiển thị trạng thái trống thật. Một số khóa ảnh cũ không có ảnh hợp lệ trong kho media dùng chung; giao diện hiện thông báo thay ảnh. Không thêm dữ liệu mẫu vào cửa hàng để tạo ảnh chụp.

## Xác minh

Build toàn solution thành công, **0 warning, 0 error**:

```powershell
dotnet build BadmintonShop.sln --no-restore -p:OutDir=D:\Desktop\BadmintonShop-UI-build\
dotnet D:\Desktop\BadmintonShop-UI-build\BadmintonShop.SV4.Checks.dll --ui-readonly
```

**25 kiểm tra đạt, 0 thất bại**. Gồm 13 kiểm tra có sẵn về validation, tiền, luồng trạng thái, quyền, phân bổ doanh thu, báo cáo rỗng, constructor, ảnh và định dạng xuất XLSX/PDF; cùng 12 kiểm tra kết nối chỉ đọc và WinForms. Chế độ mới chỉ đọc SQL, dùng phiên đăng nhập mô phỏng trong tiến trình kiểm tra để mở Form; không kiểm tra xác thực đăng nhập bằng mật khẩu và không gửi lệnh ghi dữ liệu.

Đã chạy các Form WinForms thật trong tiến trình kiểm tra trên Windows và bơm message loop: sáu màn hình chính, hộp thoại sản phẩm, chuyển trang, không tạo POS trùng, giữ giỏ, giữ bản nháp danh mục, và menu nhân viên. Thêm/bỏ dòng giỏ chỉ thao tác bộ nhớ, không thanh toán.

Ảnh trong [ui-sv4/screenshots](ui-sv4/screenshots) là **render từ control WinForms bằng DrawToBitmap**, gồm 1366×768, 1920×1080 và mô phỏng scale 125%/150%. Đây không phải ảnh chụp màn hình desktop; DrawToBitmap có thể không thể hiện đầy đủ scrollbar native. Đã rà ảnh trực quan. **Chưa thay đổi mức scale Windows hoặc kiểm thử chuyển màn hình DPI thực tế.** Khi scale lớn, vùng làm việc/bảng/sidebar có thanh cuộn để truy cập nội dung.

Chưa kiểm thử ghi sản phẩm/danh mục, thanh toán, giao/hủy đơn, transaction cạnh tranh hoặc phục hồi trên CSDL cửa hàng. Bộ integration ghi CSDL riêng có sẵn chưa chạy. Không khẳng định đã kiểm thử các thao tác này.

Log: [ui-sv4/results.txt](ui-sv4/results.txt).

## File thay đổi

| File | Nội dung |
| --- | --- |
| `BadmintonShop.Desktop/ShopTheme.cs` (mới) | Màu/font/nút/bảng, trạng thái tải và worker, giao ước bản nháp |
| `BadmintonShop.Desktop/Sv4Ui.cs` | Control chung, định dạng, màu trạng thái, trạng thái trống |
| `BadmintonShop.Desktop/FrmMain.cs`, `FrmMain.Shell.cs` (mới) | Tích hợp sidebar, quyền, cache trang, bảo vệ khi đóng |
| `BadmintonShop.Desktop/FrmTongQuan.cs` (mới) | Thẻ số liệu, biểu đồ, hai danh sách |
| `BadmintonShop.Core/Services/TongQuanService.cs` (mới) | Ghép dữ liệu thật qua repository/nghiệp vụ có sẵn |
| `BadmintonShop.Core/DataAccess/HoaDonRepository.cs` | Đọc hóa đơn tương thích các trường tùy chọn và kiểm tra khả năng POS |
| `BadmintonShop.Desktop/FrmSanPham.cs` | Style, bộ lọc, xem ảnh, tải/ghi không chặn UI |
| `BadmintonShop.Desktop/FrmSanPhamSua.cs` | Hộp thoại, tải/lưu, bảo vệ thay đổi, co theo màn hình |
| `BadmintonShop.Desktop/FrmDanhMuc.cs` | Tìm kiếm, bản nháp, tải/ghi |
| `BadmintonShop.Desktop/FrmDonHang.cs` | Tải danh sách/chi tiết, nút trạng thái, xuất PDF |
| `BadmintonShop.Desktop/FrmPOS.cs` | Bố cục hai cột, thanh toán/tiền thừa, giữ giỏ |
| `BadmintonShop.Desktop/FrmThongKe.cs`, `BieuDoCot.cs` | Thẻ tổng hợp, tải/xuất, biểu đồ tự điều chỉnh |
| `BadmintonShop.Desktop/Program.cs`, `BadmintonShop.Desktop.csproj` | Culture Việt Nam và PerMonitorV2 |
| `tests/BadmintonShop.SV4.Checks/Program.cs`, `UiReadOnly.cs` (mới) | Kiểm tra WinForms/SQL chỉ đọc và ảnh render |
| `docs/UI-SV4.md`, `docs/ui-sv4/*` (mới) | Hướng dẫn, log và ảnh bàn giao |

`App.config` vẫn là thay đổi có sẵn của người dùng; không nằm trong phần chỉnh sửa giao diện.
