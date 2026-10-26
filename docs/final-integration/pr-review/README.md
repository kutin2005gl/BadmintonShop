# Rà soát trước PR — chạy lại từ ổ D, 26/10/2026

Worktree đang dùng: **D:\CodexWorktrees\BadmintonShop-sv4**. Nhánh **feature/sv4-final-integration**, nền origin/develop **920a576** sau fetch; không có cập nhật nền cần tích hợp thêm. Bản ổ C chỉ là dự phòng; không sửa/build tại đó. Git worktree list và rev-parse nhận đúng ổ D sau sao chép + repair. Không reset/clean/stash/force checkout; giữ nguyên cấu hình cá nhân.

Commit chứa mã đã kiểm: **3e6ec96e92f465a5b10fc9c59508aca2e06588aa** (điền sau khi commit nguồn, trước khi commit tài liệu). [SHA-256 nguồn](source-sha256.tsv) gồm Core/Desktop/Web/test/SQL/Python kiểm file, để nhận diện nội dung trước khi có SHA commit. Bản thân cập nhật tài liệu sau commit nguồn không làm thay đổi mã được kiểm. Người dùng đã cho phép commit, push và mở PR vào develop; **không merge, không auto-merge, không thêm reviewer hoặc nhắn tin cho người khác**.

## Rà code và sửa cuối

Phần tích hợp POS/khuyến mãi trước đây mới nằm trên feature/sv4-desktop; develop mới có giao diện PR #4. Áp dụng lại đúng phần tích hợp vào nền develop, giữ API cũ của Core, endpoint xác thực/chủ đơn của Web và trigger SV1. Các thay đổi Web chỉ phục vụ kiểm chủ đơn/chi tiết và yêu cầu xác nhận lại báo giá khi giá/ưu đãi đổi, không bổ sung tính năng ngoài SV4.

- Core/Web/POS dùng chung GioHangService và TinhKhuyenMaiService. Serializable transaction tính lại giá, tồn, trạng thái bán, khuyến mãi; ghi phần trăm giảm chi tiết và giảm cấp đơn đúng trường. Voucher tăng lượt cùng transaction; trigger duy nhất tính tổng/trừ hoặc hoàn tồn.
- Token POS + SHA-256 payload + applock: retry giao dịch đã lưu trả hóa đơn cũ trước khi tính ưu đãi hiện tại; đổi payload bị từ chối. Ca tranh tồn/voucher cuối, rollback giữa transaction và retry đã chạy lại.
- Voucher sai/chưa mở/hết hạn/hết lượt/tắt có thông báo riêng. POS kiểm đủ cột và unique filtered index trước khi sẵn sàng. Không tin tổng/giảm từ UI.
- **Lỗi phát hiện khi chạy cửa sổ thật:** Label chiPhi cao cố định 62 bị cắt dòng giảm cấp hóa đơn ở môi trường hiển thị hiện tại. Đổi nhãn sang AutoSize, tăng vùng thanh toán và cho vùng thao tác cuộn; giữ đủ cả ba dòng và các nút. P18 bổ sung kiểm chiều cao PreferredSize của nhãn. [Trước sửa](screenshots/native-pos-before-layout-fix.png), [bản cuối mở lại sau sửa](screenshots/native-pos-after-layout-fix.png).
- Khi lưu thành công, lần nạp sản phẩm sau đó không còn ghi đè mã hóa đơn bằng lời nhắc quy tắc. Hiển thị “Hóa đơn gần nhất” để phân biệt với giỏ mới; vẫn chỉ thông báo thành công sau transaction.
- SQL tổng gồm 04/06, demo 05 chỉ nhận CSDL test/demo, có nhãn riêng và chạy lại không thêm trùng. Không đổi/xóa cột/bảng cũ, không chỉnh trigger.

Các file chính: HoaDonRepository, DonHangOnlineRepository, KhuyenMaiRepository, SanPhamRepository; GioHangService, TinhKhuyenMaiService, XacNhanGia, GiaoDichBanHang; FrmPOS/FrmDonHang/XuatTaiLieu và bảo vệ vòng đời Form; hai controller/view Web cần cho xác nhận giá/chủ đơn; migration 06, script tổng/demo; các test và tài liệu SV4. Không thay App.config/appsettings.json trong repo.

Khi rà staged bytes, phát hiện Git text:auto chuẩn hóa xuống dòng trong hai PDF lịch sử. Thêm .gitattributes binary cho PDF/XLSX/ảnh, stage lại và đối chiếu byte của toàn bộ artifact trong index với file gốc đã kiểm. Không sửa nội dung lịch sử hay xuất lại để che sai lệch.

## Build và kiểm tra lại

