# Quy ước tích hợp SV4

## SV1 — lược đồ, tồn kho, phân quyền

Áp dụng `database/04_SV4_Migration.sql` trên mỗi môi trường. Thêm nullable `HoaDon.LyDoHuy NVARCHAR(500)`, `MaYeuCauPOS UNIQUEIDENTIFIER`, `TienKhachDua DECIMAL(18,0)` và unique filtered index cho token khác NULL. Web cũ không cần truyền các cột mới. Model HoaDon đã bổ sung property tương ứng.

Giữ nguyên trigger `trg_CTPN_TangTon`, `trg_CTHD_GiamTon`, `trg_HoaDon_CapNhat`. Trigger giảm tồn khi **INSERT chi tiết**, kể cả đơn Online Chờ xác nhận; không trừ thêm lúc xác nhận/hoàn thành. Hủy qua UPDATE trạng thái làm trigger hoàn kho. Không sửa/xóa từng dòng sau lưu; hoàn/trả sau giao chưa có nghiệp vụ.

`DbHelper.ExecuteTransaction` có thêm tham số optional IsolationLevel, mặc định giữ ReadCommitted. Báo cáo dùng Serializable để đọc tập dữ liệu nhất quán. POS khóa sản phẩm UPDLOCK/HOLDLOCK theo mã tăng dần, kiểm lại giá và tồn, rồi ghi hóa đơn/chi tiết trong transaction. Những luồng ghi khác vẫn phải dùng transaction và xử lý lỗi trigger.

Giữ `ChucNang`, `PhienDangNhap`, `QuyenForm`, `FrmMain.ApDungPhanQuyen`. Quản lý nhân viên, nhập hàng, sao lưu không thay đổi. Repository dùng chung không phụ thuộc phiên desktop; web phải xác thực/ủy quyền tại endpoint. Kiểm quyền trong Form là ranh giới UI desktop, không thay cho quyền SQL Server/API.

Quy tắc hủy nhân viên hiện triển khai: Chờ xác nhận/Đã xác nhận, trước Đang giao. Cần SV1/SV3 review khi gộp; chưa có luồng trả hàng sau hoàn thành.

## SV2 — danh mục, sản phẩm và ảnh

- Không tạo DanhMucRepository/SanPhamRepository khác. Giữ nguyên LayTatCa/LayTheoMa. SV4 thêm `Luu`, `Xoa` danh mục; `Luu`, `NgungBan`, `LayDanhSachHienThi` sản phẩm.
- Web chỉ hiển thị bán sản phẩm TrangThai=`Đang bán`; ảnh mới/tên/giá thay đổi thấy sau khi nạp lại. Tồn mới 0, tăng qua phiếu nhập. Không cho web ghi tồn theo dữ liệu form.
- `SanPham.HinhAnh` là **khóa tương đối** `products/<uuid>.jpg`, dấu `/`, không chứa ổ đĩa, `..`, URL, backslash. Khóa NULL nghĩa là không có ảnh. `KhoAnhSanPham.DuongDanDayDu` kiểm khóa và ghép gốc.
- Gốc kho dùng biến môi trường `BadmintonShop__MediaRoot`; mặc định `AppContext.BaseDirectory/media` chỉ phù hợp phát triển một app. Khi triển khai phải cấu hình gốc lưu chung bền vững, quyền ghi cho desktop và quyền đọc cho web. Khác máy: dùng thư mục chia sẻ/volume; thiết kế này chưa hỗ trợ upload API/object storage.
- SV2 cần mount vùng `products` của kho làm StaticFileProvider với URL ví dụ `/media/products/<uuid>.jpg`; CSDL không lưu prefix `/media`. URL tạo bằng `"/media/" + HinhAnh`, không ghép root vật lý vào HTML. Chỉ phục vụ ảnh từ vùng này, không expose toàn bộ thư mục tùy ý. Cung cấp ảnh fallback khi NULL/404/sai khóa, không lỗi cả trang.
- Desktop giải mã ảnh ≤10 MB/30 triệu điểm ảnh và mã hóa JPEG. Không dùng tên file gốc, không tự xóa ảnh cũ dùng chung. Dọn ảnh mồ côi cần đối chiếu CSDL và backup, chưa tự động thực hiện.
- Đánh giá/bình luận, tài khoản/hồ sơ web và menu duyệt phản hồi thuộc SV2, còn placeholder trên nhánh nền này.

