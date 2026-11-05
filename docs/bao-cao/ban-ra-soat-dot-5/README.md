# Bộ tài liệu kỹ thuật chờ nhóm duyệt

Bản ngày 05/11/2026, do Codex đóng gói từ ba nguồn Markdown hiện tại. Đây là bản để TV1–TV4 rà soát và tự thực hành, chưa phải bộ nộp cuối hoặc chữ ký nghiệm thu của thành viên.

| Nội dung | Word | PDF | Trang đã kiểm tra |
|---|---|---|---|
| Báo cáo bảy nhóm yêu cầu | [Word](bao-cao-7-nhom.docx) | [PDF](bao-cao-7-nhom.pdf) | [1](bao-cao-7-nhom-pages/page-1.png), [2](bao-cao-7-nhom-pages/page-2.png) |
| Hướng dẫn bàn giao và tự kiểm tra | [Word](huong-dan-ban-giao.docx) | [PDF](huong-dan-ban-giao.pdf) | [1](huong-dan-ban-giao-pages/page-1.png) |
| Kịch bản bảo vệ và bàn giao | [Word](kich-ban-bao-ve.docx) | [PDF](kich-ban-bao-ve.pdf) | [1](kich-ban-bao-ve-pages/page-1.png), [2](kich-ban-bao-ve-pages/page-2.png) |

Đã xem đủ 5 trang render cuối: tiếng Việt, tiêu đề, số trang, danh sách và bảng câu hỏi không bị cắt/chồng; đã sửa đường kẻ tiêu đề từ template, số thứ tự từng phần và trang cuối gần trống. Không chỉ dựa vào trích xuất chữ để kết luận hình thức.

## Nguồn và công cụ tái lập

Nguồn là `bao-cao-ky-thuat-7-nhom.md`, `huong-dan-ban-giao-dot-4.md`, `kich-ban-bao-ve-dot-3.md` trong thư mục cha; SHA256 được ghi ở `source-hashes.json`. Dùng Python/Node đóng gói được trả bởi workspace dependencies. `build_review_documents.py` tạo Word từ nguồn. LibreOffice không có trên PATH (giữ log probe); fallback dùng Word cài sẵn xuất PDF qua object model, không điều khiển UI. `render_review_documents.py` nhập mô-đun `render_docx.py` đóng gói và giữ bước rasterize/Poppler của mô-đun đó, thay riêng bộ chuyển đổi DOCX → PDF bằng Word. Không sửa runtime được quản lý và không giả rằng đã chạy được LibreOffice.

## Dữ liệu và mức kiểm chứng

Đợt 5 dùng DB thử `BadmintonShop_SV4_Test_20261105170422_f787c19b`. Đơn #12 mới chạy liên tục trong phiên tiếp tục: Web đặt → đủ nút/MessageBox Desktop test host → Web từng trạng thái → báo cáo 200 → 300 → XLSX/PDF snapshot 300. Tồn 94 → 93. Kỳ UI 01/01–05/11/2026 có thu 300, chi 0; tổng 2025–2026 có thu 711, chi 650. Hai kỳ khác nhau; không dùng file 300 để đại diện thu hai năm 711.

11 bảng XLSX/11 trang PDF thống kê mới khớp đáp án độc lập và đã xem từng trang render. 96/96 hồi quy và 8/8 hợp đồng JS cục bộ chạy mới, ghi riêng; 3/3 tích hợp tự động vẫn là lịch sử đợt 3. Các tập có phần trùng, không cộng thành tổng độc lập. Lệnh mở cặp file mới trong phần mềm đọc đã được gửi, nhưng người dùng dừng Computer Use bằng Escape trước khi quan sát xác nhận Excel/Chrome mới; không nhận kiểm tra phần mềm đọc đó là đạt.

## Việc nhóm cần xác nhận

TTL 10 phút cấu hình được, phạm vi main–sub và nhập Excel chỉ thêm mới vẫn chờ chốt. Cần thành viên tự chạy đăng nhập Desktop sản phẩm, nghiệm thu JavaScript tắt bằng thiết lập trình duyệt, bố cục/DPI trong phạm vi cuối, nhận bàn giao và tập bảo vệ. CSP chặn script đã kiểm tra fallback; đây không phải thay đổi thiết lập JavaScript toàn trình duyệt. Khi nội dung/phạm vi được chốt, xuất và kiểm tra lại bộ nộp cuối.
