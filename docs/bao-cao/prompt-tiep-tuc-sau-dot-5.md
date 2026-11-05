# Prompt tiếp tục sau đợt 5

Tiếp tục dự án tại `D:\DoAn_CNPM\BadmintonShop`, nhánh `develop`, theo `docs/bao-cao/checklist-tien-do-4-thanh-vien.md`. Đọc git status/log trước khi làm; giữ nguyên thay đổi có sẵn. Không reset, tự commit hoặc push nếu tôi chưa yêu cầu trong phiên mới.

Đọc trước:
- `docs/bao-cao/ke-hoach-hoan-thien-yeu-cau-mon-hoc.md`.
- `docs/bao-cao/quy-tac-va-tien-do-dot-1.md` đến `quy-tac-va-tien-do-dot-5.md`.
- `docs/bao-cao/checklist-tien-do-4-thanh-vien.md`.
- `docs/bao-cao/bao-cao-ky-thuat-7-nhom.md`.
- `docs/bao-cao/huong-dan-ban-giao-dot-4.md` và `kich-ban-bao-ve-dot-3.md` (nguồn đã cập nhật đợt 5, tên file giữ nguyên).
- `docs/bao-cao/ban-ra-soat-dot-5/README.md`.

Trạng thái sau đợt 5:
- Build thành công, 0 lỗi; build incremental không chứng minh đã sửa cảnh báo lịch sử.
- Hồi quy mới 96/96 và hợp đồng JS cục bộ 8/8 đạt, ghi riêng, không cộng thành tổng độc lập. Acceptance tự động 3/3 vẫn là lịch sử đợt 3, chưa chạy lại ở đợt 5.
- DB thử mới `BadmintonShop_SV4_Test_20261105170422_f787c19b`: #11 nối phiên do hết quota; #12 mới đã chạy liên tục trong phiên tiếp tục qua UI Web → các nút/MessageBox Desktop → Web từng trạng thái → báo cáo 200 → 300 → xuất XLSX/PDF cùng snapshot. Tồn lượt #12 là 94 → 93; tổng thu 2025–2026 là 711. Desktop vẫn là test host `--acceptance-desktop-review`, bỏ qua đăng nhập ứng dụng sản phẩm.
- `minh-chung-dot-5/ui-snapshot-300.xlsx` và `.pdf`: kỳ 01/01–05/11/2026, thu 300/chi 0; 11 bảng/11 trang khớp đáp án độc lập và đã xem từng trang render. Không dùng bộ này thay snapshot hai năm 711/650.
- Main–sub đã kiểm tra trên trình duyệt thật qua proxy loopback: phản hồi cũ bị giữ 15 giây, chọn mới, hủy request cũ; HTTP 503, đóng socket, Space phục hồi. Không nhận transport cố ý bỏ qua abort đã được thử trên trình duyệt; trường hợp đó thuộc bộ cục bộ. Link Xem hoạt động khi CSP chặn script, chưa phải tắt JavaScript bằng thiết lập trình duyệt.
- Web đã quan sát khung 390/1280/1920 px; không đồng nhất viewport với DPI Windows. Desktop cửa sổ nhỏ có nội dung cần cuộn, chưa nghiệm thu mọi kích thước/DPI.
- Bộ `ban-ra-soat-dot-5/` và `.zip`: 3 Word, 3 PDF, 5 ảnh trang tài liệu cuối đã render và xem đủ. Đây là bản kỹ thuật chờ nhóm duyệt, không phải bộ nộp cuối đã chốt.
- LibreOffice chưa có. Fallback dùng Word cài sẵn xuất PDF qua object model, sau đó mô-đun `render_docx.py` đóng gói và Poppler rasterize. Công cụ tái lập ở `tools/XuatBaoCao/`; dùng runtime từ `load_workspace_dependencies`, kiểm tra lại công cụ trước khi dùng.
- Người dùng từng dừng Computer Use bằng Escape trước khi xác minh cặp file mới trong Excel/Chrome; đã dừng UI, chưa ghi nhận lần mở phần mềm đọc mới là đạt. Web PID 30416, Desktop 31296, proxy 31340 đã được dừng; không dùng lại PID để kết luận tiến trình hiện tại.

