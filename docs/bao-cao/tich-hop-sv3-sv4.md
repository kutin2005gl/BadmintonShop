> Hồ sơ tích hợp ngày 22/10/2026 được giữ để đối chiếu. Cập nhật cuối 26/10: [biên bản final](../final-integration/README.md); SQL tổng hiện đã gồm migration 06, script demo chỉ nhận DB test/demo. Ảnh 1920×1080 của harness cũ bị giới hạn thực tế 1556×884, không dùng làm bằng chứng kiểm 1920×1080.

# Tích hợp SV3–SV4: khuyến mãi POS, đơn Web–Desktop, hóa đơn/báo cáo

Ngày thực hiện: 22/10/2026. Nhánh làm việc: `codex/sv4-sv3-integration`. Không commit, push, tạo PR hay merge vào develop/main trong đợt này.

## Nền code và phạm vi

Đã fetch remote trước khi sửa. `origin/develop` tại `fef0067` đã chứa PR #2 (SV4 nền) và #3 (SV3). PR giao diện #4 vẫn **open, chưa merged**, có thể merge ở thời điểm kiểm tra; head `2d6adbc`. Nhánh SV4 cũ ở `fd9d56e`.

Nhánh tích hợp tách từ giao diện mới rồi lấy code develop bằng squash không commit. Những file Web/SV3, script tổng/sample và biên bản tuần 02 xuất hiện trong diff là phần nhập từ develop; không chạy các script này trên CSDL cửa hàng. Các thay đổi giao diện ở PR #4 được giữ lại. `App.config` cá nhân vẫn nằm ngoài phần chỉnh sửa. Không tìm thấy AGENTS.md áp dụng trong project/đường dẫn cha; đã đọc README và quy ước tích hợp SV4.

Không làm email, cổng thanh toán, trang quản lý khuyến mãi hoặc giao diện đăng nhập khách hàng của SV3/SV2.

## Quy tắc giá dùng chung

Web và POS cùng gọi `GioHangService` → `TinhKhuyenMaiService` → `KhuyenMaiRepository`:

1. Giá gốc × số lượng, chọn % giảm sản phẩm lớn nhất đang hiệu lực.
2. Tính và làm tròn **mỗi dòng** về đồng, AwayFromZero, khớp cột tính SQL. Không lấy đơn giá đã làm tròn nhân số lượng.
3. Tổng các dòng sau giảm sản phẩm là căn cứ chọn bậc hóa đơn cao nhất đủ điều kiện (quy tắc SV3 hiện có).
4. So tiền giảm hóa đơn với voucher hợp lệ, chỉ chọn một. Bằng tiền ưu tiên voucher như SV3; voucher thua không tăng lượt.
5. Tổng thanh toán = tổng dòng sau giảm sản phẩm − giảm cấp hóa đơn. Không trừ giảm sản phẩm lần nữa.

POS có ô voucher, Áp dụng/Bỏ mã, thông báo mã sai/hết hạn/hết lượt và ưu đãi hóa đơn tốt hơn mã. Hiển thị tiền trước giảm, giảm sản phẩm, giảm hóa đơn/voucher, tổng phải thu, khách đưa và thừa/thiếu. Thay giỏ hoặc mã đã áp dụng sẽ tính lại; sửa nội dung mã mà chưa bấm Áp dụng sẽ khóa thanh toán. Mã lỗi không bị âm thầm bỏ để thu một tổng tiền khác.

## Transaction và kiểm tra xác nhận

`KhuyenMaiRepository` có constructor nhận connection/transaction. `GioHangService.TinhTrongGiaoDich` dùng cùng calculator với preview, đọc sản phẩm bằng UPDLOCK/HOLDLOCK theo MaSP tăng dần và đọc khuyến mãi bằng repository đã bound. Không mở connection riêng để tính khuyến mãi lúc lưu.

`GiaoDichBanHang` chia sẻ quy tắc ghi giữa hai repository, không gộp cơ học API khách hàng vào API nhân viên. Trong Serializable:

