# ĐẶC TẢ USE CASE VÀ TEST CASE – PHẦN SV1

> Người viết: SV1. Viết theo hành vi thật của mã nguồn trên nhánh `develop` ngày 15/10/2026. Bản Word: [`dac-ta-sv1.docx`](dac-ta-sv1.docx), xuất bằng `node tools/XuatBaoCao/xuat-docx.js docs/bao-cao/dac-ta-sv1.md`.
> Test case ở mục 6 được gộp vào bảng test case chung của SV4. Cột "Kết quả" ghi kết quả SV1 tự kiểm; **SV4 kiểm thử chéo** và điền lại ở bảng chung (⏳ chờ SV4).

## 1. Danh sách use case

| Mã | Tên use case | Tác nhân | Ứng dụng |
|---|---|---|---|
| UC01 | Đăng nhập, đăng xuất, đổi mật khẩu | Quản trị viên, Quản lý, Nhân viên | Desktop |
| UC02 | Đăng nhập, đăng ký tài khoản khách hàng (cơ chế xác thực) | Khách hàng | Web |
| UC03 | Quản lý nhà cung cấp | Quản trị viên, Quản lý | Desktop |
| UC04 | Quản lý nhân viên và tài khoản | Quản trị viên | Desktop |
| UC05 | Lập phiếu nhập hàng | Quản trị viên, Quản lý | Desktop |
| UC06 | Sao lưu – phục hồi cơ sở dữ liệu | Quản trị viên | Desktop |

Giao diện đăng ký/đăng nhập trên web do SV2 làm; SV1 làm phần cơ chế (băm mật khẩu, cookie, chặn tài khoản khóa). Đặc tả UC02 phía giao diện nằm trong phần của SV2.

## 2. UC01 – Đăng nhập, đăng xuất, đổi mật khẩu (desktop)

| Mục | Nội dung |
|---|---|
| Tác nhân | Quản trị viên (ADMIN), Quản lý (QUANLY), Nhân viên (NHANVIEN) |
| Mô tả | Nhân viên đăng nhập ứng dụng desktop bằng tài khoản được cấp; hệ thống hiển thị menu theo vai trò |
| Tiền điều kiện | Tài khoản đã được quản trị viên cấp; SQL Server đang chạy |
| Hậu điều kiện | Đăng nhập thành công: thông tin người dùng lưu trong phiên, form chính hiện đúng các menu được phép |

**Luồng sự kiện chính**

1. Người dùng mở ứng dụng desktop; hệ thống hiện màn hình đăng nhập.
2. Người dùng nhập tên đăng nhập, mật khẩu (ô mật khẩu ẩn ký tự, có ô tick để hiện) và bấm Đăng nhập hoặc Enter.
3. Hệ thống kiểm tra kết nối CSDL, tìm tài khoản theo tên đăng nhập, so mật khẩu với chuỗi băm BCrypt và kiểm tra trạng thái tài khoản.
4. Hệ thống lưu phiên đăng nhập và mở form chính; menu hiển thị theo bảng phân quyền của vai trò.

**Luồng thay thế và ngoại lệ**

- 2a. Bỏ trống tên đăng nhập hoặc mật khẩu: báo "Vui lòng nhập tên đăng nhập" / "Vui lòng nhập mật khẩu".
- 3c. Không kết nối được CSDL: báo "Không kết nối được cơ sở dữ liệu" kèm chi tiết lỗi, vẫn ở màn hình đăng nhập.
- 3a. Sai tên đăng nhập hoặc sai mật khẩu: báo chung "Sai tên đăng nhập hoặc mật khẩu" (không tiết lộ tài khoản có tồn tại hay không).
- 3b. Tài khoản bị khóa (mật khẩu đúng): báo tài khoản đã bị khóa, liên hệ quản trị viên.
- Đổi mật khẩu: người dùng vào Hệ thống → Đổi mật khẩu, nhập mật khẩu hiện tại, mật khẩu mới (tối thiểu 8 ký tự, có chữ hoa, chữ thường, chữ số) và nhập lại. Mật khẩu hiện tại sai, mật khẩu mới yếu, nhập lại không khớp hoặc trùng mật khẩu cũ thì báo lỗi tương ứng.
- Đăng xuất: người dùng chọn Hệ thống → Đăng xuất và xác nhận; hệ thống xóa phiên và quay về màn hình đăng nhập.

