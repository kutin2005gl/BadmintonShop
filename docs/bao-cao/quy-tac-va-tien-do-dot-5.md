# Kiểm chứng và bàn giao đợt 5

Ngày 05/11/2026. Người thực hiện: Codex. Giữ nguyên thay đổi có sẵn, không reset, commit, push hoặc đổi cấu hình kết nối. Không xác nhận thay TV1–TV4.

## Dữ liệu thử và lượt UI

Thêm `--acceptance-ui-setup` trong dự án Checks: luôn tạo DB tên mới, nạp bộ mẫu, DDL migration 07 không dữ liệu đánh giá mẫu, tạo khách thử và dừng trước khi tự đặt đơn. Không tạo đường tắt trong ứng dụng sản phẩm. Tăng thời gian chờ riêng lệnh CREATE DATABASE thử lên 120 giây sau lần quá thời gian; giữ log lỗi `setup-timeout.txt`. Không xóa DB dở dang hoặc khởi động lại SQL Server.

DB mới: `BadmintonShop_SV4_Test_20261105170422_f787c19b`, server `.\SQLEXPRESS`. Setup 2/2 đạt, thu 511, chi 650. Web sản phẩm cổng 5285; proxy thử loopback 5286; Desktop `--acceptance-desktop-review` mở form thật bằng phiên Admin thử, **bỏ qua đăng nhập Desktop sản phẩm**. Không ghi DB nguyên mẫu hoặc các DB đợt trước. Kết nối chỉ đặt riêng từng tiến trình.

Đơn #11 đặt qua UI Web lúc 17:07, giá 100; SQL tồn 95 → 94, đơn Chờ xác nhận chưa làm tăng thu 511. Desktop đã bấm Xác nhận và Giao hàng, đồng ý MessageBox; Web theo dõi đúng từng trạng thái. Buổi thao tác bị gián đoạn do hết quota trước Yes của bước Hoàn thành; tiếp tục sau khi người dùng yêu cầu, không gọi đây là lượt liên tục. Sau Yes, #11 Hoàn thành trên Desktop/Web; SQL thu 2025–2026 = 611, kỳ UI 01/01–05/11/2026 = 200. Báo cáo nền 100 giữ nguyên khi chỉ quay lại form; Xem báo cáo mới cập nhật 200. Không gán số truy vấn cho lượt UI.

## Lượt mới liên tục của đơn 12

Sau khi tiếp tục buổi làm việc, đặt **đơn #12 mới trên DB đợt 5** qua Web lúc 19:11 (khác đơn #12 của DB đợt 3–4). Baseline kỳ 01/01–05/11/2026 là 200, giá P = 100, tồn trước đặt 94; sau đặt 93, thu vẫn 200 khi còn chờ. Trên Desktop tải danh sách, chọn đúng #12, bấm Xác nhận → Giao hàng → Hoàn thành và Yes mỗi MessageBox. Web được tải lại sau từng bước: đúng Đã xác nhận, Đang giao, Hoàn thành. Đây là lượt mới liên tục **sau phiên đăng nhập thử Web**, vẫn dùng Desktop test host; không chứng minh đăng nhập Desktop sản phẩm.

Quay lại báo cáo đang mở vẫn giữ 200, bấm Xem báo cáo thành 300, bấm Làm mới vẫn 300. [SQL sau hoàn thành](minh-chung-dot-5/sql-order12-completed.txt): #11/#12 Hoàn thành, tồn 93; tổng 2025–2026 là 711, kỳ UI là 300. Không suy số câu SQL từ UI vì lượt này không gắn bộ đếm; sau khoảng gián đoạn, TTL cũng đã qua hạn nên không quy toàn bộ tải lại chỉ cho tín hiệu commit.

