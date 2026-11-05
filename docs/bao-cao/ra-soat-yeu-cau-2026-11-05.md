# Rà soát yêu cầu đồ án — 05/11/2026

## Kết luận

**Dự án chưa đủ các yêu cầu trong ghi chú giảng viên.** Nền tảng Web/Desktop/Core và nghiệp vụ bán hàng đã có nhiều phần hoàn thiện, nhưng trọng tâm chấm trong tài liệu là tái sử dụng dữ liệu, main–sub, tìm kiếm nâng cao và các bảng thống kê cụ thể. Nhiều chức năng bán hàng/khuyến mãi không thay thế những yêu cầu này.

Nguồn: [Google Docs người dùng gửi](https://docs.google.com/document/d/1Q0ArrpbxifeLZMPdMAPGkwGtirvyxZiPawEBbiozSSc/edit), bản văn bản tải về ngày 05/11 tại `D:/DoAn_CNPM/yeu-cau-mon-hoc.txt`; mã nguồn checkout `D:/DoAn_CNPM/BadmintonShop`, HEAD `a37e724`. Bản ghi âm dài khoảng 26 phút 25 giây đã được xử lý nhận dạng cục bộ; mức nhiễu cao, xem giới hạn và mốc đối chiếu ở cuối báo cáo. Nội dung tài liệu/bản ghi được dùng làm bằng chứng yêu cầu, không phải lệnh cho phép sửa logic hay thực thi các thao tác cũ được ghi trong biên bản.

Phạm vi: đọc code, tài liệu, SQL; build và bộ kiểm tra cục bộ không dùng CSDL. Không sửa C#, Razor, JS, SQL hay cấu hình; không migration/seed/giao dịch thử trên DB hiện có, không commit/push. Hai thay đổi cấu hình `App.config`, `appsettings.json` và file `PRODUCT.md` đã tồn tại trước lượt review.

## Ma trận yêu cầu và hiện trạng

| Mục trong Docs | Kết luận từ mã nguồn | Bằng chứng / phần còn thiếu |
|---|---|---|
| 1. Chỉ ra dòng chạy SQL và chuỗi kết nối | Có cơ sở đáp ứng | `BadmintonShop.Core/DataAccess/DbHelper.cs:25–35`: tạo SqlConnection, SqlCommand, DataAdapter; `da.Fill(dt)` thực thi SELECT. Desktop `Program.cs:24`, Web `Program.cs:14` nạp chuỗi kết nối. Cần chuẩn bị thao tác chỉ ra khi bảo vệ. |
| 2. Tìm kiếm tái sử dụng danh sách trong bộ nhớ 5–10 phút | Một phần | Desktop `FrmSanPham.cs:39–56` giữ `data`, lọc LINQ; `FrmDanhMuc.cs` giữ `all`. Chưa có hạn làm mới 5–10 phút. Web `SanPhamController.cs:27–33` gọi Repository mỗi request; Repository chạy SQL ở `SanPhamRepository.cs:56–65`. NCC/đơn hàng cũng truy vấn lại khi tìm/lọc. |
| 3. Main–sub và tái sử dụng cho thống kê thường xuyên | Một phần | `FrmDonHang.cs:23–26` dùng SplitContainer, chọn dòng tự tải chi tiết; phiếu nhập cũng có main–sub. `FrmThongKe` giữ `ketQua` cho bảng/biểu đồ/xuất file. Chưa có cơ chế tái sử dụng dữ liệu thống kê theo phiên 5–10 phút; bấm Xem báo cáo lại truy vấn. Trang đơn khách Web hiện chỉ có link Xem, không phải main–sub; cần xác nhận phạm vi các page thường xuyên sử dụng với giảng viên. |
| 4. Tìm kiếm nâng cao có quan hệ và AND/OR/NOT | Một phần, chưa đủ minh chứng chức năng | Có các điều kiện cố định (ngày, trạng thái, loại bán, tồn), có AND/OR trong SQL/LINQ. Chưa có UI/API tìm hóa đơn tổng 1–2 triệu, chưa chọn toán tử/nhóm điều kiện hoặc NOT; chưa có ví dụ KH TP.HCM OR Hà Nội. Việc SQL nội bộ dùng AND/OR chưa đủ chứng minh người dùng làm được các tìm kiếm yêu cầu. |
| 5a. Tổng tiền hóa đơn ngày A–B | Đã có trong phạm vi chính sách hiện tại | `BaoCaoRepository.cs:15–24` lấy khoảng ngày, bao gồm ngày cuối; `BaoCaoService.cs:29–39` cộng TongTien mỗi hóa đơn Hoàn thành đúng một lần. Cần xác nhận giảng viên có muốn gồm đơn chưa hoàn thành; chưa đổi chính sách doanh thu. |
| 5b. Tháng/quý/năm/các năm và thu–chi–lợi nhuận | Một phần | Có ngày/tháng/năm. Chưa có quý, tổng chi từ phiếu nhập/chi phí và bảng thu–chi–lợi nhuận Q1–Q4. Báo cáo hiện chỉ đọc hóa đơn/chi tiết/sản phẩm/danh mục. |
| 5c. Nhân viên × năm, tương tự khách hàng | Chưa có trong module báo cáo | SQL báo cáo không lấy MaNV/MaKH; service không nhóm theo nhân viên/khách hàng, UI không có bảng chéo này. Các model có trường tương ứng không đồng nghĩa đã có thống kê. |
| 5d. Sản phẩm × quý theo năm, có tổng | Chưa có | Bảng Bán chạy hiện cộng số lượng toàn khoảng, không phải tổng tiền từng SP × Q1…Q4. Theo danh mục cũng không thay thế bảng sản phẩm. Đây là mục Docs nhấn mạnh quan trọng nhất. |
| 6. Đọc/ghi Excel; mọi bảng thống kê xuất Excel | Một phần | `XuatTaiLieu.Excel` xuất XLSX thật bằng ClosedXML từ sáu bảng hiện có. Không thấy luồng nhập Excel cho người dùng; các lệnh mở XLSX để kiểm tra nằm trong project tests. Các bảng thống kê bắt buộc còn thiếu nên chưa thể xuất đủ. |
| 7. Xuất báo cáo PDF | Đã có cho các báo cáo hiện triển khai | `XuatTaiLieu.BaoCaoPdf` và `HoaDonPdf`; bộ kiểm tra đã mở lại PDF. Cần bổ sung đầu ra cho bảng mới khi triển khai, không nhận sáu bảng hiện tại là đủ toàn bộ mục 5. |

Không quy đổi thành phần trăm hoàn thành hoặc điểm môn học: chưa biết trọng số chấm và chưa có demo giảng viên xác nhận. Trong 7 nhóm, mục 1 và cơ chế mục 7 có căn cứ đáp ứng; 5 nhóm còn lại đều có phần cần hoàn thiện.

## Những điểm dễ hiểu nhầm cần sửa trong ghi chú yêu cầu

1. Câu main–sub đang ghi “bấm link chi tiết thì mới được xem” cần diễn đạt rõ đây là thao tác gây bất tiện khi xem thường xuyên; yêu cầu mong muốn là chọn dòng hóa đơn để vùng chi tiết cập nhật ngay. Không hiểu ngược thành bắt buộc mở trang riêng.
2. Mục 5c/5d nói tổng tiền hóa đơn nhưng tiêu đề tổng lại ghi “TỔNG CHI”. Với bảng hóa đơn bán, nên ghi trung tính “Tổng tiền hóa đơn” hoặc “Tổng doanh thu” khi đã thống nhất trạng thái được tính. Nếu nói khách hàng thì “tổng tiền mua hàng” là góc nhìn của khách. Cần đối chiếu ghi âm, không mặc nhiên sửa thành số chi nhập hàng.
3. Mốc 5–10 phút nói về tái sử dụng dữ liệu đọc để tìm kiếm/thống kê thường xuyên; không nên hiểu thành giữ giá/tồn cũ khi thanh toán. Checkout vẫn cần kiểm tra dữ liệu mới để tránh bán quá tồn và sai giá. Đây là đề xuất thiết kế, không phải khẳng định giảng viên đã nói quy tắc thanh toán đó.
4. “Lợi nhuận = thu − chi” phải chốt nghĩa của chi: tổng phiếu nhập trong kỳ và chi phí khác theo yêu cầu bài học, hay giá vốn hàng đã bán. Hai cách cho kết quả khác nhau khi còn tồn kho. Repo hiện có TongTien phiếu nhập và Luong nhân viên nhưng chưa có bảng chi phí thực trả theo kỳ; không tự nhân lương và nhận là chi phí đã phát sinh.
5. Không tự thêm yêu cầu phải có một trình dựng biểu thức tùy ý nếu giảng viên chỉ cần demo các phép so sánh và logic. Có thể dùng các trường/toán tử hợp lệ, nhưng phải thể hiện được các ví dụ trong Docs. SQL injection và chuỗi biểu thức tự do cần được tránh trong thiết kế sau này.

## Phát hiện kỹ thuật đáng chú ý cho hướng xử lý

**Lọc sản phẩm Desktop vẫn có thể phát sinh SQL phụ.** `Loc()` không chạy SQL trực tiếp, nhưng gán lại DataSource dẫn tới SelectionChanged → XemAnh() → LayTheoMa(), ở `FrmSanPham.cs:28,56,69`. Do đó không nên tuyên bố “mọi lần lọc không truy vấn DB” chỉ vì thấy LINQ. Khi làm tiếp cần đo truy vấn thực tế cả chuỗi sự kiện; có thể đưa khóa ảnh/thông tin xem trước vào snapshot sản phẩm hoặc cache chi tiết. Review này chưa đo số câu SQL thực chạy.

**Báo cáo hiện có làm đúng việc tránh cộng hóa đơn lặp theo chi tiết.** Service nhóm theo MaHD trước khi cộng TongTien. Khi làm bảng SP × quý cần phân bổ giảm cấp đơn về dòng sản phẩm nếu dùng doanh thu sau giảm, để tổng sản phẩm khớp tổng hóa đơn. Có thể tái sử dụng ý tưởng PhanBo hiện dùng cho danh mục, giữ quy tắc làm tròn và tổng đúng đến đồng.

**Báo cáo chưa có dữ liệu đủ cho các chiều bắt buộc.** `BaoCaoRepository` thiếu mã/tên nhân viên, mã/tên khách và dữ liệu chi từ phiếu nhập. Khi làm tiếp cần thiết kế tập dữ liệu dùng chung trước khi thêm nhiều nút/UI, tránh mỗi bảng lại có một bộ tính lệch nhau. MaKH/MaNV NULL (khách vãng lai, đơn Online chưa gán nhân viên) cần nhóm riêng, không bỏ mất doanh thu.

**Ngày phân kỳ cần ghi rõ.** Code hiện dùng NgayLap cho hóa đơn đang Hoàn thành. Cần thống nhất với giảng viên ngày lập hay ngày hoàn thành/thu tiền; hiện không có ngày hoàn thành riêng để thay thế. Dữ liệu test phải có nhiều năm, đủ Q1–Q4 và kỳ không có phát sinh.

## Các tài liệu đã sửa trong lượt review

- `README.md`: sửa chuỗi SQL cài mới thành 01 → 02 → 03 → 04 → 06 → 07; công thức ghép file tổng thêm 07; dẫn tới báo cáo này.
- `huong-dan-cai-dat.md`: sửa thứ tự SQL, tách nâng cấp DB có dữ liệu khỏi tạo mới, tài khoản KH đã có trong seed, 16 bảng, ghi rõ SMTP chưa kiểm chứng gửi thật.
- `mo-ta-csdl.md`: phân biệt 15 bảng nền và 16 bảng hiện tại; bổ sung DanhGiaSanPham, bốn cột HoaDon và index POS.
- `docs/ERD.md`, `chuong-kien-truc.md`: thêm cập nhật lược đồ hiện tại, chỉ rõ hình nền chưa xuất lại.
- `dac-ta-sv4.md`, `tong-ket-sv4.md`: bổ sung khoảng cách với yêu cầu môn học, không nhận các phần bắt buộc còn thiếu là hướng phát triển tùy chọn.

Giữ nguyên log/biên bản kiểm thử lịch sử ngày 18/10, 22/10, 26/10; không thay số PASS cũ bằng kết quả hôm nay. Các bản DOCX/PNG/PDF lịch sử chưa xuất lại; bản Markdown cập nhật là căn cứ cho lượt review này. Google Docs gốc chưa bị ghi đè; bản biên tập yêu cầu được giao riêng để nhóm xem và dùng.

## Hướng đi tiếp theo, theo thứ tự ưu tiên

1. **Chốt cách hiểu với giảng viên:** bảng thu–chi–lợi nhuận dùng chi nào, trạng thái hóa đơn nào được tính, ngày phân kỳ nào; tổng theo nhân viên của đơn Online chưa gán NV; nhập Excel áp dụng cho bảng nào. Mang bản yêu cầu đã biên tập và các điểm chưa rõ, tránh tự suy thành yêu cầu mới.
2. **Ưu tiên bảng SP × quý/năm:** SP ở hàng, Q1–Q4 và Tổng ở cột, có hàng Tất cả; giữ SP không phát sinh bằng 0 khi phù hợp phạm vi. Xác định rõ tổng tiền trước/sau giảm và kiểm tổng bằng tổng hóa đơn. Đây là bảng được nhấn mạnh nhất trong ghi chú.
3. **Hoàn thiện thống kê bắt buộc còn lại:** thu/chi/lợi nhuận theo quý/năm, nhân viên × năm, khách hàng × năm. Dùng dữ liệu thu và chi riêng tránh JOIN nhân dòng. Thêm Excel/PDF cho tất cả các bảng; so tổng lưới = Excel = PDF = dữ liệu đối chiếu.
4. **Hoàn thiện tái sử dụng dữ liệu:** snapshot đọc có thời điểm tải, dùng lại cho tìm/lọc trong 5–10 phút; làm mới khi hết hạn hoặc người dùng yêu cầu, làm mất hiệu lực sau CRUD liên quan. Có cơ chế phù hợp cho Web qua nhiều request. Đo cả truy vấn phụ do sự kiện chọn dòng/ảnh. Bảng/biểu đồ/xuất file dùng cùng snapshot. Giữ kiểm tra giá/tồn mới khi ghi giao dịch.
5. **Tìm kiếm nâng cao:** hóa đơn tổng tiền 1–2 triệu (>= và <= kết hợp AND); khách ở TP.HCM OR Hà Nội; NOT theo điều kiện hợp lệ; các phép >, <, =, != theo tiêu chí đã chốt. Có kiểm tra kiểu/giới hạn/ngày và whitelist trường/toán tử. Tái sử dụng snapshot ở bước 4.
6. **Nhập Excel:** chọn phạm vi nhỏ hữu ích như danh mục/sản phẩm; có file mẫu, xem trước, báo lỗi theo dòng, kiểm dữ liệu trùng và quyền, xác nhận mới ghi. Không nhập trực tiếp SoLuongTon vì tồn do phiếu nhập/trigger quản lý. Đây là đề xuất triển khai, không có trong code hiện tại.
7. **Chuẩn bị bảo vệ:** demo dòng thực thi SELECT và chuỗi kết nối; tìm 10 từ khóa trong một phiên có log đếm SQL; chọn các vùng trên dòng HĐ để hiện chi tiết; trình diễn tìm nâng cao, đủ bảng thống kê và xuất/nhập file. Chạy Web khách thật → Desktop xử lý → báo cáo trên DB demo riêng. Cập nhật lại Word/ERD/PDF cuối cùng sau khi chức năng chốt.

Gợi ý chia việc, nhóm tự xác nhận: SV1 chuẩn hóa snapshot/dữ liệu và cập nhật tài liệu; SV2 tìm kiếm Web/logic nâng cao theo phạm vi; SV3 đối chiếu thu/chi, quy tắc doanh thu và phân bổ giảm; SV4 bảng thống kê/Excel nhập–xuất/main–sub. Đây là kế hoạch đề xuất, chưa giao việc hay gửi tin cho thành viên.

## Kiểm tra đã chạy ngày 05/11/2026

- `dotnet build BadmintonShop.sln --no-restore -v minimal -m:1 /p:UseSharedCompilation=false`: thành công, **0 lỗi, 1 cảnh báo CS8602 tại FrmKhuyenMai.cs:278**. Không sửa cảnh báo vì người dùng yêu cầu không sửa logic.
- `dotnet run --project tests/BadmintonShop.SV4.Checks --no-build`: **13 đạt, 0 lỗi**, gồm quy tắc, phân bổ, kết quả báo cáo hiện có, mở lại XLSX/PDF và xử lý ảnh. Chạy không truyền cờ integration; không tạo/ghi DB.
- `git diff --check`: không lỗi định dạng. So với trạng thái đầu lượt, chỉ sửa/thêm Markdown; cấu hình cá nhân đã có từ trước được giữ nguyên.

Đây là review tĩnh kết hợp kiểm tra cục bộ, chưa phải kết quả chạy UI/SQL đếm truy vấn hoặc nghiệm thu đầy đủ 7 nhóm yêu cầu. Các con số 74/74, 15/15 trong tài liệu cũ là kết quả lịch sử riêng, không phải lần chạy hôm nay.

## Đối chiếu bản ghi âm và giới hạn xác minh

Đã xử lý toàn bộ file `14.33, 29 thg 10__1.m4a` bằng nhận dạng tiếng Việt tại máy; bản ghi không được gửi lên dịch vụ chép lời. Bản chép thô lưu tại `D:/DoAn_CNPM/voice-transcript.txt`. **Bản chép có nhiều câu sai và lặp ảo, không phải bản chép lời đáng tin để trích nguyên văn.** Không khẳng định đã xác minh từng chữ của Google Docs theo lời nói. Kết luận thiếu chức năng trong báo cáo chủ yếu dựa vào Docs đọc được và mã nguồn; những diễn giải chỉ từ voice chưa rõ được giữ là điểm cần xác nhận.

Các mốc gần đúng để nghe lại và đối chiếu:

| Mốc | Nội dung nhận dạng có dấu hiệu phù hợp với Docs | Đánh giá |
|---|---|---|
| 03:40–05:50 | Nhiều từ khóa tìm kiếm, chạy lại câu truy vấn hay tái sử dụng biến bộ nhớ, phiên làm việc | Khớp chủ đề mục 2. Đoạn khoảng 04:43 có vẻ nêu 5, 10, 15 phút như ví dụ; không đủ căn cứ coi 5–10 là thời hạn cứng duy nhất. |
| 07:58–09:30 | Bấm vào các vùng trên dòng để hiện chi tiết hóa đơn | Hỗ trợ cách biên tập main–sub; chưa chứng minh mọi trang Web đều bắt buộc cùng bố cục. |
| 10:55–11:48 | Thống kê thường xuyên và tái sử dụng bộ nhớ | Phù hợp phần cuối mục 3; giữ phân biệt thống kê thường dùng/ít dùng. |
| 13:16–15:20 | Ví dụ tập khách hàng TP.HCM/Hà Nội và kết hợp điều kiện | Không đồng nhất từ “và” trong tiếng Việt với AND khi ý muốn là hợp hai tập. Docs đã ghi OR ở ví dụ này. |
| 16:45–17:34 | Tổng tiền hóa đơn theo khoảng ngày và kỳ thời gian | Phù hợp mục 5a/5b; các công thức cụ thể chưa nghe/nhận dạng đủ rõ để sửa định nghĩa chi. |
| 19:45–20:42 | Bảng quan trọng, hỗ trợ kế hoạch kinh doanh năm sau | Phù hợp nhấn mạnh mục 5d. Tên từng cột vẫn dựa trên bảng đã ghi trong Docs. |
| 21:40–21:59 | Đọc dữ liệu từ Excel và xuất bảng thống kê | Hỗ trợ việc mục 6 có cả đọc và ghi; chọn phạm vi nhập vẫn cần xác nhận. |

Hai chỗ phải thận trọng: chưa xác nhận chắc chắn giảng viên gọi cột tổng ở 5c/5d là “tổng chi” hay ghi chú bị nhầm; chưa xác minh nguyên văn đoạn PDF và phần cuối có gợi ý MAX/MIN/AVG do nhận dạng quá nhiễu. Giữ yêu cầu PDF đã ghi rõ trong Docs, không tự thêm MAX/MIN/AVG thành tiêu chí bắt buộc.

Bản [yêu cầu biên tập](yeu-cau-mon-hoc-bien-tap.md) làm rõ câu main–sub và tên cột tổng, nhưng ghi rõ phần diễn giải/chưa chắc. Google Docs gốc chưa sửa trực tiếp; nhóm có thể đối chiếu và dùng bản biên tập này thay vì nhận các đoạn nhận dạng nhiễu là lời giảng viên chính xác.
