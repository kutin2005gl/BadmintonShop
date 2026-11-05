# Triển khai và minh chứng — đợt 2, 05/11/2026

Tiếp nối [đợt 1](quy-tac-va-tien-do-dot-1.md), theo [checklist](checklist-tien-do-4-thanh-vien.md). Không commit/push; giữ nguyên hai file cấu hình kết nối và các thay đổi người dùng đã có. Không chạy phần DROP/CREATE của script cài đặt lên DB có sẵn. Mọi kiểm tra ghi SQL đều tạo DB có tên mới, không xóa các DB này sau kiểm tra.

## Quy tắc và giả định

- Giữ doanh thu của hóa đơn **Hoàn thành**, phân kỳ theo **NgayLap**, khoảng ngày bao gồm cả ngày cuối. Chi là **tiền nhập hàng trong kỳ**. Thu trừ chi ghi **Chênh lệch thu–chi**, không phải lợi nhuận kế toán. Phân bổ giảm giá giữ quy tắc đợt 1.
- Phạm vi bộ nhớ tạm triển khai: Desktop sản phẩm, danh sách/chi tiết hóa đơn, chi tiết phiếu nhập và báo cáo; Web danh sách sản phẩm/danh mục công khai. Không giữ hồ sơ hoặc đơn hàng cá nhân trong bộ nhớ dùng chung Web.
- TTL tạm **10 phút**, cấu hình bằng biến môi trường `BadmintonShop__CacheMinutes` (số nguyên 1–60; thiếu/sai dùng 10). Đây vẫn là giả định, chưa phải xác nhận của giảng viên.
- Main–sub tạm áp dụng Desktop hóa đơn và phiếu nhập. Web giữ luồng mở trang chi tiết có kiểm tra chủ sở hữu; chưa nhận đạt yêu cầu main–sub Web khi phạm vi chưa chốt.
- Nhập Excel tạm áp dụng **sản phẩm mới**, Admin/Quản lý theo nhóm quyền sản phẩm hiện có. MaSP để trống cho DB tự sinh, không nhập tồn kho, không cập nhật/ghi đè mã cũ. Trùng tên + danh mục + thương hiệu bị từ chối. Toàn bộ file thành công hoặc rollback; không ghi riêng các dòng hợp lệ khi file còn lỗi. Chính sách này đã triển khai và kiểm tra nhưng vẫn chờ nhóm chấp thuận phạm vi cuối.

## 1. Dữ liệu và DB demo riêng

`--report-demo-only` tạo một DB mới, lấy **phần định nghĩa bảng sau tiền tố xóa/tạo DB** từ schema, rồi nạp trigger/migration trong DB mới. Không chạy tiền tố phá hủy DB gốc. Bộ mẫu được dựng từ đáp án ghi tay của đợt 1; trigger thật tính tổng nhập/bán và tồn kho.

**DB nguyên mẫu giữ để đối chiếu:** `BadmintonShop_SV4_Test_20261105154154_a5e27e92`, SQL Server `.\SQLEXPRESS`. Chế độ này không chạy tiếp các kiểm tra sửa sản phẩm, nhập Excel hoặc tạo hóa đơn biên triệu. Xem [log 25/25](minh-chung-dot-2/pristine-demo-results.txt).

DB thử có tài khoản Desktop `sv4test`, mật khẩu thử `TestOnly@123`, quyền Admin. Đây là tài khoản được tạo riêng cho DB thử, không phải tài khoản DB đang dùng. Để mở demo, đặt `ConnectionStrings__BadmintonShop` cho riêng tiến trình tới server/catalog trên với Integrated Security rồi chạy Desktop; không sửa hai file cấu hình có sẵn.

| Chỉ tiêu | 2025 | 2026 | Hai năm |
|---|---:|---:|---:|
| Thu hoàn thành | 411 | 100 | 511 |
| Chi nhập | 500 | 150 | 650 |
| Chênh lệch thu–chi | -89 | -50 | -139 |
| SP1 | 231 | 70 | 301 |
| SP2 | 180 | 30 | 210 |

