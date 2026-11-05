# Kiểm chứng và bàn giao đợt 4

Ngày 05/11/2026. Người thực hiện: Codex. Tiếp nối đợt 3; không xác nhận thay TV1–TV4 hoặc giảng viên. Giữ nguyên thay đổi có sẵn; không reset, commit, push hoặc sửa cấu hình kết nối.

## Tiếp tục đơn Web đã đặt ở đợt 3

Đọc SQL lại trước khi dùng DB `BadmintonShop_SV4_Test_20261105161022_7f4ab55a`: #11 Hoàn thành, #12 Chờ xác nhận, thu 2025–2026 là 611. Không ghi vào DB nguyên mẫu `...154154_a5e27e92` hoặc DB acceptance `...162207_dc9ab928`.

Mở Web sản phẩm thật riêng (PID 26564, cổng 5279) và Desktop test host `--acceptance-desktop-review` (PID 28972), cùng DB thử nêu trên bằng biến môi trường riêng tiến trình. Desktop mở FrmMain thật bằng phiên Admin thử; **không kiểm chứng vòng đăng nhập Desktop sản phẩm**.

Các bước do công cụ điều khiển UI Windows thực hiện:

1. Mở Thống kê trước xử lý: kỳ 01/01/2026–05/11/2026, tất cả loại bán, tổng 200. Lưu [ảnh nền](minh-chung-dot-4/report-before.png).
2. Mở Đơn hàng, dòng #12, chi tiết Vợt thử, 1 × 100, tổng 100. Bấm Xác nhận, đọc MessageBox “Chờ xác nhận → Đã xác nhận”, bấm Yes: [ảnh](minh-chung-dot-4/confirm-12.png).
3. Bấm Giao hàng, đọc MessageBox “Đã xác nhận → Đang giao”, bấm Yes: [ảnh](minh-chung-dot-4/shipping-12.png).
4. Bấm Hoàn thành, đọc MessageBox “Đang giao → Hoàn thành”, bấm Yes: [hộp xác nhận](minh-chung-dot-4/complete-12.png), [kết quả](minh-chung-dot-4/completed-12.png).
5. Trở lại form báo cáo đang mở: vẫn giữ snapshot cũ 200 cho tới khi bấm Xem báo cáo. Bấm Xem báo cáo cùng điều kiện: tổng 300; dòng 05/11 tăng 100 → 200: [ảnh](minh-chung-dot-4/report-after.png). Bấm Làm mới: vẫn 300: [ảnh](minh-chung-dot-4/report-refresh.png). Không gán số truy vấn cho lần UI này vì không gắn bộ đếm; số 2/0/2 thuộc kiểm tra tự động đợt 3.
6. Đăng nhập khách thử trong trình duyệt vào Web sản phẩm: lịch sử #12 Hoàn thành, chọn dòng hiện chi tiết đúng #12/tổng 100/địa chỉ giả lập. [Ảnh Web](minh-chung-dot-4/web-space-rapid.png), [DOM](minh-chung-dot-4/web-space-rapid.txt).
7. [SQL đọc độc lập sau UI](minh-chung-dot-4/sql-after.txt): #12 Hoàn thành; thu 2025–2026 = **711**; kỳ UI = **300**. Hai số khác nhau vì kỳ UI không gồm năm 2025.

Kết quả: đã hoàn tất phần UI Desktop và báo cáo còn thiếu của **cùng đơn #12 được đặt qua UI Web ở đợt 3**. Chuỗi này nối tiếp hai buổi, không phải một lượt mới liên tục từ đăng nhập/đặt hàng. Không kiểm tra lại tồn trước/sau đặt #12; chưa xuất lại file từ snapshot 300; không dùng file 611 đợt 3 làm minh chứng snapshot mới. CHUNG-02 toàn nhóm vẫn mở để thành viên tự chạy trọn kịch bản và xác nhận.

## Main–sub và giao diện