Bấm Xuất Excel và Xuất PDF, thao tác SaveFileDialog và thông báo kết quả; cả hai xuất từ cùng snapshot 300, không đổi điều kiện hoặc ghi giao dịch ở giữa. [XLSX mới](minh-chung-dot-5/ui-snapshot-300.xlsx), [PDF mới](minh-chung-dot-5/ui-snapshot-300.pdf). Lần nhập đường dẫn có dấu `/` vào hộp Windows bị từ chối tên file; đã đổi sang `\` và lưu thành công, không phải lỗi logic xuất.

`verify_ui_snapshot_300.py` dựng đáp án độc lập từ fixture và hai đơn UI 100 + 100, không dùng BaoCaoService hoặc file xuất làm đáp án. Tổng 70 + 30 + 100 + 100 = 300; chi trong kỳ UI bằng 0 vì phiếu 150 nằm 31/12/2026 ngoài kỳ. [Đọc độc lập](minh-chung-dot-5/export-results.txt): **11 bảng XLSX / 11 trang PDF khớp mọi ô**, đúng schema/số hàng/cột và tổng 300. Đã render 11 trang bằng Poppler và xem từng trang 01–11: chữ Việt, tiêu đề, bảng, tổng và chân trang không bị cắt/chồng. Kiểm tra render không đồng nghĩa xem mọi sheet/trang trong Excel/Chrome hoặc thành viên đã nghiệm thu.

## Main–sub qua proxy trên trình duyệt thật

`main-sub-test-proxy.cjs` chỉ lắng nghe 127.0.0.1, chuyển tiếp tới Web sản phẩm trên DB thử; không thêm nhánh gây lỗi vào Web sản phẩm. Điều khiển bằng file JSON trong thư mục minh chứng. Log chỉ ghi đường dẫn, thời điểm, mã lỗi và độ trễ, không lưu cookie/mật khẩu/body cá nhân.

- Request #11 bị giữ 15 giây, sau đó chọn #12 có trễ 100 ms. UI hiện Đang tải rồi đúng #12, aria-busy false, URL vẫn `/DonHang`. Log id 30 của #12 phát ra trước id 29 của #11; id 29 ghi browserClosed true vì AbortController đã hủy yêu cầu cũ. Sau khi proxy thả phản hồi cũ, UI vẫn #12. Đây là kiểm tra hủy request cũ và chọn mới trên mạng mô phỏng thật; tình huống transport bỏ qua abort vẫn thuộc 8 hợp đồng cục bộ đợt 4.
- HTTP 503 trả thông báo lỗi, không chèn body lỗi. Proxy đóng socket của request chi tiết cho thông báo lỗi tải; không tắt mạng hệ điều hành. Bỏ lỗi và nhấn Space trên dòng #12: chi tiết phục hồi. [Ảnh phục hồi](minh-chung-dot-5/network-recovered.jpg), [nhật ký proxy](minh-chung-dot-5/proxy-events.jsonl).
- Proxy thêm CSP `script-src 'none'` cho trang HTML: bấm link Xem mở trang `/DonHang/ChiTiet/12`, đúng chi tiết. Đây là kiểm tra **script bị trình duyệt chặn bằng CSP**, không nhận đã đổi thiết lập JavaScript toàn trình duyệt. [Ảnh fallback](minh-chung-dot-5/scripts-blocked-fallback.jpg).
- Kiểm tra Web tại khung 390 × 844, 1280 × 800, 1920 × 1080; bảng ở 390 px cuộn ngang trong vùng bảng, chi tiết đọc được; Enter chọn đúng #12. Đã khôi phục viewport mặc định. Không quy đổi CSS viewport thành DPI thật và không nhận mọi mức DPI. Desktop quan sát khung 1920 × 1020 và cửa sổ thu nhỏ khoảng 1106 × 740: ở khung nhỏ phải cuộn, có cột/nút ngoài vùng nhìn; chưa nghiệm thu tốt toàn bộ bố cục nhỏ/DPI.

Minh chứng tại [minh-chung-dot-5](minh-chung-dot-5/). Các ảnh chụp lúc đang tải không được dùng riêng để chứng minh trạng thái cuối. Kết quả cuối có SQL, DOM, file và ảnh sau tải tương ứng. Bộ tài liệu kỹ thuật đã đóng gói như mục dưới; chưa nhận bản nộp cuối được nhóm duyệt.

## Hồi quy và tài liệu đã đóng gói

Chạy mới `--report-integration --integration --server .\SQLEXPRESS`: **96/96 đạt** trên hai DB mới `...193045_0028e895` và `...193323_747b5afb`; [log](minh-chung-dot-5/regression-results.txt). Bộ script cục bộ chạy lại **8/8**: [log](minh-chung-dot-5/main-sub-local.txt). Hai tập được ghi riêng, không cộng với nhau hoặc 3/3 acceptance lịch sử đợt 3. Đợt này không chạy lại chế độ acceptance tự động E01.

Build trong đợt 5 thành công, 0 lỗi: [build](minh-chung-dot-5/build-setup-retry.txt). Đây là build incremental; không chứng minh cảnh báo CS8602 lịch sử đã được sửa. Các thay đổi C# riêng đợt này chỉ ở dự án Checks (nhánh setup UI và thời gian chờ CREATE DB thử), không đổi mã sản phẩm hoặc cấu hình kết nối.

Đã cập nhật nguồn báo cáo 7 nhóm, hướng dẫn bàn giao và kịch bản bảo vệ; [bộ Word/PDF kỹ thuật chờ duyệt](ban-ra-soat-dot-5/README.md) có **2 + 1 + 2 = 5 trang cuối**, đã render và xem từng trang. LibreOffice vẫn thiếu; probe có log lỗi và lỗi dọn thư mục tạm của môi trường giới hạn. Fallback Word cài sẵn xuất PDF qua object model; mô-đun render_docx.py đóng gói được dùng cho rasterize với Poppler. Lần Word đầu lỗi cách truyền tham chiếu khi đóng COM, đã sửa và render lại thành công; giữ log lỗi riêng. Không nhận bản DOCX chưa render là hoàn chỉnh, không gọi bộ chờ duyệt là bản nộp cuối đã chốt.

Người dùng dừng Computer Use bằng Escape khi chuẩn bị xác minh Excel/Chrome mới; đã dừng toàn bộ thao tác UI. Không có ảnh xác nhận phần mềm đọc mới, không suy đạt từ lệnh mở file. Nỗ lực đọc lại DB bảo vệ bằng truy vấn chéo catalog bị timeout; [log](minh-chung-dot-5/protected-databases-readonly.txt). Không ghi vào các DB này, không nhận lần đọc lại timeout là đã xác minh trạng thái mới.

Đã dừng đúng các tiến trình thử Web 30416, Desktop test host 31296 và proxy 31340 sau khi kiểm tra dấu nhận diện dòng lệnh; không dừng ứng dụng khác. [Nhật ký dừng](minh-chung-dot-5/processes-stopped.txt). Hash hai cấu hình trước/sau khớp: Desktop `5A27D65B...F6AA6914`, Web `65CA48BC...244D99D`; [hash cuối](minh-chung-dot-5/config-hashes-after.txt). `git diff --check` kết thúc 0, chỉ có cảnh báo chuẩn hóa LF/CRLF ở file sẵn có; không reset/commit/push. Các thay đổi có sẵn vẫn được giữ nguyên.

[Gói bàn giao kỹ thuật](ban-ra-soat-dot-5.zip) gồm ba Word, ba PDF, năm ảnh trang tài liệu, nguồn và cặp file thống kê 300; bản này chờ nhóm duyệt. [Danh mục SHA256](minh-chung-dot-5/artifacts.sha256) đi kèm bằng chứng, không phải chữ ký xác nhận của thành viên.

## Tái lập phần độc lập

```text
dotnet build BadmintonShop.sln --no-restore -v minimal -m:1 /p:UseSharedCompilation=false
dotnet run --project tests/BadmintonShop.SV4.Checks --no-build -- --acceptance-ui-setup --server .\SQLEXPRESS
node tests/BadmintonShop.SV4.Checks/order-main-sub.checks.cjs
python tests/BadmintonShop.SV4.Checks/verify_ui_snapshot_300.py docs/bao-cao/minh-chung-dot-5
```

Python cho đối chiếu file/tài liệu là runtime đóng gói đã có openpyxl/pdfplumber, không mặc định máy nào cũng có đủ package. Bộ oracle 300 chỉ dành cho đúng fixture/kỳ và hai đơn UI đợt này; không tái dùng làm đáp án cho DB đã ghi thêm. Proxy thử chỉ chạy loopback, không triển khai lên môi trường sản phẩm.

## Giới hạn giữ nguyên

Doanh thu chỉ Hoàn thành theo NgayLap; chi là tiền nhập hàng trong kỳ; thu trừ chi là Chênh lệch thu–chi. TTL 10 phút cấu hình được, phạm vi main–sub và nhập chỉ thêm mới vẫn chờ chốt. Test host không thay vòng đăng nhập sản phẩm hoặc nghiệm thu của thành viên.
