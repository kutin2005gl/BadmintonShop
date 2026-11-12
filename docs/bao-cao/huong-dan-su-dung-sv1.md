# HƯỚNG DẪN SỬ DỤNG PHẦN SV1

## 1. Chuẩn bị và quyền sử dụng

Cài đặt theo `huong-dan-cai-dat.md`. Hai ứng dụng dùng cùng CSDL SQL Server. Tài khoản mẫu nằm trong `readme.txt` và mục 6.3 của hướng dẫn cài đặt.

| Chức năng | ADMIN | QUANLY | NHANVIEN |
|---|---|---|---|
| Đăng nhập, đổi mật khẩu, đăng xuất | Có | Có | Có |
| Nhà cung cấp, phiếu nhập | Có | Có | |
| Nhân viên, tài khoản | Có | | |
| Sao lưu, phục hồi | Có | | |

Giao diện hiện tại dùng thanh điều hướng bên trái. Các nhãn thao tác bên dưới lấy từ `FrmMain.Shell.cs` và các form SV1.

## 2. Đăng nhập desktop

1. Chạy `BadmintonShop.Desktop.exe`. Màn hình **Đăng nhập hệ thống** xuất hiện.
2. Nhập **Tên đăng nhập** và **Mật khẩu**, ví dụ `admin` và `Admin@123`.
3. Có thể chọn **Hiện mật khẩu** để kiểm tra ký tự đã nhập.
4. Bấm **Đăng nhập** hoặc Enter. Hệ thống mở màn hình chính và hiển thị quyền của tài khoản.
5. Bấm **Thoát** khi muốn kết thúc từ màn hình đăng nhập.

Bỏ trống tài khoản hiện “Vui lòng nhập tên đăng nhập.”. Bỏ trống mật khẩu hiện “Vui lòng nhập mật khẩu.”. Sai thông tin hiện “Sai tên đăng nhập hoặc mật khẩu.”. Với `nhanvien2` và mật khẩu mẫu, thông báo là “Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.”.

Nếu mất kết nối, hộp **Lỗi kết nối** bắt đầu bằng “Không kết nối được cơ sở dữ liệu.” và kèm chi tiết để kiểm tra SQL Server, cấu hình kết nối.

![TODO: chụp màn hình đăng nhập desktop và tài khoản bị khóa](anh-sv1/dang-nhap-desktop.png)

## 3. Đổi mật khẩu và đăng xuất desktop

### 3.1. Đổi mật khẩu

1. Trong nhóm **HỆ THỐNG** ở thanh bên trái, chọn **Đổi mật khẩu**.
2. Nhập **Mật khẩu hiện tại**, **Mật khẩu mới** và **Nhập lại mật khẩu mới**.
3. Mật khẩu mới cần tối thiểu 8 ký tự, có chữ hoa, chữ thường và chữ số, đồng thời khác mật khẩu hiện tại.
4. Bấm **Lưu**. Thành công hiện “Đổi mật khẩu thành công.” và đóng hộp thoại.
5. Bấm **Hủy** để đóng khi chưa muốn lưu.

Nhập sai mật khẩu hiện tại hiện “Mật khẩu hiện tại không đúng.”. Hai ô mật khẩu mới khác nhau hiện “Nhập lại mật khẩu mới không khớp.”. Dùng lại mật khẩu cũ hiện “Mật khẩu mới phải khác mật khẩu hiện tại.”.

![TODO: chụp màn hình đổi mật khẩu desktop](anh-sv1/doi-mat-khau.png)

### 3.2. Đăng xuất

1. Bấm **Đăng xuất** ở góc trên bên phải.
2. Tại thông báo “Bạn có chắc muốn đăng xuất?”, chọn **Yes** để trở về màn hình đăng nhập, hoặc **No** để tiếp tục phiên.

## 4. Nhà cung cấp

