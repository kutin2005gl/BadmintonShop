# Biên bản kết quả SV4 — 18/10/2026

## Phiên bản và phạm vi

Nhánh `feature/sv4-desktop`, tạo từ `origin/develop` sau fetch tại `c57ef31`. Ban đầu working tree sạch trên `feature/sv2-web`. Biên bản ghi nhận kết quả trước bước commit/push; việc xuất bản nhánh và mở PR được thực hiện theo yêu cầu bàn giao tiếp theo của người dùng. Không sửa mã web, trigger/schema nền 01/02/03, cấu hình kết nối hoặc dữ liệu CSDL BadmintonShop có sẵn.

Môi trường: Windows, .NET SDK 10.0.203, target .NET 8 Windows; LocalDB 17.0.1000.7. Server localhost có phản hồi ban đầu nhưng sau đó timeout cả SqlClient và sqlcmd; đã chuyển toàn bộ integration sang LocalDB. Không restart hoặc reset SQL Server localhost.

## Lệnh và kết quả lần cuối

```powershell
dotnet build BadmintonShop.sln -c Release --no-restore
dotnet run --project tests/BadmintonShop.SV4.Checks -c Release --no-build -- --integration --server '(localdb)\MSSQLLocalDB'
```

- Build **thành công, 0 warning, 0 error**, cả Core, Desktop, Web và project kiểm tra.
- **35 kiểm tra đạt, 0 lỗi**: U01–U13, I01–I16, GUI render 6 form.
- CSDL lần cuối: `BadmintonShop_SV4_Test_20261018212348_1953608f`, được tạo riêng mới và giữ lại để review. Những lần thử trước cũng giữ CSDL test riêng cùng prefix; không dùng/xóa CSDL người dùng.
- Kết quả gốc: `%TEMP%/BadmintonShop-SV4-Checks/20261018-212346/results.txt`, cùng thư mục chứa ảnh render, XLSX/PDF. [Log lưu trong repository](minh-chung-sv4/results.txt) đã thay phần đường dẫn profile bằng `%TEMP%`, không sửa kết quả kiểm tra.
- `git diff --check`: không lỗi whitespace. Các phần chưa commit là mã nguồn, SQL bổ sung, tài liệu và ảnh render; bin/obj/config máy không đưa vào thay đổi.

## Nghiệp vụ đã kiểm chứng

CRUD danh mục, tên trùng, dữ liệu không hợp lệ, chặn xóa khi có sản phẩm; thêm sản phẩm tồn 0, phiếu nhập tăng tồn bằng trigger, sửa sản phẩm không ghi tồn. Ảnh thật đọc/mã hóa lại được, chặn file giả/quá dung lượng và kiểm đường dẫn tương đối.

POS lưu đầy đủ header/chi tiết, đúng loại/trạng thái/nhân viên/khách vãng lai, kiểm thiếu tiền/thiếu tồn/giá cũ/ngừng bán/trùng dòng. Cùng token retry trả một hóa đơn; hai yêu cầu đồng thời chỉ trừ tồn một lần. Hai người mua sản phẩm còn một đơn vị chỉ một giao dịch thành công, tồn không âm.

Rollback sau header và dòng hợp lệ đầu tiên rồi lỗi tồn dòng thứ hai: không để lại header/chi tiết, cả hai tồn trở lại giá trị cũ. Hủy bắt buộc lý do, hủy lặp và hai yêu cầu hủy đồng thời chỉ hoàn kho một lần. Trigger cũng chặn mở lại đơn đã hủy. Chặn cập nhật trạng thái cũ, nhảy bước hoặc hủy đang giao.

Đơn được lọc đúng mã, tên/SĐT/email khách test, loại, trạng thái và ngày cuối bao gồm 23:59:59.997. API đọc sản phẩm cũ vẫn dùng được. Demo SV4 đủ 12 tháng; chạy lần hai không thêm trùng.