## 3. UC03 – Quản lý nhà cung cấp

| Mục | Nội dung |
|---|---|
| Tác nhân | Quản trị viên, Quản lý |
| Mô tả | Xem, tìm kiếm, thêm, sửa, xóa nhà cung cấp |
| Tiền điều kiện | Đã đăng nhập với vai trò ADMIN hoặc QUANLY |
| Hậu điều kiện | Danh sách nhà cung cấp trong CSDL được cập nhật |

**Luồng sự kiện chính**

1. Người dùng chọn menu Nhập hàng → Nhà cung cấp; hệ thống hiện danh sách nhà cung cấp.
2. Người dùng nhập từ khóa (tên hoặc số điện thoại) và bấm Tìm để lọc danh sách.
3. Thêm: người dùng nhập tên (bắt buộc), địa chỉ, số điện thoại, email rồi bấm Thêm; hệ thống kiểm tra và lưu.
4. Sửa: người dùng chọn một dòng, sửa thông tin, bấm Lưu sửa; hệ thống kiểm tra và lưu.
5. Xóa: người dùng chọn một dòng, bấm Xóa và xác nhận; hệ thống xóa nhà cung cấp.

**Luồng thay thế và ngoại lệ**

- 3a/4a. Bỏ trống tên, số điện thoại không phải 10–11 chữ số, email sai định dạng: báo lỗi và đưa con trỏ về ô cần sửa.
- 5a. Nhà cung cấp đã có phiếu nhập: không cho xóa, báo lý do và gợi ý chỉ sửa thông tin.

## 4. UC04 – Quản lý nhân viên và tài khoản

| Mục | Nội dung |
|---|---|
| Tác nhân | Quản trị viên |
| Mô tả | Quản lý hồ sơ nhân viên; cấp tài khoản, đổi vai trò, khóa/mở khóa, đặt lại mật khẩu |
| Tiền điều kiện | Đã đăng nhập với vai trò ADMIN |
| Hậu điều kiện | Bảng NhanVien, TaiKhoan được cập nhật; mật khẩu luôn lưu dạng băm |

**Luồng sự kiện chính**

1. Quản trị viên chọn menu Nhân sự → Nhân viên & tài khoản; hệ thống hiện 2 tab Nhân viên và Tài khoản đăng nhập.
2. Tab Nhân viên: tìm kiếm, thêm, sửa, xóa nhân viên (họ tên, SĐT, email, địa chỉ, chức vụ, lương, ngày vào làm).
3. Tab Tài khoản – cấp tài khoản: chọn nhân viên chưa có tài khoản, nhập tên đăng nhập, mật khẩu, chọn vai trò, bấm Cấp tài khoản.
4. Chọn một tài khoản để đổi vai trò, khóa/mở khóa, hoặc đặt lại mật khẩu (hệ thống sinh mật khẩu ngẫu nhiên 10 ký tự, hiển thị một lần).

**Luồng thay thế và ngoại lệ**

- 2a. Họ tên hoặc chức vụ trống, SĐT/email sai định dạng, lương âm hoặc không phải số, ngày vào làm ở tương lai: báo lỗi.
- 2b. Xóa nhân viên đã có tài khoản, phiếu nhập hoặc hóa đơn: không cho xóa, gợi ý khóa tài khoản thay vì xóa.
- 3a. Tên đăng nhập trùng, không đúng định dạng (3–50 ký tự, chữ không dấu, số, dấu _ hoặc .), hoặc mật khẩu yếu: báo lỗi.
- 3b. Không còn nhân viên nào chưa có tài khoản: báo không thể cấp thêm (mỗi nhân viên tối đa 1 tài khoản).
- 4a. Quản trị viên tự khóa hoặc tự đổi vai trò của chính mình: hệ thống chặn và báo lý do.

