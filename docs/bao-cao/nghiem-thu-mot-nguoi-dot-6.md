# Kịch bản nghiệm thu cho một người

Bản chuẩn bị ngày 06/11/2026; chưa phải kết quả đã thực hiện. Dùng cùng [checklist](checklist-tien-do-4-thanh-vien.md) và [biên bản đợt 6](quy-tac-va-tien-do-dot-6.md). Một người có thể lần lượt trình bày bốn phần; ghi đúng người thực hiện, không ký thay TV1–TV4.

## Thứ tự thực hiện

1. **Codex kiểm tra môi trường:** đọc kết nối SQL thành công, đối chiếu DB hiện có chỉ đọc; giữ nguyên cấu hình. Nếu vẫn timeout, ghi lỗi cụ thể, không tự reset SQL/DB. Chỉ sau khi kết nối ổn mới chạy setup DB thử mới.
2. **Codex chuẩn bị DB mới:** dùng `--acceptance-ui-setup --server .\SQLEXPRESS`, ghi tên DB mới từ log. Chế độ này tạo mẫu và khách thử rồi dừng trước khi đặt đơn. Không dùng `--acceptance` nếu mục đích là để người dùng tự chạy đơn đầu tiên, vì chế độ đó tự đặt/hoàn thành một đơn. Không tái dùng DB đợt 5 cho lượt ghi mới.
3. **Codex mở hai ứng dụng sản phẩm:** đặt `ConnectionStrings__BadmintonShop` riêng từng tiến trình, cùng DB thử mới; không sửa App.config/appsettings.json. Desktop chạy entry point sản phẩm, không `--acceptance-desktop-review`.
4. **Người dùng tự đăng nhập:** Desktop `sv4test` / `TestOnly@123`; Web `acceptance@example.invalid` / `TestOnly@123`. Chỉ dùng trên DB thử đã xác minh. Skill không cho Codex tự động thao tác đăng nhập. Dừng ở màn đăng nhập nếu môi trường chưa sẵn sàng; không báo đăng nhập đạt từ việc mở cửa sổ.
5. **Tự chạy luồng mới:** mở báo cáo nền B, chọn kỳ chứa ngày thực chạy (không giữ ngày kết thúc 05/11 cho đơn đặt ngày 06/11); Web đặt một sản phẩm giá P. Ghi MaHD và tồn trước/sau; khi Chờ xác nhận doanh thu vẫn B. Desktop chọn đúng mã, Xác nhận → Giao hàng → Hoàn thành, đọc từng MessageBox; xem Web từng trạng thái. Báo cáo cùng điều kiện sau hoàn thành phải B+P; xem lại/Làm mới vẫn B+P. Codex hỗ trợ UI ngoài đăng nhập trong phạm vi skill và lưu SQL chỉ đọc/ảnh nếu công cụ cho phép.
6. **Xuất và đọc:** xuất XLSX/PDF mới cùng snapshot; tên file riêng, ghi bộ điều kiện. Mở Excel/Chrome, đối chiếu tổng và bảng sản phẩm × quý/thu–chi. File 300 cũ không đại diện snapshot mới. Chỉ nhận phần đã quan sát.
7. **Người dùng tắt JavaScript và kiểm tra:** tự đổi thiết lập cho trang thử trong Chrome, tải lại Đơn hàng của tôi, bấm Xem phải tới đúng chi tiết. Ghi thiết lập đang tắt, URL/mã đơn và ảnh kết quả; khôi phục thiết lập ban đầu. Không dùng CSP thay kết quả này. Nếu công cụ không xác định được URL, người dùng tự thao tác/lưu ảnh, Codex không vượt chặn.
8. **Người dùng kiểm tra DPI:** trước hết chọn phạm vi với giảng viên/nhóm. Ma trận đề xuất để rà soát: 100%, 125%, 150% tại độ phân giải máy hỗ trợ; ghi giá trị thực, kích thước cửa sổ, đăng nhập/báo cáo/đơn hàng/nhập Excel, khả năng tới mọi nút và cột bằng cuộn. Đây là đề xuất, chưa phải phạm vi đã chốt hoặc kết quả đạt. Khôi phục thiết lập ban đầu sau thử. Không quy viewport CSS thành DPI.

## Nội dung cần trình bày khi bảo vệ

