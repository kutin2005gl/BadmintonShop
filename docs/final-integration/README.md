# Bàn giao SV4 final — 26/10/2026

**Kết quả hiện tại sau chuyển ổ D:** xem [rà soát trước PR](pr-review/README.md): build 0 lỗi/cảnh báo, integration chạy lại 74/74, kiểm cài đặt từ D 15/15, demo cửa sổ thật và đối chiếu SQL/Excel/PDF. Đã sửa cắt dòng giảm giá POS, giữ mã hóa đơn sau tải lại. SHA nguồn và commit được ghi riêng cho lượt mới; không dùng số PASS hoặc tổng tiền cũ làm kết quả hiện tại.

## Hồ sơ lượt kiểm trước khi chuyển worktree (giữ để đối chiếu)

Nhánh **feature/sv4-final-integration**, tạo từ origin/develop **920a576ed2f2244271965fe4f7c637a3d8185fa1** sau fetch. Worktree hiện tại: `D:\CodexWorktrees\BadmintonShop-sv4` (bản ổ C chỉ là dự phòng). Tận dụng thay đổi đã có tại `6e7657a` (feature/sv4-desktop), không làm lại từ đầu. Đây là hồ sơ lượt kiểm tra trước khi chuyển worktree. Theo yêu cầu mới, lượt rà cuối từ ổ D được phép commit/push/PR, không merge. Manifest bên dưới giữ nguyên cho lượt cũ.

Không có AGENTS.md trong repo/các thư mục cha áp dụng. Đã đọc phân công DOCX, hướng dẫn cài đặt, schema/trigger, code Core/POS/Web/báo cáo và bộ kiểm tra. Checkout gốc `D:\Desktop\BadmintonShop` vẫn giữ thay đổi cá nhân App.config; worktree mới không sửa App.config/appsettings.json. Không reset/stash.

## Thiếu sót đã xác nhận và xử lý

| Điểm thiếu | Kết quả |
| --- | --- |
| develop đã merge PR #2/#4 nhưng thiếu tích hợp POS/khuyến mãi mới trên nhánh SV4 | Áp dụng lại phần tích hợp có sẵn vào nền develop, giữ API dùng chung Web/POS và bảo vệ vòng đời Form. |
| Voucher chỉ có thông báo lỗi gộp | KhuyenMaiRepository/TinhKhuyenMaiService trả riêng mã không tồn tại, chưa đến hạn, hết hạn, hết lượt, ngừng áp dụng, quá 30 ký tự; GioHangService dùng chung thông báo cho hai app. |
| POS kiểm đủ cột nhưng chưa kiểm unique index | HoTroPOS yêu cầu unique filtered index hoạt động trên duy nhất MaYeuCauPOS (khác NULL), chặn trạng thái sẵn sàng khi thiếu. |
| SQL tổng mới có migration 04 | Thêm 06 vào cuối SQL tổng; migration bổ sung riêng vẫn chạy lại được. Không đổi/xóa bảng/cột cũ hoặc trigger. |
| Demo không có giới hạn CSDL đích và chưa có ưu đãi để trình diễn | 05 chỉ nhận tên BadmintonShop_SV4_Test_* hoặc BadmintonShop_SV4_Demo*, bỏ USE cứng, chọn bằng -d. Giữ khóa transaction/sentinel, nhập trước bán, đủ 12 tháng; thêm ba CTKM nhãn demo và voucher SV4DEMO15. Không thêm tài khoản. |
| Tài liệu còn gọi service khung, POS giá thường, hướng dẫn xóa giỏ khi giá đổi | Cập nhật HDSD/đặc tả/tổng kết/tích hợp/kế hoạch/kết quả/sơ đồ/UI; giữ hồ sơ cũ có ngày và thêm checklist SV2 chưa đánh dấu. |
| Harness đặt tên ảnh 1920×1080 dù Windows giới hạn còn 1556×884 | Kiểm kích thước thật; tên ảnh và render-sizes.tsv ghi kích thước thực. Log LIMIT chỉ rõ 1920×1080 chưa xác minh. Không coi ảnh bị giới hạn là kiểm màn hình đích. |

## Thiết kế thanh toán được giữ và kiểm lại

Một GioHangService/TinhKhuyenMaiService: SP giảm trước; chọn bậc hóa đơn có ngưỡng cao nhất đạt được, so với voucher bằng số tiền giảm; chỉ chọn một ưu đãi cấp đơn, hòa ưu tiên voucher. Làm tròn dòng một lần theo DECIMAL(18,0), AwayFromZero; không nhân giá đơn vị đã làm tròn.