## 5. UC05 – Lập phiếu nhập hàng

| Mục | Nội dung |
|---|---|
| Tác nhân | Quản trị viên, Quản lý |
| Mô tả | Lập phiếu nhập hàng từ nhà cung cấp, cập nhật tồn kho; xem danh sách phiếu nhập |
| Tiền điều kiện | Đã đăng nhập với vai trò ADMIN hoặc QUANLY; đã có nhà cung cấp và sản phẩm |
| Hậu điều kiện | Phiếu nhập và chi tiết được lưu; tồn kho các sản phẩm tăng đúng số lượng nhập; tổng tiền phiếu được tính |

**Luồng sự kiện chính**

1. Người dùng chọn menu Nhập hàng → Phiếu nhập; hệ thống hiện danh sách phiếu nhập (lọc được theo khoảng ngày và nhà cung cấp, chọn một phiếu để xem chi tiết).
2. Người dùng bấm "+ Lập phiếu nhập"; hệ thống mở form lập phiếu, người lập là người đang đăng nhập.
3. Người dùng chọn nhà cung cấp và ngày nhập.
4. Người dùng chọn sản phẩm (hệ thống gợi ý đơn giá nhập lần gần nhất), nhập số lượng, đơn giá và bấm Thêm vào phiếu. Lặp lại cho các sản phẩm khác.
5. Hệ thống hiển thị thành tiền từng dòng và tổng tiền phiếu, cập nhật ngay khi thêm, xóa hoặc sửa dòng.
6. Người dùng bấm Lưu phiếu và xác nhận; hệ thống lưu phiếu và toàn bộ chi tiết trong một giao tác; trigger tăng tồn kho và tính tổng tiền.
7. Hệ thống báo số phiếu vừa lưu, quay lại danh sách và chọn sẵn phiếu mới.

**Luồng thay thế và ngoại lệ**

- 3a. Chưa chọn nhà cung cấp hoặc phiếu chưa có sản phẩm nào: không cho lưu, báo lỗi.
- 4a. Thêm sản phẩm đã có trong phiếu: gộp vào dòng cũ (cộng số lượng, lấy đơn giá mới).
- 5a. Sửa trực tiếp số lượng (phải > 0) hoặc đơn giá (phải > 0) trên lưới; giá trị sai thì không cho rời ô. Thêm dòng có đơn giá bằng 0 cũng bị chặn: "Đơn giá nhập phải lớn hơn 0."
- 6a. Lỗi CSDL trong lúc lưu: hoàn tác toàn bộ, không có phiếu nào được lưu, tồn kho giữ nguyên, báo lỗi.
- 7a. Đóng form khi phiếu chưa lưu: hệ thống hỏi lại trước khi bỏ phiếu.

## 6. UC06 – Sao lưu – phục hồi cơ sở dữ liệu

| Mục | Nội dung |
|---|---|
| Tác nhân | Quản trị viên |
| Mô tả | Sao lưu toàn bộ CSDL ra file .bak và phục hồi từ file .bak |
| Tiền điều kiện | Đã đăng nhập với vai trò ADMIN; tài khoản SQL Server có quyền sao lưu/phục hồi |
| Hậu điều kiện | Sao lưu: có file .bak mới. Phục hồi: dữ liệu trở về đúng thời điểm sao lưu, người dùng phải đăng nhập lại |

**Luồng sự kiện chính**

1. Quản trị viên chọn menu Hệ thống → Sao lưu - phục hồi CSDL.
2. Sao lưu: chọn thư mục (mặc định là thư mục Backup của SQL Server), bấm Sao lưu ngay; hệ thống tạo file `BadmintonShop_yyyyMMdd_HHmmss.bak` và báo đường dẫn.
3. Phục hồi: chọn file .bak, bấm Phục hồi, xác nhận cảnh báo ghi đè dữ liệu.
4. Hệ thống kiểm tra file, ngắt các kết nối khác, phục hồi CSDL, đưa CSDL về chế độ nhiều người dùng.
5. Hệ thống báo thành công, đăng xuất và yêu cầu đăng nhập lại.

