# Kế hoạch hoàn thiện yêu cầu môn học

Ngày lập: 05/11/2026. Căn cứ: `ra-soat-yeu-cau-2026-11-05.md` và `yeu-cau-mon-hoc-bien-tap.md`.

Theo dõi thực hiện tại [Checklist tiến độ công việc — TV1–TV4](checklist-tien-do-4-thanh-vien.md): công việc từng thành viên, mốc bàn giao, trạng thái và minh chứng nghiệm thu.

Đây là kế hoạch đề xuất, chưa triển khai chức năng hay sửa logic. Các mốc dưới đây là ước lượng cho nhóm 4 người trong khoảng 10 ngày làm việc, không phải cam kết tiến độ khi chưa biết lịch của nhóm. Mục tiêu là nghiệm thu đủ 7 nhóm yêu cầu; không dùng số tính năng bán hàng để thay thế yêu cầu môn học.

## 1. Chốt phạm vi và quy tắc — ngày 1

**Đầu ra:** một bảng quy tắc dùng chung cho code, dữ liệu đối chiếu và báo cáo.

- Xác nhận trạng thái hóa đơn được tính, ngày lập hay ngày hoàn thành dùng để phân kỳ. Hiện ứng dụng tính đơn Hoàn thành theo NgayLap; giữ quy tắc này làm phương án tạm và ghi rõ cho đến khi được xác nhận.
- Xác nhận “chi” là tiền nhập hàng trong kỳ, thêm chi phí khác, hay giá vốn hàng đã bán. Không tự suy chi phí đã phát sinh từ mức lương trong hồ sơ nhân viên. Nếu dùng tiền nhập hàng, ghi rõ đó là cách tính thu–chi theo yêu cầu bài học, không đồng nhất với lợi nhuận kế toán.
- Xác nhận phạm vi màn hình cần main–sub, phạm vi đọc Excel và thời gian tái sử dụng dữ liệu. Đề xuất ban đầu: 10 phút, có thể cấu hình; cần xác nhận vì ghi âm chưa đủ rõ để coi đây là thời hạn cứng.
- Chốt bảng sản phẩm tính doanh thu sau giảm giá; doanh thu dòng phải cộng lại đúng tổng tiền hóa đơn. Nếu giảng viên yêu cầu cách tính khác, sửa bảng quy tắc trước khi làm báo cáo.
- Khách vãng lai và đơn chưa gán nhân viên có nhóm riêng; không bỏ giao dịch khỏi tổng.

**Điều kiện qua bước:** nhóm có cùng một bảng quy tắc. Những mục phải xác nhận với giảng viên được đánh dấu rõ; các phần độc lập vẫn triển khai. Bảng thu–chi chưa được coi là nghiệm thu khi nghĩa của “chi” còn chưa chốt.

## 2. Chuẩn bị dữ liệu báo cáo chung — ngày 1–2

**Phạm vi chính:** `BaoCaoRepository`, `BaoCaoService` và bộ kiểm tra báo cáo.

- Bổ sung dữ liệu nhân viên, khách hàng, sản phẩm và nguồn chi đã thống nhất.
- Giữ tập hóa đơn, dòng bán và khoản chi riêng khi tổng hợp để tránh nhân đôi tiền do JOIN nhiều dòng.
- Dùng chung một cách phân kỳ tháng/quý/năm và một cách phân bổ giảm cấp đơn xuống sản phẩm. Tái sử dụng cách phân bổ hiện có nếu phù hợp; quy định rõ xử lý làm tròn.
- Lập bộ dữ liệu mẫu trên DB demo riêng: ít nhất 2 năm, đủ Q1–Q4, có kỳ rỗng, nhiều dòng trong một hóa đơn, giảm cấp đơn, đơn hủy/chưa hoàn thành, khách vãng lai và nhân viên chưa gán. Có bản tính kỳ vọng độc lập, không lấy chính kết quả chương trình làm đáp án.

**Điều kiện qua bước:** tổng tiền không bị lặp theo số dòng chi tiết; tiền các dòng sau phân bổ bằng tiền hóa đơn đến đồng; giao dịch ngày cuối kỳ được đưa vào đúng kỳ.

## 3. Hoàn thiện đủ bảng thống kê — ngày 3–5

Làm theo thứ tự dưới đây. Giao diện, biểu đồ và file xuất phải dùng cùng kết quả đã tính.

