# Checklist công việc SV1 SV2 SV3 SV4 theo file phân công và góp ý của cô

Ngày đối chiếu: **13/11/2026**, múi giờ Asia/Saigon. Phiên bản mã nguồn: nhánh `develop`, commit `52702741204edbdf4983716183e32413164e5666`.

**Nguồn phân công gốc:** [CNPM_01_Phan_cong_cong_viec.docx][phan-cong] ở thư mục ngoài `BadmintonShop`, áp dụng cho các mục `SV1-*` đến `SV4-*` và `CHUNG-*`. Ký hiệu **PC §5 SV1**, chẳng hạn, chỉ mục 5, phần SV1 của file này. Các file trong dự án là bằng chứng thực hiện, không tự bổ sung yêu cầu vào phần phân công gốc. Công việc lặp lại giữa bảng tổng quan, nhiệm vụ chi tiết và kế hoạch tuần đã được gộp.

**Nguồn góp ý bổ sung:** [ghi âm buổi góp ý ngày 12/11/2026][ghi-am-co], được theo dõi riêng tại phần **Góp ý bổ sung của cô ngày 12/11** bằng mã `CO-01` đến `CO-07`. Nội dung được diễn giải từ lời nhận dạng và [bản phụ đề tự động][phu-de-co], không phải trích nguyên văn. Mốc ghi âm dùng để nghe đối chiếu; các chỗ chưa rõ được ghi cần xác minh. Phân công ở phần này là dự kiến theo vai trò, không thay đổi phân công gốc.

## Cách đọc

- `[x]`: có đủ bằng chứng cho nội dung của mục, ở tầng kiểm tra được ghi kèm. Không đồng nghĩa đã kiểm mọi thao tác giao diện.
- `[ ]` **Thiếu**: chưa có phần triển khai/tài liệu đáp ứng yêu cầu trong bản dự án được rà soát.
- `[ ]` **Một phần**: đã có nhưng còn thiếu điều kiện, nội dung hoặc phạm vi của yêu cầu.
- `[ ]` **Chưa xác minh**: chưa đủ bằng chứng để kết luận; không khẳng định sinh viên chưa làm ở nơi khác.

“SV đã làm được” ở đây là phần việc **được giao cho SV đó đã có trong dự án**, không xác nhận tác giả thực tế từ tên file/comment. Kiểm tra do người rà soát chạy không thay cho việc sinh viên tự viết tài liệu, kiểm thử chéo, họp hoặc luyện demo.

Theo lựa chọn đã thống nhất, kiểm thử chéo lấy **PC §6** làm chuẩn: **SV4 kiểm SV1; SV3 kiểm SV2; SV1 kiểm SV3; SV2 kiểm SV4**. Không dùng hai phân công ngược ở tuần 5 để tạo thêm nhiệm vụ. PC §1, §4 và §8 là thông tin vai trò, ước lượng và mẫu tự đánh giá; con số 100 trong mẫu không được dùng làm bằng chứng hoàn thành.

| Phần việc | Đã đánh dấu | Còn để trống | Tổng số mục |
|---|---:|---:|---:|
| SV1 | 27 | 12 | 39 |
| SV2 | 17 | 21 | 38 |
| SV3 | 16 | 14 | 30 |
| SV4 | 25 | 10 | 35 |
| Quy tắc chung của cả bốn SV | 1 | 4 | 5 |

Đây là số ô của **phần phân công gốc** theo cách tách yêu cầu để đối chiếu, không phải phần trăm khối lượng hay điểm đóng góp cá nhân. Các mục `CO-*` được đếm riêng ở phần góp ý bổ sung, không cộng vào bảng trên.

## SV1 Nền tảng và điều phối

### Mã nguồn và cơ sở dữ liệu

- [x] **SV1-01 Tạo repository GitHub, nhánh main/develop và .gitignore.** Nguồn: PC §5 SV1, §7 T1. Bằng chứng: Git có `origin`, `main`, `develop` và các nhánh tính năng; [.gitignore][gitignore] loại `bin/`, `obj/`, `packages/` và cấu hình riêng.
- [ ] **SV1-02 Tạo bảng công việc, mỗi việc có thẻ, người phụ trách và hạn chót.** Nguồn: PC §5 SV1, §9. **Chưa xác minh:** chưa thấy liên kết hoặc bản xuất bảng công việc; các bảng tiến độ trong tài liệu chưa chứng minh đã có thẻ cho mọi việc.
- [x] **SV1-03 Dựng solution gồm Core, Web và Desktop.** Nguồn: PC §7 T1. Bằng chứng: [solution][solution] và build mới thành công, xem KT1 cuối file.
- [x] **SV1-04 Viết SQL tạo bảng, khóa chính, khóa ngoại, CHECK và chỉ mục.** Nguồn: PC §5 SV1. Bằng chứng: [script tạo CSDL][schema], [script tổng][sql-tong]; cài mới đạt 16 bảng, có ràng buộc và chỉ mục; KT2.
- [x] **SV1-05 Viết script dữ liệu mẫu.** Nguồn: PC §5 SV1, §7 T1. Bằng chứng: [03_SampleData.sql][seed] và script tổng chạy được trên CSDL thử mới; KT2.
- [x] **SV1-06 Nhập hàng làm tăng tồn kho.** Nguồn: PC §5 SV1. Bằng chứng: `trg_CTPN_TangTon` trong [02_Triggers.sql][triggers]; I03 và A07 kiểm tăng tồn đúng lượng nhập.
- [x] **SV1-07 Bán hàng Web/POS làm giảm tồn kho.** Nguồn: PC §5 SV1. Bằng chứng: `trg_CTHD_GiamTon`; I04, P14, H01 kiểm cả hóa đơn tại quầy và Online.
- [x] **SV1-08 Chặn bán quá tồn.** Nguồn: PC §5 SV1. Bằng chứng: trigger và `sp_KiemTraTonKho`; I05/I06/I10, P16/P19 kiểm thiếu tồn, rollback và tranh sản phẩm cuối; A09 kiểm thủ tục.
- [x] **SV1-09 Tạo Models và DAL dùng chung trong BadmintonShop.Core.** Nguồn: PC §5 SV1, §7 T1/T3. Bằng chứng: [Core][core-project], các Models/Repository, [DbHelper][dbhelper]; được các lượt kiểm tra SQL/repository sử dụng.
- [x] **SV1-10 Đọc chuỗi kết nối từ cấu hình, cả Web và Desktop tham chiếu Core.** Nguồn: PC §5 SV1, §9. Bằng chứng: [Web Program][web-program], [Desktop Program][desktop-program], hai `.csproj`; kiểm tra chạy với biến môi trường trỏ cùng CSDL thử.
- [x] **SV1-11 Cơ chế đăng nhập và bảo vệ chức năng Web cho khách hàng trong KhachHang.** Nguồn: PC §3, §5 SV1. Bằng chứng: [KhachHangRepository][kh-repo], [Cookie Authentication][auth-web], `[Authorize]`; W01–W10 và H01 kiểm cookie, đăng nhập/đăng xuất, trang cần đăng nhập và chủ đơn.
- [x] **SV1-12 Đăng nhập Desktop bằng TaiKhoan gắn với NhanVien và phân quyền.** Nguồn: PC §5 SV1, §7 T2. Bằng chứng: [TaiKhoanRepository][tk-repo], [PhanQuyen][phanquyen], [FrmDangNhap][login-desktop]; A01/A02 kiểm đúng/sai/khóa tài khoản, U04 kiểm ma trận quyền.
- [x] **SV1-13 Băm mật khẩu bằng BCrypt hoặc SHA-256 có salt.** Nguồn: PC §5 SV1. Bằng chứng: [MatKhauHelper][password] dùng BCrypt; A03 kiểm xác thực hash và salt khác nhau giữa hai lần băm.
- [x] **SV1-14 Desktop quản lý nhà cung cấp.** Nguồn: PC §5 SV1, §7 T2. Bằng chứng: [FrmNhaCungCap][ncc-form] và repository; A04 kiểm thêm/sửa/xóa, smoke mở form thành công.
- [x] **SV1-15 Lập phiếu nhập: chọn NCC, thêm dòng chi tiết và tự tính tổng tiền.** Nguồn: PC §5 SV1, §7 T3. Bằng chứng: [FrmPhieuNhap][pn-form], [PhieuNhapRepository][pn-repo]; A07 kiểm tổng 200 đồng cho 2 × 100, A08 kiểm rollback khi dòng sau lỗi; đã smoke form phiếu và danh sách phiếu.
- [x] **SV1-16 Desktop thêm/sửa nhân viên.** Nguồn: PC §5 SV1, §7 T2. Bằng chứng: [FrmNhanVien][nv-form]; A05 kiểm thêm/sửa hồ sơ qua repository, smoke mở form thành công.
- [x] **SV1-17 Desktop cấp tài khoản, khóa tài khoản và đổi vai trò.** Nguồn: PC §5 SV1. Bằng chứng: form nhân viên/tài khoản và TaiKhoanRepository; A06 kiểm cấp tài khoản, đổi vai trò, khóa/mở khóa và đổi mật khẩu.
- [x] **SV1-18 Sao lưu CSDL.** Nguồn: PC §5 SV1, §7 T4. Bằng chứng: [FrmSaoLuu][backup-form], [SaoLuuRepository][backup-repo]; B01 tạo bản sao lưu CSDL thử và `RESTORE VERIFYONLY` thành công từ SQL Server. Giới hạn thao tác giao diện ghi tại KT4.
- [x] **SV1-19 Phục hồi CSDL.** Nguồn: PC §5 SV1, §7 T4. Bằng chứng: B02 gọi `SaoLuuRepository.PhucHoi` trên CSDL thử, dữ liệu chèn sau thời điểm sao lưu biến mất sau phục hồi. Form có chọn file và xác nhận; chưa xác nhận bấm nút phục hồi với thư mục Backup mặc định trên máy này, xem KT4.

### Tài liệu và bàn giao