Môi trường: Windows x64 10.0.26200, SDK 10.0.402, target .NET 8/WinForms; SQL Server 16.0.1000.6, localhost/`.` → DESKTOP-FGE20L2; Python openpyxl/pypdf. Output build riêng ở D:\CodexWorktrees\BadmintonShop-sv4-validation-final, ngoài repo. Không tự tắt tiến trình của người dùng; các helper test do agent tạo đã dừng và hai phiên ứng dụng demo đã đóng.

| Thao tác từ worktree D | Kết quả thực tế | Bằng chứng |
| --- | --- | --- |
| dotnet build BadmintonShop.sln --no-restore -p:OutDir=D:\CodexWorktrees\BadmintonShop-sv4-validation-final\ | **0 lỗi, 0 cảnh báo** | [build.txt](build.txt) |
| dotnet …\BadmintonShop.SV4.Checks.dll --integration --server . | **74 đạt, 0 lỗi**; có 6 LIMIT 1920×1080 chưa kiểm | [results.txt](results.txt) |
| --install-checks --server . (chạy từ ổ D) | **15 đạt, 0 lỗi**, gồm 13 unit + J01/J02 | [install-results.txt](install-results.txt) |
| python tests/BadmintonShop.SV4.Checks/verify_integration_exports.py docs/final-integration/pr-review | **Đạt** cả file tự động 12 bộ lọc và file xuất bằng cửa sổ thật | [independent-exports.txt](independent-exports.txt) |
| --schema-readonly | Đọc được schema CSDL người dùng, chưa tương thích POS; không ghi dữ liệu | [schema-readonly.txt](schema-readonly.txt) |

CSDL hồi quy: **BadmintonShop_SV4_Test_20261026221535_faa9d1f7**, kết quả Temp/BadmintonShop-SV4-Checks/20261026-221531. Doanh thu cuối khoảng hai năm đến hết 26/10/2026, mọi loại bán, chỉ Hoàn thành: **13.172.279đ**. Không sao chép số của lượt trước. [Excel](final-integration-report.xlsx), [PDF](final-integration-report.pdf), [12 tổng SQL theo bộ lọc](filtered-expected.tsv), [hóa đơn giá lịch sử](promotion-web-invoice.pdf). Python đọc độc lập đủ 6 sheet/trang và đối chiếu tổng/giảm lịch sử.

CSDL cài mới/demo: **BadmintonShop_SV4_Test_Install_20261026215522_fe1596b9**. Script tổng được chạy trên DB mới, bỏ đoạn DROP/CREATE và đổi USE sang đúng tên test; 04/06/05 chạy lại được. Không chạy script tạo lại lên CSDL đang giữ.

## Demo bằng ứng dụng WinForms thật

Dùng skill computer-use điều khiển **BadmintonShop.Desktop.exe** được build ở ổ D. Chỉ bản config **sinh ra trong output test ngoài repo** trỏ tới CSDL test trên; config cá nhân trong source không đổi. Ảnh tên native-* là ảnh chụp cửa sổ thật, **khác với DrawToBitmap** của harness. Không thay DPI/cấu hình hiển thị hệ thống.

1. Đăng nhập tài khoản mẫu admin trên CSDL test; thêm `[SV4-DEMO-LIVE] Vot D`, giá 500.000, ảnh fixture màu xanh (không phải ảnh cá nhân). Mã SP **15**, tồn **0**; [hộp thoại](screenshots/native-product-editor.png), [sản phẩm đã tạo](screenshots/native-product-created.png).
2. Phiếu nhập SV1 chọn NCC demo, nhập **5 × 300.000**, lưu phiếu **8** tổng 1.500.000; tồn tăng lên **5**. [Phiếu trước lưu](screenshots/native-import-ready.png), [sau lưu](screenshots/native-import-saved.png).
3. POS thêm sản phẩm mới và một vợt demo đang giảm 10%; áp SV4DEMO15. Tiền hàng **1.000.000**, giảm SP **50.000**, giảm voucher **142.500**, trả **807.500**; đưa **1.000.000**, thừa **192.500**. Xác nhận thanh toán, hóa đơn **42**, tồn SP15 còn **4**, lượt voucher **1**. [Đơn đã lưu](screenshots/native-order-pos42.png), [PDF xuất từ nút POS](native-invoice-42.pdf).
4. Đơn Online demo **37** đi đủ Chờ xác nhận → Đã xác nhận → Đang giao → Hoàn thành bằng các nút và hộp thoại thật. [Kết quả](screenshots/native-online37-completed.png). Đơn này được seed từ script demo; **không nhận là khách đặt qua browser**.
5. Thống kê **01/01/2026–26/10/2026, tất cả loại bán**: tổng **46.602.300đ**; xuất Excel/PDF bằng nút trên UI. [Màn hình](screenshots/native-report.png), [Excel](native-report.xlsx), [PDF](native-report.pdf). [SQL chỉ đọc trên DB demo](native-sql.txt) và Python đối chiếu đúng cùng bộ lọc, không dùng lẫn tổng 13.172.279 của DB hồi quy khác.
6. Đóng phiên demo, mở **bản build sau sửa**; đăng nhập lại, mở POS và thêm vợt giảm SP để kiểm lại cả ba dòng tổng tiền. [Ảnh sau sửa](screenshots/native-pos-after-layout-fix.png). Giỏ thử chưa thanh toán được bỏ và ứng dụng đã đóng. Không tạo thêm hóa đơn cho bước này.

