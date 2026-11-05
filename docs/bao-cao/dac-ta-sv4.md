# Đặc tả chức năng SV4 — Desktop quản trị

Cập nhật 26/10/2026. Nhánh `feature/sv4-final-integration`, nền `origin/develop` tại `920a576`, tái sử dụng tích hợp `6e7657a`; thay đổi hiện chưa commit. Mã nguồn thực tế là căn cứ.

## Tác nhân và quyền

ADMIN dùng toàn bộ chức năng. QUANLY dùng sản phẩm, danh mục, đơn hàng, POS, thống kê. NHANVIEN chỉ dùng đơn hàng và POS. Chưa đăng nhập không được mở form. `FrmMain.ApDungPhanQuyen()` giữ nguyên; mỗi form kiểm `QuyenForm.KiemTraKhiMo` trong OnLoad, thao tác CSDL đi qua `Sv4Ui.Chay` kiểm lại `PhienDangNhap.CoQuyen`.

## UC4.1 — Quản lý danh mục

- Tiền điều kiện: có quyền `SanPhamDanhMucKhuyenMai`, CSDL sẵn sàng.
- Luồng: mở Sản phẩm → Danh mục; danh sách xuất hiện; chọn dòng để sửa hoặc Mới; nhập tên/mô tả; Lưu và xác nhận; tải lại danh sách.
- Tên được trim, bắt buộc, tối đa 100 ký tự; mô tả tối đa 255. Trùng tên xét theo collation của CSDL, được chặn cả khi có thao tác đồng thời bằng khóa và UNIQUE.
- Xóa cần chọn dòng và xác nhận; nếu có bất kỳ sản phẩm nào, kể cả Ngừng bán, chặn xóa và giải thích cần chuyển danh mục.
- Ngoại lệ: bản ghi đã xóa từ máy khác, tên trùng, mất kết nối được thông báo; không báo lưu thành công khi lỗi.
- Hậu điều kiện: thay đổi đúng một danh mục; không xóa sản phẩm kèm theo.

## UC4.2 — Quản lý sản phẩm

- Tiền điều kiện: quyền sản phẩm; có danh mục trước khi lưu sản phẩm.
- Danh sách: mã, tên, tên danh mục, thương hiệu, giá, đơn vị, tồn, trạng thái, cảnh báo. Tìm theo mã/tên/thương hiệu; lọc danh mục/thương hiệu/trạng thái/mức tồn; sắp xếp bằng tiêu đề cột.
- Thêm/Sửa mở `FrmSanPhamSua`. Tên bắt buộc ≤150, đơn vị bắt buộc ≤20, thương hiệu ≤50, giá nguyên đồng không âm trong DECIMAL(18,0), danh mục hợp lệ, trạng thái Đang bán hoặc Ngừng bán.
- Sản phẩm mới tồn 0. Tồn chỉ xem; dùng Phiếu nhập (SV1) để tăng hàng. Giá trị SoLuongTon truyền vào repository không được dùng khi ghi.
- Cảnh báo: 0 = hết; 1–5 = sắp hết; >5 = còn hàng. Đây là ngưỡng cố định của đồ án, chưa phải ngưỡng riêng từng sản phẩm.
- Ảnh: chọn JPEG/PNG/BMP, tối đa 10 MB và 30 triệu điểm ảnh; giải mã rồi mã hóa JPEG với tên UUID. Xem trước; ảnh thiếu/sai hiển thị thông báo, cho chọn khác hoặc Bỏ ảnh. Không lưu ảnh mới nếu chỉ đóng form.
- Lưu và ngừng bán cần xác nhận. Ngừng bán giữ mọi chi tiết giao dịch; không có thao tác xóa vật lý sản phẩm.
- Ảnh trong CSDL là khóa `products/<uuid>.jpg`, gốc lưu lấy từ cấu hình dùng chung. Không lưu đường dẫn máy người dùng.

## UC4.3 — Xử lý đơn hàng

- Tiền điều kiện: quyền `DonHangPOS`, migration 04 đã áp dụng.
- Lọc khoảng ngày (ngày cuối bao gồm cả ngày), trạng thái, Online/Tại quầy; tìm theo mã đơn hoặc tên/SĐT/email khách. Xem giá, lượng, giảm % từng dòng, thành tiền, giảm hóa đơn, tổng, lý do hủy.
- Chỉ đơn Online đi theo Chờ xác nhận → Đã xác nhận → Đang giao → Hoàn thành.
- Nhân viên chỉ được hủy Chờ xác nhận hoặc Đã xác nhận; lý do sau trim dài 1–500. Khách tự hủy trên web qua DonHangOnlineRepository: chỉ Chờ xác nhận và đúng chủ đơn; endpoint có Authorize và anti-forgery.
- UPDATE so sánh trạng thái đang hiển thị trong WHERE. Nếu khác: báo đơn đã thay đổi, người dùng tải lại; không ghi đè. Không nhảy bước, mở lại đơn hủy, hủy đang giao/hoàn thành hoặc sửa POS hoàn thành.
- Trigger hiện có hoàn tồn khi chuyển sang Đã hủy; repository không hoàn lần hai. Hủy lặp không thay đổi tồn.
- Có thể xuất lại PDF cho mọi đơn đã lưu, bản in ghi rõ trạng thái (đơn chưa hoàn thành không được hiểu là đã thanh toán).

