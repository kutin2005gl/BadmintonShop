# Checklist tiến độ công việc — TV1–TV4

Ngày lập: 05/11/2026. Nguồn phân công và tiêu chí: [Kế hoạch hoàn thiện yêu cầu môn học](ke-hoach-hoan-thien-yeu-cau-mon-hoc.md).

Các mốc ngày 1–10 tính từ ngày nhóm bắt đầu thực hiện, không phải ngày trong tháng. Đợt 1 có kiểm tra cục bộ; [đợt 2](quy-tac-va-tien-do-dot-2.md) bổ sung SQL trên DB demo riêng, kiểm tra UI tự động, đếm truy vấn và đối chiếu file/render. Các mức kiểm chứng được ghi riêng; chưa thay thế nghiệm thu thủ công cuối. Tiêu chí nghiệm thu chi tiết vẫn theo kế hoạch gốc.

## Cách cập nhật

Cập nhật mới nhất: [đợt 5](quy-tac-va-tien-do-dot-5.md). DB thử mới `...170422_f787c19b`: #11 tiếp tục sau gián đoạn quota; #12 mới chạy liên tục trong phiên tiếp tục qua Web → nút/MessageBox Desktop test host → báo cáo 200 → 300 → XLSX/PDF cùng snapshot. Tồn lượt #12 là 94 → 93, tổng hai năm 711. Đã kiểm tra mạng mô phỏng trình duyệt; 11 bảng/11 trang xuất khớp đáp án độc lập và xem từng trang render; ba tài liệu Word/PDF kỹ thuật chờ duyệt đã render 5 trang và xem đủ. Hồi quy mới 96/96, hợp đồng JS cục bộ 8/8. Vẫn chờ đăng nhập Desktop sản phẩm, JavaScript tắt bằng thiết lập trình duyệt, DPI/phạm vi cuối và xác nhận thành viên. Người dùng dừng Computer Use bằng Escape khi chuẩn bị xác minh phần mềm đọc mới; đã dừng UI, không suy nghiệm thu Excel/Chrome mới từ lệnh mở file.

Lịch sử đợt 4: [biên bản](quy-tac-va-tien-do-dot-4.md) tiếp tục #12 trên DB UI cũ `...161022_7f4ab55a`, kỳ 2026 tăng 200 → 300, SQL hai năm 711; đây là DB và đơn khác với đợt 5. Không xác nhận thay nhóm/giảng viên.

- Đánh dấu `[x]` khi đã hoàn thành và có minh chứng kiểm tra; đang làm hoặc đang chờ vẫn để `[ ]`.
- Cuối mỗi buổi, cập nhật bảng tổng quan và ghi mã công việc đang làm, vướng mắc, người cần phối hợp.
- Minh chứng có thể là đường dẫn commit/PR, ảnh, log truy vấn, dữ liệu kỳ vọng, kết quả kiểm tra hoặc file xuất. Ghi cạnh mục tương ứng hoặc trong sổ bàn giao bên dưới.
- Không suy tiến độ thực tế từ mốc dự kiến. Mục còn chờ chốt quy tắc chưa được đánh dấu hoàn tất.

## Tổng quan tiến độ

| Thành viên | Mốc dự kiến | Trạng thái hiện tại | Mã việc đang làm / vướng mắc | Cập nhật gần nhất |
|---|---|---|---|---|
| TV1 | Dữ liệu chung ngày 2; bảng sản phẩm và kiểm tra ngày 5 | Đang làm | Đã đạt SQL trên DB demo riêng; còn xác nhận quy tắc/bàn giao và demo trực tiếp | 05/11/2026 |
| TV2 | Các bảng còn lại và bàn giao xuất file ngày 5 | Đang làm | Bảng, SQL, lưới/biểu đồ và file đã đối chiếu; còn bàn giao và nghiệm thu thủ công | 05/11/2026 |
| TV3 | Bộ nhớ/main–sub ngày 6; tìm kiếm nâng cao ngày 7 | Đang làm | Đã có bộ nhớ/làm mới, tín hiệu khác tiến trình, main–sub Desktop và lọc nâng cao; còn chốt phạm vi Web/TTL | 05/11/2026 |
| TV4 | Nhập/xuất ngày 8; tài liệu/minh chứng ngày 10 | Đang làm | Đã có nhập Excel, thao tác file Windows/MessageBox, mở Excel/Chrome; còn bàn giao, chính sách cuối và tập bảo vệ | 05/11/2026 |

Trạng thái dùng chung: **Chưa bắt đầu / Đang làm / Chờ phối hợp / Hoàn tất**.

## TV1 — Quy tắc, dữ liệu chung và sản phẩm × quý