SP1/2025 theo quý: 111, 0, 120, 0; SP2/2025: 50, 90, 0, 40. Giữ hóa đơn nhiều dòng giảm cấp đơn 101/200, đơn hủy/chờ không tính thu, khách vãng lai, nhân viên chưa gán, kỳ rỗng, giao dịch sát biên quý/cuối năm và đầu 2027 ngoài phạm vi. Nhân viên 1 tổng 231; nhóm chưa gán 120; khách vãng lai 90.

`--report-integration` dùng DB thử khác để chạy các phép sửa/nhập/lọc. Vì các phép này thay đổi mẫu, không dùng trạng thái cuối của DB đó để thay cho bộ 511/650 nguyên mẫu. Lần tập trung cuối dùng `BadmintonShop_SV4_Test_20261105155047_909f4603`. Đợt chạy gộp dùng hai DB khác, tên ghi trong log.

DB-R01/R02 xác minh SQL JOIN, trigger, tổng không lặp theo chi tiết, nhóm rỗng, lọc Online, kỳ chỉ có chi và biên ngày. Dữ liệu bán và nhập vẫn đọc riêng trong cùng giao dịch Serializable.

## 2. Bộ nhớ, làm mới và main–sub

`BoNhoPhien` lưu tối đa 8 bộ điều kiện mỗi chủ sở hữu, có thời điểm tải, TTL, làm mới và đồng hồ tiêm vào để kiểm tra đúng biên hết hạn. Lỗi tải không thay snapshot thành công. Khóa báo cáo gồm ngày/loại bán; phạm vi gồm kết nối, tài khoản, vai trò và tín hiệu thay đổi. Các cache Desktop thuộc từng form, đóng form giải phóng phạm vi đó.

Mọi ghi thành công qua helper hiện có tăng phiên bản sau commit; rollback không tăng. Giao dịch đọc báo cáo khai báo rõ không ghi. Các batch scalar ghi hiện có cũng làm mất hiệu lực; SELECT thuần không tăng phiên bản. Giá/tồn khi ghi giao dịch vẫn dùng kiểm tra hiện hành và khóa SQL, không lấy snapshot danh sách làm quyết định bán.

`TinHieuDuLieu` ghi token ngẫu nhiên bằng thay file nguyên tử, tên file băm từ server/catalog, không ghi mật khẩu hay dữ liệu khách. Các tiến trình cùng Windows user dùng thư mục mặc định `%LOCALAPPDATA%/BadmintonShop/CacheSignals`. Nếu khác user/máy, đặt `BadmintonShop__CacheSignalRoot` tới thư mục chung mà cả hai được đọc/ghi và dùng cùng cách ghi tên SQL Server/catalog. Không thay cấu hình thật trong đợt này. Mỗi lần lọc chỉ đọc token file, không hỏi SQL để kiểm tra thay đổi.

Nếu thư mục tín hiệu không ghi được, giao dịch đã commit vẫn được báo thành công; đồng bộ tức thời giữa tiến trình không được bảo đảm, cần Làm mới/TTL. Nếu không đọc được token do lỗi quyền/I/O, bỏ qua bộ nhớ. Ghi SQL ngoài ứng dụng không phát tín hiệu: dùng Làm mới hoặc chờ TTL. Chưa kiểm tra tín hiệu trên share mạng hoặc máy khác.

| Kịch bản thực chạy | SQL ban đầu | SQL thêm |
|---|---:|---:|
| Sản phẩm Desktop: 10 từ khóa + chọn dòng/xem ảnh | 1 | 0 |
| Sản phẩm Desktop: Tải lại | — | 1 |
| Sản phẩm Desktop: lọc sau sửa thành công | — | 1 |
| Báo cáo cùng bộ lọc / Làm mới / sau ghi | 2 | 0 / 2 / 2 |
| Web catalog: 10 HTTP request lọc | 2 | 0 |
| Web catalog: Làm mới / sau ghi | — | 2 / 2 |
| Lọc nâng cao AND/OR/NOT sau nạp danh sách và chi tiết | đã nạp | 0 |
| TTL: trước hạn 9 phút / đúng 10 phút | 1 | 0 / 1 |
| Tiến trình con commit: Desktop / catalog Web / báo cáo | đã nạp | 1 / 2 / 2 |

