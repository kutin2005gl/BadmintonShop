# Triển khai và kiểm chứng — đợt 3, 05/11/2026

Tiếp nối [đợt 2](quy-tac-va-tien-do-dot-2.md), theo [checklist TV1–TV4](checklist-tien-do-4-thanh-vien.md). Codex thực hiện; không thay xác nhận bàn giao của bốn thành viên hoặc giảng viên.

## 1. Trạng thái ban đầu và an toàn dữ liệu

Đã kiểm tra git status trước khi làm: nhiều file sửa và chưa được theo dõi từ đợt trước. Giữ nguyên các thay đổi này, không reset, commit hoặc push. Hai cấu hình kết nối giữ nguyên trong đợt 3, SHA256 đầu/cuối:

| File | SHA256 |
|---|---|
| Desktop/App.config | 5A27D65BCCEDC0FE1C58F2742EA8B07C13CCF54AB41C43D06F288794F6AA6914 |
| Web/appsettings.json | 65CA48BC0DA9D3C97F9A324B0460EE435C3522FC95415E85C305E0F3A244D99D |

Giữ riêng DB nguyên mẫu `BadmintonShop_SV4_Test_20261105154154_a5e27e92`, không ghi vào DB này. Mọi thao tác ghi dùng DB thử mới. Không chạy tiền tố DROP/CREATE của script cài đặt lên DB có sẵn. Tài khoản, địa chỉ và sản phẩm nghiệm thu đều là dữ liệu giả lập.

Giữ quy tắc doanh thu Hoàn thành theo NgayLap; chi tiền nhập trong kỳ; Chênh lệch thu–chi không phải lợi nhuận kế toán. TTL 10 phút cấu hình được, nhập chỉ thêm mới và phạm vi main–sub vẫn là phương án tạm, chưa được xác nhận cuối.

## 2. Web và Desktop ở hai tiến trình

Thêm `--acceptance --server .\SQLEXPRESS` vào bộ Checks. Luôn tạo DB tên mới, nạp mẫu 511/650, rồi khởi chạy **startup Web sản phẩm thật** bằng tiến trình dotnet riêng. Không thêm endpoint đăng nhập/giỏ hàng thử vào Web. HTTP dùng đăng nhập BCrypt, cookie, anti-forgery, thêm giỏ và xác nhận đơn qua các route sản phẩm.

Phía Desktop là **test host STA mở FrmDonHang và FrmThongKe thật**, tạo phiên Admin thử; không chạy vòng đăng nhập của Desktop.exe. Đổi trạng thái gọi handler lưu bất đồng bộ của form, bỏ qua MessageBox trong kiểm tra tự động. Vì vậy đây là tích hợp tự động hai tiến trình, chưa phải nghiệm thu thủ công đầy đủ bằng hai ứng dụng sản phẩm.

| Mốc | Doanh thu | SQL báo cáo |
|---|---:|---:|
| Mẫu 2025–2026 | 511 | Đã nạp snapshot |
| Khách đặt một vợt giá 100, đơn còn chờ | 511 | Snapshot tải lại sau token ghi Web |
| Desktop xác nhận → đang giao → hoàn thành | 611 | Xem lại: 2 truy vấn |
| Xem cùng điều kiện lần nữa | 611 | Thêm 0 |
| Bấm Làm mới bằng kiểm tra tự động | 611 | Thêm 2 |

Mỗi trạng thái Desktop được đối chiếu bằng SQL và trang lịch sử Web. Kiểm tra thêm endpoint chi tiết dòng: chưa đăng nhập bị chuyển đăng nhập, khác chủ sở hữu nhận 404, chủ sở hữu nhận partial HTML có `Cache-Control: no-store`.

Lần cuối: DB `BadmintonShop_SV4_Test_20261105162207_dc9ab928`, Web PID 26076, Desktop test host PID 29056; **3/3 đạt**, gồm DB-R01/R02 và E01. [Log cuối](minh-chung-dot-3/results.txt). DB cuối này có thu 611, chi 650, không phải mẫu nguyên trạng.

