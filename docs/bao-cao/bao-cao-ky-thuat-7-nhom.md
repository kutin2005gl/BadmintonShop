# Báo cáo kỹ thuật bảy nhóm yêu cầu

Ngày 05/11/2026. Tài liệu tổng hợp cho TV1–TV4 chuẩn bị báo cáo và bảo vệ. Các chức năng có minh chứng kỹ thuật dưới đây; phạm vi tạm và xác nhận thành viên còn mở. Nguồn chi tiết là biên bản đợt 1–5 và checklist tiến độ, không phải nghiệm thu thay giảng viên.

Cập nhật đợt 5: trên DB thử mới `...170422_f787c19b`, lượt #12 mới trong phiên tiếp tục đã chạy Web → nút/MessageBox Desktop test host → báo cáo 200 → 300 → xuất XLSX/PDF cùng snapshot. Tồn 94 → 93, Web theo đúng từng trạng thái; tổng hai năm 711. 11 bảng/11 trang mới khớp đáp án độc lập và đã xem từng trang render. Đây là chứng cứ kỹ thuật do Codex thực hiện, vẫn chưa kiểm tra vòng đăng nhập Desktop sản phẩm hoặc xác nhận bàn giao thành viên. Chi tiết tại [đợt 5](quy-tac-va-tien-do-dot-5.md).

## Quy tắc số liệu

Doanh thu chỉ tính hóa đơn Hoàn thành, lấy TongTien sau giảm, phân kỳ theo NgayLap. Khoản chi là tiền nhập hàng theo NgayNhap trong kỳ. Thu trừ chi là Chênh lệch thu–chi, không phải lợi nhuận kế toán. Khoảng ngày gồm cả ngày cuối; kỳ không phát sinh bằng 0. Giảm cấp đơn phân bổ xuống sản phẩm theo trọng số, tổng dòng bằng tổng đơn đến đồng; nhóm khách vãng lai và chưa gán nhân viên vẫn được giữ.

TTL 10 phút cấu hình được, main–sub Desktop hóa đơn/phiếu nhập và Web Đơn hàng của tôi, nhập Excel chỉ thêm sản phẩm mới là phương án đã triển khai nhưng chưa chốt phạm vi cuối.

## 1 Kết nối và truy vấn

Desktop Program.cs ưu tiên biến môi trường ConnectionStrings__BadmintonShop rồi App.config. Web Program.cs đọc GetConnectionString qua configuration. DbHelper.TaoKetNoi tạo SqlConnection; ExecuteQuery dùng adapter Fill, ExecuteNonQuery và ExecuteScalar thực thi lệnh. BaoCaoRepository đọc bán và nhập riêng trong transaction để không nhân đôi thu/chi do JOIN. Vị trí mã và hướng dẫn trình diễn ở biên bản đợt 2 mục 7. Chưa có xác nhận TV1 đã tự trình diễn.

## 2 Tìm kiếm trong bộ nhớ

DB-C01–05 đợt 2 ghi nhận sản phẩm nạp 1 truy vấn, tìm 10 từ khóa/chọn ảnh thêm 0, tải lại thêm 1. Web catalog nạp 2, 10 request lọc thêm 0. Hết hạn kiểm tra bằng đồng hồ tiêm đúng biên 10 phút. Tín hiệu commit giữa tiến trình làm mất hiệu lực cache; SQL ngoài ứng dụng cần Làm mới/TTL. Chưa nghiệm thu share mạng hoặc khác máy.

## 3 Main sub và báo cáo tái sử dụng

Desktop chọn mọi cột của ba dòng hóa đơn/phiếu nhập, chi tiết đối chiếu SQL ở DB-M01. Web đợt 3 chọn 12 ô/hai dòng và Enter, đợt 4 thêm Space/chọn nhanh hai dòng; quyền chủ sở hữu và no-store có HTTP kiểm tra. Đợt 5 dùng trình duyệt thật qua proxy loopback: request cũ chậm 15 giây bị hủy khi chọn mới, HTTP 503, đóng socket và Space phục hồi đều đúng. Link Xem hoạt động khi CSP chặn script; chưa nhận đã tắt JavaScript bằng thiết lập trình duyệt. Tám hợp đồng cục bộ đợt 4 kiểm tra thêm transport cố ý bỏ qua abort. Web đã quan sát khung 390/1280/1920 px; Desktop cửa sổ nhỏ còn cần cuộn, mọi DPI chưa được nghiệm thu. Báo cáo tự động đợt 3 tải lại 2 truy vấn, xem lại thêm 0, Làm mới thêm 2; không gán số này cho lượt UI đợt 5.

