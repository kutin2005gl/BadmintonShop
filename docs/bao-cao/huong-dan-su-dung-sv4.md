# Hướng dẫn sử dụng SV4

## Cài đặt và chạy

1. Máy Windows có .NET 8 Desktop Runtime/SDK, SQL Server và CSDL theo hướng dẫn SV1.
2. Với CSDL đã tồn tại, **chỉ chạy migration bổ sung**:
   ```powershell
   sqlcmd -S localhost -E -C -I -b -f 65001 -i database/04_SV4_Migration.sql
   ```
   Không chạy lại `01_CreateDatabase.sql` hoặc `BadmintonShop.sql` vì chúng xóa/tạo lại CSDL. File 04 giữ nguyên dữ liệu, chạy lại được.
3. Nếu cần demo, tự chọn chạy `database/05_SV4_Demo.sql` sau 04 bằng cùng tham số. Script tự nhập kho trước, tạo 12 tháng hóa đơn tại quầy và đơn Online mẫu không có khách. Chạy lại không thêm trùng. Mọi bản ghi riêng có nhãn `[SV4-DEMO]`. Không có chức năng tự xóa demo.
4. Đặt chuỗi kết nối bằng biến môi trường phiên PowerShell (không commit thông tin máy):
   ```powershell
   $env:ConnectionStrings__BadmintonShop='Server=localhost;Database=BadmintonShop;Trusted_Connection=True;TrustServerCertificate=True;'
   $env:BadmintonShop__MediaRoot='D:\BadmintonShopMedia'
   dotnet build BadmintonShop.sln
   dotnet run --project BadmintonShop.Desktop
   ```
   Thư mục ảnh là ví dụ cấu hình máy triển khai; CSDL chỉ lưu khóa tương đối. Web/desktop cần trỏ đến cùng kho ảnh.
5. Đăng nhập bằng tài khoản SV1 có quyền phù hợp. Xem tài khoản mẫu trong README; không cần tạo tài khoản cho nhân viên demo SV4.

## Danh mục và sản phẩm

- Sản phẩm → Danh mục: Mới, nhập tên/mô tả, Lưu. Chọn dòng để sửa hoặc xóa; danh mục có sản phẩm sẽ được chặn xóa.
- Sản phẩm → Sản phẩm: thêm/sửa, chọn danh mục, nhập giá nguyên đồng, đơn vị. Không có ô sửa tồn. Muốn có hàng, vào Nhập hàng → Phiếu nhập của SV1.
- Chọn ảnh và xem trước; Bỏ ảnh nếu ảnh cũ thiếu/không hợp lệ. Chọn ảnh mới chỉ ghi kho khi Lưu. Không xóa ảnh cũ tự động vì web có thể còn dùng/cache.
- Ngừng bán giữ lịch sử. Có thể Sửa trạng thái về Đang bán khi cần bán tiếp; đây là trạng thái sản phẩm, không phải mở lại đơn đã hủy.
- Nhập từ khóa, chọn bộ lọc và Lọc. Tải lại cập nhật dữ liệu và đặt danh mục/thương hiệu về tất cả. Bấm tiêu đề cột để sắp xếp. Ngưỡng sắp hết là 1–5 đơn vị, hết hàng là 0.

## Đơn hàng

- Bán hàng → Đơn hàng: mặc định 12 tháng gần nhất, có thể mở rộng ngày; lọc loại/trạng thái và mã/tên/SĐT/email.
- Chọn dòng để xem chi tiết và tiền giảm hóa đơn. Các nút Xác nhận, Giao hàng, Hoàn thành chỉ hoạt động theo luồng. Chỉ hủy trước khi giao và phải nhập lý do.
- Nếu báo đơn đã thay đổi, tải lại để xem trạng thái mới; không cố nhấn lại theo trạng thái cũ.
- Xuất PDF hóa đơn cho dòng đang chọn để xem/in lại; giá lấy từ chi tiết đã lưu.

## POS

1. Bán hàng → Bán hàng tại quầy. Tìm và chọn sản phẩm ở bảng trên.
2. Chọn số lượng, Thêm vào giỏ. Muốn sửa: chọn dòng ở bảng dưới, đặt số lượng mới rồi Sửa số lượng giỏ. Bỏ dòng giỏ để xóa trước khi lưu.
3. Nhập tiền khách đưa. Tổng, tiền thiếu/thừa cập nhật tại màn hình.
4. Thanh toán tiền mặt và xác nhận. Repository kiểm tồn và giá một lần nữa; nếu giá đổi, dùng Giỏ mới, tải sản phẩm và chọn lại giỏ trước khi thanh toán.
5. Khi nhận mã hóa đơn thành công, bấm Xuất lại PDF. Hóa đơn trước đây xuất từ Đơn hàng.

Khuyến mãi SV3 chưa sẵn sàng, toàn bộ dòng POS bán theo giá thường. Không có voucher được áp thành công giả.

Nếu thanh toán báo lỗi kết nối, giữ nguyên giỏ để thử lại cùng mã yêu cầu hoặc kiểm tra Đơn hàng trên khoảng ngày hiện tại. Chỉ Giỏ mới sau khi xác định giao dịch trước đã/ chưa lưu; nút có nhắc điều này. Không sửa chi tiết hóa đơn sau khi lưu.

## Báo cáo

- Báo cáo → Thống kê: chọn ngày, loại bán, Xem báo cáo. Mặc định từ đầu năm đến hôm nay; để xem đủ demo 12 tháng cần mở rộng về năm trước.
- Chuyển các tab để xem bảng và biểu đồ. Chỉ Hoàn thành tính doanh thu và số lượng; Số đơn theo trạng thái đếm cả đơn hủy.
- Excel/PDF xuất tất cả tab của **kết quả đang hiển thị**. Đổi bộ lọc chưa bấm Xem không làm đổi dữ liệu xuất; khoảng và loại bán thực tế có trong file.
- File XLSX có thể mở bằng Excel/LibreOffice; PDF bằng trình đọc PDF. Bản in hóa đơn phục vụ đồ án, không phải hóa đơn điện tử thuế.

## Chạy bộ kiểm tra

```powershell
# Quy tắc + xuất file, không dùng CSDL
dotnet run --project tests/BadmintonShop.SV4.Checks
# Tạo CSDL test mới hoàn toàn trên LocalDB, giữ lại để review
dotnet run --project tests/BadmintonShop.SV4.Checks -- --integration --server '(localdb)\MSSQLLocalDB'
```

Tên CSDL test luôn là `BadmintonShop_SV4_Test_<timestamp>_<random>`. Không dùng CSDL `BadmintonShop`, không chạy khối DROP/CREATE của script 01. Kết quả, ảnh render form thật, XLSX/PDF nằm trong thư mục Temp được in ở cuối lệnh. Cần quyền CREATE DATABASE trên server test. Bộ kiểm tra không tự dọn CSDL để tránh xóa nhầm; có thể tự quản lý các CSDL test đã nhận diện sau khi review.

Kiểm độc lập file xuất (Python có `openpyxl` và `pypdf`): `python tests/BadmintonShop.SV4.Checks/verify_exports.py '<thu-muc-ket-qua>' --expected-revenue <tong-doanh-thu-trong-csdl-test>`. Chỉ dùng đúng tổng và thư mục từ cùng lần chạy.
