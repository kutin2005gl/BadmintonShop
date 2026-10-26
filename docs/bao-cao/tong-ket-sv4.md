# Tổng kết SV4 — 26/10/2026

Nhánh `feature/sv4-final-integration`, nền `origin/develop` tại `920a576`. Tận dụng phần tích hợp đã có tại `6e7657a` trên `feature/sv4-desktop`; các chỉnh sửa lần này chưa commit. Không lấy số liệu kiểm tra ngày 18/10 hoặc 22/10 làm kết quả chạy ngày 26/10.

## Kết quả triển khai

- Sản phẩm/danh mục: CRUD, kiểm đầu vào, ảnh tương đối an toàn, ngừng bán giữ lịch sử, lọc/tìm/cảnh báo tồn. Sản phẩm mới tồn 0; nhập hàng qua chức năng SV1 và trigger.
- Đơn Online: so trạng thái kỳ vọng, xác nhận → giao → hoàn thành; nhân viên hủy trước giao, bắt buộc lý do. Khách chỉ hủy đúng đơn của mình khi Chờ xác nhận. Trigger hoàn kho đúng một lần.
- POS: cùng `GioHangService`/`TinhKhuyenMaiService` với web; giảm SP trước rồi chọn một ưu đãi cấp hóa đơn/voucher. Hiển thị tiền hàng, hai cấp giảm, tổng, tiền khách và tiền thừa. Mã sai/chưa đến hạn/hết hạn/hết lượt/ngừng áp dụng có lý do riêng.
- Thanh toán đọc lại giá, trạng thái bán, tồn, khuyến mãi trong Serializable transaction; chỉ lưu sau khi báo giá khớp. Voucher tăng lượt atomic cùng header/chi tiết. Không tự trừ tồn/tổng ngoài trigger.
- Retry POS dùng token và SHA-256 yêu cầu. Giao dịch đã lưu trả lại mã hóa đơn trước khi tính khuyến mãi hiện tại; đổi payload bị từ chối. Giữ nguyên giỏ/token khi lỗi; thành công mới xóa giỏ.
- Báo cáo có bộ lọc ngày/tháng/năm/khoảng, loại bán, bảng/biểu đồ và XLSX/PDF thật. Doanh thu chỉ Hoàn thành, lấy `TongTien` đã lưu; phân bổ danh mục không giảm hai lần.
- Sidebar, tải nền, cache trang và giữ bản nháp tiếp tục được dùng. Bảo vệ callback sau đóng và log vòng đời được giữ từ đợt điều tra trước.

## Kiểm chứng và giới hạn

Kết quả chạy mới, môi trường, ảnh và file xuất: [biên bản 26/10](../final-integration/README.md). CSDL test luôn có tên riêng; CSDL người dùng chỉ được đọc metadata. CSDL đang cấu hình còn thiếu migration 04/06, do đó cần cập nhật có chủ đích trước khi dùng POS tích hợp.

HTTP test chạy MVC/Kestrel thật với cookie/anti-forgery và đăng nhập fixture trong test host; không phải E2E trình duyệt đăng nhập/catalog SV2. Chưa có bằng chứng SV2 kiểm thử chéo; [checklist bàn giao](checklist-sv2-desktop.md) vẫn để trống.

Ảnh là `Show/OnLoad + DrawToBitmap` của control thật tại 1366×768 và 1556×884; 1920×1080 bị Windows giới hạn nên chưa xác minh. Scale 125%/150% là mô phỏng `Control.Scale`, chưa đổi DPI Windows, chưa xác minh Designer, máy in và mọi nhánh hộp thoại. Chưa fault injection mạng đúng khoảng sau commit/trước phản hồi, dù retry sau commit và rollback giữa transaction đã được kiểm tra.

Lỗi `Dispose/CreateHandle` chưa tái hiện ở đợt debugger 22/10; chưa xác định lời gọi gốc, không khẳng định đã sửa dứt điểm. Giữ [log và giới hạn điều tra](../lifecycle-diagnostics/README.md).

## Phối hợp và hướng phát triển

SV1 review migration, quyền và trigger trước triển khai; SV3 xác nhận quy tắc ưu đãi/hủy (hiện không hoàn lượt voucher khi hủy), email và idempotency web; SV2 hoàn thiện login/catalog/mount kho ảnh và kiểm thử chéo desktop. Không mở rộng trả hàng, thanh toán thật hoặc hóa đơn điện tử.

Doanh thu hiện phân kỳ theo NgayLap của đơn đang Hoàn thành; chưa có ngày hoàn thành và snapshot tên/danh mục. Ảnh dùng filesystem chung, chưa có object storage/dọn ảnh mồ côi. Query chạy nền trên desktop nhưng chưa benchmark dữ liệu lớn; Serializable có thể chặn ghi. Đây là giới hạn đồ án, không tuyên bố sẵn sàng vận hành thương mại.


## Rà cuối trước PR, sau chuyển ổ D

Worktree sử dụng: `D:\CodexWorktrees\BadmintonShop-sv4`, nhánh `feature/sv4-final-integration`. [Kết quả và commit mới](../final-integration/pr-review/README.md) có 74/74 integration, 15/15 install, kiểm độc lập 12 bộ lọc và demo cửa sổ thật. Đã sửa chiều cao tóm tắt POS để không cắt dòng giảm giá và giữ thông tin hóa đơn gần nhất sau tải lại. 1920×1080, DPI Windows thật, browser E2E và kiểm chéo SV2 vẫn chưa xác minh. Được phép commit/push/mở PR theo yêu cầu mới, không merge.
