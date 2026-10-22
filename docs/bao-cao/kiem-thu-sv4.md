# Kế hoạch và kết quả kiểm thử SV4

Ngày thực hiện: 18/10/2026. Không đánh dấu đạt cho module chưa chạy hoặc màn hình giả lập.

## Phạm vi và môi trường

- Windows, SDK .NET 10.0.203 build target .NET 8; có runtime .NET/WindowsDesktop 8.
- SQL Server LocalDB 17.0.1000.7, Windows integrated authentication, **CSDL test riêng sinh tên mới**. Không chạy migration/demo trên CSDL BadmintonShop có sẵn.
- Bộ kiểm tra console `tests/BadmintonShop.SV4.Checks`, STA cho WinForms; kết thúc mã 0 khi mọi kiểm tra đạt, mã 1 nếu có lỗi.
- Unit: ma trận trạng thái/phân quyền, kiểm dữ liệu/đường dẫn, phân bổ giảm giá (1.000 bộ ngẫu nhiên + giá trị lớn), báo cáo rỗng.
- Integration: schema/trigger thật, migration hai lần, giao dịch thành công/lỗi, yêu cầu đồng thời, dữ liệu demo 12 tháng, đối chiếu số liệu SQL.
- GUI smoke: thực sự Show và OnLoad các form, dùng DrawToBitmap lưu ảnh của điều khiển thật. Có kiểm nút Mới của danh mục và thêm/bỏ giỏ POS. Không phải kiểm thử đầy đủ thao tác người dùng/Designer.
- File: mở lại bằng ClosedXML/PDFsharp, xác thực OpenXML, kiểm độc lập bằng openpyxl/pypdf, render PDF bằng Poppler và xem ảnh.

Lệnh chạy và lưu trữ bằng chứng: xem [hướng dẫn SV4](huong-dan-su-dung-sv4.md). File kết quả nằm ở thư mục Temp in tại cuối lần chạy. Mỗi lần integration tạo CSDL test mới và để lại để review. Không tự xóa CSDL.

## Bảng test case

Đạt (tự động) chỉ chứng minh tầng/bước ghi trong cột bằng chứng; không suy rộng thành đã kiểm E2E trên web hoặc toàn bộ UI. Chờ = chưa thực hiện.