**Trình duyệt thật qua công cụ UI**: Space trên dòng #12 hiển thị chi tiết #12, URL giữ `/DonHang`. Bấm nhanh dòng #11 rồi #12, cuối cùng chi tiết là #12. Không mô phỏng độ trễ/đảo thứ tự mạng trong lượt trình duyệt này. Một lệnh chờ heading dùng chữ hoa theo CSS đã timeout; đọc lại nội dung DOM xác nhận heading thực là “Đơn hàng #12 Hoàn thành”, không ghi timeout đó thành lỗi tải ứng dụng.

**Kiểm tra cục bộ script thật**: bổ sung `tests/BadmintonShop.SV4.Checks/order-main-sub.checks.cjs`, dùng VM và DOM/transport giả có điều khiển để chạy chính file sản phẩm. [Log 8/8](minh-chung-dot-4/main-sub-local.txt): phản hồi cũ đến sau; body cũ giải quyết sau; lỗi/finally cũ không xóa busy mới; mất mạng và chọn lại phục hồi; HTTP lỗi/chuyển đăng nhập không chèn body; Enter/Space và link con; click có phím sửa đổi; trang không có panel. Transport cố ý bỏ qua abort để xác minh chặn ghi đè bằng phiên bản yêu cầu. Đây không phải nghiệm thu mạng thật hoặc kiểm tra trình duyệt tự động.

**Quan sát bố cục**: ảnh Desktop ở khung 1920 × 1020, không đồng nhất với nghiệm thu toàn màn 1920 × 1080 hoặc mọi DPI. Nút thao tác và báo cáo đọc được; lưới đơn nhiều cột cần cuộn ngang, email dài bị rút gọn trong ô. Web ở viewport hẹp khoảng 390 px có thanh cuộn ngang cho bảng; chưa nhận đạt mọi kích thước, DPI hoặc bộ đọc màn hình. JavaScript tắt chưa kiểm tra bằng trình duyệt; link Xem có route chi tiết theo nguồn view, chỉ là rà soát mã.

## Tài liệu và các điểm chưa chốt

Đã chuẩn bị [bản tổng hợp 7 nhóm](bao-cao-ky-thuat-7-nhom.md) để đóng gói báo cáo và [hướng dẫn bàn giao](huong-dan-ban-giao-dot-4.md). Chưa có chữ ký, kết quả tự kiểm tra hoặc tập bảo vệ TV1–TV4.

Giữ doanh thu chỉ Hoàn thành theo NgayLap; chi là tiền nhập trong kỳ; thu trừ chi gọi Chênh lệch thu–chi. TTL 10 phút cấu hình được, phạm vi main–sub và nhập chỉ thêm mới vẫn là giả định chưa chốt.

Word/PDF/hình báo cáo cuối vẫn chờ chốt nội dung. Bộ runtime tài liệu được kiểm tra nhưng không có LibreOffice đóng gói trên Windows để thực hiện render và kiểm tra từng trang theo workflow; không xuất DOCX chưa qua kiểm tra hình thức rồi gọi là bản hoàn chỉnh. Ảnh minh chứng UI đã có, không thay cho các trang báo cáo.

## Tái lập phần mới

```powershell
node tests/BadmintonShop.SV4.Checks/order-main-sub.checks.cjs
```

Đây là 8 hợp đồng script cục bộ, không cộng với 96/96 hồi quy hoặc 3/3 tích hợp đợt 3 thành tổng độc lập. [Build mới đợt 4](minh-chung-dot-4/build.txt) thành công, 0 lỗi; build incremental 0 warning không chứng minh đã sửa cảnh báo lịch sử. `git diff --check` không có lỗi khoảng trắng (chỉ cảnh báo chuẩn hóa LF/CRLF ở file sẵn có). Đợt 4 không đổi logic sản phẩm; 96/96 hồi quy đợt 3 là lịch sử, không tuyên bố đã chạy lại cả bộ ở lượt này.

Hai tiến trình thử PID 28972/26564 đã dừng sau lưu minh chứng để giải phóng file build; không dừng ứng dụng khác. [Mã băm cấu hình](minh-chung-dot-4/config-hashes.txt) khớp đợt 3. [Danh mục SHA256 minh chứng](minh-chung-dot-4/artifacts.sha256) giúp kiểm tra bộ bàn giao.
