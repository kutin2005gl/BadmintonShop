# Hướng dẫn bàn giao và tự kiểm tra

Ngày 05/11/2026. Tài liệu cho TV1–TV4 tự kiểm tra và ghi nhận kết quả; chưa có xác nhận của bên nhận.

Bổ sung 06/11/2026: hiện người dùng cho biết đang thực hiện một mình. Dùng [kịch bản một người đợt 6](nghiem-thu-mot-nguoi-dot-6.md) để lần lượt kiểm tra các phần; không ký nhận thay TV1–TV4. [Đợt 6](quy-tac-va-tien-do-dot-6.md) ghi riêng kết quả Excel, oracle và các bước SQL/Chrome chưa đạt.

Phiên tiếp tục 06/11: nhật ký Windows SQL ghi lỗi thiếu bộ nhớ 701 và lỗi tác vụ nền; ERRORLOG đã mở trong Notepad quản trị nhưng công cụ bị giới hạn quyền điều khiển, đang cần người dùng cung cấp đoạn cuối. Chưa phục hồi kết nối, chưa tạo DB hay chạy lượt UI mới. Build 0 lỗi/1 cảnh báo; kiểm tra không DB 23/23 và JS cục bộ 8/8 ghi riêng, không thay hồi quy SQL 96/96 lịch sử. Kịch bản một người đã bổ sung lời trình bày và phương án khi SQL không hoạt động; chưa nhận đã tập bảo vệ.

Tiếp nối sau khi nhận ERRORLOG: người dùng cho phép restart riêng SQLEXPRESS; kết nối phục hồi, SELECT DB đợt 5 khớp 711/300 và tồn 93. Setup tạo DB mới `BadmintonShop_SV4_Test_20261106021438_b10b8080`, không đặt đơn E01; Web/Desktop sản phẩm cùng DB, cấu hình riêng từng tiến trình. Người dùng đã tự đăng nhập Desktop; Codex quan sát báo cáo nền **100**, kỳ 01/01–06/11/2026. Chờ xác minh đăng nhập Web; chưa đặt đơn mới, dự kiến một vợt giá 100 sẽ cho 200 sau Hoàn thành. Cập nhật cùng ngày: đơn #11 đã chạy đủ Chờ xác nhận → Hoàn thành, tồn 95 → 94, báo cáo 100 → 200 — xem [biên bản](minh-chung-dot-6/nghiem-thu-san-pham-200.md). [Biên bản đợt 6](quy-tac-va-tien-do-dot-6.md) lưu lỗi mã hóa lần đọc SQL đầu và lượt UTF-8 đúng; không sửa DB để ép số liệu.

Cập nhật đợt 5: [biên bản mới](quy-tac-va-tien-do-dot-5.md) có DB thử `...170422_f787c19b`, #11/#12 Hoàn thành, tồn vợt 93, thu hai năm 711; kỳ 01/01–05/11/2026 là 300. Đơn #12 mới chạy liên tục trong phiên tiếp tục, có XLSX/PDF snapshot 300 đã đối chiếu 11 bảng/11 trang. Chưa kiểm tra đăng nhập Desktop sản phẩm; không thay việc bên nhận tự chạy. `--acceptance-ui-setup --server .\SQLEXPRESS` chỉ tạo DB thử mới và khách thử, dừng trước tự đặt đơn; thích hợp chuẩn bị buổi thực hành UI từ mẫu.

## Chọn dữ liệu đúng mục đích

DB mẫu nguyên trạng `BadmintonShop_SV4_Test_20261105154154_a5e27e92` chỉ đối chiếu 511 thu, 650 chi, chênh lệch -139. Không dùng DB này cho nhập Excel, đặt đơn hoặc đổi trạng thái.

DB UI `BadmintonShop_SV4_Test_20261105161022_7f4ab55a` hiện #11 và #12 Hoàn thành, thu 2025–2026 là 711. Không dùng số mẫu 511 hoặc 611 cho trạng thái hiện tại. DB acceptance `...162207_dc9ab928` đã có giao dịch; cũng không phải nguyên mẫu.

Khi cần tự thao tác UI từ đầu, dùng `--acceptance-ui-setup --server .\SQLEXPRESS` để tạo DB thử mới, ghi lại tên DB từ log; chế độ này dừng trước khi đặt đơn. `--acceptance` dành cho lượt tự động và tự tạo/hoàn thành một đơn, không thay lượt UI từ mẫu nguyên trạng. Setup đã áp dụng DDL migration 07, bỏ USE DB gốc và dữ liệu đánh giá mẫu. Không chạy script cài đặt tổng lên DB đã có. Chỉ đặt `ConnectionStrings__BadmintonShop` cho riêng tiến trình, không sửa hai file cấu hình.

## Thực hành theo vai trò

TV1 chỉ nơi lấy kết nối và câu SQL, đối chiếu mẫu theo năm/quý và phân bổ giảm giá. TV2 mở các bảng cùng bộ điều kiện, kiểm tra tổng sản phẩm/nhân viên/khách và thu–chi. TV3 dùng log bộ nhớ và main–sub, nêu rõ TTL bằng đồng hồ tiêm và giới hạn tín hiệu SQL bên ngoài ứng dụng. TV4 chọn file sai/đúng, xác nhận nhập, mở XLSX/PDF và đối chiếu cùng snapshot.

Mỗi bên nhận ghi ngày, DB, bộ điều kiện, kết quả thực chạy và liên kết ảnh/log. Không ký hoặc đánh dấu thay người khác. Dùng [kịch bản bảo vệ](kich-ban-bao-ve-dot-3.md); log lỗi phải lưu cùng phần đạt và giải thích bước khắc phục.

## Một lượt tích hợp mới của nhóm

1. Khởi chạy Web sản phẩm và Desktop sản phẩm trên cùng DB thử mới. Đăng nhập Desktop bằng tài khoản thử; test host review không kiểm tra bước này.
2. Mở báo cáo nền B trong kỳ chứa ngày đặt. Đặt sản phẩm giá cuối P qua trình duyệt; ghi MaHD và tồn trước/sau. Chờ xác nhận chưa làm tăng thu.
3. Bấm lần lượt Xác nhận, Giao hàng, Hoàn thành trên Desktop; đọc và đồng ý MessageBox. Kiểm tra Web theo từng trạng thái, không chọn nhầm đơn có sẵn.
4. Bấm Xem báo cáo cùng điều kiện: B+P; bấm lại và Làm mới vẫn đúng. Đếm SQL bằng bộ kiểm tra riêng nếu cần, không suy số truy vấn từ ảnh.
5. Xuất XLSX/PDF mới từ snapshot này, đối chiếu đúng bộ điều kiện và lưu tên file riêng. File 611 của đợt 3 không đại diện cho DB hiện có thu 711.

## Điều kiện đóng bàn giao

Chốt TTL, phạm vi main–sub và chính sách nhập với nhóm/giảng viên. Đợt 5 đã kiểm tra mạng chậm/lỗi/phục hồi qua proxy trong trình duyệt thật và link Xem khi CSP chặn script; tắt JavaScript bằng thiết lập trình duyệt và DPI Windows vẫn chưa nhận đạt. Cần chạy lượt mới có vòng đăng nhập Desktop sản phẩm, xác nhận thành viên và tập bảo vệ. Có thể dùng bản Word/PDF kỹ thuật chờ duyệt đã render để rà soát; sau khi chốt phải xuất và kiểm tra lại đúng bản nộp. Không nhận toàn môn hoàn tất trước các bước này.