Hai lỗi kịch bản đã sửa và giữ log: lần đầu chờ chưa hết chuỗi lưu/nạp bất đồng bộ; lần sau so chữ Việt với HTML chưa giải mã entity. [Lần đầu](minh-chung-dot-3/first-attempt-results.txt), [HTML encoded](minh-chung-dot-3/encoded-html-attempt-results.txt). Không tính các lần lỗi là nghiệm thu đạt.

Trong kiểm tra trình duyệt còn phát hiện fixture thiếu migration 07 nên trang sản phẩm báo thiếu DanhGiaSanPham. Bổ sung **phần DDL** của migration 07 vào setup acceptance, bỏ USE DB gốc và bỏ dữ liệu đánh giá mẫu; trang sản phẩm đã mở lại thành công. Không sửa logic sản phẩm để che lỗi schema.

## 3. Thao tác Windows và phần mềm đọc thật

Trên DB thử `BadmintonShop_SV4_Test_20261105160133_cc5a95b1`, đã thao tác bằng công cụ điều khiển giao diện Windows, có quan sát trực tiếp:

1. Tải mẫu → Save As → lưu [ui-template.xlsx](minh-chung-dot-3/ui-template.xlsx).
2. Chọn [file sai](minh-chung-dot-3/ui-invalid.xlsx) bằng OpenFileDialog: lỗi dòng 2, DonGiaBan; nút xác nhận khóa. [Ảnh](minh-chung-dot-3/ui-invalid.png).
3. Chọn [file đúng](minh-chung-dot-3/ui-valid.xlsx): một dòng, không lỗi; nút xác nhận mở. [Preview](minh-chung-dot-3/ui-preview.png).
4. Bấm Xác nhận nhập → MessageBox mô tả thêm mới/tồn 0 → Yes. [Hộp xác nhận](minh-chung-dot-3/ui-confirm.png), [kết quả](minh-chung-dot-3/ui-import-success.png).
5. Đọc SQL độc lập: MaSP 4, giá 123000, tồn 0. [Đối chiếu](minh-chung-dot-3/ui-import-sql.txt). Không ghi vào DB demo nguyên mẫu.

Đây là thao tác UI do Codex điều khiển, không ghi là thành viên nhóm đã tự nghiệm thu. Quyền/rollback/file trùng vẫn có bộ kiểm tra SQL riêng, không suy từ một file hợp lệ trên UI.

[Báo cáo sau giao dịch](minh-chung-dot-3/acceptance-report.xlsx) đã mở bằng Microsoft Excel, không có yêu cầu sửa file; nhìn thấy tổng 611 và dòng phát sinh mới. [Ảnh Excel](minh-chung-dot-3/excel-open.png). [PDF](minh-chung-dot-3/acceptance-report.pdf) đã mở bằng trình đọc PDF Chrome, hiển thị 13 trang. [Ảnh trình đọc](minh-chung-dot-3/pdf-open.png). Việc mở file không đồng nghĩa đã kiểm tra trực quan từng sheet/trang trong phần mềm đó.

`verify_progress_exports.py` nhận thêm danh sách prefix tùy chọn, giữ mặc định cũ. Bộ đọc openpyxl/pdfplumber độc lập xác nhận **13 bảng XLSX / 13 trang PDF khớp từng ô**, đủ cột, không tràn ngang so với snapshot cùng lúc xuất. [Log](minh-chung-dot-3/export-results.txt). File này từ lần 16:10, trước đơn Web UI thứ hai; cùng quy tắc và kết quả 611 như lần cuối.

## 4. Main–sub Web và rà soát phạm vi

Giả định triển khai độc lập: main–sub Web áp dụng **Đơn hàng của tôi**; Web khách không có màn phiếu nhập. Desktop hóa đơn/phiếu nhập giữ phần đã kiểm tra đợt 2. Không tự mở quyền quản trị hoặc đưa đơn cá nhân vào bộ nhớ công khai.

Click bất kỳ ô trên dòng hoặc Enter/Space khi dòng có focus sẽ nạp chi tiết ngay dưới danh sách. Link Xem vẫn dùng được khi JavaScript tắt; các phím sửa đổi để mở tab mới giữ hành vi link. Yêu cầu cũ bị hủy, chỉ phản hồi của lựa chọn mới nhất được phép cập nhật vùng chi tiết. Có trạng thái tải/lỗi và aria-live. Nội dung dùng lại view chi tiết, kiểm tra chủ sở hữu cả hóa đơn và dòng.