Đã thử các bước của kịch bản demo; tài liệu trình bày 5–7 phút nằm trong HDSD. Không khẳng định đây là lượt thuyết trình được bấm giờ 5–7 phút. HTTP Kestrel/MVC có cookie/anti-forgery/chủ đơn là test fixture; login/catalog SV2 chưa có nên **chưa browser E2E**, chưa gửi email SMTP.

## Schema hiện tại và giới hạn

Kiểm tra mới vẫn cho Desktop/Web trỏ localhost / BadmintonShop, server thực DESKTOP-FGE20L2; thiếu HoaDon.LyDoHuy NVARCHAR(500), MaYeuCauPOS UNIQUEIDENTIFIER, TienKhachDua DECIMAL(18,0), DauVanTayPOS VARCHAR(64) (đều nullable), và unique filtered index UX_HoaDon_MaYeuCauPOS khác NULL. Các trường giảm giá/cột tính đúng kiểu và ba trigger bật. Chỉ SELECT metadata; **không migration/seed/ghi thử lên DB người dùng**.

1366×768 đã render Form thật. Yêu cầu 1920×1080 bị Windows giới hạn 1556×884: [render-sizes.tsv](render-sizes.tsv), chưa xác minh kích thước đích. 125%/150% là mô phỏng Control.Scale; không đổi Windows DPI thật. Native screenshot ở kích thước cửa sổ thực 1536×816 và các hộp thoại, không dùng nó để khẳng định đã kiểm 1920×1080 hoặc cả hai mức DPI. Chưa kiểm Designer, máy in, mọi nhánh hộp thoại hay fault injection mạng đúng sau commit. Retry sau commit đã kiểm tự động.

Lỗi Dispose/CreateHandle gốc vẫn chưa tái hiện trong hồ sơ debugger 22/10; không khẳng định đã sửa dứt điểm. Không có kiểm thử chéo SV2 hay xác nhận chính sách voucher của SV3. Giữ quy tắc hiện tại không hoàn lượt khi hủy. Ngày doanh thu là NgayLap của đơn hiện Hoàn thành, tên/danh mục sản phẩm chưa snapshot, chưa benchmark tải lớn.

## Mở và chạy lại

Mở **D:\CodexWorktrees\BadmintonShop-sv4\BadmintonShop.sln**, Startup Project = BadmintonShop.Desktop. Để dùng demo hiện có, đặt biến môi trường phiên trước khi mở Visual Studio hoặc chạy dotnet:

```powershell
$env:ConnectionStrings__BadmintonShop='Server=.;Database=BadmintonShop_SV4_Test_Install_20261026215522_fe1596b9;Integrated Security=True;TrustServerCertificate=True;'
$env:BadmintonShop__MediaRoot='D:\CodexWorktrees\BadmintonShop-sv4-validation\media'
dotnet run --project BadmintonShop.Desktop
```

Kho ảnh này chứa ảnh fixture của SP15 đã tạo; máy khác cấu hình đường dẫn dùng chung mới theo HDSD. Không commit cấu hình máy. Tài khoản seed mẫu xem README/HDSD. Muốn DB demo mới, chạy --install-checks --server . rồi dùng đúng tên test được in ra. Không chạy 01/SQL tổng lên dữ liệu cần giữ.

[Checklist SV1/SV2/SV3](../../bao-cao/checklist-ban-giao-sv4.md), [checklist SV2 chi tiết](../../bao-cao/checklist-sv2-desktop.md), [HDSD/kịch bản](../../bao-cao/huong-dan-su-dung-sv4.md). Hai script tạo tài liệu trong AppData/Local/Temp chỉ là hỗ trợ một lần, **không phải deliverable**; cả tám tài liệu yêu cầu đã có trong repo. Không stage bin/obj/.vs, build output, DB test hoặc file cấu hình cá nhân; chỉ giữ log/ảnh/file xuất fixture có chủ đích. Việc tạo PR không đồng nghĩa đủ điều kiện merge.