- [x] **SV1-20 Viết chương kiến trúc Web – Desktop – CSDL và lý do tách Core.** Nguồn: PC §5 SV1. Bằng chứng: [chương kiến trúc][architecture], [sơ đồ kiến trúc][architecture-diagram]; nội dung có mô hình phân lớp và giải thích dùng chung thư viện.
- [x] **SV1-21 ERD, lược đồ quan hệ và mô tả các bảng/thuộc tính.** Nguồn: PC §5 SV1. Bằng chứng: [ERD][erd], [lược đồ quan hệ][relational], [mô tả CSDL][db-doc]; có bổ sung `DanhGiaSanPham` và các cột HoaDon mới.
- [x] **SV1-22 Hướng dẫn cài đặt thành file riêng.** Nguồn: PC §5 SV1, §7 T4. Bằng chứng: [hướng dẫn cài đặt][install-doc] và bản `.docx`; có tạo CSDL, cấu hình, build/chạy, tài khoản mẫu, sao lưu/phục hồi và đóng gói.
- [ ] **SV1-23 Ghép và định dạng báo cáo cuối.** Nguồn: PC §5 SV1, §7 T5. **Một phần:** có [báo cáo tổng Markdown][report] và Word, đã ghép kiến trúc/CSDL/tài liệu SV1/SV4; vẫn còn TODO ở giới thiệu, SRS/use case SV2, sơ đồ SV3, HDSD SV2/SV3, tổng kết cả nhóm và tự đánh giá. Chưa thể coi là báo cáo cuối hoàn chỉnh.
- [x] **SV1-24 Bàn giao BadmintonShop.sql chạy được trên CSDL trống.** Nguồn: PC §5 SV1, Bàn giao. Bằng chứng: KT2 chạy nội dung script tổng trên CSDL tên mới, không sửa schema/dữ liệu; 16 bảng, 3 trigger, 1 thủ tục. Không suy kết quả này thành đã thử trên máy sạch.
- [x] **SV1-25 Core và hai ứng dụng chạy được trên môi trường kiểm tra.** Nguồn: PC §5 SV1, Bàn giao. Bằng chứng: KT1, KT3–KT5; Web chạy startup thật và nhận yêu cầu HTTP, các form Desktop liên quan đã được smoke. Không phải xác nhận mọi chức năng của cả nhóm đã đủ.
- [ ] **SV1-26 Bàn giao báo cáo cuối dạng PDF.** Nguồn: PC §5 SV1, Bàn giao. **Thiếu:** chưa thấy PDF của báo cáo tổng cuối; các PDF hóa đơn, báo cáo thống kê và bộ báo cáo kỹ thuật đợt 5 không thay thế sản phẩm này.
- [ ] **SV1-27 In và bàn giao báo cáo bản giấy.** Nguồn: PC §5 SV1, Bàn giao; §7 T5. **Chưa xác minh:** dự án không có bằng chứng đã in/bàn giao.
- [ ] **SV1-28 Chạy thử trên máy sạch và chuẩn bị máy demo.** Nguồn: PC §7 T4/T5. **Một phần:** có hướng dẫn, thư mục publish và kết quả kiểm CSDL mới; [hướng dẫn cài đặt][install-doc] vẫn ghi TODO chạy máy sạch/chốt gói nộp. Máy phát triển hiện tại không phải máy sạch.

### Tích hợp và công việc chung

- [x] **SV1-29 Gộp code và tích hợp Web/Desktop dùng chung CSDL/Core.** Nguồn: PC §3, §5 SV1, §7 T4/T5. Bằng chứng: lịch sử Git có các merge PR #1–#18; H01 và P14 kiểm Web đặt đơn, Desktop cập nhật, Web đọc cùng đơn/trạng thái.
- [ ] **SV1-30 Mọi thay đổi code qua PR, được SV1 duyệt/gộp hoặc SV4 duyệt thay.** Nguồn: PC §2, §9. **Một phần:** có nhiều merge PR, nhưng có merge nhánh trực tiếp và commit tích hợp trong lịch sử; chưa đủ hồ sơ approval để xác nhận toàn bộ thay đổi tuân thủ đúng quy trình.
- [ ] **SV1-31 Chốt CSDL/Core cuối tuần 1 và thống nhất giao diện dùng chung với nhóm.** Nguồn: PC §2, §6. **Chưa xác minh:** các migration hiện có chủ yếu bổ sung bảng/cột; biên bản tuần 1 còn chờ nhóm xác nhận. Chưa có hồ sơ đủ để xác nhận mốc chốt và việc giữ lược đồ xuyên suốt sau mốc đó.
- [ ] **SV1-32 Hỗ trợ các bạn khi kẹt truy vấn và chạy thử toàn hệ thống mỗi tuần.** Nguồn: PC §5 SV1, §7 T3. **Một phần:** có README hướng dẫn dùng Core/DAL và nhiều kết quả tích hợp; chưa có bằng chứng đủ từng tuần và việc hỗ trợ cụ thể để đánh dấu hoàn tất nhiệm vụ này.
- [x] **SV1-33 Viết đặc tả use case và test case phần mình.** Nguồn: PC §2, §3. Bằng chứng: [đặc tả SV1][spec1] gồm luồng chính/ngoại lệ và **36 test case**. Đây là bằng chứng có tài liệu, không phải cả 36 ca đã chạy đạt.
- [ ] **SV1-34 Chụp màn hình giao diện phần mình.** Nguồn: PC §2, §3, §7 T5. **Thiếu trong bộ tài liệu phần SV1:** [HDSD SV1][guide1] còn liên kết `anh-sv1/...` ghi TODO; chưa có bộ ảnh tương ứng. Sơ đồ đăng nhập không thay ảnh màn hình.
- [x] **SV1-35 Viết hướng dẫn sử dụng phần mình.** Nguồn: PC §3. Bằng chứng: [HDSD SV1][guide1] mô tả đăng nhập, NCC, nhân viên/tài khoản, phiếu nhập, sao lưu/phục hồi và cơ chế đăng nhập Web. Mục ảnh chưa đủ được đánh giá riêng ở SV1-34.
- [x] **SV1-36 Tạo dữ liệu demo cho bảng phụ trách.** Nguồn: PC §3, bảng phân công CSDL. Bằng chứng KT2: 5 nhân viên, 5 tài khoản, 6 NCC, 11 phiếu nhập và 42 dòng chi tiết nhập.
- [ ] **SV1-37 Kiểm thử chéo phần SV3 trước khi gộp develop.** Nguồn: PC §2, §6. **Chưa xác minh:** P/H chứng minh kỹ thuật tích hợp, chưa chứng minh SV1 đã kiểm và xác nhận trước PR tương ứng.
- [ ] **SV1-38 Tham gia họp hằng tuần, báo cáo tiến độ và ghi biên bản thực tế.** Nguồn: PC §9. **Một phần:** có [biên bản tuần 1–5][meetings], nhưng ngày/giờ/hình thức, người tham gia và xác nhận còn trống/TODO; phần lớn là tổng hợp Git, chưa đủ bằng chứng họp thật.
- [ ] **SV1-39 Luyện demo cùng nhóm.** Nguồn: PC §3, §7 T5. **Chưa xác minh:** có kịch bản và log do công cụ chạy, chưa có xác nhận SV1 đã luyện demo.

## SV2 Web khách hàng A

### Tài khoản và giao diện khách hàng

- [x] **SV2-01 Dựng project Web, layout và màn hình đăng ký/đăng nhập.** Nguồn: PC §7 T1. Bằng chứng: [Web project][web-project], [layout][layout], [TaiKhoanController][account-controller] và Views; KT1/KT5.
- [x] **SV2-02 Đăng ký tài khoản và kiểm tra trùng email.** Nguồn: PC §5 SV2. Bằng chứng: account controller và KhachHangRepository; W01 đăng ký thành công, W08 chặn email đã tồn tại.
- [ ] **SV2-03 Kiểm tra trùng SĐT khi đăng ký.** Nguồn: PC §5 SV2. **Thiếu:** controller/repository chỉ kiểm trùng email; GAP03 đăng ký được hai tài khoản khác email nhưng cùng SĐT trên CSDL thử.
- [ ] **SV2-04 Yêu cầu mật khẩu mạnh khi đăng ký.** Nguồn: PC §5 SV2. **Một phần:** Core có `LaMatKhauManh`, nhưng [DangKyViewModel][registration-model] chỉ yêu cầu tối thiểu 6 ký tự và controller không gọi helper; GAP02 xác nhận mật khẩu `abc123` vẫn được nhận.
- [x] **SV2-05 Đăng nhập và đăng xuất Web.** Nguồn: PC §5 SV2, §7 T2. Bằng chứng: W05–W07 kiểm đăng xuất bảo vệ trang hồ sơ, sai mật khẩu bị chặn, mật khẩu đúng đăng nhập được.
- [ ] **SV2-06 Quên mật khẩu.** Nguồn: PC §5 SV2. **Một phần:** W09/W10 chạy được OTP demo → đặt lại → đăng nhập; các action chỉ hoạt động khi `IsDevelopment()`, trả 404 ở môi trường khác, OTP hiển thị ngay trên trang. Chưa có luồng quên mật khẩu dùng được cho bản chạy thông thường ngoài Development.
- [x] **SV2-07 Đổi mật khẩu.** Nguồn: PC §5 SV2. Bằng chứng: `DoiMatKhau` kiểm mật khẩu hiện tại và cập nhật hash; W04 xác nhận bằng phiên đăng nhập mới với mật khẩu đã đổi.
- [x] **SV2-08 Hồ sơ: xem/sửa thông tin và địa chỉ nhận hàng.** Nguồn: PC §5 SV2, §7 T2. Bằng chứng: `HoSo`, `CapNhatHoSo`; W02/W03 kiểm hiển thị và lưu lại họ tên/địa chỉ.
- [x] **SV2-09 Trang chủ hiển thị nhóm sản phẩm nổi bật.** Nguồn: PC §5 SV2. Bằng chứng: [Home/Index][home-view] có nhóm sản phẩm mới và nhóm gắn nhãn “BÁN CHẠY”. Nhóm này hiện chọn theo tên sau khi loại sản phẩm mới, không tính từ doanh số; file phân công không quy định thuật toán chọn nổi bật.
- [ ] **SV2-10 Trang chủ hiển thị sản phẩm khuyến mãi.** Nguồn: PC §5 SV2. **Thiếu:** HomeController/Home view chỉ lấy sản phẩm đang bán, nhóm mới và nhóm nổi bật; chưa đọc CTKM để tạo nhóm sản phẩm khuyến mãi.
- [x] **SV2-11 Trang chủ có danh mục vợt, cầu, giày, quần áo, phụ kiện.** Nguồn: PC §5 SV2. Bằng chứng: Home view lấy danh mục từ CSDL và tạo liên kết lọc `maDM`; script mẫu có các danh mục này.