Ưu tiên tiếp theo:
1. Kiểm tra lại trạng thái DB thử bằng SQL chỉ đọc trước khi dùng; mở và xác minh cặp XLSX/PDF snapshot 300 trong Excel/Chrome. Đối chiếu đúng kỳ và số liệu, lưu ảnh; không suy đạt từ lệnh mở file.
2. Hoàn thiện kiểm tra JavaScript tắt bằng thiết lập trình duyệt và bố cục/DPI trong phạm vi đã chốt. Đọc skill Computer Use trước; chỉ nhận đạt điều đã quan sát. CSP chặn script không thay cho thiết lập JS toàn trình duyệt.
3. Chuẩn bị lượt tự kiểm tra có đăng nhập Desktop sản phẩm và xác nhận của TV1–TV4. Đọc mã `--acceptance`, `--interactive-import`, `--acceptance-ui-setup`, `--acceptance-desktop-review` trước khi dùng. Test host không thay vòng đăng nhập sản phẩm; không tự động hóa hộp đăng nhập nếu skill cấm. Khi cần thành viên thao tác, chuẩn bị sẵn môi trường và kịch bản cụ thể, không xác nhận thay họ.
4. Rà soát phạm vi còn mở, sửa phần kỹ thuật độc lập có bằng chứng, cập nhật checklist/minh chứng sau từng phần. Bộ nộp cuối chỉ xuất lại sau chốt nội dung/phạm vi; mọi Word/PDF mới phải render và xem từng trang.

Giới hạn dữ liệu và nghiệp vụ:
- SQL Server `.\SQLEXPRESS`. Không sửa App.config/appsettings.json; đặt kết nối riêng từng tiến trình thử.
- Không ghi vào DB mẫu `BadmintonShop_SV4_Test_20261105154154_a5e27e92` (mẫu 511/650). DB UI cũ `...161022_7f4ab55a`, acceptance cũ `...162207_dc9ab928` và DB đợt 5 đều đã có giao dịch; không coi là mẫu nguyên trạng. Lần đọc chéo catalog DB bảo vệ cuối đợt 5 bị timeout, không phải xác minh mới thành công.
- Không xóa/tạo lại DB có sẵn. Mọi lượt thử ghi mới dùng DB thử tên mới. `--acceptance-ui-setup --server .\SQLEXPRESS` tạo DB mới, mẫu và khách thử rồi dừng trước khi tự đặt đơn; có DDL migration 07 bỏ USE DB gốc và dữ liệu đánh giá mẫu.
- Tài khoản Desktop thử `sv4test` / `TestOnly@123`; khách thử `acceptance@example.invalid` / `TestOnly@123`, chỉ dùng trên DB thử.
- Doanh thu chỉ đơn Hoàn thành, phân kỳ NgayLap. Chi là tiền nhập hàng trong kỳ; thu trừ chi gọi Chênh lệch thu–chi, không phải lợi nhuận kế toán.
- TTL 10 phút cấu hình được, phạm vi main–sub và nhập Excel chỉ thêm mới vẫn là giả định chờ nhóm/giảng viên chốt. Tiếp tục các phần độc lập; không tự chốt hoặc xác nhận bàn giao, tự kiểm tra hay tập bảo vệ thay thành viên.

Phiên mới này tôi cho phép tiếp tục điều khiển UI trên dữ liệu thử cho các bước kiểm tra nêu trên, trong giới hạn skill. Nếu tôi dừng bằng Escape, dừng UI ngay và ghi rõ phần chưa kiểm chứng. Không cần hỏi lại thông tin đã có trong tài liệu. Mục tiêu là hoàn thiện phần độc lập còn làm được, không tuyên bố toàn dự án hoàn tất khi còn thiếu nghiệm thu.
