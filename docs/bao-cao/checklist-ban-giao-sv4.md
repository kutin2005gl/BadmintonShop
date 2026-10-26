# Checklist phối hợp trước khi tích hợp SV4

Chưa có xác nhận từ SV1/SV2/SV3. Người review tự điền ngày/commit/môi trường và bằng chứng; không đánh dấu thay. Không tự gửi thông báo hoặc chỉ định reviewer.

## SV1 — schema và CSDL dùng chung

- [ ] Review migration 04 rồi **06_SV4_POS_Request.sql**, cột nullable DauVanTayPOS VARCHAR(64), unique filtered index token, khả năng chạy lại và quyền sp_getapplock.
- [ ] Sao lưu và chọn đúng DB trước khi áp dụng trên môi trường dùng chung. Hiện DB người dùng còn thiếu 4 cột/index; ứng dụng không tự cập nhật. Không chạy 01/SQL tổng lên DB có dữ liệu cần giữ.
- [ ] Kiểm tra trigger SV1 vẫn bật và chỉ trigger sửa tồn/tổng. Kiểm các client dùng chung giữ transaction, không trừ kho/giảm tiền hai lần.
- [ ] Xác nhận luồng nhân viên hủy trước giao và cấp quyền SQL phù hợp; review migration trên bản sao/test trước triển khai.

## SV2 — kiểm thử chéo desktop, web và ảnh

- [ ] Thực hiện [checklist desktop](checklist-sv2-desktop.md), đặc biệt quyền, hộp thoại, 1920×1080 và DPI Windows thật 125%/150%.
- [ ] Tích hợp login/catalog UI và thử khách thật qua browser: đặt COD → desktop xử lý → web cập nhật; không thay fixture HTTP thành kết quả E2E.
- [ ] Web và Desktop dùng cùng DB/kho ảnh, mount/fallback products đúng; không lộ đường dẫn máy và không sửa tồn từ form web.

## SV3 — chính sách khuyến mãi và đặt hàng

- [ ] Xác nhận giảm SP trước, chọn một ưu đãi cấp đơn giảm nhiều hơn, bằng nhau ưu tiên voucher; bậc hóa đơn chọn ngưỡng cao nhất đạt được.
- [ ] Xác nhận **hiện chưa hoàn lượt voucher khi hủy**; đây là cách code cũ đang làm, chưa có quyết định nghiệp vụ mới.
- [ ] Review dùng chung Core/transaction, chống tranh lượt cuối, lỗi giá/ưu đãi đổi cần xác nhận lại và không đổi API Web ngoài phạm vi cần thiết.
- [ ] Kiểm màn hình quản lý CTKM, email SMTP và idempotency web nếu làm tiếp; không xem token POS là token chống trùng cho web.

Người review: …; ngày: …; commit: …; CSDL test: …; kết quả/đường dẫn bằng chứng: …