### Catalog và chi tiết sản phẩm

- [x] **SV2-12 Hiển thị danh sách sản phẩm và tìm kiếm theo tên.** Nguồn: PC §5 SV2, §7 T2/T3. Bằng chứng: [SanPhamController.Index][product-controller], [catalog view][catalog-view]; W11 kiểm trang kết quả với từ khóa.
- [x] **SV2-13 Lọc sản phẩm theo danh mục.** Nguồn: PC §5 SV2. Bằng chứng: tham số `maDM`, combobox danh mục và điều kiện lọc trong controller; W11.
- [ ] **SV2-14 Lọc riêng theo thương hiệu.** Nguồn: PC §5 SV2. **Một phần:** ô từ khóa tìm cả tên lẫn thương hiệu, nhưng chưa có tham số/điều kiện lọc riêng theo thương hiệu để kết hợp với các bộ lọc khác; GAP04 và mã Index.
- [ ] **SV2-15 Lọc theo khoảng giá.** Nguồn: PC §5 SV2. **Thiếu:** Index và form catalog chưa có giá từ/đến hoặc điều kiện tương ứng; GAP04.
- [x] **SV2-16 Sắp xếp theo giá và tên.** Nguồn: PC §5 SV2. Bằng chứng: `gia-tang`, `gia-giam`, `ten-z-a`, mặc định tên A–Z; các lựa chọn có trong controller/view.
- [ ] **SV2-17 Sắp xếp theo mới nhất.** Nguồn: PC §5 SV2. **Thiếu:** catalog chưa có lựa chọn/nhánh sắp xếp mới nhất. Nhóm “mới” ở trang chủ không thay yêu cầu sắp xếp danh sách.
- [x] **SV2-18 Phân trang và giữ tham số lọc trên URL.** Nguồn: PC §5 SV2. Bằng chứng: 8 sản phẩm/trang, `Skip/Take`, form GET và liên kết trang giữ `tuKhoa/maDM/sapXep`; W11. Chỉ xác nhận các bộ lọc hiện có, không tính các bộ lọc thiếu ở SV2-14/15.
- [ ] **SV2-19 Trang chi tiết hiển thị ảnh của sản phẩm.** Nguồn: PC §5 SV2. **Một phần:** [ChiTiet.cshtml][detail-view] có ảnh nhưng gọi `AnhMinhHoa.TheoDanhMuc`, chưa dùng `Model.HinhAnh`; chưa hiển thị đúng ảnh sản phẩm được quản lý/tải lên từ Desktop.
- [x] **SV2-20 Trang chi tiết hiển thị thông số sản phẩm.** Nguồn: PC §5 SV2. Bằng chứng: detail view hiển thị `MoTa`; dữ liệu mẫu có 3U/4U, độ cứng và size trong mô tả (truy vấn KT2 tìm được 25 sản phẩm có các thông số này). Không yêu cầu thêm cột thông số riêng ngoài file phân công.
- [x] **SV2-21 Trang chi tiết hiển thị tồn kho.** Nguồn: PC §5 SV2. Bằng chứng: detail view dùng `SoLuongTon`, phân biệt hết/sắp hết/còn hàng và giới hạn số lượng thêm giỏ.
- [ ] **SV2-22 Trang chi tiết hiển thị giá sau khuyến mãi.** Nguồn: PC §5 SV2. **Thiếu:** view in `Model.DonGiaBan`; action ChiTiet chưa lấy mức giảm theo sản phẩm từ service khuyến mãi. Giỏ tính giá giảm chưa thay việc hiển thị tại chi tiết.
- [x] **SV2-23 Nút thêm giỏ gọi chức năng của SV3.** Nguồn: PC §5 SV2, §6. Bằng chứng: form POST `GioHang/Them` truyền `maSP/soLuong`; W12 thêm sản phẩm từ trang chi tiết thành công.

### Đánh giá, tài liệu và bàn giao

- [x] **SV2-24 Gửi chấm sao và nội dung đánh giá.** Nguồn: PC §5 SV2. Bằng chứng: `GuiDanhGia`, [DanhGiaRepository][review-repo], view form; W/GAP01 xác nhận có ghi đánh giá chờ duyệt. Điều kiện đã mua được đánh giá riêng ở mục sau.
- [ ] **SV2-25 Chỉ khách đã mua mới được đánh giá.** Nguồn: PC §5 SV2. **Thiếu:** controller và repository không kiểm HoaDon/ChiTietHoaDon; GAP01 xác nhận tài khoản vừa đăng ký, chưa mua sản phẩm, vẫn gửi đánh giá thành công.
- [x] **SV2-26 Desktop duyệt bình luận, Web chỉ hiện bình luận đã duyệt.** Nguồn: PC §3, §5 SV2, §7 T4. Bằng chứng: [FrmQuanLyDanhGia][review-form], `CapNhatTrangThai`, `LayTheoSanPham`; A10 kiểm trước/sau duyệt và smoke mở form.
- [ ] **SV2-27 Desktop ẩn bình luận đã hiển thị.** Nguồn: PC §5 SV2. **Một phần:** có nút từ chối đánh giá chờ duyệt, nhưng handler chặn mọi đánh giá không còn “Chờ duyệt”. Chưa ẩn lại được bình luận “Đã duyệt” qua màn hình này.
- [ ] **SV2-28 Viết SRS gồm chức năng, phi chức năng và người dùng hệ thống.** Nguồn: PC §5 SV2, §7 T1. **Thiếu:** [báo cáo tổng][report] mục 2 còn `TODO(SV2): Cung cấp SRS...`; chưa thấy SRS riêng đáp ứng nội dung này.
- [ ] **SV2-29 Sơ đồ use case tổng quát cho cả Web và Desktop.** Nguồn: PC §5 SV2, §7 T1. **Thiếu:** chưa thấy sơ đồ này trong tài liệu/sơ đồ bàn giao; mục 2 báo cáo còn TODO.
- [ ] **SV2-30 Đặc tả chi tiết các use case phía khách hàng.** Nguồn: PC §5 SV2, §7 T4; §2/§3. **Thiếu:** phụ lục hiện có đặc tả SV1/SV4, chưa có bộ đặc tả use case tài khoản/catalog/đánh giá và các use case khách hàng được phân công cho SV2.
- [ ] **SV2-31 Bàn giao module tài khoản, catalog và đánh giá đáp ứng đầy đủ yêu cầu.** Nguồn: PC §5 SV2, Bàn giao. **Một phần:** các module đã chạy nhưng còn các mục chưa đạt ở SV2-03/04/06/10/14/15/17/19/22/25/27; không đánh dấu hoàn tất cả module.
- [x] **SV2-32 Dữ liệu mẫu ≥40 sản phẩm, ≥20 khách hàng và ≥30 đánh giá.** Nguồn: PC §5 SV2, Bàn giao; §3. Bằng chứng KT2 trước khi tạo dữ liệu thử: **40 / 23 / 34**. Đánh giá hiện lưu tại `DanhGiaSanPham`, bảng `PhanHoi` nền vẫn tồn tại nhưng rỗng; không coi bảng rỗng này là 34 đánh giá.
- [ ] **SV2-33 Tự viết test case phần mình.** Nguồn: PC §2. **Chưa xác minh:** bộ 61 ca của SV4 có ca liên quan Web, nhưng chưa có bộ test/biên bản nhận trách nhiệm của SV2. Test do người rà soát chạy không thay nghĩa vụ này.
- [ ] **SV2-34 Chụp màn hình và viết HDSD phần tài khoản/catalog/đánh giá.** Nguồn: PC §3, §7 T5. **Thiếu trong bộ bàn giao SV2:** chưa thấy bộ ảnh và HDSD đủ phạm vi; báo cáo mục 8.3 vẫn ghi TODO hướng dẫn khách hàng và đánh giá.
- [ ] **SV2-35 Kiểm thử chéo phần SV4 trước khi gộp develop.** Nguồn: PC §2, §6. **Chưa xác minh:** [checklist SV2 kiểm Desktop][cross2] còn trống, chưa điền người kiểm/ngày/build/kết quả; các log tự động không thay chữ xác nhận của SV2.
- [ ] **SV2-36 Phối hợp SV4 về sản phẩm và SV3 về thêm giỏ.** Nguồn: PC §6. **Một phần:** dùng chung dữ liệu và nút thêm giỏ đã nối, nhưng ảnh Desktop chưa được dùng đúng trên chi tiết Web; chưa có xác nhận phối hợp hoàn tất.
- [ ] **SV2-37 Tham gia họp hằng tuần và cập nhật thẻ công việc có hạn chót.** Nguồn: PC §9. **Chưa xác minh:** phần SV2 trong biên bản còn trống/TODO; chưa có bằng chứng bảng thẻ công việc.
- [ ] **SV2-38 Sửa lỗi phần mình và luyện demo ở tuần cuối.** Nguồn: PC §3, §7 T5. **Một phần:** Git có các cập nhật module SV2; chưa có hồ sơ sửa lỗi/kiểm lại và xác nhận luyện demo đủ để đóng nhiệm vụ tuần cuối; các thiếu sót đang thấy vẫn giữ ô trống.

## SV3 Web khách hàng B và khuyến mãi

### Giỏ hàng, đơn hàng và khuyến mãi

