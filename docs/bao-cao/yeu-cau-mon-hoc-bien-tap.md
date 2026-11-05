# Yêu cầu đồ án — bản biên tập để nhóm đối chiếu

Nguồn chính: [ghi chú Google Docs](https://docs.google.com/document/d/1Q0ArrpbxifeLZMPdMAPGkwGtirvyxZiPawEBbiozSSc/edit), đọc ngày 05/11/2026. Đây là bản làm rõ câu chữ của nhóm, chưa phải xác nhận/chấm điểm của giảng viên; không ghi đè bản gốc.

## 1. Giải thích truy cập CSDL

Chỉ ra được câu SQL nằm ở đâu, dòng lệnh thực sự thực thi SELECT, chuỗi kết nối được lấy từ đâu và cách mở kết nối tới CSDL. Không chỉ nói tên lớp Repository mà không chỉ được chỗ chạy lệnh.

## 2. Tái sử dụng dữ liệu tìm kiếm

Đọc danh sách vào biến bộ nhớ và dùng lại để tìm nhiều từ khóa trong một phiên làm việc khoảng 5–10 phút, tránh truy vấn lại liên tục cho mỗi lần tìm. Sau khoảng đó cho phép đọc lại dữ liệu. Voice khoảng 04:43 có vẻ nêu cả ví dụ 5, 10, 15 phút; do nhận dạng nhiễu, chưa coi 5–10 phút là thời hạn cứng duy nhất. Phân biệt việc giữ một biến cục bộ trong một request Web với tái sử dụng dữ liệu qua nhiều lần tìm.

Phương án hết hạn, làm mới thủ công và làm mất hiệu lực sau cập nhật là việc nhóm cần thiết kế/giải thích. Ghi chú gốc chưa quy định chi tiết thuật toán cache.

## 3. Main–sub cho màn hình thường dùng

Danh sách hóa đơn ở vùng chính, chi tiết ở vùng phụ. Khi chọn một hóa đơn bằng cách bấm vào các ô của dòng, chi tiết tương ứng xuất hiện ngay. Câu trong bản gốc mô tả việc phải bấm link chi tiết mỗi lần cần được đọc như tình huống gây bất tiện, tránh hiểu ngược thành yêu cầu bắt buộc mở trang riêng.

Thống kê thường xuyên dùng dữ liệu bộ nhớ đã đọc; thống kê ít dùng có thể chạy truy vấn khi cần. Cần xác nhận những form/page nào trong phạm vi demo phải áp dụng mẫu main–sub.

## 4. Tìm kiếm nâng cao

Thể hiện được các phép so sánh `>`, `<`, `>=`, `<=`, `=`, `!=` và kết hợp logic AND, OR, NOT theo nhu cầu người dùng. Dấu `==` trong ghi chú là phép bằng ở mức ngôn ngữ lập trình; T-SQL dùng `=` và `<>`/`!=` cho khác.

Ví dụ phải giải thích/demo được:

- Hóa đơn có tổng tiền từ 1 triệu đến 2 triệu: hai điều kiện >= và <= kết hợp AND.
- Khách hàng ở TP.HCM hoặc Hà Nội: hai điều kiện địa chỉ kết hợp OR.
- Một điều kiện phủ định hợp lệ để thể hiện NOT.

Ghi chú gốc không yêu cầu nhập câu SQL tự do hoặc hỗ trợ biểu thức tùy ý không giới hạn.

## 5. Thống kê

### 5a. Khoảng ngày

Tổng tiền hóa đơn từ ngày A đến ngày B. Ghi rõ cách tính ngày cuối và trạng thái hóa đơn nào được đưa vào tổng.

### 5b. Theo thời gian, thu–chi–lợi nhuận

Có thống kê theo tháng, quý, năm và nhiều năm. Bảng theo năm cần thể hiện Q1, Q2, Q3, Q4 cùng các chỉ tiêu tổng thu từ hóa đơn, tổng chi có xét tiền nhập hàng, và lợi nhuận.

Cần xác nhận với giảng viên “chi” là tiền nhập trong kỳ cộng các khoản phí nào, hay giá vốn hàng bán. Chưa đủ căn cứ để tự thêm lương/điện/nước thành chi phí đã phát sinh.

### 5c. Theo nhân viên và khách hàng

Hàng là nhân viên, cột là các năm, có tổng theo hàng và hàng Tất cả. Làm tương tự cho khách hàng.

Bản gốc dùng tiêu đề “TỔNG CHI” trong bảng mô tả tổng tiền hóa đơn. Để tránh nhầm góc nhìn, tạm dùng “Tổng tiền hóa đơn”; nếu chỉ tính đơn hoàn thành thì có thể dùng “Tổng doanh thu” phía cửa hàng hoặc “Tổng tiền mua hàng” phía khách sau khi thống nhất với giảng viên. Chưa khẳng định đây là lời giảng viên nói sai.

### 5d. Theo sản phẩm × quý/năm

Chọn năm; hàng là sản phẩm, cột Q1, Q2, Q3, Q4 và Tổng; thêm hàng Tất cả. Chỉ tiêu trong ghi chú là tổng tiền hóa đơn theo sản phẩm, không chỉ số lượng bán chạy. Ghi rõ cách xử lý giảm giá khi phân bổ tiền về sản phẩm.

Đây là bảng ghi chú nhấn mạnh quan trọng nhất để hỗ trợ kế hoạch kinh doanh năm sau. Cần đưa vào phạm vi hoàn thiện trước khi nộp.

## 6. Đọc/ghi Excel

Có chức năng đọc và ghi file Excel hỗ trợ người dùng. Mỗi bảng thống kê ở mục 5 phải xuất được Excel. Đọc file trong project kiểm thử để xác minh file xuất không thay thế chức năng đọc/nhập Excel trong ứng dụng.

Phạm vi nhập dữ liệu cụ thể chưa được ghi rõ trong Docs; nhóm cần xác nhận bảng nào, định dạng mẫu và cách xử lý lỗi/trùng.

## 7. Xuất PDF

Có khả năng xuất báo cáo PDF. Khi các bảng bắt buộc được bổ sung, đầu ra cần phản ánh đúng bảng, phạm vi lọc và tổng số đang trình bày.

## Những quyết định còn cần xác nhận

- Trạng thái hóa đơn và mốc ngày dùng để thống kê.
- Định nghĩa tổng chi và lợi nhuận theo bài học.
- Phạm vi form/page cần main–sub và tái sử dụng dữ liệu.
- Phạm vi đọc/nhập Excel.
- Cách gán doanh thu nhân viên khi đơn Online chưa có MaNV, khách vãng lai khi MaKH NULL.

Các mục chưa rõ được giữ thành câu hỏi nghiệp vụ, không tự sửa code để áp đặt cách hiểu. Xem [báo cáo rà soát và hướng đi](ra-soat-yeu-cau-2026-11-05.md).

## Giới hạn đối chiếu voice

Bản ghi đã được nhận dạng toàn bộ tại máy nhưng có nhiều câu sai/lặp ảo. Đây là bản biên tập dựa chính vào Docs; chưa xác nhận nguyên văn từng yêu cầu theo voice. Các mốc nghe lại và điểm chưa rõ được ghi trong báo cáo rà soát, nhất là tên cột tổng và định nghĩa chi phí.