## SV3 — hóa đơn, trạng thái và khuyến mãi

`HoaDonRepository` trước đó chưa tồn tại, SV4 tạo **một** repository chung. SV3 bổ sung API đặt hàng vào chính lớp này, không tạo repository hóa đơn song song. Các API hiện có: `LayTheoMa`, `LayDanhSachHienThi`, `LayChiTietHienThi`, `ChuyenTrangThai`, `ThanhToanPOS`.

Online: Chờ xác nhận → Đã xác nhận → Đang giao → Hoàn thành. `QuyTacBanHang.DuocChuyen` chia sẻ ma trận. `ChuyenTrangThai(ma, trạng thái kỳ vọng, mới, lý do)` UPDATE có điều kiện để tránh ghi đè; chỉ luồng nhân viên Online. Khách web tự hủy cần endpoint kiểm đúng MaKH và chỉ Chờ xác nhận; không gọi API nhân viên mà thiếu kiểm quyền/chủ đơn.

Hóa đơn POS: MaKH=NULL, MaNV=phiên, LoaiBan=`Tại quầy`, HinhThucTT=`Tiền mặt`, TrangThai=`Hoàn thành`. Giá từng dòng được kiểm lại với sản phẩm trong transaction. Token GUID bền vững chỉ dùng cho POS, không phải mã hóa đơn hiển thị. Khi retry giữ nguyên token và giỏ/tiền; index chống ghi hai lần. Mã yêu cầu khác là giao dịch khác.

Giảm theo SP là phần trăm `ChiTietHoaDon.GiamGia`; `ThanhTien` là cột tính. Giảm cấp hóa đơn/voucher ghi `HoaDon.TienGiamGia`, `MaCTKM`; `TongTien` do trigger tính. Không tự tính lại rồi trừ lần hai.

`TinhKhuyenMaiService` ở develop hiện vẫn ném NotImplementedException. SV4 không sửa ruột service, không bắt lỗi rồi báo áp voucher thành công. POS hiện công khai giá thường và truyền giảm 0. Khi SV3 hoàn thiện cần một thay đổi tích hợp riêng: tính lại khuyến mãi trong transaction thanh toán, kiểm lượt voucher atomic, cập nhật tổng tiền/tiền khách trước commit và kiểm thử chéo. Chưa tự bật theo sự tồn tại của class.

## Thống kê và thư viện

Doanh thu/số bán chỉ Hoàn thành; số đơn đếm mọi trạng thái. Lọc theo NgayLap và LoaiBan, ngày cuối tính hết ngày. Chưa có cột ngày hoàn thành nên doanh thu theo **ngày lập của đơn hiện Hoàn thành**, không theo ngày giao xong; cần thống nhất khi báo cáo tài chính thật.

Net từng hóa đơn là TongTien trong CSDL. Phân bổ cho danh mục hiện tại theo ThanhTien sau giảm sản phẩm, lấy phần nguyên và chia đồng dư theo phần dư lớn nhất, hòa ưu tiên MaDM nhỏ. Sản phẩm chuyển danh mục sẽ làm thống kê danh mục lịch sử thay đổi; cần cột snapshot nếu nhóm muốn giữ danh mục tại thời điểm bán.

Bảng/biểu đồ/file dùng `KetQuaBaoCao` cùng lần đọc. Không dùng query riêng cho tổng. Biểu đồ gộp phần dư khi quá 12 dòng nhưng giữ tổng. XLSX giữ dữ liệu số thật, PDF có Unicode và tự phân trang.

Thư viện thêm, tương thích project .NET 8 Windows, giấy phép MIT phù hợp đồ án:

- [ClosedXML 0.105.0](https://www.nuget.org/packages/ClosedXML/0.105.0): tạo XLSX thật, không cần cài Excel.
- [PDFsharp-MigraDoc-GDI 6.2.0](https://www.nuget.org/packages/PDFsharp-MigraDoc-GDI/6.2.0): PDF, dùng font Windows; không tuyên bố chạy export GDI trên Linux.
- Biểu đồ vẽ bằng WinForms/GDI sẵn có, không thêm gói biểu đồ.

Giữ thông báo bản quyền/license của các package khi phân phối theo MIT; phụ thuộc bắc cầu do NuGet restore theo dependency metadata, không chép DLL thủ công. Không dùng license trả phí/trial hoặc watermark. Version trực tiếp được pin trong csproj, NuGet audit chạy khi restore/build.