1. Với tài khoản ADMIN hoặc QUANLY, chọn **Nhà cung cấp** trong nhóm **NHẬP HÀNG**.
2. Tìm dữ liệu bằng ô **Tìm (tên / SĐT)**, bấm **Tìm** hoặc Enter.
3. Để thêm, bấm **Làm mới**, nhập **Tên NCC (*)**, **SĐT**, **Địa chỉ**, **Email**, rồi bấm **Thêm**. Thành công hiện “Đã thêm nhà cung cấp.”.
4. Để sửa, chọn một dòng, chỉnh thông tin rồi bấm **Lưu sửa**. Thành công hiện “Đã cập nhật nhà cung cấp.”.
5. Để xóa, chọn dòng rồi bấm **Xóa**, xác nhận **Yes** trong hộp **Xác nhận xóa**. Thành công hiện “Đã xóa nhà cung cấp.”.

Tên là bắt buộc. SĐT khi có giá trị cần gồm 10–11 chữ số. Email khi có giá trị cần đúng định dạng. NCC đã phát sinh phiếu nhập hiện “Không thể xóa: nhà cung cấp này đã có phiếu nhập hàng.” và “Bạn chỉ có thể sửa thông tin.”.

![TODO: chụp màn hình nhà cung cấp và các nút Thêm Lưu sửa Xóa](anh-sv1/nha-cung-cap.png)

## 5. Nhân viên và tài khoản

### 5.1. Hồ sơ nhân viên

1. Đăng nhập ADMIN, chọn **Nhân viên, tài khoản** trong nhóm **HỆ THỐNG**.
2. Mở tab **Nhân viên**. Tìm theo ô **Tìm (tên / SĐT / email)** và bấm **Tìm**.
3. Bấm **Làm mới** để nhập nhân viên mới. Điền họ tên, SĐT, email, địa chỉ, chức vụ, lương và ngày vào làm.
4. Bấm **Thêm**. Thành công hiện “Đã thêm nhân viên.”.
5. Chọn một nhân viên và bấm **Lưu sửa** sau khi chỉnh thông tin. Thành công hiện “Đã cập nhật nhân viên.”.
6. Chọn dòng, bấm **Xóa** và xác nhận **Yes** nếu cần xóa. Thành công hiện “Đã xóa nhân viên.”.

Họ tên và chức vụ là bắt buộc. Lương cần là số không âm, ngày vào làm tối đa là ngày hiện tại. Khi nhân viên đã có tài khoản, phiếu nhập hoặc hóa đơn, thông báo hướng dẫn “Hãy khóa tài khoản thay vì xóa nhân viên.”.

![TODO: chụp màn hình tab Nhân viên](anh-sv1/nhan-vien.png)

### 5.2. Cấp tài khoản

1. Mở tab **Tài khoản đăng nhập**, khu vực **Cấp tài khoản mới**.
2. Chọn **Nhân viên** chưa có tài khoản, nhập **Tên đăng nhập**, **Mật khẩu**, chọn **Vai trò** là ADMIN, QUANLY hoặc NHANVIEN.
3. Tên đăng nhập dài 3–50 ký tự, gồm chữ không dấu, số, dấu gạch dưới hoặc dấu chấm. Mật khẩu tối thiểu 8 ký tự, có chữ hoa, chữ thường và chữ số.
4. Bấm **Cấp tài khoản**. Thông báo có dạng “Đã cấp tài khoản "tên" (vai trò).”.

Nếu hết nhân viên có thể cấp tài khoản, thông báo là “Không còn nhân viên nào chưa có tài khoản.”. Tên bị trùng hiện “Tên đăng nhập đã được dùng.”.

### 5.3. Đổi quyền, khóa hoặc mở khóa

1. Chọn một dòng tài khoản ở lưới.
2. Trong **Tài khoản đang chọn**, chọn **Vai trò**, bấm **Đổi**. Thông báo là “Đã đổi vai trò. Người dùng cần đăng nhập lại để quyền mới có hiệu lực.”.
3. Bấm **Khóa tài khoản** đối với tài khoản đang hoạt động, hoặc **Mở khóa** đối với tài khoản đã bị khóa.
4. Kết quả hiện “Đã khóa tài khoản.” hoặc “Đã mở khóa tài khoản.”.

