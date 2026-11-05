# Product

<!-- impeccable:product-schema 1 -->

## Platform

web

## Users
Khách hàng mua dụng cụ cầu lông trên website (BadmintonShop.Web). Xác nhận: người chơi nghiêm túc, hay so sánh thông số (thương hiệu, loại sản phẩm, giá, giảm giá) trước khi mua. Nhân viên/quản lý dùng ứng dụng Desktop riêng, không thuộc phạm vi giao diện web này.

## Product Purpose
Hệ thống bán đồ thể thao cầu lông (đồ án CNPM, nhóm 4 người): web cho khách xem sản phẩm, đăng ký/đăng nhập, giỏ hàng, đặt hàng (COD / thanh toán online), theo dõi và hủy đơn, đánh giá sản phẩm; Desktop cho nhân viên (POS, đơn hàng, kho, khuyến mãi, thống kê). Thành công = khách tìm đúng sản phẩm, hiểu giá sau khuyến mãi và đặt hàng không vướng.

## Capabilities and Constraints
- Stack: ASP.NET Core MVC (.NET 8), Razor views, Tailwind + `wwwroot/css/smashpro.css` (theme SmashPro hiện có).
- Nội dung tiếng Việt có dấu; tiền tệ VND.
- Chức năng đã có: trang chủ, danh mục, danh sách/chi tiết sản phẩm, đánh giá, tài khoản (đăng ký, đăng nhập, hồ sơ, đổi mật khẩu), giỏ hàng (ngăn kéo), đặt hàng, thanh toán online, đơn hàng của tôi, khuyến mãi (theo sản phẩm, hóa đơn, voucher).
- Không được đổi nghiệp vụ, route, tên controller/action hay form field: chỉ làm lại phần giao diện.

## Brand Commitments
Tên cửa hàng: BADMINTONSHOP. Giữ nhận diện đen-trắng của theme SmashPro hiện có (người dùng chọn làm đẹp phần này, không thay thế).

## Evidence on Hand
Chưa có ảnh sản phẩm thật, logo hay màu thương hiệu riêng; người dùng sẽ bổ sung sau. Không bịa ảnh, đánh giá hay số liệu; dùng placeholder rõ ràng cho ảnh.

## Product Principles
- Thông số và giá là nội dung chính: khách phải so sánh được nhanh.
- Giá sau khuyến mãi luôn rõ ràng, không gây nhầm lẫn.
- Đặt hàng ít bước, ít vướng, dùng tốt cả mobile lẫn desktop.
- Giao diện sạch, nhất quán; không thay đổi nghiệp vụ khi làm đẹp.