| Phần | Thực hành của người trình bày | Minh chứng cần có |
|---|---|---|
| Kết nối và dữ liệu | Chỉ Program.cs → DbHelper → lệnh SQL; giải thích ngày lập, trạng thái hoàn thành và phân bổ giảm giá | Vị trí mã, SQL đối chiếu đúng kỳ |
| Thống kê | Sản phẩm × quý; nhân viên/khách × năm; thu–chi; quý rỗng và nhóm chưa gán | Màn hình và XLSX/PDF cùng bộ lọc |
| Bộ nhớ và main–sub | Giải thích 10 từ khóa dùng lại dữ liệu, Làm mới/TTL; chọn dòng đúng chi tiết; AND/OR/NOT | Log đếm SQL lịch sử ghi rõ phạm vi, demo mới nếu có |
| Nhập/xuất | File sai khóa xác nhận; file đúng preview rồi xác nhận, sản phẩm mới tồn 0 | File/ảnh và SQL trên DB thử mới |
| Tích hợp | Web đặt → Desktop xử lý → thống kê → xuất file | MaHD, B/P/B+P, tồn trước/sau, tên DB, người và ngày thực chạy |

## Việc chỉ con người có thể xác nhận

- Người dùng tự thao tác đăng nhập, thay thiết lập JavaScript và tỷ lệ hiển thị; Codex chuẩn bị và hướng dẫn.
- Giảng viên chấp thuận quy tắc nghiệp vụ/phạm vi cuối: doanh thu Hoàn thành theo NgayLap, chi nhập hàng và tên Chênh lệch thu–chi, TTL 10 phút, main–sub/tìm kiếm, nhập chỉ thêm mới, phạm vi DPI. Nghĩa chi nhập hàng đã có lựa chọn của người dùng đợt 1; chưa đồng nghĩa giảng viên nghiệm thu.
- Người thực hiện tự ghi kết quả chạy, ngày và minh chứng. Tài liệu không ghi ba thành viên khác đã nhận bàn giao/tập bảo vệ nếu chưa thực hiện.
- Người dùng trao đổi trực tiếp với giảng viên về việc làm một mình, phân công/đóng góp và hình thức bảo vệ. Codex có thể giúp soạn nội dung, không tự gửi hoặc đại diện nhóm xác nhận.

## Phiếu ghi kết quả sau khi chạy

| Mục | Giá trị thực tế |
|---|---|
| Người thực hiện / ngày | 06/11/2026: người dùng tự đăng nhập hai sản phẩm, Codex chạy UI sau đăng nhập/đối chiếu; chưa xác nhận người dùng tự tập trọn |
| Nhánh / commit | chore/tiep-tuc-nghiem-thu-dot-6 / 86d44d8; có tài liệu/minh chứng chưa commit |
| Server / DB thử mới | .\SQLEXPRESS / BadmintonShop_SV4_Test_20261106021438_b10b8080 |
| Đăng nhập Desktop sản phẩm | Người dùng tự đăng nhập Desktop/Web; quan sát SV4 TEST / Quản trị viên và KHÁCH NGHIỆM THU, không review host |
| Kỳ / loại bán / B / P / B+P | 01/01–06/11/2026, Tất cả, B=100/P=100/B+P=200; xem lại và Làm mới vẫn 200 |
| MaHD / trạng thái / tồn trước–sau | #11 ngày 06/11/2026 02:22; đủ Chờ/Đã xác nhận/Đang giao/Hoàn thành trên Web, đủ nút/MessageBox Desktop; tồn 95 → 94 |
| XLSX/PDF / tổng đối chiếu | ui-snapshot-200.xlsx/pdf; 11 bảng/11 trang khớp oracle, xem đủ render/Chrome 11 trang; Excel đủ 11 sheet, không nhận mọi ô trực quan ở cửa sổ nhỏ; [biên bản](minh-chung-dot-6/nghiem-thu-san-pham-200.md) |
| JavaScript tắt / kết quả fallback | Chưa kiểm tra |
| Độ phân giải / DPI / cửa sổ / lỗi bố cục | Chưa kiểm tra |
| Quy tắc/phạm vi được giảng viên chấp thuận | Chưa xác nhận |

Sau khi hoàn tất mới điền kết quả và xem xét đóng các mục tương ứng. Bộ Word/PDF nộp cuối chỉ xuất lại sau khi nội dung được chốt và phải render, xem đủ từng trang.

## Bài trình bày một người — bản tập 15–20 phút

Đây là lời dẫn đề xuất, chưa phải biên bản đã tập. Người trình bày tự thay tên, ngày và số liệu bằng kết quả thực chạy; không điền B/P/B+P khi chưa có lượt mới.

**Mở đầu, 1 phút:** “Em trình bày phần kỹ thuật hiện có của hệ thống Web/Desktop dùng chung Core và SQL Server. Phân công TV1–TV4 trong hồ sơ là kế hoạch ban đầu. Hiện em đang tiếp tục một mình; phần đóng góp và hình thức bảo vệ em xin giảng viên xác nhận. Kết quả kiểm thử có ghi người thực hiện và ngày; em không ký thay các thành viên.”