### Ngày 1–2: chốt quy tắc và bàn giao dữ liệu

- [ ] **TV1-01 · Ngày 1:** Tổng hợp và ghi bảng quy tắc chung: trạng thái hóa đơn, ngày phân kỳ, nghĩa của “chi”, doanh thu sau giảm giá và cách làm tròn; đánh dấu câu hỏi cần giảng viên xác nhận.
- [ ] **TV1-02 · Ngày 1:** Cùng TV3/TV4 chốt phạm vi main–sub, đọc Excel và thời hạn tái sử dụng dữ liệu; ghi rõ phương án tạm và điểm chưa chốt.
- [x] **TV1-03 · Ngày 2:** Chuẩn hóa dữ liệu trong `BaoCaoRepository`/`BaoCaoService`, có nhân viên, khách hàng, sản phẩm và nguồn chi đã thống nhất; tách hóa đơn, dòng bán và khoản chi khi tổng hợp để tránh cộng trùng.
- [x] **TV1-04 · Ngày 2:** Dùng chung cách phân kỳ ngày/tháng/quý/năm và phân bổ giảm giá; tổng các dòng sau phân bổ bằng tiền hóa đơn đến đồng.
- [x] **TV1-05 · Ngày 2:** Chuẩn bị DB demo và bản tính kỳ vọng độc lập: ít nhất 2 năm, đủ Q1–Q4, kỳ rỗng, nhiều dòng/đơn, giảm cấp đơn, đơn hủy/chưa hoàn thành, khách vãng lai và nhân viên chưa gán.
- [ ] **TV1-06 · Ngày 2:** Bàn giao cấu trúc dữ liệu và ví dụ kết quả cho TV2/TV4; thống nhất phần sửa dùng chung và ghi nhận bên nhận đã kiểm tra được.

### Ngày 3–5: bảng sản phẩm × quý

- [x] **TV1-07 · Ngày 5:** Hoàn thành bảng sản phẩm theo năm được chọn, có Q1–Q4, Tổng, hàng tổng; kỳ rỗng bằng 0.
- [x] **TV1-08 · Ngày 5:** Đối chiếu bảng với bản tính kỳ vọng; kiểm tra ngày cuối kỳ, phân bổ giảm giá và hóa đơn nhiều dòng không bị cộng trùng.
- [x] **TV1-09 · Ngày 5:** Cùng TV2 đối chiếu tổng sản phẩm, nhân viên, khách hàng với doanh thu trên cùng tập hóa đơn; bàn giao kết quả cho TV4 xuất file.
- [x] **TV1-10 · Ngày 5–8:** Phối hợp TV3 xác định khi nào dữ liệu báo cáo phải làm mới; bàn giao minh chứng kiểm tra phần mình cho TV4.
- [ ] **TV1-11 · Ngày 9–10:** Chuẩn bị demo nơi lấy chuỗi kết nối và dòng chạy SQL, che mật khẩu thật; tham gia tích hợp và tập bảo vệ.

## TV2 — Các bảng thống kê và giao diện báo cáo

### Ngày 1–2: chuẩn bị và nhận dữ liệu

- [ ] **TV2-01 · Ngày 2:** Đọc bảng quy tắc, nhận dữ liệu chung từ TV1, thống nhất phần sửa `BaoCaoRepository`/`BaoCaoService` trước khi triển khai.
- [x] **TV2-02 · Ngày 2:** Xác nhận cách tính “chi” và tên chỉ tiêu với TV1; ghi rõ điểm đang chờ, chưa nghiệm thu thu–chi nếu nghĩa của “chi” chưa chốt.

### Ngày 3–5: hoàn thiện bảng và giao diện

- [x] **TV2-03 · Ngày 5:** Hoàn thành thu–chi–chênh lệch theo Q1–Q4 và tổng năm; truy được nguồn thu/chi, tên chỉ tiêu đúng cách tính đã chốt.
- [x] **TV2-04 · Ngày 5:** Hoàn thành nhân viên × khoảng năm, có tổng hàng/cột và nhóm chưa gán nhân viên.
- [x] **TV2-05 · Ngày 5:** Hoàn thành khách hàng × khoảng năm, có tổng hàng/cột và nhóm khách vãng lai.
- [x] **TV2-06 · Ngày 5:** Hoàn thiện tổng hóa đơn A–B, tháng/quý/năm và nhiều năm; tiêu đề, phạm vi và chính sách tính rõ ràng; báo cáo cũ vẫn hoạt động.
- [x] **TV2-07 · Ngày 5:** Tích hợp bộ chọn báo cáo và bộ lọc; bảng và biểu đồ dùng cùng kết quả tính.
- [x] **TV2-08 · Ngày 5:** Đối chiếu từng bảng với dữ liệu kỳ vọng, gồm kỳ rỗng và biên thời gian; cùng TV1 kiểm tra tổng doanh thu giữa các bảng.
- [ ] **TV2-09 · Ngày 5:** Bàn giao cấu trúc kết quả và bộ lọc của toàn bộ bảng mình phụ trách cho TV4; ghi nhận bên nhận đã kiểm tra được.
- [x] **TV2-10 · Ngày 6–8:** Phối hợp TV3 về tái sử dụng/làm mới báo cáo và TV4 về nút xuất; đối chiếu màn hình, XLSX, PDF trên cùng bộ điều kiện.
- [ ] **TV2-11 · Ngày 9–10:** Bàn giao minh chứng, trình bày số liệu thống kê; tham gia tích hợp và tập bảo vệ.