| Ưu tiên | Bảng cần có | Đầu ra và tiêu chí nghiệm thu |
|---|---|---|
| 1 | Sản phẩm × quý của năm được chọn | Hàng là sản phẩm; cột Q1, Q2, Q3, Q4, Tổng; có hàng tổng. Kỳ rỗng bằng 0. Tổng bảng khớp doanh thu cùng phạm vi. Đây là bảng giảng viên nhấn mạnh nhất. |
| 2 | Thu–chi–chênh lệch/lợi nhuận theo cách tính đã chốt | Có Q1–Q4 và tổng năm; thu, chi và kết quả thu trừ chi đối chiếu được về nguồn. Tên chỉ tiêu khớp nghĩa thực tế của chi. |
| 3 | Nhân viên × năm | Chọn khoảng năm; mỗi nhân viên một hàng, mỗi năm một cột, có tổng hàng/cột và nhóm chưa gán. |
| 4 | Khách hàng × năm | Cấu trúc tương tự nhân viên, có nhóm khách vãng lai. |
| 5 | Hoàn thiện khoảng ngày và kỳ thời gian | Giữ báo cáo ngày/tháng/năm hiện có; bổ sung quý và tổng hợp nhiều năm. Tổng hóa đơn A–B có tiêu đề và chính sách tính rõ ràng. |

**Điều kiện qua bước:** từng bảng khớp bản tính kỳ vọng; tổng theo sản phẩm, nhân viên, khách hàng khớp tổng doanh thu khi dùng cùng tập hóa đơn và quy tắc. Các bảng cũ vẫn hoạt động.

## 4. Tái sử dụng bộ nhớ và main–sub — ngày 2–6, song song phần thống kê

**Tìm kiếm:** tải danh sách một lần, giữ thời điểm tải, lọc trong bộ nhớ trong thời hạn đã chốt. Có nút Làm mới, xử lý hết hạn và làm mất hiệu lực sau thao tác thêm/sửa/xóa liên quan. Web cần cơ chế dùng lại qua nhiều request; không chỉ giữ biến trong một request. Phạm vi lưu dữ liệu phải tuân theo quyền và người dùng, tránh dùng chung dữ liệu cá nhân sai phạm vi.

**Thống kê:** lưu kết quả theo bộ điều kiện đã chọn và quyền truy cập; bảng, biểu đồ, Excel, PDF dùng cùng kết quả. Chỉ truy vấn khi đổi phạm vi chưa có dữ liệu, hết hạn, người dùng làm mới hoặc dữ liệu liên quan thay đổi. Báo cáo ít dùng có thể truy vấn khi yêu cầu, đúng phạm vi đã chốt.

**Main–sub:** chọn một ô bất kỳ trên dòng hóa đơn/phiếu nhập thì vùng chi tiết cập nhật đúng bản ghi; không cần bấm riêng một link. Desktop đã có nền tảng, cần kiểm tra hành vi. Chỉ mở rộng Web theo phạm vi thống nhất.

**Điểm phải kiểm chứng:** lọc sản phẩm Desktop hiện có thể kéo theo truy vấn ảnh/chi tiết qua sự kiện chọn dòng. Đếm toàn bộ câu SQL thực chạy, không chỉ hàm lọc. Thông tin giá/tồn lúc ghi giao dịch vẫn phải được kiểm tra lại bằng dữ liệu hiện hành.

**Điều kiện qua bước:** ghi nhận số truy vấn ban đầu; tìm 10 từ khóa liên tiếp trong cùng tập dữ liệu chưa hết hạn không phát sinh thêm truy vấn thuộc danh sách/ảnh đã lưu; làm mới và hết hạn có tải lại. Kiểm tra thay đổi dữ liệu và thao tác chọn dòng không hiển thị nhầm chi tiết.

## 5. Tìm kiếm nâng cao — ngày 5–7

- Cung cấp các trường và toán tử hợp lệ: `>`, `<`, `>=`, `<=`, `=`, `!=`; thể hiện rõ AND, OR, NOT trong bộ lọc. `=` trên UI tương ứng phép bằng; không bắt người dùng nhập cú pháp code.
- Tối thiểu phải demo được: tổng hóa đơn từ 1 đến 2 triệu bằng AND; khách TP.HCM OR Hà Nội; NOT một điều kiện, ví dụ loại đơn đã hủy; và ví dụ cho từng phép so sánh còn lại.
- Nếu phối hợp nhiều điều kiện, hiển thị rõ nhóm/ngoặc để AND và OR không bị hiểu sai. Chưa cần xây trình nhập biểu thức tự do.
- Kiểm tra kiểu số/ngày, khoảng đảo ngược, giá trị rỗng và phép so sánh không phù hợp. Khi cần SQL, dùng tham số và danh sách trường/toán tử được phép.
- Dùng dữ liệu đã lưu ở bước 4 trong phạm vi tìm kiếm thường xuyên.