- Kiểm tra đầu vào, sản phẩm đang bán, giá, số lượng và tồn hiện tại; ngày hiệu lực được lấy lại ở thời điểm lưu.
- Tính lại ưu đãi và đối chiếu toàn bộ báo giá đã xác nhận (giá, %, dòng, MaCTKM, voucher, tổng).
- Khác báo giá hoặc voucher hết hiệu lực: ném `GiaThanhToanThayDoiException`, rollback trước khi thu tiền. POS cập nhật tổng mới nhưng cần lần bấm/xác nhận mới. Web so thêm dấu vết báo giá ở Session giữa GET xác nhận và POST; thay đổi giá không âm thầm tạo đơn.
- Voucher được đọc với update lock và tăng `DaDung` bằng UPDATE có điều kiện lượt/ngày/trạng thái/CTKM, trong chính transaction lưu đơn.
- Ghi `ChiTietHoaDon.GiamGia` là %, `HoaDon.TienGiamGia` là tiền giảm cấp hóa đơn và `MaCTKM` là ưu đãi cấp hóa đơn được chọn.
- Trigger tự giảm tồn và tính tổng. Sau ghi, kiểm lại tổng SQL với báo giá; không khớp thì rollback cả đơn, chi tiết, tồn và lượt voucher.

Lỗi SQL/nghiệp vụ ở bất cứ bước nào đều đi qua rollback của `DbHelper.ExecuteTransaction`. Không có lệnh production tự trừ kho hoặc tự cập nhật TongTien.

## Token POS và migration riêng

Migration mới: `database/06_SV4_POS_Request.sql`, chạy lại được, sau migration 04. Chỉ thêm nullable `HoaDon.DauVanTayPOS VARCHAR(64)`, không đổi cột/index cũ. **Đã chạy hai lần trên CSDL test mới; chưa chạy trên CSDL cửa hàng.** POS kiểm khả năng schema và báo rõ nếu chưa có cột cần thiết.

Dấu vân tay SHA-256 lưu yêu cầu đã xác nhận: nhân viên, tiền khách đưa, giỏ đã sắp xếp, giá/%/thành tiền, giảm cấp hóa đơn, MaCTKM và cả **voucher được nhập nhưng không thắng ưu đãi hóa đơn**. Không thể suy đầy đủ yêu cầu này từ MaCTKM nên cần thêm một cột.

Khóa `sp_getapplock` theo GUID, owner Transaction, giữ cùng token trong khi xử lý. Đọc token bằng READCOMMITTED để không khóa khoảng index rỗng. Cách SELECT range-lock ban đầu đã gây deadlock khi Web giữ sản phẩm rồi cần chèn header trong cùng khoảng index; kiểm thử tìm ra và đã thay cách khóa.

Retry token đã thành công kiểm fingerprint rồi trả MaHD cũ **trước khi đọc lại giá, tồn và hiệu lực voucher hiện tại**. Không tăng lượt/trừ kho lần hai; đổi giỏ/voucher/tiền bị từ chối. Hóa đơn POS trước migration 06 có fingerprint NULL chỉ hỗ trợ retry tương thích theo dòng lịch sử khi không có ưu đãi/voucher như phiên bản POS cũ; không đoán voucher của hóa đơn không có dấu vết.

POS giữ token/báo giá khi lỗi chưa rõ kết quả; không tự tạo token mới khi retry. Nếu người dùng chủ động tạo giỏ mới sau lỗi kết nối, vẫn có lời nhắc kiểm tra Đơn hàng trước. Chưa thử ngắt mạng vật lý đúng thời điểm sau commit; đã kiểm retry giao dịch thành công và hai yêu cầu đồng thời.

## Đơn hàng và ranh giới quyền

- Web tạo Online/Chờ xác nhận, trừ kho một lần ở INSERT chi tiết.
- Desktop xác nhận → giao → hoàn thành; UPDATE kèm trạng thái kỳ vọng và LoaiBan Online. Các bước tiến không trừ thêm kho.
- Khách chỉ hủy Online của chính mình khi Chờ xác nhận bằng UPDATE có điều kiện MaHD/MaKH/trạng thái. Không dùng API hủy của nhân viên.
- Nhân viên hủy trước giao theo `QuyTacBanHang`, bắt buộc lý do. Web hiển thị được lý do nhân viên đã lưu.
- Hai bên lấy chi tiết theo giá/% đã lưu; API controller Web truyền MaKH vào truy vấn chi tiết, ngoài kiểm tra sở hữu header.
- Hủy đồng thời với xác nhận chỉ một thao tác thắng; trigger hoàn kho khi mới chuyển sang hủy nên không hoàn lại lần hai.
- **Hoàn lượt voucher khi hủy chưa được nhóm chốt**: giữ hành vi SV3 hiện có là không giảm DaDung. Cần SV1/SV3 thống nhất riêng; không tự thay chính sách trong đợt này.

## Hóa đơn và báo cáo