**Luồng thay thế và ngoại lệ**

- 2a. Thư mục không tồn tại hoặc SQL Server không có quyền ghi: báo lỗi kèm gợi ý dùng thư mục mặc định.
- 3a. File không tồn tại hoặc file hỏng: báo lỗi, dữ liệu hiện tại giữ nguyên, CSDL vẫn dùng được.
- 4a. File .bak tạo trên máy khác (đường dẫn file dữ liệu khác): hệ thống tự chuyển file dữ liệu về đúng vị trí của máy hiện tại.

## 7. Test case

Ghi chú cột Kết quả: **Đạt** = SV1 đã chạy và đúng mong đợi (ghi rõ cách kiểm); **–** = chưa chạy trên giao diện, chờ SV4 kiểm thử chéo.

| Mã TC | UC | Mô tả / các bước | Dữ liệu vào | Kết quả mong đợi | Kết quả |
|---|---|---|---|---|---|
| TC01 | UC01 | Đăng nhập đúng tài khoản ADMIN | admin / Admin@123 | Vào form chính, thấy đủ menu: Nhân sự, Nhập hàng, Sản phẩm, Bán hàng, Báo cáo, Sao lưu | Đạt (test tự động) |
| TC02 | UC01 | Đăng nhập vai trò QUANLY | quanly / Quanly@123 | Không thấy menu Nhân sự và Sao lưu; thấy các menu còn lại | Đạt (test tự động) |
| TC03 | UC01 | Đăng nhập vai trò NHANVIEN | nhanvien / Nhanvien@123 | Chỉ thấy menu Bán hàng (đơn hàng, POS) và Hệ thống | Đạt (test tự động) |
| TC04 | UC01 | Sai mật khẩu | admin / sai123 | Báo sai tên đăng nhập hoặc mật khẩu, vẫn ở màn hình đăng nhập | Đạt (test tự động) |
| TC05 | UC01 | Tên đăng nhập không tồn tại | khongco / Admin@123 | Báo giống TC04 | Đạt (test tự động) |
| TC06 | UC01 | Tài khoản bị khóa | nhanvien2 / Nhanvien@123 | Báo tài khoản đã bị khóa | Đạt (test tự động) |
| TC07 | UC01 | Mất kết nối CSDL | Tắt SQL Server, mở app, bấm Đăng nhập | Báo không kết nối được cơ sở dữ liệu, không bị văng app | – |
| TC08 | UC01 | Đổi mật khẩu hợp lệ | MK cũ đúng, MK mới Abcd1234 | Báo thành công; đăng nhập lại bằng MK mới được, MK cũ không được | Đạt (test tự động) |
| TC09 | UC01 | Đổi mật khẩu – mật khẩu mới yếu | MK mới abc | Báo chưa đủ mạnh | – |
| TC10 | UC01 | Đăng xuất | Hệ thống → Đăng xuất → Yes | Quay về màn hình đăng nhập | – |
| TC11 | UC03 | Thêm NCC hợp lệ | Tên "NCC A", SĐT 0901234567, email a@b.com | Thêm thành công, NCC hiện trên lưới | – |
| TC12 | UC03 | Thêm NCC bỏ trống tên | Tên rỗng | Báo nhập tên, không lưu | – |
| TC13 | UC03 | SĐT sai định dạng | SĐT 09abc hoặc 123 | Báo SĐT 10–11 chữ số | Đạt (test hàm kiểm tra) |
| TC14 | UC03 | Email sai định dạng | a@b, "x y@b.com" | Báo email không hợp lệ | Đạt (test hàm kiểm tra) |
| TC15 | UC03 | Xóa NCC đã có phiếu nhập | NCC số 1 | Không xóa, báo đã có phiếu nhập | Đạt (test tự động) |
| TC16 | UC03 | Xóa NCC chưa có phiếu nhập | NCC số 4 | Hỏi xác nhận, xóa thành công | Đạt (test tự động, phần kiểm tra) |
| TC17 | UC03 | Tìm kiếm theo tên / SĐT | Từ khóa "Yonex" hoặc một phần SĐT | Lưới chỉ còn các NCC khớp | – |
| TC18 | UC04 | Thêm nhân viên hợp lệ | Họ tên, chức vụ Kho, lương 5000000 | Thêm thành công, xuất hiện trong danh sách cấp tài khoản | Đạt (test tự động) |
| TC19 | UC04 | Lương âm / không phải số | -1 hoặc "abc" | Báo lương phải là số không âm | – |
| TC20 | UC04 | Xóa nhân viên đã có tài khoản | Nhân viên số 1 | Không xóa, gợi ý khóa tài khoản | Đạt (test tự động) |
| TC21 | UC04 | Cấp tài khoản hợp lệ | NV mới, test_tv1 / Abcd1234, NHANVIEN | Cấp thành công; đăng nhập được bằng tài khoản mới | Đạt (test tự động) |
| TC22 | UC04 | Cấp tài khoản trùng tên đăng nhập | Tên "admin" | Báo tên đăng nhập đã được dùng | – |
| TC23 | UC04 | Khóa rồi mở khóa tài khoản | Tài khoản test_tv1 | Sau khóa: không đăng nhập được; sau mở: đăng nhập được | Đạt (test tự động) |
| TC24 | UC04 | Đổi vai trò | test_tv1: NHANVIEN → QUANLY | Đăng nhập lại thấy menu của Quản lý | Đạt (test tự động) |
| TC25 | UC04 | Đặt lại mật khẩu | Tài khoản test_tv1 | Hiện mật khẩu mới 10 ký tự đủ mạnh; MK cũ hết hiệu lực | Đạt (test tự động) |
| TC26 | UC04 | ADMIN tự khóa / tự đổi vai trò mình | Đăng nhập admin, chọn dòng admin | Hệ thống chặn, báo không thể tự khóa / tự đổi vai trò | – |
| TC27 | UC05 | Lập phiếu nhập hợp lệ | NCC 1; SP1 ×3 giá 120.000; SP2 ×3 giá 50.000 | Lưu thành công; TongTien = 510.000; tồn SP1 +3, SP2 +3 | Đạt (test tự động trên CSDL) |
| TC28 | UC05 | Thêm trùng sản phẩm trong phiếu | SP1 ×2 giá 100.000, rồi SP1 ×1 giá 120.000 | Còn 1 dòng SP1: SL 3, giá 120.000, thành tiền 360.000 | Đạt (test tự động trên form) |
| TC29 | UC05 | Lưu khi chưa chọn NCC / chưa có dòng | – | Báo lỗi, không lưu | – |
| TC30 | UC05 | Lỗi giữa chừng khi lưu | Dòng 2 có mã sản phẩm không tồn tại | Không lưu phiếu nào, tồn kho dòng 1 không đổi | Đạt (test tự động trên CSDL) |
| TC31 | UC05 | Gợi ý đơn giá nhập gần nhất | Chọn SP đã từng nhập | Ô đơn giá điền sẵn giá lần nhập gần nhất | Đạt (test tự động) |
| TC32 | UC05 | Lọc danh sách phiếu nhập | Khoảng ngày + NCC | Chỉ hiện phiếu khớp, tổng giá trị nhập đúng | – |
| TC33 | UC06 | Sao lưu | Thư mục hợp lệ | Tạo file .bak, báo đường dẫn | Đạt (test tự động trên LocalDB) |
| TC34 | UC06 | Phục hồi khi đang có kết nối khác | File .bak vừa tạo; thêm 1 NCC trước khi phục hồi | Phục hồi thành công, NCC vừa thêm biến mất, yêu cầu đăng nhập lại | Đạt (test tự động, phần dữ liệu) |
| TC35 | UC06 | Phục hồi file không tồn tại / file hỏng | File rác .bak | Báo lỗi, CSDL vẫn dùng được bình thường | Đạt (test tự động) |
| TC36 | UC06 | Tài khoản QUANLY mở sao lưu | Đăng nhập quanly | Không thấy menu Sao lưu - phục hồi | Đạt (test tự động phân quyền) |