## 4 Tìm kiếm nâng cao

A01/DB-A01 kiểm tra đủ >, <, >=, <=, =, != và AND/OR/NOT; khoảng 1–2 triệu gồm cả hai biên. OR TP.HCM/Hà Nội so toàn chuỗi địa chỉ, không tự tách thành phố. Đầu vào sai kiểu hoặc khoảng đảo bị từ chối. Bộ lọc dùng dữ liệu đã nạp; không ghép biểu thức người dùng thành SQL tùy ý. Minh chứng tại thư mục đợt 2.

## 5 Thống kê và kỳ thời gian

Có tổng A–B, ngày/tháng/quý/năm, nhiều năm, sản phẩm × quý, nhân viên × năm, khách × năm và thu–chi quý/năm. Mẫu nguyên trạng 2025 thu 411/chi 500, 2026 thu 100/chi 150, tổng 511/650. SQL DB-R01/R02 và đáp án độc lập kiểm tra biên kỳ, kỳ rỗng, giảm cấp đơn và nhóm rỗng. Đợt 4 xử lý #12 bằng UI: báo cáo 2026 tới 05/11 tăng 200 → 300; SQL toàn 2025–2026 tăng 611 → 711.

## 6 Đọc Excel và xuất XLSX

Nhập sản phẩm có mẫu, preview, lỗi dòng/cột, xác nhận, kiểm tra quyền và transaction toàn file; sản phẩm mới tồn 0. Đợt 3 đã thao tác SaveFileDialog/OpenFileDialog và MessageBox, SQL xác nhận giá 123000/tồn 0. Không ghi đè sản phẩm cũ hoặc nhập tồn trực tiếp. Đọc độc lập XLSX/PDF và đối chiếu snapshot/lưới có trong đợt 2–3; file XLSX 611 đã mở bằng Excel. Chính sách nhập cuối và bàn giao vẫn chờ xác nhận.

## 7 Xuất PDF

Mọi bảng có tiêu đề, bộ lọc, đơn vị và tổng; bảng nhiều năm chia phần, tổng toàn kỳ lặp không được cộng lần nữa. Đợt 2 có 40 trang được đọc độc lập/render; đợt 3 có 13 trang acceptance đối chiếu từng ô và mở Chrome. Các lần có phần trùng, không cộng thành tập kiểm tra độc lập. Chưa nhận kiểm tra trực quan từng trang trong phần mềm đọc thật hoặc mọi giá trị tối đa schema.

## Phân loại minh chứng và giới hạn

Đợt 5 chạy lại hồi quy 96/96 và 8/8 hợp đồng JavaScript cục bộ đạt; ghi log riêng. Tích hợp tự động hai tiến trình 3/3 vẫn là lịch sử đợt 3, không tuyên bố đã chạy lại ở đợt 5. Lượt UI mới và mạng proxy đợt 5 do Codex điều khiển trên Desktop test host và trình duyệt thật; SQL chỉ đọc đối chiếu. Các bộ có phần trùng, không cộng thành tổng độc lập. Không thay thế tự kiểm tra hoặc tập bảo vệ của bốn thành viên.

Đơn #12 của DB đợt 3–4 được đặt bằng trình duyệt ở đợt 3 và hoàn tất ở đợt 4, nối hai buổi. Đợt 5 có DB khác và đơn #12 mới, chạy liên tục từ đặt Web tới xuất file snapshot 300; Desktop vẫn là test host. Thành viên cần tự chạy lại với vòng đăng nhập Desktop sản phẩm theo hướng dẫn bàn giao trước khi nhóm đóng CHUNG-02.

## Danh mục nguồn bàn giao

- `quy-tac-va-tien-do-dot-1.md` tới `quy-tac-va-tien-do-dot-5.md`: quy tắc, kết quả và giới hạn từng đợt.
- `checklist-tien-do-4-thanh-vien.md`: trạng thái theo công việc và thành viên.
- `kich-ban-bao-ve-dot-3.md` và `huong-dan-ban-giao-dot-4.md`: thực hành và điều kiện xác nhận.
- `minh-chung-dot-2/`, `minh-chung-dot-3/`, `minh-chung-dot-4/`, `minh-chung-dot-5/`: log, file và ảnh tương ứng. Không dùng file xuất cũ để khẳng định số liệu DB sau giao dịch mới.

Đóng gói bản kỹ thuật chờ nhóm duyệt từ nguồn hiện tại; kiểm tra từng trang render trước khi bàn giao file. Bộ báo cáo nộp cuối vẫn chờ chốt nội dung/phạm vi và xác nhận thành viên; chưa có xác nhận nghiệm thu toàn môn.
