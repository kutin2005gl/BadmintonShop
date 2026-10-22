# Đặc tả chức năng SV4 — Desktop quản trị

Ngày: 18/10/2026. Nhánh triển khai: `feature/sv4-desktop`, nền `origin/develop` tại `c57ef31`. Mã nguồn thực tế là căn cứ cho tài liệu này.

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
- Nhân viên chỉ được hủy Chờ xác nhận hoặc Đã xác nhận; lý do sau trim dài 1–500. Khách tự hủy trên web cần SV3 áp quy tắc chặt hơn: chỉ Chờ xác nhận và đúng chủ đơn.
- UPDATE so sánh trạng thái đang hiển thị trong WHERE. Nếu khác: báo đơn đã thay đổi, người dùng tải lại; không ghi đè. Không nhảy bước, mở lại đơn hủy, hủy đang giao/hoàn thành hoặc sửa POS hoàn thành.
- Trigger hiện có hoàn tồn khi chuyển sang Đã hủy; repository không hoàn lần hai. Hủy lặp không thay đổi tồn.
- Có thể xuất lại PDF cho mọi đơn đã lưu, bản in ghi rõ trạng thái (đơn chưa hoàn thành không được hiểu là đã thanh toán).

## UC4.4 — Bán tại quầy

- Tiền điều kiện: quyền `DonHangPOS`, phiên có MaNV, migration 04 và trigger đã áp dụng.
- Tìm sản phẩm đang bán ở lưới trên; chọn số lượng và Thêm vào giỏ. Chọn dòng lưới dưới để Sửa số lượng/Bỏ dòng. Hiển thị đơn giá, thành tiền, tổng.
- Nhập tiền khách đưa nguyên đồng; hiển thị thiếu/thừa. Không cho lưu giỏ rỗng, thiếu tiền, lượng ≤0, sản phẩm trùng, sản phẩm ngừng bán.
- Khi xác nhận thanh toán: khóa các sản phẩm theo thứ tự mã; kiểm lại giá hiện tại, trạng thái và tồn trong transaction; giá khác yêu cầu tải lại và chọn lại giỏ; không âm thầm bán theo giá mới.
- Ghi HoaDon và toàn bộ ChiTietHoaDon atomic. MaKH=NULL, MaNV lấy phiên, LoaiBan=Tại quầy, HinhThucTT=Tiền mặt, TrangThai=Hoàn thành. Trigger tính tổng và giảm tồn.
- Token GUID có unique index ngăn lưu lặp, kể cả retry sau phản hồi mạng lỗi. Cùng token nhưng giỏ/nhân viên/tiền khác bị từ chối. Chỉ tạo token mới khi thành công hoặc người dùng chủ động Giỏ mới.
- Sau lỗi kết nối không rõ đã commit chưa: thử lại cùng giỏ/token hoặc kiểm tra Đơn hàng; không tự tạo lại giao dịch khác.
- Sau thành công có mã hóa đơn, cho xuất PDF ngay hoặc từ Đơn hàng.
- Khuyến mãi SV3 vẫn là khung, nên giao diện công khai bán theo giá thường; không có ô voucher giả hoạt động.

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

Không sửa nghiệp vụ khuyến mãi/email/web của SV2/SV3. Không chỉnh trigger SV1. Thống kê truy vấn đồng bộ phù hợp dữ liệu đồ án; dữ liệu lớn có thể cần async, phân trang và tối ưu tổng hợp.