**Điều kiện qua bước:** có tập dữ liệu biết trước đáp án cho từng ví dụ, kiểm tra cả giá trị đúng bằng 1 triệu/2 triệu và giá trị sát ngoài khoảng. AND/OR/NOT trả đúng tập bản ghi.

## 6. Đọc/ghi Excel và xuất PDF — ngày 6–8

**Xuất:** nối toàn bộ bảng thống kê bắt buộc vào cơ chế XLSX/PDF sẵn có. File có tên báo cáo, bộ lọc, kỳ báo cáo, đơn vị tiền, tiêu đề cột, tổng hàng/cột. Bảng nhiều năm phải đọc được khi xuất PDF; dùng trang ngang hoặc chia phần hợp lý nếu cần.

**Đọc Excel:** đề xuất nhập danh mục/sản phẩm theo phạm vi giảng viên chấp nhận. Có mẫu tải về, chọn file, xem trước, báo lỗi theo dòng, kiểm tra mã trùng và liên kết danh mục, xác nhận trước khi ghi. Quy định rõ thêm mới hay cập nhật; không tự ghi đè bản ghi trùng. Không nhập trực tiếp tồn kho vì tồn đang do nghiệp vụ phiếu nhập/trigger quản lý.

**Điều kiện qua bước:** mọi bảng thống kê xuất được cả XLSX và PDF; số trên màn hình = XLSX = PDF. Mở lại được bằng phần mềm đọc file. Nhập file hợp lệ thành công; file sai chỉ ra dòng/cột và không để dữ liệu ghi dở theo chính sách nhập đã chọn. Kiểm tra quyền nhập.

## 7. Tích hợp, cập nhật tài liệu, tập bảo vệ — ngày 9–10

- Chạy build và bộ kiểm tra hiện có; bổ sung kiểm tra tập trung vào số liệu mới, phân kỳ, phân bổ giảm, tìm kiếm và luồng nhập file.
- Chạy luồng Web khách đặt hàng → Desktop xử lý → thống kê thay đổi trên DB demo riêng. Kiểm tra làm mới dữ liệu đã lưu sau thay đổi.
- Chuẩn bị màn demo chỉ ra chuỗi kết nối và dòng thực thi SQL; không hiển thị mật khẩu thật khi trình chiếu.
- Cập nhật đặc tả, mô tả CSDL nếu có thay đổi, ERD và hướng dẫn; xuất lại Word/PDF/hình từ bản đã chốt. Giữ kết quả kiểm thử lịch sử tách khỏi lần nghiệm thu mới.
- Mỗi yêu cầu có một minh chứng: màn hình, dữ liệu đối chiếu, log truy vấn hoặc file xuất; ghi trạng thái Đạt/Chưa đạt và người kiểm tra.

**Điều kiện hoàn tất:** 7 nhóm yêu cầu đều có minh chứng đạt; không còn điểm chưa chốt ảnh hưởng cách tính; nhóm tự chạy được toàn bộ kịch bản bảo vệ.

## Phân công TV1–TV4

Phân công trực tiếp theo yêu cầu của nhóm; các mốc ngày tính từ khi bắt đầu kế hoạch.