| ID | Tiền điều kiện và bước/dữ liệu thử | Kết quả mong đợi | Kết quả / bằng chứng |
|---|---|---|---|
| TC01 | Tên `  Vợt  `, rỗng, 101 ký tự | Trim; từ chối rỗng/quá 100 | Đạt — U01 |
| TC02 | Giá -1, 0.5, 10^18, 0 | Chặn âm/lẻ/vượt precision; nhận 0 | Đạt — U02 |
| TC03 | Thử tất cả 25 cặp trạng thái | Chỉ 3 bước tiến và 2 trường hợp hủy trước giao | Đạt — U03 |
| TC04 | ADMIN/NHANVIEN, vai trò lạ, không đăng nhập | ADMIN đủ quyền; NV chỉ đơn/POS; còn lại chặn | Đạt — U04, tầng quyền |
| TC05 | Kho ảnh: products/test.jpg; ổ đĩa, URL, ../, backslash | Nhận khóa tương đối đúng vùng; chặn đường dẫn sai | Đạt — U05 |
| TC06 | Net 101, hai danh mục trọng số 100/100 | Phân bổ 51/50, tổng 101 | Đạt — U06 |
| TC07 | Phân bổ 0 và trọng số 0 | Không chia cho 0, mọi phân bổ 0 | Đạt — U06 |
| TC08 | 1.000 bộ trọng số ngẫu nhiên và net 18 chữ số | Không âm/tràn, tổng chính xác | Đạt — U07 |
| TC09 | Hóa đơn hoàn thành và hủy trong fixture | Chỉ hoàn thành có doanh thu/lượng bán | Đạt — U08 |
| TC10 | Báo cáo theo ngày/tháng/năm/danh mục | Các tổng bằng tổng hóa đơn hoàn thành | Đạt — U08, I12 |
| TC11 | Không có dòng nguồn | Doanh thu 0; bảng doanh thu trống; 5 trạng thái bằng 0 | Đạt — U09 |
| TC12 | Tạo và Dispose 6 form, chưa có DB/phiên | Constructor không gọi DB, không lỗi | Đạt — U10 |
| TC13 | Xuất báo cáo fixture thành .xlsx | 6 sheet, ô tiền numeric, tổng 161; OpenXML hợp lệ | Đạt — U11 |
| TC14 | Xuất PDF fixture, hóa đơn 180 dòng, báo cáo trống | PDF đọc lại được; hóa đơn nhiều trang; báo cáo rỗng không lỗi | Đạt — U12 |
| TC15 | Ảnh PNG hợp lệ → JPEG trong kho test | Kích thước/khóa tương đối đúng, mở lại được | Đạt — U13 |
| TC16 | Ảnh giả, file thiếu, >10 MB | Từ chối, không ghi ảnh giả | Đạt — U13 |
| TC17 | Migration 04 chạy hai lần trên schema thật | Không lỗi, cột/index giữ nguyên | Đạt — I01/setup |
| TC18 | Thêm SP truyền SoLuongTon=999 | CSDL vẫn tồn 0 | Đạt — I01 |
| TC19 | Danh mục thêm/sửa/xóa không dùng | Đúng ID, trim tên; xóa xong không còn | Đạt — I02 |
| TC20 | Thêm tên danh mục trùng | Từ chối và giữ dữ liệu trước | Đạt — I02 |
| TC21 | Xóa danh mục đang có SP | Từ chối rõ ràng, SP còn nguyên | Đạt — I02 |
| TC22 | SP tên rỗng/giá âm | Từ chối trước SQL | Đạt — I03 |
| TC23 | Nhập 100 hàng qua phiếu nhập; sửa SP truyền tồn 100.000 | Trigger tăng 100; sửa SP không đổi tồn | Đạt — I03 |
| TC24 | POS 2 SP cùng mã đã gộp, giá 100, khách đưa 250 | HĐ Hoàn thành/Tại quầy/Tiền mặt, MaKH NULL, MaNV đúng; tổng 200; tồn giảm 2 | Đạt — I04 |
| TC25 | Gọi POS hai lần cùng token/giỏ/tiền | Cùng MaHD, không giảm tồn lần hai | Đạt — I04 |
| TC26 | Dùng lại token nhưng đổi giỏ | Từ chối, không tạo HĐ khác | Đạt — I04 |
| TC27 | POS thiếu tồn hoặc tiền 99 cho đơn 100 | Chặn, số HĐ/tồn không đổi | Đạt — I05 |
| TC28 | POS giá cũ 99 trong khi DB 100; SP đã ngừng bán | Chặn, yêu cầu nạp lại | Đạt — I05 |
| TC29 | POS truyền hai dòng trùng mã | Chặn trước ghi | Đạt — I05 |
| TC30 | Tạo header + dòng hợp lệ rồi dòng thứ hai thiếu tồn | Trigger rollback toàn bộ header/chi tiết/tồn | Đạt — I06 |
| TC31 | Hủy Chờ xác nhận với lý do trắng | Chặn và không hoàn kho | Đạt — I07 |
| TC32 | Hủy hợp lệ rồi hủy lặp/mở lại | Lưu lý do, hoàn đúng 1 lần; không mở lại | Đạt — I07, gồm trigger trực tiếp |
| TC33 | Đơn đã xác nhận nhưng yêu cầu mang trạng thái Chờ cũ | Từ chối cập nhật stale | Đạt — I08 |
| TC34 | Đi đủ luồng; thử hủy Đang giao | Đúng luồng hoàn thành, chặn hủy muộn | Đạt — I08 |
| TC35 | 2 task POS đồng thời cùng token | Cùng một hóa đơn, tồn giảm 1 lần | Đạt — I09 |
| TC36 | 2 task mua sản phẩm chỉ còn 1 | Chỉ 1 thành công, tồn 0 | Đạt — I10 |
| TC37 | Lọc mã đơn/ngày/trạng thái/loại, ngày ngược | Kết quả đúng bộ lọc; chặn khoảng sai | Đạt — I11 |
| TC38 | Demo 05 chạy hai lần | Không trùng, nhập trước bán, đủ 12 tháng | Đạt — I12 |
| TC39 | Tổng SQL vs bảng/sheet danh mục; Online + Tại quầy | Khớp toàn bộ; không tính hủy | Đạt — I12 |
| TC40 | 2 task hủy cùng trạng thái kỳ vọng | Chỉ 1 thành công; hoàn tồn đúng 1 | Đạt — I13 |
| TC41 | Đơn 31/01 lúc 23:59:59.997 | Có trong 31/01; không trong 01/02 | Đạt — I14 |
| TC42 | API đọc sản phẩm cũ, tìm tên + danh mục | Vẫn trả model và kết quả như trước; JOIN có tên danh mục | Đạt — I14 |
| TC43 | Khách test riêng; tìm tên/SĐT/email | Đúng đơn của khách; không cần dữ liệu SV2 | Đạt — I15 |
| TC44 | Xuất lại HĐ POS đã lưu | PDF tạo từ giá/tiền CSDL, tổng chi tiết khớp header | Đạt — I15 |
| TC45 | Dòng 2×100 giảm 7,5% + 1×50; giảm HĐ 30 | Tiền hàng 235, net 205, danh mục tổng 205; PDF giữ 7,5% | Đạt — I16 |
| TC46 | Mở 6 form trên CSDL test | OnLoad nạp được, render có dữ liệu | Đạt — GUI smoke |
| TC47 | Danh mục chọn dòng rồi Mới | Mã về mới, ô nhập rỗng; không ghi đè dòng cũ | Đạt — GUI FrmDanhMuc |
| TC48 | POS thêm sản phẩm rồi bỏ dòng giỏ | Giỏ có 1 rồi 0 dòng trước lưu | Đạt — GUI FrmPOS |
| TC49 | Đăng nhập NV rồi mở trực tiếp form sản phẩm/báo cáo ngoài menu | Thông báo thiếu quyền, form đóng; kiểm cả nút sau đăng xuất | Chờ kiểm thủ công (ma trận quyền đã đạt TC04) |
| TC50 | Form sản phẩm lọc thương hiệu/tồn/trạng thái và sắp xếp; scale 125/150% | Lưới đúng tập lọc, không che nút | Chờ thao tác thủ công, đã smoke ở scale mặc định |
| TC51 | VS mở Designer 6 form | Không truy cập DB khi thiết kế; mở và thay bố cục được | Chờ Visual Studio; constructor đạt TC12 |
| TC52 | Web đăng ký/đăng nhập, tài khoản khóa | Xác thực đúng, không lộ mật khẩu | Chờ SV2 tích hợp UI và kiểm E2E |
| TC53 | Web catalog thấy SP desktop thêm/sửa/ngừng bán | Giá/ảnh/danh mục đồng nhất, ẩn SP ngừng bán | Chờ SV2 tích hợp |
| TC54 | Web ảnh NULL/404/sai khóa | Ảnh fallback; không lộ đường dẫn vật lý | Chờ mount kho ảnh với SV2 |
| TC55 | Web giỏ hàng đổi số lượng/thiếu tồn | Không bán quá tồn, giữ giỏ để sửa | Chờ SV3 |
| TC56 | Web COD/online giả lập → desktop xác nhận/giao/hoàn thành | Cùng hóa đơn/trạng thái và tồn, không ghi trùng | Chờ SV3 E2E; luồng repository đã đạt |
| TC57 | Khách web hủy đơn của mình còn Chờ | Lưu lý do, hoàn đúng 1; không hủy đơn người khác/đã xác nhận | Chờ SV3 endpoint kiểm chủ đơn |
| TC58 | Voucher hết hạn/hết lượt/SP + HĐ giảm chồng nhau | Từ chối voucher sai, không trừ hai lần, lượt dùng atomic | Chờ SV3; POS hiện không áp voucher |
| TC59 | Gửi email xác nhận web | Nội dung đúng đơn, không gửi trùng sau retry | Chờ SV3 cấu hình/test SMTP |
| TC60 | Khách chưa mua/đã mua gửi đánh giá, desktop duyệt | Chặn đánh giá trái quyền; ẩn/hiện đúng | Chờ SV2 |
| TC61 | Mất mạng ngay sau commit POS trước nhận kết quả | Retry cùng token trả đơn cũ; thao tác người dùng không tạo đơn mới | Chờ fault injection mạng; retry bền vững đã đạt I04/I09 |

## Tiêu chí bàn giao và kiểm thử chéo

Build solution không lỗi, unit/integration pass trên CSDL riêng, không sửa CSDL người dùng; file có số liệu và cấu trúc đọc được; không tuyên bố web/khuyến mãi đã tích hợp. SV2 cần kiểm thử chéo phần SV4 theo phân công, đặc biệt nhập liệu tương tác, hộp thoại, DPI và Designer. SV1/SV3 review quy tắc hủy và transaction dùng chung trước PR.

Kiểm độc lập: pypdf đọc Unicode tiếng Việt/PDF nhiều trang, Poppler render xem bố cục; openpyxl đọc đủ 6 sheet, tổng danh mục bằng tổng CSDL. Không kiểm bằng cách đổi đuôi tệp. Kết quả theo lần chạy được lưu trong [biên bản kiểm thử](ket-qua-kiem-thu-sv4.md).