Đếm bằng `DiagnosticListener` của SqlClient, gồm adapter và lệnh tạo trực tiếp trong transaction, không chỉ `DbHelper.ExecuteQuery`. Log không in tham số/kết nối. Kiểm tra TTL dịch đồng hồ, **không giả là đã đợi 10 phút thời gian thật**. Tiến trình con sửa sản phẩm và tăng giảm giá một hóa đơn; phiên bản RAM của tiến trình cha không đổi nhưng token đổi, báo cáo tải lại và thu giảm đúng 1 đồng. Tín hiệu sau commit là thật, không gọi tay hàm vô hiệu hóa trong tiến trình cha để thay kiểm chứng.

Ảnh sản phẩm lấy khóa ảnh từ chính bảng danh sách đã tải; bỏ SQL `LayTheoMa` trong sự kiện xem ảnh. Vẫn đọc ảnh từ kho file, không giữ bitmap vô hạn. Phiếu nhập đổi sang `CurrentCellChanged`, bỏ phụ thuộc trạng thái Selected và xóa chi tiết khi danh sách rỗng. Hóa đơn giữ kiểm tra phiên bản phản hồi bất đồng bộ; cache chi tiết theo mã và tín hiệu. DB-M01 chọn mọi cột hiển thị của 3 dòng đầu ở cả hai màn, so chi tiết với repository độc lập.

## 3. Tìm kiếm nâng cao

Mở **Đơn hàng → Tìm nâng cao**. Hai điều kiện có trường, toán tử, giá trị và NOT riêng; chọn AND/OR, có thể tắt điều kiện 2. Mô tả có ngoặc xuất hiện trên màn đơn hàng. Danh sách hóa đơn tải theo khoảng ngày rồi lọc trong bộ nhớ; đổi trạng thái/loại bán/từ khóa hoặc điều kiện nâng cao không tải lại danh sách còn hiệu lực.

- Đủ `>`, `<`, `>=`, `<=`, `=`, `!=`. Văn bản chỉ cho `=`/`!=`; số nguyên theo văn hóa vi-VN; ngày đúng `dd/MM/yyyy`, so theo ngày. Có danh sách trường cho phép, không ghép SQL/DataView từ biểu thức người dùng.
- AND tổng từ 1.000.000 tới 2.000.000 trả cả hai biên, bỏ 999.999 và 2.000.001. OR địa chỉ bằng TP.HCM hoặc Hà Nội trả đúng hai khách trong mẫu. Đây là **so bằng toàn bộ địa chỉ**, không tự tách thành phố từ địa chỉ tự do.
- NOT trạng thái = Đã hủy loại đúng nhóm hủy. Thiếu giá trị không khớp phép so; NOT bao gồm nhóm thiếu giá trị. Từ chối trường/toán tử sai, số/ngày sai, giá trị rỗng, so thứ tự trên văn bản và khoảng AND đảo cận.

A01 kiểm tra cục bộ từng toán tử và đầu vào sai; DB-A01 mở hộp thoại thật từ màn đơn hàng, nhập các ví dụ, bấm Áp dụng, đối chiếu tập mã hóa đơn từ SQL. Sau khi nạp chi tiết, các lần lọc/chọn không thêm SQL. [Ảnh kết quả NOT](minh-chung-dot-2/advanced-search.png).

## 4. Nhập Excel

Mở **Sản phẩm → Nhập Excel**. Có tải mẫu gồm trang SanPham và hướng dẫn/danh mục; chọn XLSX, xem trước, xác nhận rồi ghi. Chặn quyền ở cả mở màn và phương thức đọc/lưu; không chỉ ẩn nút. File tối đa 5 MB, dữ liệu giải nén tối đa 30 MB, 1000 dòng, đúng 7 cột; không nhận công thức hoặc thêm cột tồn kho.

Kiểm tra MaSP phải trống, tên/đơn vị/độ dài, mã danh mục, thương hiệu, giá nguyên đồng không âm, trạng thái, trùng trong file/danh sách. Lỗi có dòng và cột. Nút xác nhận bị khóa nếu file sai; chọn file mới xóa preview cũ. Khi commit, kiểm tra lại danh mục và sản phẩm trùng dưới khóa Serializable; lỗi dòng sau rollback cả các dòng trước. Sản phẩm mới có tồn 0. Nhập xong đóng cửa sổ sẽ tải lại danh sách, đồng thời phiên bản/tín hiệu ghi làm mất hiệu lực cache khác.