- [ ] **SV3-01 Khảo sát luồng đặt hàng và thiết kế giỏ/thanh toán bằng bản giấy hoặc Figma.** Nguồn: PC §7 T1. **Chưa xác minh:** có màn hình đã triển khai, chưa thấy bản khảo sát/bản thiết kế ban đầu. Không suy ra tài liệu thiết kế từ việc có code.
- [x] **SV3-02 Giỏ hàng thêm, sửa số lượng, xóa và có trang xem giỏ.** Nguồn: PC §5 SV3, §7 T2. Bằng chứng: [GioHangController][cart-controller], [giỏ hàng view][cart-view]; W12–W14 kiểm thêm/cập nhật/xóa qua controller thật.
- [x] **SV3-03 Lưu giỏ bằng session hoặc CSDL.** Nguồn: PC §5 SV3. Bằng chứng: [GioHangSession][cart-session] lưu mã/số lượng vào session; KT5 dùng cùng cookie qua nhiều request, giỏ vẫn còn.
- [x] **SV3-04 Kiểm tra tồn kho trước khi đặt.** Nguồn: PC §5 SV3. Bằng chứng: [GioHangService][cart-service] đọc tồn, transaction tính lại và chặn thiếu tồn; I05, P11/P16/P19. Tồn thực tế do trigger SV1 cập nhật.
- [x] **SV3-05 Đặt hàng có địa chỉ nhận hàng.** Nguồn: PC §5 SV3. Bằng chứng: [checkout view][checkout-view], [DatHangController][checkout-controller], [DonHangOnlineRepository][online-repo]; W15 tạo đơn COD và Online với địa chỉ thử.
- [ ] **SV3-06 Đặt hàng có ghi chú.** Nguồn: PC §5 SV3. **Thiếu:** view, action XacNhan, model và thao tác ghi HoaDon hiện chưa có trường ghi chú.
- [x] **SV3-07 Thanh toán COD.** Nguồn: PC §5 SV3, §7 T3. Bằng chứng: W15 COD và H01 tạo hóa đơn, chi tiết, trạng thái chờ và trừ tồn thành công.
- [x] **SV3-08 Thanh toán online giả lập.** Nguồn: PC §5 SV3, §7 T3. Bằng chứng: W15 Online chuyển đến màn hình giả lập, W16 xác nhận rồi đến trang thành công; `HinhThucTT=Online`. Hiện xác nhận chỉ báo thành công, không có trạng thái thanh toán riêng; file phân công không yêu cầu tích hợp cổng thật.
- [x] **SV3-09 Khuyến mãi theo sản phẩm.** Nguồn: PC §5 SV3, §7 T4. Bằng chứng: [TinhKhuyenMaiService][promotion-service], [KhuyenMaiRepository][promotion-repo], `KM_TheoSP`; P02/P06.
- [x] **SV3-10 Khuyến mãi theo giá trị hóa đơn.** Nguồn: PC §5 SV3. Bằng chứng: `KM_TheoHD`, chọn bậc đạt điều kiện; P03/P05/P06.
- [x] **SV3-11 Áp mã voucher.** Nguồn: PC §5 SV3. Bằng chứng: `KM_TheoVoucher`, action áp/bỏ mã trên giỏ, calculator và ghi lượt dùng trong transaction; P04/P07/P08.
- [x] **SV3-12 Kiểm tra ngày bắt đầu/kết thúc khuyến mãi.** Nguồn: PC §5 SV3. Bằng chứng: repository kiểm ngày/trạng thái; P07, N01 kiểm mã tương lai/hết hạn/tắt/hết lượt.
- [x] **SV3-13 Ghi GiamGia vào ChiTietHoaDon.** Nguồn: PC §5 SV3. Bằng chứng: [GiaoDichBanHang.GhiChiTiet][sale-transaction] ghi phần trăm giảm vào cột `GiamGia`; P02/P17/F01 đối chiếu chi tiết, tổng và file xuất.
- [x] **SV3-14 Ghi HoaDon và ChiTietHoaDon trong một transaction, dùng cơ chế tồn kho của SV1.** Nguồn: PC §5 SV3, §7 T3; §6. Bằng chứng: `TaoDon` dùng Serializable transaction, trigger SV1 trừ tồn; I06 và P12/P13 kiểm rollback header/chi tiết/lượt voucher/tồn. Code dùng trigger, không gọi trực tiếp `sp_KiemTraTonKho` để trừ tồn; phù hợp cơ chế trigger/thủ tục được giao ở PC §5 SV1.
- [x] **SV3-15 Theo dõi đủ các trạng thái đơn hàng và lịch sử mua hàng.** Nguồn: PC §5 SV3, §7 T4. Bằng chứng: [DonHangController][orders-controller], `LayLichSu`, [view đơn hàng][orders-view]; W17, P14/H01 kiểm Web thấy cập nhật Chờ → Xác nhận → Đang giao → Hoàn thành, và nhánh hủy.
- [x] **SV3-16 Khách được hủy khi đơn còn chờ xác nhận.** Nguồn: PC §5 SV3. Bằng chứng: `HuyDon` ràng buộc MaKH, loại Online và trạng thái chờ; P15/H01 kiểm hủy, chặn sai chủ đơn và hoàn tồn đúng một lần.
- [ ] **SV3-17 Gửi email xác nhận đơn hàng bằng Gmail SMTP/app password.** Nguồn: PC §5 SV3, §7 T4. **Một phần:** [EmailService][email-service] có SMTP Gmail, đọc cấu hình và được gọi sau tạo đơn. Chưa có cấu hình hoạt động/minh chứng gửi thật; [hướng dẫn cài đặt][install-doc] mục email còn TODO. Phiên rà soát tắt SMTP và không gửi thư cho người khác; chưa đánh dấu hoàn tất việc gửi email.
- [x] **SV3-18 Desktop quản lý chương trình khuyến mãi theo SP/HĐ/voucher.** Nguồn: PC §3, §5 SV3, §7 T4. Bằng chứng: [FrmKhuyenMai][promotion-form], [KhuyenMaiQuanLyRepository][promotion-management]; A11 kiểm lưu/đọc/tắt/xóa đủ 3 loại qua repository. Lượt mở form chờ hộp thoại và chưa nghiệm thu thao tác tay; KT4 ghi giới hạn này.
- [ ] **SV3-19 Sinh mã voucher từ màn hình Desktop.** Nguồn: PC §5 SV3. **Chưa xác minh:** `SinhVoucher()` có form nhập số mã/tiền tố/mức giảm/lượt và code sinh mã ngẫu nhiên; chưa chạy trọn nút sinh → lưu → dùng mã trên UI. Giữ ô trống cho phần chức năng chưa đủ bằng chứng thực thi.

### Tài liệu, bàn giao và việc chung

- [ ] **SV3-20 Sơ đồ tuần tự luồng Đặt hàng.** Nguồn: PC §5 SV3, §7 T1. **Thiếu:** [báo cáo tổng][report] mục 5.1 còn TODO; sơ đồ tuần tự lập phiếu nhập của SV1 không thay sơ đồ đặt hàng.
- [ ] **SV3-21 Sơ đồ tuần tự luồng Thanh toán.** Nguồn: PC §5 SV3. **Thiếu:** chưa thấy sơ đồ bàn giao cho luồng này; mô tả transaction trong code không thay sơ đồ.
- [ ] **SV3-22 Sơ đồ hoạt động xử lý đơn từ lúc đặt đến giao xong.** Nguồn: PC §5 SV3. **Thiếu:** mục 5.1 còn TODO; activity đăng nhập Desktop không phải activity xử lý đơn.
- [ ] **SV3-23 Tự viết đặc tả use case và test case phần mình.** Nguồn: PC §2, §3. **Chưa xác minh:** có bộ P/H trong kiểm thử tích hợp SV3–SV4 nhưng chưa có bộ đặc tả/use case và test case do SV3 nhận trách nhiệm; không lấy test của SV4/người rà soát làm bằng chứng tự viết của SV3.
- [ ] **SV3-24 Chụp màn hình và viết HDSD phần giỏ/đặt hàng/khuyến mãi.** Nguồn: PC §3, §7 T5. **Một phần:** có ảnh trạng thái đơn Web trong minh chứng đợt 5/6 và ảnh POS khuyến mãi; chưa có bộ ảnh đủ phần giỏ/thanh toán/quản lý CTKM, báo cáo mục 8.3 vẫn TODO HDSD của SV3.
- [x] **SV3-25 Dữ liệu mẫu ≥3 CTKM và ≥20 đơn ở đủ trạng thái.** Nguồn: PC §5 SV3, Bàn giao; §3. Bằng chứng KT2 trước thử: **4 CTKM** gồm 1 theo SP, 1 theo HĐ, 2 voucher; **28 đơn** gồm 4 Chờ xác nhận, 3 Đã xác nhận, 3 Đang giao, 15 Hoàn thành, 3 Đã hủy.
- [ ] **SV3-26 Bàn giao module giỏ – đặt hàng – khuyến mãi hoàn chỉnh.** Nguồn: PC §5 SV3, Bàn giao. **Một phần:** các luồng chính chạy được, nhưng còn thiếu ghi chú đặt hàng, chưa xác minh sinh voucher từ UI và gửi email thật; chưa đánh dấu toàn bộ phần bàn giao.
- [ ] **SV3-27 Kiểm thử chéo tài khoản/catalog/đánh giá của SV2 trước gộp develop.** Nguồn: PC §2, §6. **Chưa xác minh:** chưa có biên bản SV3 kiểm SV2; việc phát hiện lỗi Web ở KT5 là kết quả người rà soát, không phải SV3.
- [ ] **SV3-28 Thống nhất với SV1/SV4 về transaction, tồn kho, trạng thái đơn và khuyến mãi.** Nguồn: PC §6. **Một phần:** code dùng chung đã tích hợp và kiểm thành công; [checklist phối hợp][handoff4] vẫn chờ xác nhận quy tắc của SV3, chưa có bằng chứng nhóm đã chốt đầy đủ.
- [ ] **SV3-29 Tham gia họp hằng tuần và cập nhật thẻ công việc có hạn chót.** Nguồn: PC §9. **Chưa xác minh:** biên bản còn trống/TODO ở thông tin họp và phần xác nhận; chưa có bảng thẻ công việc.
- [ ] **SV3-30 Sửa lỗi phần mình và luyện demo tuần cuối.** Nguồn: PC §3, §7 T5. **Một phần:** có commit tích hợp/sửa module và kịch bản; chưa có xác nhận luyện demo của SV3, những mục thiếu ở trên chưa được đóng.

## SV4 Desktop quản trị

### Sản phẩm, đơn hàng, POS và thống kê