PDF hóa đơn ghi riêng tiền hàng trước giảm, giảm sản phẩm, tiền hàng sau giảm sản phẩm, giảm hóa đơn/voucher, MaCTKM đã lưu, tổng thu và tiền khách/thừa. Dữ liệu lấy từ giá/%/tiền của hóa đơn đã lưu; không gọi service khuyến mãi hiện tại khi xuất lại.

Giữ BaoCaoService: doanh thu/số bán chỉ Hoàn thành, theo NgayLap; hủy không có doanh thu. Giảm cấp hóa đơn phân bổ cho danh mục theo tỷ trọng thành tiền sau giảm sản phẩm, phần dư lớn nhất, tổng khớp đến đồng. Danh mục/tên sản phẩm vẫn là dữ liệu hiện tại theo thiết kế gốc; chưa có snapshot danh mục/tên tại lúc bán.

Đã đối chiếu tổng SQL, dòng/header, báo cáo ngày/tháng/năm/danh mục và Excel. File cuối được xuất sau toàn bộ thao tác Web/Desktop, có `final-expected-revenue.txt` là tổng SQL cùng thời điểm. Kiểm độc lập dùng openpyxl/pypdf xác minh XLSX numeric, tổng, PDF Unicode, tổng và các khoản giảm trên hóa đơn lịch sử.

## Chạy kiểm tra và demo an toàn

Lượt chạy cuối: **build 0 lỗi/0 cảnh báo; 68 kiểm tra đạt, 0 thất bại**, process exit code 0. CSDL riêng: `BadmintonShop_SV4_Test_20261022163229_52671bee`. Kiểm độc lập openpyxl/pypdf đạt: 6 sheet, 6 trang báo cáo, tổng doanh thu Hoàn thành của **fixture test** là **13.172.235 đồng**, các khoản giảm trên PDF lịch sử khớp. Đây không phải doanh thu CSDL cửa hàng.

Đã sửa hai lỗi lộ ra trong kiểm tra: khoảng khóa token gây deadlock Web/POS, và callback bind báo cáo chạy sai thread khi mở/đóng nhiều Form. Bộ tải hiện dùng control đích để marshal thao tác bind/cập nhật về UI thread, có bảo vệ khi Form bị dispose.

```powershell
dotnet build BadmintonShop.sln -p:OutDir=D:\Desktop\BadmintonShop-SV3-integration-build\
dotnet D:\Desktop\BadmintonShop-SV3-integration-build\BadmintonShop.SV4.Checks.dll --integration --server .
```

Mỗi lần chạy tạo **CSDL mới** tên `BadmintonShop_SV4_Test_<thời điểm>_<guid>` trên server được chỉ định, không nhận tên CSDL cửa hàng. Schema 01/02 + migrations 04/06 chỉ áp dụng cho CSDL test. Không tự xóa CSDL test sau chạy để có thể kiểm tra bằng SSMS. Bộ test tạo account desktop `sv4test / TestOnly@123` trong CSDL đó để demo thủ công; đây không phải tài khoản cửa hàng.

Kết quả cuối và tên CSDL: xem [log kiểm tra](../integration-sv3/results.txt). Bằng chứng được giữ tại [integration-sv3](../integration-sv3); thư mục Temp đầy đủ in cuối log.

Để demo POS bằng ứng dụng, mở PowerShell mới và đặt biến môi trường chỉ trong phiên đó, thay tên CSDL bằng tên được log của lần chạy:

```powershell
$env:ConnectionStrings__BadmintonShop='Server=.;Database=BadmintonShop_SV4_Test_<tên trong log>;Integrated Security=True;TrustServerCertificate=True;'
& 'D:\Desktop\BadmintonShop-SV3-integration-build\BadmintonShop.Desktop.exe'
```

Đăng nhập account test ở trên, chọn Bán tại quầy. Bộ fixture có sản phẩm Integration và voucher HTTP20; chọn đúng sản phẩm của chương trình hoặc xem KM_TheoSP trong SSMS để demo giảm sản phẩm. Áp mã, sửa số lượng, bỏ mã, nhập tiền, thanh toán và xuất lại PDF; chỉ làm trên CSDL test. Muốn demo đủ các ca độc lập và reset fixture, chạy lại bộ kiểm tra để tạo CSDL mới.

Web hiện chưa có TaiKhoanController/View đăng nhập khách hàng ở nền develop. H01 khởi chạy **Kestrel thật với controller/view production**, Session/cookie auth/CSRF thật, dùng endpoint đăng nhập fixture chỉ trong tiến trình test. Các endpoint `/__test/*` nằm trong project kiểm tra, không thêm vào startup Web production. Vì vậy đã kiểm HTTP có xác thực, đặt/hủy/theo dõi đơn; **chưa kiểm giao diện đăng nhập thực tế hoặc thao tác browser thủ công xuyên hai ứng dụng**. Không nhận là đã triển khai/kiểm email/cổng thanh toán.