Tự đổi vai trò bị chặn bằng “Bạn không thể tự đổi vai trò của chính mình.”. Tự khóa bị chặn bằng “Bạn không thể tự khóa tài khoản của chính mình.”.

### 5.4. Đặt lại mật khẩu

1. Chọn tài khoản, bấm **Đặt lại mật khẩu**.
2. Chọn **Yes** tại câu hỏi có dạng “Đặt lại mật khẩu cho "tên"?”.
3. Hộp **Đã đặt lại mật khẩu** hiển thị mật khẩu ngẫu nhiên mới. Ghi lại để giao cho đúng nhân viên và nhắc đổi mật khẩu sau khi đăng nhập.

![TODO: chụp màn hình tab Tài khoản đăng nhập với mật khẩu đã che](anh-sv1/tai-khoan.png)

## 6. Phiếu nhập hàng

### 6.1. Danh sách và xem chi tiết

1. Chọn **Phiếu nhập** trong nhóm **NHẬP HÀNG**.
2. Chọn **NCC**, hoặc **(Tất cả)** để xem mọi nhà cung cấp.
3. Bật ô chọn cạnh **Từ ngày** và **Đến ngày** để giới hạn ngày. Bỏ chọn từng ô để bỏ giới hạn tương ứng.
4. Bấm **Lọc**. Lưới hiển thị số phiếu, ngày nhập, nhà cung cấp, người lập và tổng tiền.
5. Chọn một dòng để xem vùng **Chi tiết phiếu nhập số ...** phía dưới. Dòng tổng kết hiển thị số phiếu và tổng giá trị nhập.

Nếu khoảng ngày đảo ngược, hệ thống hiện “"Từ ngày" phải trước hoặc bằng "Đến ngày".”.

![TODO: chụp màn hình danh sách và chi tiết phiếu nhập](anh-sv1/danh-sach-phieu-nhap.png)

### 6.2. Lập phiếu nhập

1. Từ danh sách, bấm **+ Lập phiếu nhập**.
2. Chọn **Nhà cung cấp (*)**, kiểm tra **Ngày nhập** và **Người lập**. Người lập được lấy từ phiên đăng nhập.
3. Trong **Thêm sản phẩm vào phiếu**, chọn **Sản phẩm**, nhập **SL** và **Đơn giá**, bấm **Thêm vào phiếu**.
4. Chọn sản phẩm gợi ý đơn giá nhập gần nhất khi có dữ liệu. Số lượng và đơn giá cần lớn hơn 0.
5. Nếu thêm lại sản phẩm đang có, hệ thống cộng số lượng và áp dụng đơn giá vừa nhập cho dòng gộp.
6. Có thể sửa **Số lượng**, **Đơn giá nhập** ngay trên lưới. Bấm **Xóa dòng đang chọn** để bỏ dòng. Kiểm tra **Thành tiền** và **Tổng tiền**.
7. Bấm **Lưu phiếu** rồi chọn **Yes** tại hộp **Xác nhận** có tên NCC, số sản phẩm và tổng tiền.
8. Thành công hiện “Đã lưu phiếu nhập số ... Tồn kho đã được cập nhật.”. Bấm **OK** để trở về danh sách và xem phiếu vừa tạo.
9. Bấm **Đóng** khi muốn rời form. Phiếu đang có chi tiết sẽ hỏi “Phiếu nhập chưa được lưu. Đóng và bỏ phiếu này?”.

Chưa chọn NCC hiện “Vui lòng chọn nhà cung cấp.”. Ngày nhập vượt thời điểm hiện tại hơn 5 phút bị chặn với “Ngày nhập không được ở tương lai.”. Nếu lỗi SQL khi lưu, hộp **Lỗi** bắt đầu bằng “Không lưu được phiếu nhập, dữ liệu chưa thay đổi.”.