DB-X01/X02 chạy thật: thêm 2 sản phẩm, tồn 0; file sai và trùng không ghi; preview cũ bị từ chối nếu sản phẩm đã được nhập; danh mục sai ở dòng thứ hai rollback dòng thứ nhất; nhân viên không được đọc/lưu. DB-X03 mở form xem trước thật, đưa đường dẫn vào hàm đọc, kiểm tra trạng thái nút và bảng. **Chưa tự động thao tác hộp chọn/lưu file Windows hoặc bấm xác nhận ghi qua MessageBox**; phần commit được kiểm tra qua cùng phương thức mà nút gọi. Không đánh đồng với nghiệm thu thủ công toàn luồng.

Minh chứng: [mẫu](minh-chung-dot-2/import-template.xlsx), [file đúng](minh-chung-dot-2/import-valid.xlsx), [file sai](minh-chung-dot-2/import-invalid.xlsx), [lỗi dòng/cột](minh-chung-dot-2/import-errors.txt), [ảnh preview](minh-chung-dot-2/import-preview.png).

## 5. Xuất XLSX/PDF và kiểm tra trực quan

PDF các bảng nhân viên/khách hàng có nhiều năm được chia nhóm tối đa 3 năm khi bảng vượt 7 cột; lặp mã/tên và cột **Tổng toàn kỳ**, có chú thích không cộng tổng lặp giữa các phần. Bảng trang ngang, số canh phải, hàng Tất cả tô nền/đậm, tiêu đề cột lặp khi sang trang. XLSX vẫn giữ đầy đủ năm trong cùng sheet.

DB-C02 so từng ô của lưới với snapshot, xác nhận biểu đồ dùng cùng DataTable; xuất `ui-report` từ đúng snapshot này và kiểm tra không truy vấn SQL khi xuất. `verify_progress_exports.py` dùng **openpyxl và pdfplumber** đọc độc lập, so từng ô XLSX và ô PDF với snapshot, kiểm tra phủ đủ cột/năm và tọa độ chữ không tràn ngang:

| Bộ minh chứng | Bảng XLSX | Trang PDF | Kết quả |
|---|---:|---:|---|
| demo-report — mẫu SQL 2025–2026 | 13 | 13 | Mọi ô khớp |
| wide-pivots — nhân viên/khách hàng 2005–2026, tên dài, tiền 12–13 chữ số | 2 | 16 | Mọi ô khớp, đủ 22 năm và tổng toàn kỳ |
| ui-report — snapshot thật từ màn thống kê | 11 | 11 | Lưới/biểu đồ/snapshot/XLSX/PDF khớp |

[Log đọc độc lập](minh-chung-dot-2/export-results.txt). PDF được render bằng Poppler và xem ảnh để kiểm tra chữ Việt, lề, tổng, cột cuối, tên xuống dòng, chân trang: [13 trang demo](minh-chung-dot-2/demo-contact.png), [16 trang nhiều năm](minh-chung-dot-2/wide-contact.png), [bảng sản phẩm × quý](minh-chung-dot-2/demo-page-10.png), [trang nhiều năm lớn](minh-chung-dot-2/wide-page-01.png). Đây là kiểm tra trực quan qua renderer, **chưa mở file bằng Excel/Acrobat trên máy để nghiệm thu thao tác phần mềm đọc**. Bộ mẫu này chưa kiểm tra mọi độ dài tên hoặc mức tiền tối đa 18 chữ số của schema.

Đã xem thêm [11 trang từ snapshot UI](minh-chung-dot-2/ui-contact.png); mã kiểm tra toàn vẹn các file bàn giao nằm trong [artifacts.sha256](minh-chung-dot-2/artifacts.sha256).

## 6. Lệnh tái lập và kết quả

Chạy từ thư mục BadmintonShop; tên SQL Server là tham số dòng lệnh, không sửa App.config/appsettings.json:

```powershell
dotnet build BadmintonShop.sln --no-restore -v minimal -m:1 /p:UseSharedCompilation=false
dotnet run --project tests/BadmintonShop.SV4.Checks --no-build -- --report-integration --integration --server .\SQLEXPRESS
dotnet run --project tests/BadmintonShop.SV4.Checks --no-build -- --report-demo-only --server .\SQLEXPRESS
python tests/BadmintonShop.SV4.Checks/verify_progress_exports.py docs/bao-cao/minh-chung-dot-2
```