**Kết nối và dữ liệu, 2 phút:** mở `BadmintonShop.Desktop/Program.cs`, dòng 24–26: biến môi trường tiến trình ưu tiên trước App.config; `BadmintonShop.Web/Program.cs:16`: GetConnectionString. Mở `BadmintonShop.Core/DataAccess/DbHelper.cs:33` tạo kết nối; dòng 43 Fill thực thi SELECT, dòng 54 ExecuteNonQuery thực thi ghi. Mở `BaoCaoRepository.cs:20–45` để chỉ truy vấn bán/nhập riêng trong một transaction, tránh nhân tiền nhập theo dòng hàng bán. Khi demo dùng đường dẫn mã hiện tại; kiểm tra lại số dòng nếu sửa code.

**Bảng báo cáo, 3 phút:** “Doanh thu tính mỗi đơn Hoàn thành một lần theo NgayLap. Giảm giá cấp đơn phân bổ về sản phẩm, tổng dòng bằng tổng đơn. Bảng sản phẩm × quý có Q1–Q4, Tổng và kỳ rỗng bằng 0; nhân viên/khách theo năm giữ nhóm chưa gán. Chi là tiền nhập trong kỳ; thu trừ chi được gọi Chênh lệch thu–chi.” Mở cùng bộ lọc cho các bảng. Giải thích 711 là tổng hai năm lịch sử của DB đợt 5, còn 300 là kỳ 01/01–05/11/2026, không hoán đổi hai số.

**Bộ nhớ, tìm kiếm, main–sub, 3 phút:** chỉ bộ log tương ứng từng tuyên bố. Trình bày 10 từ khóa dùng dữ liệu đã nạp, Làm mới/hết hạn/ghi dữ liệu làm tải lại; TTL kiểm tra bằng đồng hồ tiêm. Tìm tổng >=1 triệu AND <=2 triệu gồm cả hai biên, địa chỉ TP.HCM OR Hà Nội, NOT Đã hủy và các phép so sánh còn lại. Chọn ô trên dòng rồi đối chiếu mã chi tiết. Nêu rõ JS cục bộ, mạng proxy trình duyệt, CSP, JavaScript tắt thật và DPI là các phép kiểm tra khác nhau. Chưa có minh chứng nào thì nói chưa kiểm tra.

**Nhập/xuất, 2 phút:** file sai phải báo dòng/cột và khóa xác nhận; file đúng cần preview và xác nhận, sản phẩm thêm mới tồn 0. Chính sách chỉ thêm mới còn chờ chốt. XLSX/PDF phải từ cùng snapshot và cùng bộ lọc, số trên màn hình khớp file. Ảnh Excel đã chuyển 11 sheet không chứng minh mọi ô ở vùng chưa hiện đều được quan sát.

**Luồng mới, 4–6 phút khi SQL đã hoạt động:** dùng đúng DB mới, tự đăng nhập hai sản phẩm, lấy B; Web đặt giá P; Desktop từng trạng thái/MessageBox và Web theo dõi; chỉ Hoàn thành mới có B+P. Ghi mã đơn, tồn, kỳ chứa ngày chạy và xuất cặp file mới. Không nói đã thực hiện phần này khi mới mở ứng dụng hoặc chạy test host.

**Kết thúc, 1 phút:** nêu phần thực chạy đạt, lỗi còn mở và các quyết định cần giảng viên: quy tắc tính/phân kỳ, tên thu–chi, TTL, phạm vi main–sub/tìm kiếm, nhập chỉ thêm mới, DPI, hình thức bảo vệ và đóng góp. Chưa tự đóng CHUNG-02/04/05.

## Khi SQL chưa phục hồi

Trình bày mã nguồn và minh chứng lịch sử có ghi ngày/tên DB; gọi rõ đây là minh chứng lịch sử, không phải demo trực tiếp. Lượt kiểm tra mới không DB ngày 06/11 đạt 23/23, JS cục bộ 8/8 ghi riêng; không thay cho 96 hồi quy SQL. Nhật ký SQL mới ghi lỗi bộ nhớ/tác vụ nền; không kết luận DB hỏng hay phần mềm đúng/sai chỉ từ lỗi kết nối. Chuyển sang phần giải thích dữ liệu và câu hỏi; xin lịch chạy lại phần trực tiếp sau khi môi trường hoạt động, không nhận toàn bộ nghiệm thu đạt.