## UC4.4 — POS và khuyến mãi dùng chung web

- Tiền điều kiện: quyền DonHangPOS, MaNV hợp lệ, migration 04/06 và trigger bật; token có unique filtered index khác NULL. POS từ chối sẵn sàng khi thiếu cột/index.
- Bố cục sản phẩm trái/giỏ phải; thêm/sửa lượng/bỏ dòng, voucher Áp dụng/Bỏ mã. Hiển thị giá, giảm % và thành tiền dòng, tiền trước giảm, giảm SP, giảm cấp hóa đơn/voucher, phải trả, tiền khách và thiếu/thừa.
- Dùng GioHangService/TinhKhuyenMaiService, không có bộ tính riêng của desktop. SP chọn % cao nhất còn hiệu lực; hóa đơn lấy bậc có ngưỡng cao nhất đạt được; so với voucher, chỉ dùng một ưu đãi cấp đơn có tiền giảm cao hơn, bằng nhau chọn voucher. Giảm SP cộng với một giảm cấp đơn; voucher không được chọn không tăng lượt.
- Làm tròn AwayFromZero một lần mỗi dòng như DECIMAL(18,0), rồi làm tròn giảm cấp đơn; không nhân giá đơn vị đã làm tròn để tính dòng.
- Voucher sai, chưa mở, hết hạn, hết lượt hoặc tắt có thông báo riêng; không âm thầm bỏ mã để tạo đơn. Mã tối đa 30 ký tự, trim/uppercase.
- Serializable transaction khóa sản phẩm tăng dần, tính lại giá/trạng thái/tồn/ưu đãi và so toàn bộ báo giá đã xác nhận. Thay đổi giá hoặc ưu đãi trả báo giá mới, chưa thu tiền/lưu; UI giữ giỏ và yêu cầu xác nhận lại.
- MaKH NULL, MaNV phiên, LoaiBan Tại quầy, HinhThucTT Tiền mặt, TrangThai Hoàn thành. Ghi GiamGia % ở chi tiết, MaCTKM/TienGiamGia cấp đơn; trigger tính tổng/trừ tồn. Voucher tăng atomic cùng transaction; mọi lỗi rollback cả header/chi tiết/tồn/lượt.
- Giữ token MaYeuCauPOS và SHA-256 DauVanTayPOS. Khóa applock theo token; retry đã commit đọc mã cũ trước khi tính giá hiện tại. Payload gồm nhân viên, tiền khách, SP/lượng/giá/giảm/thành tiền, voucher nhập và ưu đãi chọn. Payload khác bị từ chối, không thêm lượt hay trừ tồn lần hai.
- Giữ giỏ/token/báo giá khi lỗi, khóa thao tác lặp khi đang lưu. Thành công mới xóa giỏ/tạo token mới. Giỏ mới có cảnh báo kiểm hóa đơn nếu lần trước lỗi kết nối.
- PDF xuất lại lấy hóa đơn/chi tiết đã lưu. Không cập nhật giá/giảm lịch sử theo CTKM hiện tại. Tên SP/danh mục chưa có snapshot.

## UC4.5 — Thống kê và xuất báo cáo

- Tiền điều kiện: quyền `ThongKeBinhLuan`.
- Chọn khoảng ngày và loại bán, bấm Xem báo cáo. Các tab: doanh thu ngày/tháng/năm, bán chạy theo lượng, doanh thu danh mục, số đơn theo trạng thái. Mỗi tab có bảng và biểu đồ cột.
- Chỉ Hoàn thành ghi doanh thu và lượng bán; dùng HoaDon.TongTien đã trừ mọi giảm giá. Đã hủy và các trạng thái chưa hoàn thành không tạo doanh thu/số bán. Số đơn theo trạng thái vẫn đếm toàn bộ.
- Danh mục phân bổ net của mỗi hóa đơn theo tỷ trọng SUM(ThanhTien) sau giảm từng dòng. Lấy phần nguyên, phân phối đồng dư theo phần dư lớn nhất, hòa thì mã danh mục tăng dần. Dùng BigInteger tránh tràn khi nhân. Tổng danh mục luôn bằng tổng net.
- Danh mục dùng phân loại hiện tại của sản phẩm; lược đồ chưa lưu snapshot danh mục tại lúc bán.
- Dữ liệu đọc trong transaction Serializable rồi giữ kết quả tại form. Biểu đồ và xuất file dùng chính kết quả đó; thay bộ lọc cần bấm Xem báo cáo để nạp mới.
- Biểu đồ >12 dòng: 11 dòng đầu và nhóm Các dòng còn lại; tổng nhãn biểu đồ vẫn bằng tổng bảng. Bảng/file giữ đủ các dòng.
- Không dữ liệu: doanh thu 0, tab doanh thu trống có thông báo, các trạng thái số đơn 0. Không chia cho 0.
- Excel là XLSX thật bằng ClosedXML; PDF phân trang bằng MigraDoc/PDFsharp GDI, font Arial Unicode. File mang bộ lọc, tổng và quy tắc thống kê.

