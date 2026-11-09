# Lượt sản phẩm mới — snapshot 200

Ngày thực chạy 06/11/2026, Asia/Saigon. Nhánh `chore/tiep-tuc-nghiem-thu-dot-6`, HEAD `86d44d8`, tài liệu/minh chứng chưa commit. Người dùng tự đăng nhập Desktop và Web; Codex điều khiển các bước UI sau đăng nhập và đối chiếu chỉ đọc. Không thay xác nhận đóng góp TV1–TV4 hoặc giảng viên.

## Môi trường và giao dịch

- Server `.\SQLEXPRESS`; DB mới **BadmintonShop_SV4_Test_20261106021438_b10b8080**. [Setup](ui-setup-new.txt) chỉ tạo fixture rồi dừng trước đơn E01. Hai ứng dụng dùng cấu hình riêng tiến trình, Desktop entry point sản phẩm, không review host.
- SQL phục hồi sau restart được người dùng cho phép; [kết quả](sql-restart-admin-result.txt). Vẫn còn tín hiệu process_physical_memory_low sau restart, chưa khắc phục dứt điểm nguyên nhân bộ nhớ.
- Nền [SQL](new-db-baseline.txt), [UI](product-report-baseline-100.jpg): kỳ **01/01–06/11/2026**, tất cả loại bán, B=100, chi=0; mã SP1 Vợt thử giá P=100, tồn 95.
- Web giỏ trống → thêm đúng một Vợt thử → COD, không giảm giá → đặt thành công **#11**, NgayLap **06/11/2026 02:22:28.167**. Địa chỉ là dữ liệu thử, không giao hàng thật; email gửi bị tắt riêng cho tiến trình thử.
- [SQL lúc chờ](new-order-11-pending-sql.txt): #11 Chờ xác nhận, tổng 100, MaKH=3, MaNV=NULL; tồn **94**, doanh thu vẫn **100**. [UI báo cáo](new-order-11-report-pending-100.jpg) vẫn 100, số đơn trong kỳ tăng 4 → 5 sau Xem báo cáo.
- Desktop chọn đúng #11; bấm **Xác nhận → Giao hàng → Hoàn thành**, đọc ba MessageBox và đồng ý Yes. Web tải lại sau từng bước và hiện đúng Chờ xác nhận / Đã xác nhận / Đang giao / Hoàn thành. Ảnh và text `new-order-11-web-*`, `new-order-11-desktop-*`, `*-message.*` lưu từng bước.
- Báo cáo đang giữ snapshot 100 khi quay lại form; bấm **Xem báo cáo** mới tải kết quả **B+P=200**. Xem lại và Làm mới cùng kỳ đều 200. Chưa đo số truy vấn SQL trong lượt UI này; không suy số truy vấn từ ảnh.
- [SQL cuối](new-order-11-completed-sql.txt): #11 Hoàn thành, 100, tồn **94**, kỳ 2026 tới 06/11 thu **200**; tổng 2025–2026 là **611**. Không dùng 711 của DB đợt 5 cho DB mới này.

## Báo cáo và xuất file

- [Bảng sản phẩm × quý](new-order-11-products-quarter-200.jpg): vợt 70/0/0/100=170; cầu 0/0/30/0=30; sản phẩm 3=0; hàng Tất cả **70/0/30/100=200**.
- [Thu–chi](new-order-11-cashflow-200.jpg): thu 200, chi 0, chênh lệch 200. Khoản nhập 150 ngày 31/12 nằm ngoài kỳ đang chọn.
- Dùng hai hộp lưu Windows từ form sản phẩm, tên mới [XLSX](ui-snapshot-200.xlsx) và [PDF](ui-snapshot-200.pdf), không ghi đè snapshot 300 lịch sử. Không sửa bộ lọc hoặc giao dịch giữa hai lần xuất; ứng dụng báo đã xuất kết quả đang hiển thị.
- [Oracle chạy mới](export-200-results.txt): **11 bảng XLSX / 11 trang PDF**, schema, số hàng, từng ô và phạm vi ngang khớp đáp án độc lập 70+30+100=200. Mã kiểm chứng `tests/BadmintonShop.SV4.Checks/verify_ui_snapshot_200.py` suy đáp án từ fixture và đúng một đơn UI 100, không lấy số từ service/file làm đáp án. Đáp án được lưu [JSON](ui-snapshot-200-expected.json).
- Đã render Poppler 100 DPI và xem đủ [11 ảnh trang](snapshot-200-pages/), không thấy cắt/chồng chữ hoặc sai dấu tiếng Việt. Đây là render PDF, không phải DPI Windows.

## Phần mềm đọc thực tế

- Excel mở đúng file 200, chuyển đủ 11 sheet theo thứ tự; ảnh `excel-200-01.jpg` tới `excel-200-11.jpg`. Bảng quý có thêm [ảnh rộng](excel-200-10-wide.jpg), thấy đủ cột Q1–Q4/Tổng và hàng tổng. Excel Unlicensed Product/tài khoản trường không còn hoạt động; không thao tác kích hoạt/đăng nhập Office và không lưu sửa workbook.
- Một số ảnh Excel ở cửa sổ nhỏ thiếu hàng cuối; sheet trạng thái chưa quan sát đủ hàng Đã hủy trong vùng ảnh đó. File đã được oracle đọc đủ ô, nhưng **không nhận trực quan mọi ô Excel**. `excel-200-07-bottom.jpg` là lần thử tới cuối chưa thay đổi vùng nhìn thấy, không dùng để chứng minh đã thấy hàng cuối.
- Chrome mở đúng `file:///D:/DoAn_CNPM/BadmintonShop/docs/bao-cao/minh-chung-dot-6/ui-snapshot-200.pdf`, nhận đủ 11 trang; điều chỉnh vừa trang 80%, đã chuyển và quan sát từng trang **1–11**. Ảnh `chrome-200-page-01.jpg` tới `chrome-200-page-11.jpg`. Chỉ điều chỉnh cách xem, không ghi chú, lưu lên Drive hoặc sửa PDF. Việc mở PDF đợt này thành công không hồi tố lần Chrome bị chặn với file 300 cũ.

## Còn mở

JavaScript tắt thật bằng thiết lập trình duyệt, phạm vi DPI Windows/chính sách cuối, người dùng tự tập trọn kịch bản và giảng viên xác nhận. Đây là lượt người dùng đăng nhập, Codex hỗ trợ thực hiện, không ghi người dùng đã tự bấm toàn bộ luồng. Chưa xuất Word/PDF nộp cuối, chưa ký nhận TV1–TV4 và chưa chốt toàn bộ nghiệm thu.