Serializable transaction đọc lại giá/trạng thái/tồn/khuyến mãi, khóa SP theo mã tăng dần, so báo giá xác nhận. Ghi GiamGia % vào chi tiết; MaCTKM/TienGiamGia cấp đơn vào header; trigger duy nhất trừ/hoàn tồn và tính tổng. Lượt voucher tăng có điều kiện trong cùng transaction, rollback toàn bộ khi header/dòng hai lỗi.

Token POS có applock theo token và SHA-256 payload. Retry đã commit trả mã cũ trước khi đọc ưu đãi mới; không trừ tồn/lượt lần hai. Đổi nhân viên, tiền, giỏ/báo giá hoặc voucher nhập (kể cả voucher thua ưu đãi hóa đơn) bị từ chối. PDF/báo cáo lấy tiền đã lưu. UI giữ giỏ/token khi lỗi, khóa thao tác trong khi lưu, chỉ thông báo thành công sau transaction.

Khách chỉ hủy đơn Online của mình khi Chờ xác nhận; nhân viên hủy Chờ/Đã xác nhận và bắt buộc lý do. Hủy đồng thời chỉ hoàn kho một lần. Giữ chính sách SV3 hiện tại: chưa hoàn lượt voucher khi hủy, không tự quyết định chính sách mới.

## Môi trường và kết quả thực tế

Windows x64 `10.0.26200`, SDK `10.0.402`, target .NET 8/WinForms, SQL Server `16.0.1000.6`, server `DESKTOP-FGE20L2` (kết nối test `.`), Python có openpyxl/pypdf. Không đổi target/framework hoặc thêm thư viện UI.

| Lệnh/phạm vi | Kết quả | Bằng chứng |
| --- | --- | --- |
| dotnet build BadmintonShop.sln --no-restore (OutDir riêng) | 0 lỗi, 0 cảnh báo | [build.txt](build.txt) |
| --integration --server . | **74 đạt, 0 lỗi**; 6 LIMIT kích thước 1920×1080 chưa kiểm | [results.txt](results.txt) |
| --install-checks --server . | **15 đạt, 0 lỗi**, gồm 13 unit + J01/J02 | [install-results.txt](install-results.txt) |
| verify_integration_exports.py | Đạt: SQL/bảng/XLSX/PDF khớp cả 12 bộ lọc, hóa đơn giữ giá/giảm lịch sử | [independent-exports.txt](independent-exports.txt) |
| --schema-readonly | Đọc được metadata; **CSDL cấu hình chưa tương thích POS** | [schema-readonly.txt](schema-readonly.txt) |

CSDL tích hợp cuối: `BadmintonShop_SV4_Test_20261026212650_f83402ea`. CSDL kiểm cài mới: `BadmintonShop_SV4_Test_Install_20261026211927_eded9f88`. Chỉ tạo/ghi fixture trong CSDL tên riêng này; không tự xóa để còn đối chiếu. SQL tổng được kiểm trên DB mới, bỏ phần DROP/CREATE và thay USE sang tên test, không chạy nguyên script lên DB có dữ liệu cần giữ.

Tổng doanh thu hoàn thành cuối **13.172.379 đồng**, khoảng 2 năm trước ngày 26/10/2026 đến hết 26/10/2026, mọi loại bán; phụ thuộc kết quả cạnh tranh trong từng lần chạy, không hardcode kết quả cũ. [XLSX](final-integration-report.xlsx), [PDF báo cáo](final-integration-report.pdf), [PDF hóa đơn lịch sử](promotion-web-invoice.pdf). Các filtered-day/month/year/range × all/online/pos có [tổng SQL riêng](filtered-expected.tsv), Excel và PDF riêng. Không so số của hai CSDL hoặc hai bộ lọc khác nhau.

74 kiểm tra gồm: U01–13, I01–16, P01–19, H01, render/smoke GUI (cả ADMIN/QUANLY/NHANVIEN), N01–05, F01. P/N kiểm voucher, quy tắc phối hợp, làm tròn, tiền sau giảm, stale giá/ưu đãi, voucher cuối và SP cuối, token/payload retry, rollback và báo cáo. H01 dùng Kestrel/MVC/cookie/anti-forgery thật nhưng đăng nhập/giỏ từ endpoint fixture **chỉ nằm trong test host**; không có endpoint bypass trong startup Web thật. Không gọi HTTP fixture/repository là E2E trình duyệt.