| Thành viên | Công việc chịu trách nhiệm | Đầu ra phải bàn giao | Mốc hoàn thành |
|---|---|---|---|
| **TV1** | Chuẩn hóa dữ liệu báo cáo dùng chung; phân kỳ ngày/tháng/quý/năm; phân bổ giảm giá; làm bảng sản phẩm × Q1–Q4 và Tổng. Đầu mối tổng hợp các câu hỏi để chốt quy tắc với giảng viên. | Cấu trúc dữ liệu chung cho TV2/TV4 trước ngày 3; bảng sản phẩm × quý có tổng đúng, kỳ rỗng bằng 0; minh chứng không cộng trùng hóa đơn. | Dữ liệu chung ngày 2; bảng và kiểm tra ngày 5. |
| **TV2** | Làm bảng thu–chi theo quý/năm, nhân viên × năm, khách hàng × năm; hoàn thiện báo cáo khoảng ngày và nhiều năm; tích hợp bộ chọn báo cáo trên giao diện. | Đủ các bảng còn lại, tổng hàng/cột và nhóm khách vãng lai/chưa gán nhân viên; đối chiếu số liệu với TV1. Cách tính chi phải được chốt trước khi nghiệm thu. | Ngày 5; bàn giao kết quả cho TV4 nối xuất file. |
| **TV3** | Tái sử dụng dữ liệu trong bộ nhớ cho tìm kiếm và báo cáo thường dùng; hết hạn/làm mới; tìm kiếm nâng cao; main–sub trên phạm vi Desktop/Web đã chốt. | Log đếm truy vấn khi tìm 10 từ khóa, kể cả truy vấn ảnh/chi tiết; đủ >, <, >=, <=, =, != và AND/OR/NOT; chọn ô trên dòng hiện đúng chi tiết. | Bộ nhớ/main–sub ngày 6; tìm nâng cao ngày 7. |
| **TV4** | Làm luồng nhập Excel có mẫu, xem trước và báo lỗi; nối xuất Excel/PDF cho toàn bộ bảng thống kê; tổng hợp tài liệu, ERD và kịch bản bảo vệ. | Nhập file hợp lệ, xử lý file sai/trùng; XLSX/PDF mở được và khớp số trên màn hình; tài liệu cuối và checklist minh chứng đủ 7 nhóm yêu cầu. | Nhập/xuất ngày 8; tài liệu và minh chứng ngày 10. |

**Trách nhiệm chung:** mỗi TV tự kiểm tra chức năng mình làm và cung cấp minh chứng; TV4 tổng hợp kết quả, không gánh toàn bộ kiểm thử. Ngày 9–10 cả bốn người cùng chạy luồng Web → Desktop → thống kê và tập bảo vệ. TV1 chuẩn bị phần trình bày chuỗi kết nối/dòng chạy SQL; TV2 trình bày số liệu thống kê; TV3 trình bày bộ nhớ/tìm kiếm/main–sub; TV4 trình bày nhập/xuất file.

**Thứ tự bàn giao:** TV1 chốt dữ liệu chung → TV2 xây các bảng còn lại; TV1/TV2 bàn giao cấu trúc kết quả → TV4 nối xuất file. TV3 làm song song và phối hợp TV1/TV2 khi thêm cơ chế làm mới báo cáo. TV4 có thể làm nhập Excel và chuẩn bị mẫu xuất trước khi các bảng hoàn tất.

**Quy tắc phối hợp:** tích hợp phần nhỏ hằng ngày. TV1 chịu trách nhiệm tích hợp phần dùng chung trong BaoCaoRepository/BaoCaoService; TV2 thống nhất phần sửa với TV1 trước khi làm. TV2 quản lý tích hợp giao diện thống kê, phối hợp TV3 về bộ nhớ và TV4 về nút xuất. Không chờ đến ngày 9 mới ghép tính năng.

## Checklist nghiệm thu cuối

- [ ] Chỉ được nơi lấy chuỗi kết nối và dòng thực thi truy vấn.
- [ ] Tìm nhiều từ khóa trong một phiên có bằng chứng tái sử dụng bộ nhớ, hết hạn/làm mới hoạt động.
- [ ] Main–sub chọn dòng hiện đúng chi tiết; báo cáo thường dùng tái sử dụng dữ liệu theo phạm vi đã chốt.
- [ ] Đủ phép so sánh và AND/OR/NOT, các ví dụ bắt buộc trả đúng dữ liệu.
- [ ] Tổng hóa đơn A–B, tháng/quý/năm/nhiều năm, thu–chi, nhân viên × năm, khách hàng × năm, sản phẩm × quý có đủ và tính đúng.
- [ ] Có luồng người dùng đọc Excel; mọi bảng thống kê xuất XLSX đúng.
- [ ] Mọi bảng thống kê xuất PDF đọc được và khớp số liệu.
- [ ] Build và kiểm tra cần thiết đạt; tài liệu cuối khớp chức năng thực tế.

Nếu ít thời gian hơn dự kiến: giảm trang trí, biểu đồ phụ và tính năng ngoài yêu cầu. Giữ nguyên các tiêu chí bắt buộc; ưu tiên hoàn thành bảng sản phẩm × quý trước, nhưng không nhận là đủ môn khi các nhóm khác chưa đạt.