## Test case mới

| ID | Phạm vi kiểm tra tự động |
| --- | --- |
| P01 | Không ưu đãi; tổng Web/POS chung |
| P02 | Giảm SP, làm tròn một lần trên dòng, đối chiếu computed SQL |
| P03 | Bậc hóa đơn cao nhất đủ điều kiện |
| P04 | Voucher bằng/tốt hơn hóa đơn, chuẩn hóa mã, tăng lượt |
| P05 | Hóa đơn thắng voucher, không cộng chồng, đổi voucher thua khi retry bị chặn |
| P06 | SP + hóa đơn và SP + voucher, Web ghi đúng giá/%/tổng |
| P07 | Mã sai/hết hạn/hết lượt/tắt bị chặn, không ghi một phần |
| P08 | Hai transaction khác sản phẩm tranh lượt voucher cuối |
| P09 | Hai yêu cầu cùng token; retry sau giá/tồn/hiệu lực đổi, không dùng thêm voucher |
| P10 | Giá đổi yêu cầu xác nhận lại trước thu tiền |
| P11 | Khuyến mãi hết hiệu lực, sản phẩm ngừng bán, hết tồn, tiền thiếu |
| P12 | Lỗi FK header sau tăng lượt: rollback lượt/đơn/tồn |
| P13 | Trigger test cố ý lỗi dòng thứ hai: rollback cả chi tiết đầu/lượt/header/tồn |
| P14 | Web đặt, Desktop xử lý, Web thấy trạng thái; sở hữu và giá lịch sử |
| P15 | Khách hủy tranh nhân viên xác nhận; lý do, hoàn kho một lần, không hoàn voucher tự ý |
| P16/P19 | Web/POS tranh sản phẩm cuối; thêm tám vòng cạnh tranh để kiểm deadlock |
| P17/F01 | SQL/chi tiết/header/phân bổ/bảng/Excel/PDF khớp; xuất cuối sau test |
| P18 | Form POS thật áp/bỏ voucher, tổng 100−10−18=72, có ảnh; không bấm nút thu tiền UI |
| H01 | HTTP qua Kestrel: CSRF, chủ đơn, Web đặt, trạng thái Desktop → Web, hủy và stale quote |

Giữ 13 unit, 16 integration SV4 cũ, 6 render Form và bộ smoke khung UI/quyền/bản nháp/scale. Các vòng test có UI đều dùng STA context và message loop; ảnh là DrawToBitmap từ Form thật. Mức 125%/150% là mô phỏng layout, chưa thay DPI Windows thực tế. Không tự xác nhận mọi thao tác keyboard/Designer.

## File triển khai và phối hợp

- Core: sửa `KhuyenMaiRepository`, `SanPhamRepository` (overload khóa trong transaction), `HoaDonRepository`, `DonHangOnlineRepository`, `GioHangService`, `TinhKhuyenMaiService`, model `GioHang`; thêm `GiaoDichBanHang` và `XacNhanGia`.
- Desktop: `FrmPOS`, nhãn khoản giảm trong `FrmDonHang`, `XuatTaiLieu`, marshal UI trong `ShopTheme`; giữ shell, bảng màu và dashboard mới.
- Web: `DatHangController`, `DonHangController`, hai view chi tiết/xác nhận đơn; giữ Authorize/ValidateAntiForgeryToken và phạm vi SV3 khác.
- Migration: `06_SV4_POS_Request.sql` riêng; không ghép/chạy lên CSDL cửa hàng.
- Test: sửa console runner/Pump, thêm PromotionIntegration/WebHttpChecks/verify_integration_exports, reference Web + ASP.NET shared framework có sẵn; không thêm package UI hay gói test server ngoài.
- Tài liệu: cập nhật README/quy ước tích hợp, dẫn báo cáo mới và lưu bằng chứng.

SV1 cần review migration 06 và quyền thực thi `sp_getapplock`, triển khai 04/06 trên môi trường phù hợp theo quy trình nhóm. SV3 cần review quy tắc hòa ưu đãi, chính sách hoàn lượt voucher, và kiểm thử chéo với màn hình quản lý khuyến mãi của mình. SV2 cần hoàn thiện đăng nhập Web để demo qua browser bằng UI thật. Không có việc nào ở đây được tự merge hoặc tự triển khai lên CSDL cửa hàng.