Nghiệm thu trình duyệt thật trên Web PID 21664, cổng 5279, DB `...161022_7f4ab55a`:

- Đăng nhập tài khoản khách thử; đặt đơn #12 qua trang sản phẩm → giỏ → xác nhận COD, giá 100; địa chỉ giả lập. Đơn #11 đã hoàn thành từ tích hợp, #12 còn chờ.
- Click đủ **12 ô trên 2 dòng**, chờ heading đúng mã đơn sau mỗi lần; Enter đổi về #12; URL vẫn `/DonHang`. [Ảnh](minh-chung-dot-3/web-main-sub.png).
- Quyền endpoint và no-store kiểm tra qua HTTP ở E01. Chưa kiểm tra mô phỏng mạng chậm/đảo thứ tự phản hồi, mất mạng, bàn phím Space, hoặc tắt JavaScript bằng trình duyệt; không nhận đạt các trường hợp này.
- UI được kiểm tra ở kích thước thực tế máy. Chưa nghiệm thu mọi màn hình/DPI hoặc xác nhận giao diện 1920×1080.

Người dùng dừng Computer Use bằng Escape khi chuẩn bị mở Desktop để bấm xử lý #12. Đã dừng điều khiển UI; **chưa bấm Xác nhận/Giao hàng/Hoàn thành qua MessageBox**, chưa có chuỗi UI trọn Web → Desktop → báo cáo. Phiên thử Desktop mở sẵn đã được dừng để giải phóng file build; Web kiểm tra được dừng sau khi lưu minh chứng. #12 vẫn chờ, tổng hoàn thành của DB UI vẫn 611.

## 5. Kiểm tra cuối và tái lập

```powershell
dotnet build BadmintonShop.sln --no-restore -v minimal -m:1 /p:UseSharedCompilation=false
dotnet run --project tests/BadmintonShop.SV4.Checks --no-build -- --acceptance --server .\SQLEXPRESS
dotnet run --project tests/BadmintonShop.SV4.Checks --no-build -- --report-integration --integration --server .\SQLEXPRESS
# Dùng Python có sẵn openpyxl/pdfplumber:
python tests/BadmintonShop.SV4.Checks/verify_progress_exports.py docs/bao-cao/minh-chung-dot-3 acceptance-report
```

Build cuối thành công, 0 lỗi ([log](minh-chung-dot-3/build.txt)); cảnh báo CS8602 lịch sử chưa được nhận là đã sửa chỉ vì build incremental không cảnh báo. Hồi quy trên mã cuối **96/96 đạt** ([log](minh-chung-dot-3/regression-results.txt)), DB thử mới `...162224_f5d1f859` và `...162250_2235cade`. Có phần trùng 3/3 acceptance và các lần đợt 2; **không cộng thành tổng kiểm tra độc lập**.

`--acceptance --interactive-import` giữ form nhập mở sau tích hợp để thao tác UI. `--acceptance-desktop-review` yêu cầu biến môi trường kết nối rõ ràng với tên DB bắt đầu BadmintonShop_SV4_Test_; mở FrmMain bằng phiên Admin thử, không kiểm tra đăng nhập. Hai chế độ chỉ nằm trong dự án tests, không thêm đường tắt vào ứng dụng sản phẩm.

## 6. Việc còn lại

- Hoàn tất thao tác Desktop xử lý đơn #12 và báo cáo trên hai tiến trình, chụp ảnh trước/sau; hiện chỉ tích hợp tự động đã đạt.
- Nhóm/giảng viên chốt TTL, phạm vi main–sub và nhập chỉ thêm mới. Xác nhận bàn giao TV1–TV4, không dùng Codex thay chữ ký thành viên.
- Chạy [kịch bản bảo vệ](kich-ban-bao-ve-dot-3.md); bổ sung minh chứng cho tình huống main–sub mạng chậm/lỗi và DPI nếu nằm trong phạm vi cuối.
- Tài liệu Word/PDF/hình cuối chỉ xuất sau khi nội dung/phạm vi chốt; đợt này cập nhật Markdown, chưa nhận là đã xuất bộ báo cáo cuối hoặc tập bảo vệ xong.
