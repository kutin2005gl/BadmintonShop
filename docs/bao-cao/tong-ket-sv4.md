# Bản nháp tổng kết SV4

## Kết quả đã triển khai

SV4 bổ sung năm nhóm menu desktop: danh mục, sản phẩm, đơn hàng, POS, thống kê. Danh mục có CRUD và chặn xóa khi đang được dùng. Sản phẩm quản lý thông tin/ảnh, lọc/sắp xếp, ngừng bán giữ lịch sử; tồn chỉ đọc, sản phẩm mới 0 và tăng qua phiếu nhập SV1.

Đơn Online có luồng trạng thái có điều kiện, lý do hủy, kiểm stale update và dùng trigger hoàn kho. POS hỗ trợ khách vãng lai, tiền mặt/tiền thừa, transaction header + chi tiết và token chống ghi lặp. Khuyến mãi chưa sẵn sàng được nêu rõ ở giao diện.

Báo cáo có ngày/tháng/năm/khoảng ngày, loại bán, số lượng bán, danh mục, số đơn theo trạng thái; có bảng, biểu đồ, XLSX/PDF thật và xuất lại hóa đơn. Tổng phân bổ danh mục khớp net hóa đơn đến đồng, không giảm hai lần. Script demo riêng có 12 tháng giao dịch tại quầy, nhập kho trước bán và đơn Online để kiểm luồng độc lập.

Repository cũ của sản phẩm/danh mục được mở rộng, không tạo bản trùng. Một HoaDonRepository dùng chung với SV3. Constructor form không truy cập DB, mỗi form có kiểm quyền; SQL có tham số nằm ở Core/DataAccess. Trigger SV1 và chức năng web/nhân sự/nhập hàng/sao lưu được giữ nguyên.

Đã có bộ kiểm tra thực thi unit, SQL LocalDB integration, yêu cầu đồng thời, xuất file và render WinForms. Số liệu và trạng thái cụ thể xem [biên bản kết quả](ket-qua-kiem-thu-sv4.md), không dùng bảng tự đánh giá 100% của mẫu phân công làm kết quả thực tế.

## Hạn chế còn lại

- TinhKhuyenMaiService SV3 còn là khung; POS hiện giá thường. Cần tích hợp ưu đãi trong transaction và kiểm lượt voucher trước khi đưa vào dùng chung.
- Web đặt hàng, tài khoản/catalog/đánh giá và email chưa có trên nhánh nền này; chưa kiểm E2E giao dịch web → desktop, chưa kiểm thử chéo SV2.
- Ảnh dùng filesystem cấu hình chung; chưa có upload API/object storage hay dọn ảnh mồ côi. SV2 cần mount đường dẫn công khai và fallback.
- Quy tắc nhân viên hủy trước giao cần cả nhóm duyệt khi tích hợp; chưa xử lý trả hàng/hoàn tiền sau hoàn thành hoặc hủy POS hoàn thành.
- Doanh thu phân kỳ theo NgayLap của đơn hiện Hoàn thành. Chưa có ngày hoàn thành hoặc snapshot tên/danh mục để giữ nguyên báo cáo sau đổi sản phẩm/danh mục.
- Truy vấn desktop còn đồng bộ, đọc tập dữ liệu phù hợp quy mô đồ án. Báo cáo Serializable có thể chặn ghi khi dữ liệu lớn; chưa benchmark tải lớn.
- Render form và một số thao tác điều khiển đã chạy; chưa thay thế kiểm thủ công các hộp thoại, DPI, trợ năng, máy in và Visual Studio Designer.
- Bộ kiểm tra mở file bằng thư viện và kiểm cấu trúc độc lập, không tự động thao tác Microsoft Excel/Adobe Reader hoặc gửi máy in thật.
- Chưa là phần mềm bán hàng triển khai thương mại, chưa có hóa đơn điện tử thuế/audit log đầy đủ.

## Hướng phát triển

Hoàn thiện tích hợp SV2/SV3 và kiểm thử chéo trước tiên; bổ sung ngày hoàn thành, snapshot sản phẩm/danh mục và audit người chuyển trạng thái. Tách dịch vụ thanh toán nghiệp vụ dùng chung để hỗ trợ ưu đãi/cổng thanh toán, mở rộng idempotency cho web. Bổ sung trả hàng/hoàn tiền có kiểm quyền và phiếu kho. Dùng API ảnh/object storage khi triển khai nhiều máy. Tối ưu truy vấn bất đồng bộ/phân trang, báo cáo snapshot isolation sau khi thống nhất với SV1. Bổ sung kiểm thử E2E, fault injection và kiểm UI/DPI trên máy demo.