- [x] **SV4-01 Dựng khung WinForms, menu và kết nối form đăng nhập.** Nguồn: PC §7 T1. Bằng chứng: [FrmMain][main-form], [shell][shell], FrmDangNhap; build và smoke mở khung/menu đạt trong KT3/KT4.
- [ ] **SV4-02 Thống nhất quy ước đặt tên.** Nguồn: PC §7 T1. **Chưa xác minh:** code dùng tiền tố Frm/tên tiếng Việt và README có quy ước Repository, nhưng chưa có bằng chứng cả nhóm đã thống nhất quy ước đặt tên do SV4 phụ trách.
- [x] **SV4-03 Quản lý danh mục: thêm/sửa/xóa.** Nguồn: PC §5 SV4, §7 T2. Bằng chứng: [FrmDanhMuc][category-form], DanhMucRepository; I02 kiểm CRUD, tên trùng và chặn xóa danh mục đang có sản phẩm.
- [x] **SV4-04 Quản lý sản phẩm: thêm/sửa/xóa hoặc ngừng bán.** Nguồn: PC §5 SV4, §7 T2. Bằng chứng: [FrmSanPham][products-form], [FrmSanPhamSua][product-edit], [SanPhamRepository][products-repo]; có thêm/sửa/ngừng bán, giữ lịch sử thay xóa cứng; I01/I03/P11.
- [x] **SV4-05 Tải và lưu ảnh sản phẩm.** Nguồn: PC §5 SV4. Bằng chứng: nút chọn ảnh/lưu, [AnhSanPham][product-image]; U13 kiểm giải mã, lưu JPEG và đọc lại, từ chối ảnh hỏng/quá lớn. Ảnh Web dùng sai nguồn được ghi ở SV2-19.
- [x] **SV4-06 Xem tồn kho và cảnh báo sản phẩm sắp hết.** Nguồn: PC §5 SV4. Bằng chứng: lưới có SoLuongTon/CanhBao, ngưỡng sắp hết 1–5; query sản phẩm và màu cảnh báo trên lưới; I03 kiểm không sửa tồn trực tiếp khi sửa sản phẩm.
- [x] **SV4-07 Tìm kiếm, lọc và sắp xếp trên DataGridView.** Nguồn: PC §5 SV4. Bằng chứng: form sản phẩm lọc từ khóa/danh mục/thương hiệu/trạng thái/tồn, bind DataTable hỗ trợ sắp xếp cột; smoke form có dữ liệu. Chưa nhận đã kiểm mọi tổ hợp lọc bằng thao tác tay.
- [x] **SV4-08 Danh sách đơn Web và xem chi tiết.** Nguồn: PC §5 SV4, §7 T3. Bằng chứng: [FrmDonHang][orders-form], [HoaDonRepository][invoice-repo]; I11/I15 và P14/H01 kiểm đơn Online, dòng chi tiết và trạng thái cùng CSDL.
- [x] **SV4-09 Xác nhận và cập nhật trạng thái giao hàng.** Nguồn: PC §5 SV4. Bằng chứng: nút xác nhận/giao/hoàn thành; `ChuyenTrangThai`; U03, I08, P14/H01 kiểm đúng các bước trạng thái.
- [x] **SV4-10 Hủy đơn kèm lý do.** Nguồn: PC §5 SV4. Bằng chứng: form nhập lý do, repository lưu `LyDoHuy`; I07/I13/P15 kiểm bắt buộc lý do, hủy đúng một lần, không mở lại đơn hủy.
- [x] **SV4-11 POS chọn sản phẩm và nhập số lượng.** Nguồn: PC §5 SV4, §7 T4. Bằng chứng: [FrmPOS][pos-form], thao tác thêm/bỏ giỏ trong GUI render và P18; I04/I05 kiểm số lượng/giỏ khi thanh toán.
- [x] **SV4-12 POS áp khuyến mãi và tính tiền.** Nguồn: PC §5 SV4. Bằng chứng: dùng chung calculator với Web; P01–P11, N02 và P18 kiểm tiền hàng, giảm, tổng, tiền khách đưa và tiền thừa.
- [x] **SV4-13 POS in hóa đơn hoặc xuất PDF.** Nguồn: PC §5 SV4. Bằng chứng: có xuất PDF qua [XuatTaiLieu][exporter]; U12/I15/P17 tạo và đọc lại hóa đơn từ dữ liệu đã lưu; đối chiếu độc lập KT6. Không yêu cầu thêm in giấy vì file cho phép PDF.
- [x] **SV4-14 POS ghi cùng bảng HoaDon với LoaiBan = Tại quầy.** Nguồn: PC §5 SV4. Bằng chứng: `ThanhToanPOS` trong invoice repository; I04 kiểm loại bán, nhân viên, trạng thái hoàn thành, tổng và tồn giảm.
- [x] **SV4-15 Báo cáo doanh thu ngày/tháng/năm.** Nguồn: PC §5 SV4. Bằng chứng: [BaoCaoService][report-service], [FrmThongKe][statistics-form]; U08, N04/F01 và KT6 đối chiếu SQL/bảng/Excel/PDF theo bộ lọc.
- [x] **SV4-16 Thống kê sản phẩm bán chạy.** Nguồn: PC §5 SV4. Bằng chứng: bảng “Bán chạy” cộng số lượng trên đơn hoàn thành và sắp giảm dần; U08 và F01.
- [x] **SV4-17 Doanh thu theo danh mục.** Nguồn: PC §5 SV4. Bằng chứng: bảng “Theo danh mục” phân bổ giảm giá từ tổng hóa đơn; U06–U08/P17/F01 và KT6 đối chiếu tổng.
- [x] **SV4-18 Số đơn theo trạng thái.** Nguồn: PC §5 SV4. Bằng chứng: bảng “Trạng thái đơn” đếm đủ 5 trạng thái, kể cả đơn hủy; U08/U09.
- [ ] **SV4-19 Vẽ biểu đồ bằng Chart control hoặc LiveCharts như file phân công nêu.** Nguồn: PC §5 SV4. **Một phần:** đã có biểu đồ cột và form thật hiển thị được, nhưng dùng [BieuDoCot tự vẽ GDI][chart], chưa dùng Chart control/LiveCharts. Giữ ô trống khi đối chiếu đúng lựa chọn công nghệ trong file; không diễn giải thành “chưa có biểu đồ”.
- [x] **SV4-20 Xuất báo cáo Excel/PDF.** Nguồn: PC §5 SV4, §7 T4. Bằng chứng: exporter dùng ClosedXML và MigraDoc/PDFsharp; U11/U12, N04/F01; KT6 đọc độc lập và đối chiếu 12 tổ hợp bộ lọc.
- [x] **SV4-21 Dữ liệu demo cho biểu đồ có số liệu nhiều tháng.** Nguồn: PC §5 SV4, Bàn giao; §3. Bằng chứng: dữ liệu cài mới có hóa đơn **5 tháng 06–10/2026**; [05_SV4_Demo.sql][demo4] trên DB thử/demo riêng tạo 12 tháng và I12/N05 kiểm được. Không tự nâng yêu cầu “nhiều tháng” thành bắt buộc 6–12 tháng của nguồn khác.
- [ ] **SV4-22 Bàn giao Desktop hoàn chỉnh đủ 4 nhóm chức năng theo phân công.** Nguồn: PC §5 SV4, Bàn giao. **Một phần:** sản phẩm, đơn hàng, POS và báo cáo đều đã chạy kiểm tra; còn khác lựa chọn công nghệ biểu đồ ở SV4-19 và chưa có xác nhận kiểm thử chéo bàn giao từ SV2.

### Tài liệu và công việc chung

- [x] **SV4-23 Sơ đồ lớp Core và tầng nghiệp vụ.** Nguồn: PC §5 SV4, §7 T1. Bằng chứng: [class diagram][class-diagram] có thực thể, repository, service và dependency; có PNG đưa vào báo cáo tổng.
- [x] **SV4-24 Kế hoạch kiểm thử và bảng ≥25 ca cho chức năng chính của cả hai ứng dụng.** Nguồn: PC §5 SV4, §7 T5. Bằng chứng: [kế hoạch kiểm thử][test-plan4] có **61 ca**, bao gồm Desktop và Web tài khoản/catalog/giỏ/đặt hàng/email/đánh giá; báo cáo tổng ghép thêm 36 ca SV1 thành 97 ca. Có kế hoạch/test case không có nghĩa mọi ca đều đạt.
- [x] **SV4-25 Viết phần kết quả, ưu/nhược điểm và hướng phát triển của SV4.** Nguồn: PC §5 SV4. Bằng chứng: [tổng kết SV4][conclusion4] và [UI-SV4][ui-doc4] trình bày chức năng làm được, lựa chọn UI/khả năng sử dụng, giới hạn và hướng phát triển.
- [ ] **SV4-26 Hoàn thiện chương tổng kết cho toàn hệ thống.** Nguồn: PC §3, §5 SV4, §7 T5. **Một phần:** chương 9 của báo cáo tổng hiện chủ yếu là SV4, vẫn TODO bổ sung kết quả SV1/SV2/SV3; chưa bao quát cả nhóm.
- [x] **SV4-27 Viết đặc tả use case và test case phần mình.** Nguồn: PC §2, §3. Bằng chứng: [đặc tả SV4][spec4] UC4.1–UC4.5 và kế hoạch/test case liên kết.
- [x] **SV4-28 Chụp màn hình giao diện phần mình.** Nguồn: PC §2, §3. Bằng chứng: [bộ ảnh SV4][images4] có danh mục, sản phẩm, sửa sản phẩm, đơn hàng, POS và thống kê. Các ảnh smoke/render có giới hạn được nêu trong kết quả kiểm thử.
- [x] **SV4-29 Viết HDSD phần mình và dựng kịch bản demo.** Nguồn: PC §3, §7 T5. Bằng chứng: [HDSD SV4][guide4] có thao tác 4 nhóm chức năng và kịch bản 5–7 phút. Không dùng kịch bản 7 nhóm yêu cầu bổ sung để thêm việc vào phần phân công gốc; góp ý trong ghi âm được theo dõi riêng tại các mục `CO-*`.
- [ ] **SV4-30 Chạy bộ test case và lập báo cáo lỗi.** Nguồn: PC §7 T5. **Một phần:** [kết quả kiểm thử][test-results4] có các đợt chạy, kết quả và lỗi/giới hạn; vẫn có ca chờ, chưa đủ bằng chứng đã chạy trọn bộ 61 ca. Lượt 86/86 của người rà soát không đóng thay các ca thủ công/sinh viên chưa xác nhận.
- [ ] **SV4-31 Kiểm thử chéo phần SV1 trước gộp develop.** Nguồn: PC §2, §6. **Chưa xác minh:** [đặc tả SV1][spec1] còn ghi chờ SV4 kiểm chéo; chưa có người kiểm/ngày/kết quả xác nhận đủ phạm vi.
- [ ] **SV4-32 Thống nhất với SV1/SV3 về tồn kho, trạng thái đơn và khuyến mãi.** Nguồn: PC §6. **Một phần:** có tài liệu tích hợp và code/test dùng chung; [checklist phối hợp][handoff4] còn chờ xác nhận của thành viên. Kết quả tích hợp kỹ thuật không chứng minh cuộc thống nhất đã hoàn tất.
- [ ] **SV4-33 Duyệt PR thay SV1 khi SV1 bận.** Nguồn: PC §2. **Chưa xác minh:** chưa có bằng chứng về trường hợp cần duyệt thay hoặc lượt SV4 đã duyệt thay; không suy “chưa làm” khi chưa biết nhiệm vụ có phát sinh.
- [ ] **SV4-34 Tham gia họp hằng tuần và cập nhật thẻ công việc có hạn chót.** Nguồn: PC §9. **Chưa xác minh:** biên bản còn thiếu xác nhận người tham gia/nội dung họp thật; chưa có bảng thẻ công việc.
- [ ] **SV4-35 Luyện demo cùng nhóm.** Nguồn: PC §3. **Chưa xác minh:** HDSD có kịch bản, nhưng chưa có bằng chứng SV4 và các thành viên đã thực hành đầy đủ.