## Schema CSDL đang cấu hình — chỉ đọc

Desktop/Web hiện đọc `localhost / BadmintonShop` (không in mật khẩu), kết nối thực tế `DESKTOP-FGE20L2 / BadmintonShop`. Thiếu:

- HoaDon.LyDoHuy NVARCHAR(500) NULL.
- HoaDon.MaYeuCauPOS UNIQUEIDENTIFIER NULL.
- HoaDon.TienKhachDua DECIMAL(18,0) NULL.
- HoaDon.DauVanTayPOS VARCHAR(64) NULL.
- Unique filtered index UX_HoaDon_MaYeuCauPOS trên MaYeuCauPOS khác NULL.

MaCTKM/TienGiamGia/chi tiết GiamGia/ThanhTien đúng kiểu; ba trigger SV1 bật. Chưa migration/seed/giao dịch thử trên CSDL này. SV1 sao lưu/review, chọn đúng DB rồi áp dụng 04 → 06; không chạy lại 01 hoặc SQL tổng. Xem [HDSD](../bao-cao/huong-dan-su-dung-sv4.md) cho lệnh schema/demo và cách chọn DB riêng.

## Giao diện, bằng chứng và phần chưa xác minh

Ảnh tại [screenshots](screenshots) là DrawToBitmap từ Form WinForms thật sau Show/OnLoad. Có tổng quan, sản phẩm, danh mục, đơn hàng, POS, thống kê, giỏ áp voucher và scale mô phỏng. [Kích thước thực](render-sizes.tsv): 1366×768 và 1556×884; **1920×1080 chưa xác minh** do Windows cap cửa sổ. Thử ép kích thước có assertion đã phát hiện 6 lỗi harness, lưu [probe](render-size-probe.txt); đã sửa tên ảnh/log cho đúng phạm vi, không sửa production để vượt giới hạn màn hình và không nhận 1920 đã đạt. 74/74 không phải khẳng định mọi yêu cầu UI đã kiểm.

125%/150% là Control.Scale mô phỏng, chưa đổi DPI Windows thật. Chưa kiểm kéo thả Designer, máy in, keyboard/trợ năng đầy đủ hoặc mọi nhánh hộp thoại Yes/No. P18 chỉ thao tác áp/bỏ mã/tổng trên UI, không bấm xác nhận thu tiền; thanh toán được kiểm qua repository/SQL và HTTP fixture. Mạng đứt đúng sau commit/trước phản hồi chưa được fault injection; retry lại giao dịch đã commit đã kiểm.

E2E login/catalog UI SV2 chưa có trên develop, chưa xác nhận web mount/fallback ảnh, SMTP và màn hình quản lý CTKM của SV3. **Chưa có SV2 kiểm thử chéo**; [checklist](../bao-cao/checklist-sv2-desktop.md) để người kiểm điền. Doanh thu theo NgayLap của đơn hiện Hoàn thành; tên/danh mục SP chưa snapshot. Chưa benchmark dữ liệu lớn.

Điều tra Dispose/CreateHandle ngày 22/10 vẫn chưa tái hiện lỗi gốc; giữ logger/bảo vệ cache/callback nhưng không nhận đã sửa dứt điểm. Bằng chứng debugger lịch sử ở [lifecycle-diagnostics](../lifecycle-diagnostics/README.md), không tính là chạy debugger mới ngày 26/10.

## Chạy và phối hợp

Mở solution tại worktree trên, Startup Project = BadmintonShop.Desktop, dùng env kết nối DB test/demo có schema đủ rồi F5. [HDSD và demo 5–7 phút](../bao-cao/huong-dan-su-dung-sv4.md) đi theo thêm SP → nhập SV1 → POS khuyến mãi → đơn web (hoặc minh họa fixture có ghi rõ) → thống kê → Excel/PDF.

SV1 review 04/06/index/applock/trigger và cập nhật DB theo quy trình; SV3 review ưu đãi/hủy, email và idempotency web; SV2 hoàn thiện login/catalog/kho ảnh và kiểm chéo. Không bổ sung trả hàng/hóa đơn điện tử/thanh toán thật. Mọi thay đổi final vẫn để review tại worktree, không commit/push/PR/merge.