## TV3 — Bộ nhớ, main–sub và tìm kiếm nâng cao

### Ngày 2–6: tái sử dụng dữ liệu và main–sub

- [ ] **TV3-01 · Ngày 2:** Xác nhận với nhóm phạm vi Desktop/Web, màn hình áp dụng và thời hạn lưu dữ liệu; 10 phút chỉ là đề xuất cho đến khi được chốt.
- [x] **TV3-02 · Ngày 6:** Tải danh sách một lần, giữ thời điểm tải và lọc trong bộ nhớ; Web tái sử dụng được qua nhiều request, dữ liệu lưu đúng phạm vi người dùng/quyền.
- [x] **TV3-03 · Ngày 6:** Hoàn thành Làm mới, hết hạn và làm mất hiệu lực sau thêm/sửa/xóa liên quan; giá/tồn lúc ghi giao dịch vẫn được kiểm tra bằng dữ liệu hiện hành.
- [x] **TV3-04 · Ngày 6:** Lưu kết quả báo cáo theo bộ điều kiện và quyền; bảng, biểu đồ, XLSX, PDF dùng cùng kết quả; tải lại khi cần theo quy tắc đã chốt.
- [ ] **TV3-05 · Ngày 6:** Kiểm tra main–sub: chọn bất kỳ ô nào trên dòng hóa đơn/phiếu nhập thì chi tiết đúng bản ghi; hoàn thiện Web trong phạm vi đã thống nhất.
  - [x] Desktop: mọi cột của 3 dòng hóa đơn/phiếu nhập, danh sách rỗng, so với SQL — DB-M01 đợt 2.
  - [x] Web kỹ thuật theo giả định Đơn hàng của tôi: 12 ô/2 dòng và Enter hiện đúng chi tiết; HTTP kiểm tra chủ sở hữu/no-store — [đợt 3](quy-tac-va-tien-do-dot-3.md#4-main–sub-web-và-rà-soát-phạm-vi). Phạm vi cuối vẫn chờ chốt nên mục cha chưa hoàn tất.
  - [x] Đợt 4: Space/chọn nhanh hai dòng trong trình duyệt; 8/8 hợp đồng script cục bộ phản hồi chậm/lỗi/phục hồi — [phân loại kết quả](quy-tac-va-tien-do-dot-4.md). Chưa nhận đạt mạng mô phỏng trình duyệt, tắt JavaScript hoặc mọi DPI.
  - [x] Đợt 5: proxy loopback giữ phản hồi #11 15 giây, chọn #12 phản hồi nhanh; hủy request cũ, lỗi HTTP 503/đóng socket và Space phục hồi đúng trên trình duyệt thật. Link Xem hoạt động khi CSP chặn script; Web khung 390/1280/1920 px — [minh chứng và giới hạn](quy-tac-va-tien-do-dot-5.md). Chưa kiểm tra tắt JS bằng thiết lập trình duyệt hoặc mọi DPI Windows; mục cha/phạm vi cuối vẫn mở.
- [x] **TV3-06 · Ngày 6:** Ghi log toàn bộ SQL, gồm ảnh/chi tiết: có số truy vấn ban đầu; tìm 10 từ khóa liên tiếp chưa hết hạn không thêm truy vấn thuộc dữ liệu đã lưu; làm mới, hết hạn và thay đổi dữ liệu tải lại đúng.

### Ngày 5–7: tìm kiếm nâng cao

- [x] **TV3-07 · Ngày 7:** Có đủ `>`, `<`, `>=`, `<=`, `=`, `!=` và AND/OR/NOT; thể hiện nhóm/ngoặc rõ ràng khi phối hợp điều kiện.
- [x] **TV3-08 · Ngày 7:** Kiểm tra bằng tập dữ liệu biết trước: hóa đơn từ 1–2 triệu bằng AND, khách TP.HCM OR Hà Nội, NOT đơn hủy; có ví dụ cho từng phép so sánh.
- [x] **TV3-09 · Ngày 7:** Kiểm tra đúng bằng 1 triệu/2 triệu và sát ngoài khoảng; số/ngày sai, khoảng đảo, rỗng và toán tử không hợp lệ; nếu truy vấn SQL, dùng tham số và danh sách trường/toán tử được phép.
- [ ] **TV3-10 · Ngày 7:** Xác nhận tìm nâng cao dùng dữ liệu đã lưu trong phạm vi tìm kiếm thường xuyên; bàn giao minh chứng kết quả lọc, log truy vấn và main–sub cho TV4.
- [ ] **TV3-11 · Ngày 9–10:** Trình bày bộ nhớ, tìm kiếm và main–sub; tham gia tích hợp, kiểm tra làm mới sau giao dịch và tập bảo vệ.

## TV4 — Nhập/xuất Excel, PDF và tài liệu

### Ngày 1–5: chuẩn bị luồng nhập và mẫu xuất

- [ ] **TV4-01 · Ngày 1–2:** Cùng nhóm chốt phạm vi nhập danh mục/sản phẩm, quyền nhập, chính sách thêm mới/cập nhật và xử lý lỗi; không nhập trực tiếp tồn kho.
- [ ] **TV4-02 · Ngày 5:** Chuẩn bị file Excel mẫu và mẫu xuất báo cáo; nhận cấu trúc dữ liệu từ TV1 ngày 2 và kết quả các bảng từ TV1/TV2 ngày 5.

### Ngày 6–8: hoàn thiện nhập/xuất

- [x] **TV4-03 · Ngày 8:** Có tải mẫu, chọn file, xem trước và xác nhận trước khi ghi; kiểm tra quyền nhập. Đạt kỹ thuật theo chính sách thêm mới tạm, không thay xác nhận TV4-01.
  - [x] Tạo mẫu, parser, xem trước thật, khóa/mở nút xác nhận và kiểm tra quyền đọc/lưu — DB-X01–03.
  - [x] Thao tác SaveFileDialog/OpenFileDialog Windows, preview file sai/đúng, MessageBox Yes, SQL xác nhận giá 123000/tồn 0 — [ảnh và SQL đợt 3](quy-tac-va-tien-do-dot-3.md#3-thao-tác-windows-và-phần-mềm-đọc-thật). Codex điều khiển UI, chưa thay buổi tự chạy của thành viên.
- [x] **TV4-04 · Ngày 8:** Báo lỗi theo dòng/cột, kiểm tra mã trùng và liên kết danh mục; không tự ghi đè bản ghi trùng; file sai không ghi dở theo chính sách đã chốt.
- [x] **TV4-05 · Ngày 8:** Kiểm tra nhập file hợp lệ, file sai/trùng và tài khoản không có quyền; lưu kết quả, phối hợp TV3 kiểm tra làm mới dữ liệu sau nhập.
- [x] **TV4-06 · Ngày 8:** Nối xuất XLSX và PDF cho mọi bảng bắt buộc: sản phẩm × quý, thu–chi, nhân viên × năm, khách hàng × năm, A–B và các kỳ thời gian.
- [x] **TV4-07 · Ngày 8:** File có tên báo cáo, bộ lọc, kỳ, đơn vị tiền, tiêu đề và tổng hàng/cột phù hợp; PDF nhiều năm đọc được, không cắt mất nội dung.
- [ ] **TV4-08 · Ngày 8:** Mở lại XLSX/PDF bằng phần mềm đọc file; cùng TV1/TV2 xác nhận màn hình = XLSX = PDF trên cùng bộ điều kiện; lưu bộ file minh chứng.
  - [x] Lưới/biểu đồ dùng cùng snapshot; đọc độc lập từng ô XLSX/PDF, 26 bảng/40 trang, render kiểm tra trực quan.
  - [x] Mở báo cáo bằng Excel và trình đọc PDF Chrome, tổng 611, 13 trang; [ảnh phần mềm thật](quy-tac-va-tien-do-dot-3.md#3-thao-tác-windows-và-phần-mềm-đọc-thật).
  - [x] Đợt 5: xuất trực tiếp qua SaveFileDialog cặp [snapshot 300](minh-chung-dot-5/ui-snapshot-300.xlsx); 11 bảng XLSX/11 trang PDF đối chiếu đáp án độc lập và xem từng trang render — [log](minh-chung-dot-5/export-results.txt). Đã gửi lệnh mở file mới nhưng chưa xác minh Excel/Chrome trước khi người dùng dừng UI bằng Escape; không đánh dấu phần mềm đọc mới đạt.
  - [ ] Xác nhận bàn giao giữa thành viên; chưa nhận thay người dùng là đã kiểm tra trực quan mọi sheet/trang.

### Ngày 9–10: tài liệu và bảo vệ

- [ ] **TV4-09 · Ngày 10:** Tổng hợp và cập nhật đặc tả, CSDL/ERD khi có thay đổi, hướng dẫn và kịch bản demo theo chức năng thực tế; xuất lại Word/PDF/hình cần thiết.
  - [x] Cập nhật phạm vi main–sub/nhập/xuất và [kịch bản bảo vệ đợt 3](kich-ban-bao-ve-dot-3.md); không thay đổi schema sản phẩm trong đợt này.
  - [x] Chuẩn bị [nguồn báo cáo 7 nhóm](bao-cao-ky-thuat-7-nhom.md) và [hướng dẫn bàn giao đợt 4](huong-dan-ban-giao-dot-4.md), ghi rõ giả định và giới hạn.
  - [x] Đợt 5: ba bản Word/PDF kỹ thuật chờ nhóm duyệt, 2 + 1 + 2 trang; xuất PDF qua Word cài sẵn, rasterize bằng mô-đun render_docx.py đóng gói và Poppler; đã xem đủ 5 trang cuối — [bộ rà soát](ban-ra-soat-dot-5/README.md).
  - [ ] Chốt nội dung/phạm vi và xuất lại bộ nộp cuối; không dùng bản kỹ thuật chờ duyệt thay xác nhận nhóm.
- [ ] **TV4-10 · Ngày 10:** Thu minh chứng từ từng TV, điền kết quả Đạt/Chưa đạt và người kiểm tra cho đủ 7 nhóm yêu cầu; tách kết quả lịch sử khỏi lần nghiệm thu mới.
- [ ] **TV4-11 · Ngày 9–10:** Trình bày nhập/xuất file, phối hợp chạy tích hợp và tập bảo vệ; mỗi TV vẫn tự kiểm tra phần mình phụ trách.

## Các mốc bàn giao và phụ thuộc

| Mốc | Bên giao → bên nhận | Điều kiện xác nhận | Hoàn tất |
|---|---|---|---|
| Ngày 1 | TV1 + cả nhóm → cả nhóm | Có bảng quy tắc, phạm vi và danh sách điểm còn chờ xác nhận | [ ] |
| Ngày 2 | TV1 → TV2, TV4 | Có cấu trúc dữ liệu chung, ví dụ kết quả và dữ liệu kỳ vọng | [ ] |
| Ngày 5 | TV1, TV2 → TV4 | Các bảng khớp kỳ vọng; nhận được kết quả/bộ lọc để nối xuất | [ ] |
| Ngày 6 | TV3 ↔ TV1, TV2 | Bộ nhớ/main–sub đạt; quy tắc làm mới đã tích hợp và có log | [ ] |
| Ngày 7 | TV3 → TV4 | Bộ ví dụ tìm nâng cao đạt và có minh chứng | [ ] |
| Ngày 8 | TV4 ↔ TV1, TV2, TV3 | Nhập đúng chính sách; mọi bảng xuất được XLSX/PDF và khớp số | [ ] |
| Ngày 9–10 | Cả nhóm → TV4 tổng hợp | Tích hợp đạt, minh chứng đủ, tài liệu khớp và tự chạy được demo | [ ] |

TV3 triển khai song song phần thống kê. TV4 có thể làm nhập Excel và mẫu xuất trước khi nhận đủ bảng. Tích hợp phần nhỏ hằng ngày, không chờ đến ngày 9.

## Checklist tích hợp chung — ngày 9–10

- [x] **CHUNG-01 · Cả 4 TV:** Build và bộ kiểm tra hiện có đạt; có kiểm tra tập trung cho số liệu mới, phân kỳ, phân bổ giảm giá, tìm kiếm và nhập file.
- [ ] **CHUNG-02 · Cả 4 TV:** Chạy Web khách đặt hàng → Desktop xử lý → thống kê thay đổi trên DB demo riêng; dữ liệu lưu trong bộ nhớ được làm mới đúng.
  - [x] Tích hợp tự động: startup Web thật ở tiến trình riêng, Desktop test host mở form thật; HTTP/SQL/trạng thái và snapshot 511 → 611 đúng, reload 2/reuse 0/refresh 2 — [E01](minh-chung-dot-3/results.txt).
  - [x] Tiếp tục UI của #12 đặt qua trình duyệt đợt 3: bấm Xác nhận/Giao hàng/Hoàn thành và Yes từng MessageBox Desktop; báo cáo cùng kỳ 200 → 300, Làm mới vẫn 300; SQL 2025–2026 = 711; Web hiện Hoàn thành — [đợt 4](quy-tac-va-tien-do-dot-4.md). Thao tác bởi Codex, Desktop test host, nối hai buổi.
  - [x] Đợt 5: #12 mới trên DB `...170422_f787c19b`, Web đặt → Desktop đủ nút/MessageBox → Web từng trạng thái → báo cáo 200 → 300/Làm mới → cặp XLSX/PDF 300, tồn 94 → 93; liên tục trong phiên tiếp tục, vẫn Desktop test host — [biên bản](quy-tac-va-tien-do-dot-5.md).
  - [ ] Thành viên tự chạy lượt UI mới liên tục với vòng đăng nhập Desktop sản phẩm, tồn trước/sau, theo dõi từng trạng thái Web và xuất snapshot báo cáo mới; chưa xác nhận thay TV1–TV4.
- [ ] **CHUNG-03 · TV1 chủ trì:** Không còn điểm chưa chốt ảnh hưởng cách tính; các bảng dùng cùng quy tắc và đối chiếu được.
- [ ] **CHUNG-04 · TV4 tổng hợp, mỗi TV xác nhận phần mình:** Tài liệu và minh chứng khớp phiên bản nghiệm thu; không dùng kết quả kiểm thử cũ để nhận đạt cho bản mới.
- [ ] **CHUNG-05 · Cả 4 TV:** Tự chạy trọn kịch bản bảo vệ theo phần trình bày đã phân công; đủ 7 nhóm yêu cầu có minh chứng đạt.

## Theo dõi nghiệm thu 7 nhóm yêu cầu

| Nhóm yêu cầu | Phụ trách | Kết quả (Chưa kiểm tra / Chưa đạt / Đạt) | Minh chứng | Người kiểm tra / ngày |
|---|---|---|---|---|
| 1. Chỉ nơi lấy chuỗi kết nối và dòng thực thi truy vấn | TV1 | Chưa kiểm tra trình diễn | [Vị trí mã/SQL](quy-tac-va-tien-do-dot-2.md#7-nơi-lấy-kết-nối-và-chạy-sql-để-demo) | Codex / 05/11/2026 |
| 2. Tìm nhiều từ khóa bằng dữ liệu trong bộ nhớ; hết hạn/làm mới | TV3 | Đạt trong phạm vi tạm | [DB-C01–05](minh-chung-dot-2/results.txt); TTL bằng đồng hồ tiêm, SQL thật | Codex / 05/11/2026 |
| 3. Main–sub và tái sử dụng dữ liệu báo cáo thường dùng | TV3, phối hợp TV1/TV2 | Chưa đạt toàn phạm vi | [DB-M01/C02](minh-chung-dot-2/results.txt); [đợt 5](quy-tac-va-tien-do-dot-5.md): mạng proxy chậm/lỗi/phục hồi, CSP chặn script, Web 390/1280/1920 px. JS tắt bằng thiết lập, DPI và phạm vi cuối còn chờ | Codex / 05/11/2026 |
| 4. Đủ phép so sánh và AND/OR/NOT, đúng dữ liệu | TV3 | Đạt kỹ thuật | [A01, DB-A01](minh-chung-dot-2/results.txt), [ảnh](minh-chung-dot-2/advanced-search.png) | Codex / 05/11/2026 |
| 5. Đủ bảng thống kê, kỳ thời gian và số liệu đúng | TV1, TV2 | Đạt trên bộ demo | [DB-R01/R02](minh-chung-dot-2/results.txt), [DB nguyên mẫu](minh-chung-dot-2/pristine-demo-results.txt) | Codex / 05/11/2026 |
| 6. Đọc Excel và xuất XLSX cho mọi bảng thống kê | TV4 | Đạt UI theo chính sách tạm; chờ chốt/bàn giao | [Windows UI và mở Excel đợt 3](quy-tac-va-tien-do-dot-3.md), [quyền/rollback](minh-chung-dot-3/regression-results.txt) | Codex / 05/11/2026 |
| 7. Xuất PDF cho mọi bảng thống kê, đọc được và khớp số | TV4 | Đạt qua đọc độc lập/render | [40 trang PDF](minh-chung-dot-2/export-results.txt), [13 trang demo](minh-chung-dot-2/demo-contact.png), [16 trang nhiều năm](minh-chung-dot-2/wide-contact.png) | Codex / 05/11/2026 |

## Sổ cập nhật và bàn giao

Thêm một dòng khi đổi trạng thái hoặc bàn giao. Bên nhận ghi kết quả kiểm tra; việc đã bàn giao nhưng chưa đạt cần ghi rõ phần phải sửa.

| Ngày | Mã việc | Người cập nhật | Trạng thái / kết quả | Minh chứng | Vướng mắc / bước tiếp theo | Bên nhận xác nhận |
|---|---|---|---|---|---|---|
| 05/11/2026 | TV1-01–08 | Codex | Đã lập quy tắc và làm bảng SP × quý; TV1-04/07/08 đạt kiểm tra cục bộ | [Đợt 1](quy-tac-va-tien-do-dot-1.md) | Chờ xác nhận quy tắc còn lại; tạo DB demo và kiểm tra SQL | Chưa xác nhận bàn giao |
| 05/11/2026 | TV2-02–08 | Codex | Người dùng chốt chi nhập; các bảng mới và bộ lọc đạt kiểm tra cục bộ | [R01–R05](minh-chung-dot-1/results.txt) | Kiểm tra tích hợp và đối chiếu dữ liệu DB | Chưa xác nhận bàn giao |
| 05/11/2026 | TV4-06–08 | Codex | Đang làm: xuất được bảng mới, số XLSX khớp, PDF mở lại được | [R06](minh-chung-dot-1/results.txt) | Cần kiểm tra trực quan PDF và nhập Excel | Chưa xác nhận bàn giao |
| 05/11/2026 | TV1-03/05/09, TV2-08 | Codex | Đạt tích hợp repository/service SQL trên DB demo riêng; chưa nghiệm thu toàn bộ UI | [SQL đợt 2](minh-chung-dot-2/sql-results.txt) | 22 đạt; DB BadmintonShop_SV4_Test_20261105150157_1ccb16d5 được giữ lại; tiếp bộ nhớ | Chưa xác nhận bàn giao giữa thành viên |
| 05/11/2026 | TV3-02/03/04/06 (phần Desktop) | Codex | Đạt SQL màn sản phẩm và thống kê: 10 từ khóa + chọn ảnh thêm 0 truy vấn; refresh/thay đổi dữ liệu tải lại | [Bộ nhớ đợt 2](minh-chung-dot-2/cache-results.txt) | TTL 10 phút cấu hình được là giả định; hết hạn mới kiểm tra đồng hồ giả lập, còn SQL hết hạn và liên tiến trình | Chưa nghiệm thu toàn nhóm |
| 05/11/2026 | TV3-02/05/06 (bổ sung) | Codex | 27/27 đạt: HTTP danh sách công khai qua 10 request thêm 0 SQL; main–sub Desktop chọn mọi cột của 3 dòng hóa đơn/phiếu nhập trả đúng chi tiết, danh sách rỗng xóa chi tiết | [Web/main–sub](minh-chung-dot-2/web-main-sub-results.txt) | Main–sub Web tạm giữ luồng chi tiết hiện có, chưa nghiệm thu mở rộng | Chưa xác nhận phạm vi cuối |
| 05/11/2026 | TV3-07/08/09, TV4-03/04/05 | Codex | 30/30 đạt: phép lọc kiểm tra cục bộ; đọc/lưu Excel kiểm tra SQL gồm quyền, trùng, rollback, tồn 0 | [Nhập/lọc](minh-chung-dot-2/import-search-results.txt), [Lỗi dòng/cột](minh-chung-dot-2/import-errors.txt) | Còn thao tác UI nhập file, lọc nâng cao trên UI và kiểm chứng PDF trực quan; chưa đánh dấu các mục nghiệm thu tổng thể | Chưa chốt chính sách nhập cuối |
| 05/11/2026 | TV3/TV4, CHUNG-01 | Codex | Chạy gộp 96/96 đạt; bổ sung đối chiếu lưới và giữ demo: 35/35 + 25/25 đạt (có kiểm tra trùng, không cộng số); 26 bảng XLSX/40 trang PDF khớp từng ô | [Đợt 2](quy-tac-va-tien-do-dot-2.md), [96 kiểm tra](minh-chung-dot-2/combined-results.txt), [35 kiểm tra mới nhất](minh-chung-dot-2/results.txt) | Còn chốt phạm vi/chính sách, file picker/phần mềm đọc, hai ứng dụng riêng, bàn giao và tài liệu cuối | Chưa nhận thay xác nhận của thành viên |


## Sổ cập nhật bổ sung — đợt 3

| Ngày | Mã việc | Người cập nhật | Kết quả và minh chứng | Phần còn mở |
|---|---|---|---|---|
| 05/11/2026 | CHUNG-01/02, TV3-03/04 | Codex | Build đạt; hồi quy 96/96; acceptance 3/3 trên DB mới, hai tiến trình, thu 511 → 611, SQL tải lại 2/tái dùng 0/làm mới 2 — [log](minh-chung-dot-3/results.txt), [hồi quy](minh-chung-dot-3/regression-results.txt) | Có phần trùng, không cộng số; Desktop là test host mở form thật, chưa trọn chuỗi thao tác UI |
| 05/11/2026 | TV4-03/08 | Codex | Hộp lưu/chọn file Windows, preview sai/đúng, MessageBox Yes, SQL tồn 0; mở Excel/PDF Chrome; 13 bảng/13 trang đối chiếu từng ô — [đợt 3](quy-tac-va-tien-do-dot-3.md) | Chính sách cuối, bàn giao thành viên, kiểm tra trực quan mọi sheet/trang |
| 05/11/2026 | TV3-05 | Codex | Main–sub Đơn hàng của tôi: 12 ô/2 dòng và Enter; đúng chi tiết, không rời danh sách; kiểm tra HTTP quyền/no-store — [ảnh](minh-chung-dot-3/web-main-sub.png) | Phạm vi cuối, Space/tắt JS/mạng chậm, DPI còn chưa nghiệm thu |
| 05/11/2026 | TV4-09/10/11 | Codex | Cập nhật đặc tả/hướng dẫn/biên bản, soạn [kịch bản bảo vệ](kich-ban-bao-ve-dot-3.md) và phân loại minh chứng | Chưa ký bàn giao TV1–TV4, chưa xuất bộ Word/PDF cuối, chưa tập bảo vệ trọn; UI bị dừng bằng Escape trước xử lý Desktop đơn #12 |


## Sổ cập nhật bổ sung đợt 4

| Ngày | Mã việc | Người thực hiện | Kết quả và minh chứng | Còn mở |
|---|---|---|---|---|
| 05/11/2026 | CHUNG-01/02 | Codex | Build mới đạt; bấm đủ nút/MessageBox #12, báo cáo 200 → 300, SQL 2025–2026 = 711 — [đợt 4](quy-tac-va-tien-do-dot-4.md) | Chuỗi nối hai buổi qua test host; chưa lượt mới đăng nhập Desktop sản phẩm và xuất snapshot mới của thành viên |
| 05/11/2026 | TV3-05 | Codex | Web Space/chọn nhanh đạt; 8/8 hợp đồng script cục bộ race/lỗi/phục hồi | Phạm vi cuối, mạng mô phỏng trình duyệt, JS tắt và DPI |
| 05/11/2026 | TV4-09/10/11 | Codex | Nguồn báo cáo 7 nhóm, biên bản và hướng dẫn bàn giao đã chuẩn bị | Chưa ký thay TV1–TV4; Word/PDF cuối chờ chốt nội dung và công cụ render đóng gói |

## Sổ cập nhật bổ sung đợt 5

| Ngày | Mã việc | Người thực hiện | Kết quả và minh chứng | Còn mở |
|---|---|---|---|---|
| 05/11/2026 | CHUNG-01 | Codex | Build 0 lỗi; hồi quy mới 96/96; cục bộ JS 8/8 ghi riêng — [log đợt 5](quy-tac-va-tien-do-dot-5.md) | Không coi build incremental là sửa cảnh báo lịch sử; 3/3 acceptance tự động vẫn là lịch sử đợt 3 |
| 05/11/2026 | TV3-05 | Codex | Proxy trình duyệt thật: chậm/đảo lịch phát phản hồi, hủy request cũ, HTTP 503, đóng socket, Space phục hồi; fallback CSP chặn script; Web 390/1280/1920 px | JS tắt bằng thiết lập, DPI Windows/cửa sổ nhỏ và phạm vi cuối; không thay nghiệm thu nhóm |
| 05/11/2026 | CHUNG-02, TV4-08 | Codex | Đơn #12 mới trên DB đợt 5 chạy liên tục trong phiên tiếp tục: UI Web → Desktop nút/MessageBox → báo cáo 300 → XLSX/PDF snapshot 300; tồn 94 → 93; 11 bảng/11 trang khớp đáp án độc lập và xem đủ ảnh render | Desktop test host bỏ qua đăng nhập sản phẩm; người dùng dừng UI bằng Escape trước xác minh Excel/Chrome mới; thành viên tự chạy và ký riêng |
| 05/11/2026 | TV4-09/10/11 | Codex | Cập nhật ba nguồn; đóng gói 3 Word/3 PDF và 5 ảnh trang tài liệu đã xem đủ — [bộ rà soát](ban-ra-soat-dot-5/README.md), [ZIP](ban-ra-soat-dot-5.zip) | Bản kỹ thuật chờ duyệt; bản nộp cuối, bàn giao TV1–TV4 và tập bảo vệ chưa xác nhận |