## Quy tắc chung cần đối chiếu cho cả SV1 SV2 SV3 SV4

Các mục dưới áp dụng cho **cả bốn SV**, được ghi chung một lần để không lặp bốn checklist giống nhau.

- [x] **CHUNG-01 Không đưa bin/, obj/, packages/ vào bản mã nguồn hiện tại; commit có thông điệp mô tả công việc.** Nguồn: PC §9. Bằng chứng: `.gitignore`, `git ls-files` không có file trong ba nhóm thư mục trên; lịch sử có thông điệp feat/fix/docs và các thay đổi chức năng. Chỉ xác nhận bản hiện tại và lịch sử đã kiểm, không xác nhận kích thước mọi commit đều nhỏ.
- [ ] **CHUNG-02 Commit nhỏ và mọi sửa đổi qua PR, có kiểm thử chéo trước gộp.** Nguồn: PC §2, §9. **Một phần:** có PR/nhánh tính năng, nhưng còn commit tổng hợp lớn, merge nhánh trực tiếp và thiếu xác nhận kiểm chéo. Đánh giá nhiệm vụ kiểm chéo cụ thể nằm trong từng SV ở trên.
- [ ] **CHUNG-03 Mỗi máy quản lý cấu hình kết nối riêng, không commit mật khẩu CSDL hoặc đè cấu hình máy khác.** Nguồn: PC §9. **Một phần:** có App.config/appsettings.json, biến môi trường và mẫu kết nối Trusted Connection; chưa có bằng chứng cấu hình từng máy và toàn bộ lịch sử không ghi đè lẫn nhau. Không công bố thông tin bí mật khi rà soát.
- [ ] **CHUNG-04 Khi trễ quá 2 ngày phải báo nhóm để chia lại việc.** Nguồn: PC §9. **Chưa xác minh:** chưa có bảng việc/hạn chót và thông báo để xác định trường hợp phát sinh, việc báo trễ và phân công lại.
- [ ] **CHUNG-05 Tuần 5 chỉ sửa lỗi/hoàn thiện tài liệu; phần chưa xong cuối tuần 4 được cắt và ghi hướng phát triển.** Nguồn: PC §7, lưu ý cuối kế hoạch. **Một phần:** Git có commit `feat` ngày 05/11 và 09/11 trong tuần 5, báo cáo vẫn còn yêu cầu chưa hoàn chỉnh; chưa có hồ sơ chốt phạm vi/cắt chức năng theo mốc. Chỉ ghi nhận tuân thủ kế hoạch gốc; yêu cầu bổ sung của các đợt sau không được tính ngược vào phân công gốc. Góp ý ngày 12/11 được theo dõi riêng bên dưới.

## Góp ý bổ sung của cô ngày 12/11

Nguồn: [ghi âm][ghi-am-co] và [phụ đề tự động][phu-de-co]. Bổ sung checklist ngày **13/11/2026**. Các tiêu chí dưới đây cụ thể hóa góp ý thành việc có thể kiểm tra; việc sửa tài liệu không xác nhận đã sửa phần mềm. Trạng thái hiện tại dựa trên đọc mã nguồn và bằng chứng cũ được dẫn lại, **chưa chạy nghiệm thu mới**. Chỉ đánh dấu `[x]` khi đủ toàn bộ tiêu chí và đã ghi người kiểm, ngày, kết quả cùng minh chứng. Chức năng đã có được kiểm tra tiếp, không mặc định phải làm lại.

| Phần việc | Đã đánh dấu | Còn để trống | Tổng số mục |
|---|---:|---:|---:|
| Góp ý bổ sung `CO-*` | 0 | 7 | 7 |

### CO-01 Mua hàng không cần đăng nhập

- [ ] **CO-01 Cho khách đặt hàng khi chưa đăng nhập và liên kết khách bằng SĐT.**
  - **Mốc ghi âm:** 01:16–03:09; 04:48–04:52.
  - **Phụ trách dự kiến:** SV3 chính; SV1 hỗ trợ dữ liệu, SV2 hỗ trợ giao diện.
  - **Trạng thái / bằng chứng hiện có — Thiếu:** [DatHangController][checkout-controller] vẫn có `[Authorize]` và lấy `MaKH` từ phiên đăng nhập; SV3-05 mới chứng minh đặt hàng có địa chỉ trong luồng đăng nhập, chưa đáp ứng đặt hàng cho khách chưa đăng nhập.
  - **Tiêu chí nghiệm thu:** khách chưa đăng nhập nhập họ tên, SĐT, địa chỉ và đặt hàng được; lưu được khách và liên kết đơn tương ứng; thử khách mới và khách quay lại dùng cùng SĐT, không tạo khách trùng ngoài quy tắc đã chốt. Nhận diện theo SĐT không tự cấp quyền xem lịch sử hoặc thông tin riêng của người khác.
  - **Ghi nhận nghiệm thu:** Người kiểm: chưa ghi; ngày: chưa ghi; kết quả/minh chứng: chưa có.

### CO-02 Kiểm soát voucher

- [ ] **CO-02 Phân biệt voucher dùng một lần với chương trình áp dụng chung và kiểm soát đủ điều kiện sử dụng.**
  - **Mốc ghi âm:** 03:32–07:25, trọng tâm 06:02–07:25.
  - **Phụ trách dự kiến:** SV3 chính; SV4 kiểm tra tích hợp POS.
  - **Trạng thái / bằng chứng hiện có — Một phần:** SV3-11/12 có áp mã, ngày hiệu lực và lượt dùng; [KhuyenMaiRepository][promotion-repo] kiểm `SoLuong/DaDung` và tăng lượt trong transaction. Chưa đủ bằng chứng giới hạn theo từng khách và phân biệt đầy đủ các chính sách cô nêu.
  - **Tiêu chí nghiệm thu:** ghi rõ chính sách của từng loại mã về tổng lượt, lượt mỗi khách, thời hạn và điều kiện hóa đơn; kiểm tra mã một lần không dùng lại được sau khi mua thành công, mã áp dụng chung được lặp theo chính sách, mã hết hạn/tắt/hết lượt bị từ chối. Áp/bỏ mã cập nhật đúng tổng tiền trên Web/POS; hai giao dịch tranh lượt cuối không vượt giới hạn và giao dịch thất bại không mất lượt. Các ngưỡng tiền/tỷ lệ trong lời cô là ví dụ, không cố định thành điều kiện cho mọi mã.
  - **Ghi nhận nghiệm thu:** Người kiểm: chưa ghi; ngày: chưa ghi; kết quả/minh chứng: chưa có.

### CO-03 Main–sub đơn hàng

- [ ] **CO-03 Chọn dòng đơn hàng để xem chi tiết ngay trong cùng màn hình.**
  - **Mốc ghi âm:** 07:32–08:15; 09:29–09:35.
  - **Phụ trách dự kiến:** SV4 Desktop; SV3 Web.
  - **Trạng thái / bằng chứng hiện có — Một phần:** [FrmDonHang][orders-form] đã có lưới chính/phụ và sự kiện `CurrentCellChanged` nạp chi tiết; SV4-08 có bằng chứng đọc chi tiết. Chưa có nghiệm thu đầy đủ thao tác chọn/đổi dòng của cả Desktop và [Web][orders-view] theo mục bổ sung này.
  - **Tiêu chí nghiệm thu:** chọn một dòng bất kỳ thì hiện đúng chi tiết của đơn đó bên cạnh hoặc bên dưới; đổi dòng cập nhật mã đơn, thông tin và dòng hàng tương ứng; không giữ chi tiết đơn cũ khi danh sách rỗng hoặc đổi lựa chọn nhanh. Kiểm tra trên Desktop và trang đơn hàng Web của khách.
  - **Ghi nhận nghiệm thu:** Người kiểm: chưa ghi; ngày: chưa ghi; kết quả/minh chứng: chưa có.

### CO-04 Quan hệ khách hàng–hóa đơn