| Phạm vi | Kết quả / minh chứng |
|---|---|
| Build cuối | Thành công, 0 lỗi; [log](minh-chung-dot-2/build.txt). Cảnh báo CS8602 cũ ở FrmKhuyenMai:278 vẫn xuất hiện khi biên dịch lại file, không coi build incremental 0 warning là đã sửa cảnh báo. |
| Chạy gộp kiểm tra mới + toàn bộ tích hợp/hồi quy | **96 đạt, 0 lỗi** — [log riêng](minh-chung-dot-2/combined-results.txt). Gồm giao dịch/race, voucher, Web HTTP, quyền, render, luồng Web → repository Desktop, đối chiếu cuối. |
| Tập trung mới nhất sau bổ sung phép so từng ô lưới khi xuất và lỗi lưu tín hiệu | **35 đạt, 0 lỗi** — [log](minh-chung-dot-2/results.txt). Sau lần 96 đã mở rộng bộ kiểm tra/nhánh tạo demo và bổ sung bắt lỗi tham số khi dọn file tín hiệu; tập trung chạy lại trên mã cuối. Không cộng 35 và 96 thành số kiểm tra độc lập. |
| Giữ DB demo nguyên mẫu | **25 đạt, 0 lỗi** — [log](minh-chung-dot-2/pristine-demo-results.txt). Có phần kiểm tra cục bộ trùng các lần trên. |
| Đọc độc lập file mới nhất | 3 bộ, 26 bảng XLSX / 40 trang PDF — [log](minh-chung-dot-2/export-results.txt). |

Các log trung gian 22/25/27/30/84 được giữ riêng để phân biệt giai đoạn, không thay log cuối. Các script đọc độc lập cũ `verify_exports.py`/`verify_integration_exports.py` có giả định 6 bảng của phiên bản trước; không dùng chúng để nhận đạt các bảng mới. Dùng script progress nêu trên cho đợt này.

## 7. Nơi lấy kết nối và chạy SQL để demo

- Desktop: `BadmintonShop.Desktop/Program.cs`, ưu tiên biến `ConnectionStrings__BadmintonShop`, sau đó App.config. Web: `BadmintonShop.Web/Program.cs`, đọc `GetConnectionString("BadmintonShop")` qua configuration.
- `DbHelper.TaoKetNoi` tạo SqlConnection; ExecuteQuery dùng adapter Fill; ExecuteNonQuery/ExecuteScalar gọi thực thi; ExecuteTransaction commit/rollback trên cùng kết nối. `BaoCaoRepository.DocDuLieu` tạo hai SqlCommand có tham số, adapter Fill trong transaction, phần thu và chi riêng.
- Khi trình diễn, chỉ hiển thị server/catalog hoặc chuỗi Integrated Security của **DB demo**; không chiếu mật khẩu thật. Có thể đặt biến môi trường cho tiến trình demo, không ghi đè hai file cấu hình có sẵn.

## 8. Phần còn mở để nghiệm thu cuối

1. Nhóm/giảng viên chốt phạm vi main–sub Web, thời hạn bộ nhớ và chính sách nhập tạm; không ghi nhận thay người dùng là đã chấp thuận.
2. Thao tác trọn luồng bằng hai ứng dụng chạy riêng: khách đặt Web → Desktop xử lý → báo cáo thay đổi; kiểm tra cả quyền và Làm mới. Đã có HTTP/repository và tín hiệu khác tiến trình, chưa thay được buổi chạy thủ công này.
3. Chọn/lưu file Windows, xác nhận nhập qua UI, mở XLSX/PDF trong phần mềm người dùng. Giao diện 1920×1080 chưa nghiệm thu: Windows giới hạn lần chạy ở 1556×884; log ghi rõ kích thước thực. Chưa khảo sát trực quan tất cả màn hình và mọi mức DPI.
4. Xác nhận bàn giao giữa TV1–TV4, cập nhật tài liệu cuối và xuất lại Word/PDF/hình khi nội dung đã chốt, tập bảo vệ đủ 7 nhóm. Không đánh dấu hoàn tất các bước này bằng kiểm tra kỹ thuật tự động.
