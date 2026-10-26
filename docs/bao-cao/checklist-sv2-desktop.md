# Checklist kiểm thử chéo SV2 → SV4

Chưa có bằng chứng SV2 đã thực hiện. Người kiểm điền ngày, commit/build, máy/DPI, CSDL test và đính kèm kết quả/ảnh; không dùng CSDL thật.

- [ ] ADMIN/QUANLY/NHANVIEN: menu đúng quyền, thử mở trực tiếp form trái quyền; đăng xuất không thao tác tiếp được.
- [ ] Danh mục/sản phẩm: thêm/sửa, tên trùng, ảnh hợp lệ/NULL/thiếu, ngừng bán; web catalog nhìn cùng dữ liệu/kho ảnh.
- [ ] Phiếu nhập SV1 làm tăng tồn; POS không sửa tồn trực tiếp.
- [ ] POS giảm SP + hóa đơn/voucher đúng quy tắc; voucher sai/chưa mở/hết hạn/hết lượt có lý do riêng.
- [ ] Thiếu/đủ/thừa tiền; thanh toán lặp bị khóa, thất bại giữ giỏ; đổi giá trước thanh toán cần xác nhận lại; xem hóa đơn trước khi Giỏ mới sau lỗi mạng.
- [ ] Chuyển menu liên tục, quay lại POS/bản nháp; đóng/đăng xuất lúc tải và mở lại trang.
- [ ] Khách thật qua UI SV2 đặt COD → desktop cùng mã/chi tiết → xác nhận/giao/hoàn thành → web đúng trạng thái. Khách khác không xem/hủy; khách chỉ hủy khi Chờ, nhân viên bắt buộc lý do trước giao.
- [ ] Thống kê đúng ngày/loại bán; đơn hủy không doanh thu; bảng/Excel/PDF cùng bộ lọc, hóa đơn dùng giá lưu.
- [ ] Cửa sổ 1366×768 và 1920×1080; DPI Windows 125%/150% thật, scroll tới nút, không cắt/chồng. Kiểm Visual Studio Designer và hộp thoại/in nếu cần demo.

Người kiểm: …; Ngày: …; Build/commit: …; Môi trường/CSDL riêng: …; Kết quả/lỗi/ảnh: …