Giảm 7,5% trên dòng 2×100 tạo thành tiền 185, cộng dòng 50 thành 235; giảm hóa đơn 30 còn **205**, báo cáo danh mục tổng 205. Phần trăm lẻ giữ đúng trên lưới và PDF. Kiểm phân bổ gồm 1.000 bộ ngẫu nhiên, trường hợp hòa, 0 và tiền 18 chữ số.

## Đối chiếu file bằng công cụ độc lập

```powershell
# Cần openpyxl và pypdf; đã dùng Python runtime có sẵn của môi trường kiểm tra.
python tests/BadmintonShop.SV4.Checks/verify_exports.py '<thu-muc-ket-qua>' --expected-revenue 13170715
```

Kết quả: PASS. openpyxl mở đủ **6 sheet**, ô số giữ kiểu numeric. Tổng ngày/tháng/năm/danh mục đều **13.170.715 đồng**, bằng `SELECT SUM(TongTien) FROM HoaDon WHERE TrangThai=N'Hoàn thành'` trên CSDL test cuối cùng. Truy vấn độc lập cũng cho tồn nhỏ nhất **0**.

pypdf mở đủ 6 trang báo cáo, đọc được tiếng Việt và quy tắc; hóa đơn nhiều trang đọc lại được. Hóa đơn POS xuất lại lấy tiền khách đưa 120, tổng 100, thừa 20 đúng từ CSDL. Hóa đơn giảm giá hiển thị **7,5% và tổng 205**. ClosedXML mở lại XLSX, OpenXmlValidator không có lỗi schema. PDFsharp mở lại PDF thành công.

Đã render bằng Poppler và xem các trang báo cáo theo mẫu, hóa đơn thường/giảm giá và bố cục nhiều trang; chữ Việt, cột tiền, tiêu đề/footer và số trang hiển thị được, không đổi đuôi CSV. Không thực hiện mở ứng dụng Microsoft Excel/Adobe Reader hoặc in giấy.

## Minh chứng giao diện thật

Ảnh dưới đây là kết quả **Show/OnLoad + DrawToBitmap của Form WinForms thật trên CSDL test**, không phải ảnh thiết kế hoặc ảnh mockup. Dữ liệu là fixture SV4. Smoke kiểm tra nút Mới của danh mục làm sạch ô nhập và POS thêm/bỏ dòng giỏ; chưa phải bộ E2E click đầy đủ.

- [Danh mục](minh-chung-sv4/FrmDanhMuc.png)
- [Sản phẩm](minh-chung-sv4/FrmSanPham.png)
- [Thêm/sửa sản phẩm](minh-chung-sv4/FrmSanPhamSua.png)
- [Đơn hàng và giảm giá lẻ](minh-chung-sv4/FrmDonHang.png)
- [POS](minh-chung-sv4/FrmPOS.png)
- [Báo cáo](minh-chung-sv4/FrmThongKe.png)

Ảnh báo cáo dùng mặc định đầu năm 2026 đến 18/10/2026, tổng 10.420.715; file kiểm độc lập dùng khoảng 18/09/2025–18/10/2026 nên tổng 13.170.715. Hai khoảng khác nhau được ghi rõ, không coi đó là sai lệch bảng/file.

## Chưa kiểm thử và phối hợp còn lại

Chưa chạy E2E web SV2/SV3, khuyến mãi/voucher/email, hộp thoại và thao tác tay đầy đủ, scale/DPI, mở Visual Studio Designer, thiết bị máy in hoặc fault injection mạng ngay sau commit. Ma trận quyền đã kiểm tự động; mở form trái quyền và hộp thoại chặn cần kiểm thủ công riêng.

SV1 cần review migration/luồng tồn; SV2 cần mount kho ảnh và kiểm thử chéo desktop; SV3 cần dùng chung HoaDonRepository, thống nhất quyền hủy khách/nhân viên và tích hợp service khuyến mãi khi hoàn thiện. Danh sách đầy đủ nằm trong [61 test case](kiem-thu-sv4.md) và [quy ước tích hợp](tich-hop-sv4.md).