- [ ] **CO-04 Đối chiếu bảng, khóa ngoại, ERD và dữ liệu ghép để hiển thị hóa đơn.**
  - **Mốc ghi âm:** 08:27–09:35.
  - **Phụ trách dự kiến:** SV1 chính; SV3/SV4 phối hợp phần truy vấn và hiển thị.
  - **Trạng thái / bằng chứng hiện có — Một phần:** [schema][schema] đã có `HoaDon.MaKH` và khóa ngoại tới `KhachHang`; SV1-21 có ERD/lược đồ. Không kết luận hệ thống hiện tại đang lưu toàn bộ khách hàng trong bảng hóa đơn; cần đối chiếu thêm tài liệu và luồng ghi/đọc thực tế.
  - **Tiêu chí nghiệm thu:** schema, script tổng, ERD và lược đồ quan hệ thống nhất về liên kết khách hàng–hóa đơn; chỉ ra thông tin thuộc bảng nào và tên/SĐT được ghép qua truy vấn hoặc view để hiển thị. Giải thích riêng `DiaChiGiao` là địa chỉ của đơn, không tự xóa dữ liệu giao hàng chỉ vì cô phản biện việc sao chép toàn bộ hồ sơ khách. Kiểm tra lưu/đọc lại đơn Online và đơn POS khách vãng lai.
  - **Ghi nhận nghiệm thu:** Người kiểm: chưa ghi; ngày: chưa ghi; kết quả/minh chứng: chưa có.

### CO-05 Tồn kho sau nhập và bán

- [ ] **CO-05 Kiểm tra tồn kho xuyên suốt nhập hàng, bán Web/POS và mở lại sản phẩm.**
  - **Mốc ghi âm:** 09:53–11:13; 12:34–12:42.
  - **Phụ trách dự kiến:** SV1 cơ chế dữ liệu; SV3/SV4 luồng bán và giao diện.
  - **Trạng thái / bằng chứng hiện có — Một phần:** SV1-06/07/08 và KT3 có kiểm tra tăng/giảm tồn, rollback và chặn quá tồn; chưa có ghi nhận demo xuyên suốt các thao tác giao diện theo mục này. Đoạn ghi âm có lời rời rạc nên không suy ra thêm lỗi tồn kho cụ thể.
  - **Tiêu chí nghiệm thu:** ghi tồn ban đầu, nhập thêm và đối chiếu tăng đúng số lượng; bán Web rồi POS và đối chiếu giảm đúng số lượng; tải lại hoặc mở lại sản phẩm thấy tồn mới. Thử bán quá tồn bị chặn; thao tác thất bại hoặc gửi lại cùng giao dịch không làm cập nhật tồn hai lần. Lưu mã sản phẩm, mã phiếu/đơn và số lượng trước/sau làm minh chứng.
  - **Ghi nhận nghiệm thu:** Người kiểm: chưa ghi; ngày: chưa ghi; kết quả/minh chứng: chưa có.

### CO-06 Thống kê theo yêu cầu demo

- [ ] **CO-06 Mở thống kê thấy dữ liệu năm hiện tại, thu–chi, sản phẩm bán chạy và biểu đồ từng sản phẩm theo quý.**
  - **Mốc ghi âm:** 13:33–14:35.
  - **Phụ trách dự kiến:** SV4 chính; SV1 hỗ trợ truy vấn.
  - **Trạng thái / bằng chứng hiện có — Một phần:** [FrmThongKe][statistics-form] đã gọi nạp khi mở, mặc định từ đầu năm hiện tại đến ngày hiện tại và có chọn năm; SV4-15/16 có doanh thu và bảng bán chạy. Các thẻ đầu màn hình hiện là doanh thu, số đơn, số sản phẩm; thu–chi nằm ở tab riêng. Chưa có bằng chứng đủ cho luồng chọn từng sản phẩm xem biểu đồ quý.
  - **Tiêu chí nghiệm thu:** mở màn hình tự hiện số liệu năm hiện tại và thể hiện rõ tổng thu/chi, danh sách sản phẩm bán chạy giảm dần; chọn sản phẩm xem doanh số theo từng quý của năm đang chọn; đổi năm cập nhật đúng số liệu, bảng và biểu đồ. Đối chiếu với dữ liệu mẫu đã biết; năm/sản phẩm không có giao dịch hiển thị rõ không có dữ liệu, không giữ số liệu lựa chọn cũ.
  - **Ghi nhận nghiệm thu:** Người kiểm: chưa ghi; ngày: chưa ghi; kết quả/minh chứng: chưa có.

### CO-07 Tự kiểm tra và luyện demo

- [ ] **CO-07 Mỗi thành viên tự thử, nắm rõ chức năng phụ trách và luyện demo cùng nhóm.**
  - **Mốc ghi âm:** 01:49–02:07.
  - **Phụ trách dự kiến:** cả SV1, SV2, SV3, SV4; SV1 tổng hợp kết quả.
  - **Trạng thái / bằng chứng hiện có — Chưa xác minh:** các mục SV1-39, SV2/SV3 về luyện demo và SV4-35 chưa đủ xác nhận thực hành; kịch bản hoặc log công cụ không thay cho việc thành viên tự thao tác.
  - **Tiêu chí nghiệm thu:** từng SV chạy chức năng mình phụ trách, nêu dữ liệu đầu vào và kết quả mong đợi, trình bày được luồng thao tác; nhóm chạy liền mạch đặt hàng → xử lý đơn → kiểm tồn → thống kê. Ghi người thực hiện/người kiểm, ngày, kết quả và lỗi còn lại; các lỗi chưa xử lý phải có người phụ trách và lần kiểm lại.
  - **Ghi nhận nghiệm thu:** Người kiểm: chưa ghi; ngày: chưa ghi; kết quả/minh chứng: chưa có.

**Các chỗ cần xác minh thêm trong ghi âm:** 11:24–12:31 (thao tác reset/nhập liệu và hiển thị màu) và 12:51–13:03 (thuật ngữ nhận dạng chưa rõ). Chưa chuyển các câu này thành lỗi hoặc nhiệm vụ sửa giao diện cụ thể; cần nghe đối chiếu và xác định màn hình/thao tác trước khi bổ sung checklist.

## Bằng chứng kiểm tra trong phiên rà soát

Phần này mô tả cách đối chiếu, **không phải công việc mới giao cho sinh viên**.

### KT1 Build

`dotnet build BadmintonShop.sln --nologo -v minimal` thành công: **0 lỗi, 1 cảnh báo CS8602 tại FrmKhuyenMai.cs dòng 278**. Lần `--no-restore` đầu báo thiếu targeting pack; build có restore sau đó thành công, không sửa project/mã nguồn.

### KT2 Cài mới và dữ liệu mẫu

Tạo riêng CSDL `BadmintonShop_Checklist_20261113_dc721a16`, chạy phần schema/dữ liệu của `database/BadmintonShop.sql`, đổi `USE` sang tên này và bỏ đoạn xóa/tạo CSDL `BadmintonShop` có sẵn. Không đổi bảng/cột/dữ liệu mẫu trong script. Kết quả **trước khi thêm fixture kiểm tra**:

| Nội dung | Kết quả |
|---|---:|
| Bảng / trigger / stored procedure | 16 / 3 / 1 |
| Sản phẩm / khách hàng / đánh giá | 40 / 23 / 34 |
| CTKM / hóa đơn | 4 / 28 |
| Nhân viên / tài khoản / NCC | 5 / 5 / 6 |
| Phiếu nhập / chi tiết phiếu nhập | 11 / 42 |
| Đơn Chờ / Xác nhận / Đang giao / Hoàn thành / Hủy | 4 / 3 / 3 / 15 / 3 |
| Tháng có hóa đơn | 5 tháng, 06–10/2026 |
| Sản phẩm tồn âm | 0 |

Không lấy các dòng do bộ kiểm thử tạo thêm để đạt ngưỡng bàn giao. Bảng `DanhGiaSanPham` có 34 dòng; `PhanHoi` nền có 0 dòng. Các test cài mới/tích hợp khác cũng tạo CSDL riêng có tiền tố `BadmintonShop_SV4_Test_*`.

### KT3 Bộ kiểm thử sẵn có

Chạy `dotnet run --no-build --project tests/BadmintonShop.SV4.Checks -- --install-checks --integration --server '.\SQLEXPRESS'`: **86 đạt, 0 lỗi**. Gồm U, R, C, A, X, J, I, P, H01, smoke/render Desktop, N và F01. Dùng mã test hiện có, không sửa test trong repository.

Kết quả: [results.txt của lượt 13/11][run-results]. Các lượt mở form chạy tự động có thể bỏ qua bước đăng nhập bằng fixture và không tương đương sinh viên kiểm thử chéo. Giới hạn kích thước cửa sổ 1920×1080 và scale mô phỏng trong log không phải yêu cầu mới của checklist.

### KT4 Kiểm tra bổ sung SV1 và phần dùng chung

Harness tạm ngoài repository chạy **18 kiểm tra đạt**: A01–A11 về tài khoản Desktop/BCrypt/NCC/nhân viên/phiếu nhập/thủ tục tồn/duyệt đánh giá/lưu 3 loại CTKM, và 7 lượt mở form đăng nhập, NCC, nhân viên, phiếu nhập, danh sách phiếu, sao lưu, duyệt đánh giá.

Lượt thử đầu có thêm một kiểm tra sao lưu bị dừng tại `File.Exists` trả false với thư mục Backup của dịch vụ SQL; chưa gọi phục hồi trong lượt đó. Kiểm tra B01/B02 riêng xác nhận file sao lưu hợp lệ từ SQL Server, sau đó **phục hồi thật trên CSDL thử thành công** và loại bỏ dòng đánh dấu đã chèn sau sao lưu. Chưa xác nhận đường chọn file/bấm nút phục hồi qua UI với quyền truy cập thư mục mặc định trên máy này.

Khi smoke `FrmKhuyenMai`, tiến trình chờ hộp thoại; Computer Use không trả về cửa sổ của tiến trình test để thao tác. Đã dừng tiến trình thử đó; không kết luận đây là lỗi chức năng, cũng không nhận đã nghiệm thu UI sinh voucher. Các kiểm tra repository quản lý CTKM vẫn đạt. Không thay đổi code để bỏ qua hộp thoại.

Log tạm: [Core/UI][core-results] và [sao lưu/phục hồi][backup-results]. Các công cụ kiểm tra trong thư mục tạm không được đưa vào mã nguồn bàn giao.

### KT5 Web chạy startup thật