## Yêu cầu kỹ thuật và giới hạn

SQL chỉ ở Core/DataAccess, có tham số; Form không viết SQL. Giữ API đọc SV1; một HoaDonRepository dùng chung với SV3. Constructor form SV4 chỉ dựng điều khiển, OnLoad mới nạp dữ liệu/kiểm quyền. Chưa xác nhận Visual Studio Designer thao tác kéo thả; các form dựng bố cục bằng mã và constructor được kiểm tra không truy cập DB.

Chỉ điều chỉnh Core/Web cần thiết cho tính lại giá trong transaction và xác nhận báo giá; giữ API đọc, xác thực và chủ đơn. Không chỉnh trigger SV1 hoặc bổ sung email/thanh toán thật. Truy vấn desktop chạy worker qua ShopTheme.Load; dữ liệu lớn vẫn cần phân trang/benchmark. Quy tắc voucher khi hủy giữ nguyên SV3: chưa hoàn lượt.


## Rà cuối trước PR, sau chuyển ổ D

Worktree sử dụng: `D:\CodexWorktrees\BadmintonShop-sv4`, nhánh `feature/sv4-final-integration`. [Kết quả và commit mới](../final-integration/pr-review/README.md) có 74/74 integration, 15/15 install, kiểm độc lập 12 bộ lọc và demo cửa sổ thật. Đã sửa chiều cao tóm tắt POS để không cắt dòng giảm giá và giữ thông tin hóa đơn gần nhất sau tải lại. 1920×1080, DPI Windows thật, browser E2E và kiểm chéo SV2 vẫn chưa xác minh. Được phép commit/push/mở PR theo yêu cầu mới, không merge.

## Đối chiếu yêu cầu giảng viên — 05/11/2026

Các chức năng/kết quả ở trên mô tả phạm vi triển khai SV4, chưa đồng nghĩa đạt đủ yêu cầu môn học. Báo cáo hiện có sáu bảng; chưa có thu–chi–lợi nhuận theo quý, nhân viên × năm, khách hàng × năm, sản phẩm × quý/năm và nhập Excel cho người dùng. Tìm kiếm dùng bộ nhớ ở một số form nhưng chưa có cơ chế làm mới 5–10 phút đồng bộ. Các phần này là yêu cầu cần hoàn thiện, không chuyển hết sang hướng phát triển tùy chọn. Xem [báo cáo rà soát](ra-soat-yeu-cau-2026-11-05.md). Không sửa logic trong lượt review này.

## Bổ sung đợt 2 — 05/11/2026

Danh sách sản phẩm/hóa đơn và báo cáo có bộ nhớ theo điều kiện, quyền, TTL và tín hiệu ghi; Web chỉ chia sẻ catalog công khai. Đơn hàng có bộ lọc hai điều kiện AND/OR/NOT với sáu phép so sánh. Màn sản phẩm có nhập XLSX mới, xem trước, lỗi dòng/cột, quyền và rollback toàn file; không nhập tồn kho hoặc ghi đè mã. PDF nhân viên/khách hàng nhiều năm chia phần và lặp tổng toàn kỳ. Quy tắc tạm, kiểm chứng và phần chưa nghiệm thu: [đợt 2](quy-tac-va-tien-do-dot-2.md).

## Cập nhật nghiệm thu đợt 3 — 05/11/2026

Đọc [biên bản đợt 3](quy-tac-va-tien-do-dot-3.md) và [kịch bản bảo vệ](kich-ban-bao-ve-dot-3.md) trước khi demo. Main–sub Web tạm mở rộng cho Đơn hàng của tôi: chọn ô/Enter/Space nạp chi tiết cùng trang, kiểm tra chủ sở hữu và không cache dữ liệu cá nhân. Không có màn phiếu nhập Web cho khách.

Đã kiểm tra hộp lưu/chọn file Windows, preview lỗi/đúng và xác nhận nhập qua MessageBox trên DB thử; mở báo cáo bằng Excel và PDF Chrome. Tích hợp hai tiến trình tự động đạt, chưa thay nghiệm thu UI trọn Web → Desktop → báo cáo. TTL, phạm vi cuối và chính sách nhập thêm mới vẫn chờ chốt. Đợt này không đổi schema nghiệp vụ; setup DB acceptance bổ sung DDL migration 07 để trang sản phẩm đọc được bảng đánh giá.