![TODO: chụp màn hình lập phiếu nhập và thông báo lưu thành công](anh-sv1/lap-phieu-nhap.png)

## 7. Sao lưu và phục hồi

### 7.1. Sao lưu

1. Đăng nhập ADMIN, chọn **Sao lưu, phục hồi** trong nhóm **HỆ THỐNG**.
2. Kiểm tra dòng **CSDL đang dùng** trước khi thao tác.
3. Trong vùng **Sao lưu**, giữ **Thư mục lưu** gợi ý của SQL Server hoặc bấm **Chọn...** để chọn thư mục có quyền ghi cho tài khoản dịch vụ SQL Server.
4. Bấm **Sao lưu ngay**, chờ trạng thái **Đang sao lưu...**.
5. Hộp **Sao lưu** hiển thị “Sao lưu thành công” kèm đường dẫn file. Tên file có dạng `BadmintonShop_yyyyMMdd_HHmmss.bak`.

Thư mục cần tồn tại, nếu sai sẽ hiện “Thư mục không tồn tại.”. Đường dẫn phải truy cập được từ máy SQL Server và máy chạy desktop vì form kiểm tra tồn tại ở phía desktop.

### 7.2. Phục hồi trên CSDL demo riêng

1. Trước buổi thử, cấu hình ứng dụng dùng CSDL demo riêng theo hướng dẫn cài đặt, rồi đăng nhập ADMIN.
2. Mở **Sao lưu, phục hồi** và kiểm tra **CSDL đang dùng** đúng CSDL demo.
3. Trong vùng **Phục hồi**, bấm **Chọn...** để chọn **File .bak**.
4. Bấm **Phục hồi**. Đọc hộp **Xác nhận phục hồi** có tên CSDL, đường dẫn file và cảnh báo ghi đè dữ liệu, ngắt kết nối mọi người đang dùng.
5. Chọn **Yes** để thực hiện trên CSDL demo đã chọn, hoặc **No** để dừng. Chờ trạng thái **Đang phục hồi...**.
6. Khi hiện “Phục hồi thành công. Vui lòng đăng nhập lại.”, bấm **OK** và đăng nhập lại.

Nếu đường dẫn sai, thông báo là “Vui lòng chọn file .bak hợp lệ.”. Repository kiểm tra bản sao lưu bằng `RESTORE VERIFYONLY` trước khi phục hồi và ánh xạ file dữ liệu sang đường dẫn của máy đích.

![TODO: chụp màn hình sao lưu phục hồi trên CSDL demo riêng](anh-sv1/sao-luu-phuc-hoi.png)

## 8. Đăng nhập web của khách hàng

1. Chạy web, mở `http://localhost:5255/TaiKhoan/DangNhap` với profile `http`, hoặc địa chỉ được in bởi tiến trình web.
2. Nhập **Email** là `kh01@gmail.com`, **Mật khẩu** là `Khach@123`.
3. Có thể chọn **Ghi nhớ đăng nhập**, rồi bấm **Đăng nhập**. Hệ thống chuyển tới trang được yêu cầu trước đó hoặc trang chủ.
4. Sai thông tin hiện “Email hoặc mật khẩu không chính xác.”. Dùng `kh20@gmail.com` cùng mật khẩu mẫu hiện “Tài khoản của bạn đã bị khóa.”.
5. Bỏ trống email hiện “Vui lòng nhập email”, email sai định dạng hiện “Email không hợp lệ”, bỏ trống mật khẩu hiện “Vui lòng nhập mật khẩu”.

Giao diện web thuộc SV2. Cơ chế xác thực dùng repository khách hàng và cookie do SV1 cung cấp.

![TODO: chụp màn hình đăng nhập web kh01 và thông báo khóa kh20](anh-sv1/dang-nhap-web.png)