Chạy Web bằng `dotnet run --no-build --no-launch-profile --project BadmintonShop.Web --urls http://127.0.0.1:5189`, môi trường Development, cấu hình riêng cho tiến trình trỏ CSDL KT2 và `Email__Bat=false`. HTTP dùng cookie và anti-forgery thật, không dùng endpoint đăng nhập fixture của bộ H01.

Sau khi sửa hai giả định của harness về URL đổi mật khẩu và cách hiển thị số lượng giỏ, kết quả cuối: **19 kiểm tra đạt, 4 tiêu chí chưa đạt**. Các mục đạt gồm đăng ký hợp lệ, trùng email, đăng nhập/đăng xuất, hồ sơ, đổi/reset mật khẩu demo, catalog URL/phân trang, giỏ, COD, online giả lập và lịch sử. Đổi mật khẩu được xác nhận bằng đăng nhập từ phiên mới; số lượng giỏ được xác nhận bằng số hiển thị thực tế.

Bốn tiêu chí chưa đạt:

1. Khách mới chưa mua vẫn gửi đánh giá được.
2. Đăng ký vẫn nhận mật khẩu `abc123`.
3. Đăng ký vẫn nhận SĐT trùng của tài khoản khác.
4. Catalog chưa có điều khiển/logic lọc riêng thương hiệu và khoảng giá; đối chiếu thêm chữ ký action và view để tránh kết luận chỉ từ một URL.

Log: [web-http-results.txt][web-results]. Tài khoản, đánh giá và đơn thử chỉ được tạo trong CSDL riêng. Không gửi email thật. Tiến trình Web thử đã dừng sau kiểm tra.

### KT6 Đối chiếu file xuất và tài liệu

Đọc độc lập XLSX/PDF bằng openpyxl/pypdf từ thư mục kết quả KT3: tổng doanh thu fixture **13.172.279 đồng** khớp SQL, các bảng ngày/tháng/năm/danh mục và file xuất; **12 tổ hợp bộ lọc** đều khớp; PDF hóa đơn giữ đúng tiền gốc/giảm sản phẩm/giảm hóa đơn/tổng đã lưu. Đây là số của CSDL kiểm thử, không phải doanh thu cửa hàng.

Script kiểm độc lập cũ cố định 6 sheet/trang. Bản chạy tạm chỉ nới điều kiện số lượng để nhận thêm báo cáo hiện có, giữ các đối chiếu tiền và bộ lọc; không sửa script trong repository và không lấy báo cáo bổ sung làm yêu cầu checklist.

Đã mở nội dung Markdown và trích văn bản Word của báo cáo tổng, kiến trúc, mô tả CSDL, đặc tả SV1, hướng dẫn cài đặt và HDSD SV1 để kiểm nội dung/TODO. Đã đếm **36 test case SV1, 61 test case SV4** từ các dòng ca kiểm thử thực tế. Chưa có bằng chứng bản in, xác nhận họp, kiểm chéo hoặc luyện demo thay cho các thành viên. Log trong Temp chỉ phục vụ truy vết trên máy hiện tại; kết luận quan trọng đã ghi trực tiếp cạnh từng ô.

[phan-cong]: ../../../CNPM_01_Phan_cong_cong_viec.docx
[ghi-am-co]: <../../../16.10, 12 thg 11__1.m4a>
[phu-de-co]: ../../../ban-chep-loi.txt
[gitignore]: ../../.gitignore
[solution]: ../../BadmintonShop.sln
[schema]: ../../database/01_CreateDatabase.sql
[sql-tong]: ../../database/BadmintonShop.sql
[seed]: ../../database/03_SampleData.sql
[triggers]: ../../database/02_Triggers.sql
[core-project]: ../../BadmintonShop.Core/BadmintonShop.Core.csproj
[dbhelper]: ../../BadmintonShop.Core/DataAccess/DbHelper.cs
[web-program]: ../../BadmintonShop.Web/Program.cs
[desktop-program]: ../../BadmintonShop.Desktop/Program.cs
[web-project]: ../../BadmintonShop.Web/BadmintonShop.Web.csproj
[kh-repo]: ../../BadmintonShop.Core/DataAccess/KhachHangRepository.cs
[auth-web]: ../../BadmintonShop.Web/Infrastructure/XacThucKhachHang.cs
[tk-repo]: ../../BadmintonShop.Core/DataAccess/TaiKhoanRepository.cs
[phanquyen]: ../../BadmintonShop.Core/Services/PhanQuyen.cs
[password]: ../../BadmintonShop.Core/Services/MatKhauHelper.cs
[login-desktop]: ../../BadmintonShop.Desktop/FrmDangNhap.cs
[ncc-form]: ../../BadmintonShop.Desktop/FrmNhaCungCap.cs
[pn-form]: ../../BadmintonShop.Desktop/FrmPhieuNhap.cs
[pn-repo]: ../../BadmintonShop.Core/DataAccess/PhieuNhapRepository.cs
[nv-form]: ../../BadmintonShop.Desktop/FrmNhanVien.cs
[backup-form]: ../../BadmintonShop.Desktop/FrmSaoLuu.cs
[backup-repo]: ../../BadmintonShop.Core/DataAccess/SaoLuuRepository.cs
[architecture]: chuong-kien-truc.md
[architecture-diagram]: ../diagrams/01-kien-truc.md
[erd]: ../diagrams/02-erd.md
[relational]: ../diagrams/03-luoc-do-quan-he.md
[db-doc]: mo-ta-csdl.md
[install-doc]: huong-dan-cai-dat.md
[report]: bao-cao-tong.md
[spec1]: dac-ta-sv1.md
[guide1]: huong-dan-su-dung-sv1.md
[meetings]: ../bien-ban-hop/
[layout]: ../../BadmintonShop.Web/Views/Shared/_Layout.cshtml
[account-controller]: ../../BadmintonShop.Web/Controllers/TaiKhoanController.cs
[registration-model]: ../../BadmintonShop.Web/Models/DangKyViewModel.cs
[home-view]: ../../BadmintonShop.Web/Views/Home/Index.cshtml
[product-controller]: ../../BadmintonShop.Web/Controllers/SanPhamController.cs
[catalog-view]: ../../BadmintonShop.Web/Views/SanPham/Index.cshtml
[detail-view]: ../../BadmintonShop.Web/Views/SanPham/ChiTiet.cshtml
[review-repo]: ../../BadmintonShop.Core/DataAccess/DanhGiaRepository.cs
[review-form]: ../../BadmintonShop.Desktop/FrmQuanLyDanhGia.cs
[cross2]: checklist-sv2-desktop.md
[cart-controller]: ../../BadmintonShop.Web/Controllers/GioHangController.cs
[cart-view]: ../../BadmintonShop.Web/Views/GioHang/Index.cshtml
[cart-session]: ../../BadmintonShop.Web/Infrastructure/GioHangSession.cs
[cart-service]: ../../BadmintonShop.Core/Services/GioHangService.cs
[checkout-view]: ../../BadmintonShop.Web/Views/DatHang/Index.cshtml
[checkout-controller]: ../../BadmintonShop.Web/Controllers/DatHangController.cs
[online-repo]: ../../BadmintonShop.Core/DataAccess/DonHangOnlineRepository.cs
[promotion-service]: ../../BadmintonShop.Core/Services/TinhKhuyenMaiService.cs
[promotion-repo]: ../../BadmintonShop.Core/DataAccess/KhuyenMaiRepository.cs
[sale-transaction]: ../../BadmintonShop.Core/DataAccess/GiaoDichBanHang.cs
[orders-controller]: ../../BadmintonShop.Web/Controllers/DonHangController.cs
[orders-view]: ../../BadmintonShop.Web/Views/DonHang/Index.cshtml
[email-service]: ../../BadmintonShop.Core/Services/EmailService.cs
[promotion-form]: ../../BadmintonShop.Desktop/FrmKhuyenMai.cs
[promotion-management]: ../../BadmintonShop.Core/DataAccess/KhuyenMaiQuanLyRepository.cs
[handoff4]: checklist-ban-giao-sv4.md
[main-form]: ../../BadmintonShop.Desktop/FrmMain.cs
[shell]: ../../BadmintonShop.Desktop/FrmMain.Shell.cs
[category-form]: ../../BadmintonShop.Desktop/FrmDanhMuc.cs
[products-form]: ../../BadmintonShop.Desktop/FrmSanPham.cs
[product-edit]: ../../BadmintonShop.Desktop/FrmSanPhamSua.cs
[products-repo]: ../../BadmintonShop.Core/DataAccess/SanPhamRepository.cs
[product-image]: ../../BadmintonShop.Desktop/AnhSanPham.cs
[orders-form]: ../../BadmintonShop.Desktop/FrmDonHang.cs
[invoice-repo]: ../../BadmintonShop.Core/DataAccess/HoaDonRepository.cs
[pos-form]: ../../BadmintonShop.Desktop/FrmPOS.cs
[exporter]: ../../BadmintonShop.Desktop/XuatTaiLieu.cs
[report-service]: ../../BadmintonShop.Core/Services/BaoCaoService.cs
[statistics-form]: ../../BadmintonShop.Desktop/FrmThongKe.cs
[chart]: ../../BadmintonShop.Desktop/BieuDoCot.cs
[demo4]: ../../database/05_SV4_Demo.sql
[class-diagram]: ../diagrams/06-lop-core-sv4.md
[test-plan4]: kiem-thu-sv4.md
[conclusion4]: tong-ket-sv4.md
[ui-doc4]: ../UI-SV4.md
[spec4]: dac-ta-sv4.md
[images4]: minh-chung-sv4/
[guide4]: huong-dan-su-dung-sv4.md
[test-results4]: ket-qua-kiem-thu-sv4.md
[run-results]: C:/Users/nhatt/AppData/Local/Temp/BadmintonShop-SV4-Checks/20261113-144417/results.txt
[core-results]: C:/Users/nhatt/AppData/Local/Temp/BadmintonShop-Checklist-20261113/core-ui-results.txt
[backup-results]: C:/Users/nhatt/AppData/Local/Temp/BadmintonShop-Checklist-20261113/backup-results.txt
[web-results]: C:/Users/nhatt/AppData/Local/Temp/BadmintonShop-Checklist-20261113/web-http-results.txt
