# Quy tắc và tiến độ triển khai — đợt 1, 05/11/2026

Nguồn công việc: [Checklist TV1–TV4](checklist-tien-do-4-thanh-vien.md). Đây là đợt triển khai nền tảng báo cáo, chưa phải nghiệm thu đủ yêu cầu môn học.

## Quy tắc dùng để triển khai

| Nội dung | Quy tắc | Căn cứ / trạng thái |
|---|---|---|
| Doanh thu | Chỉ hóa đơn Hoàn thành, lấy TongTien sau giảm hóa đơn, cộng một lần theo MaHD | Giữ chính sách hiện có; cần nhóm đối chiếu với giảng viên |
| Phân kỳ bán hàng | NgayLap; A–B bao gồm cả ngày cuối và thời điểm cuối ngày | Giữ chính sách hiện có; chưa có ngày hoàn thành riêng |
| Chi | Tổng PhieuNhap.TongTien theo NgayNhap trong kỳ; không cộng lương hồ sơ, không coi là giá vốn hàng bán | Người dùng xác nhận trong chat ngày 05/11/2026: “Tiền nhập hàng trong kỳ (đề xuất theo dữ liệu hiện có)” |
| Thu trừ chi | Ghi Chênh lệch thu–chi, được phép âm; không gọi là lợi nhuận kế toán | Theo nghĩa của khoản chi đã chọn |
| Loại bán | Doanh thu lọc Online/Tại quầy như trước; thu–chi chỉ xuất hiện khi chọn Tất cả loại bán | Phiếu nhập không có nguồn phân bổ chi theo kênh |
| Phân bổ giảm hóa đơn | Chia số tiền sau giảm theo trọng số ThanhTien từng sản phẩm; lấy đồng nguyên, chia phần dư lớn nhất, hòa chọn MaSP tăng dần | Tái sử dụng PhanBo; tổng tiền dòng luôn bằng hóa đơn |
| Danh mục | Cộng từ tiền sản phẩm đã phân bổ; không phân bổ lại độc lập theo danh mục | Tránh hai cách chia phần dư lệch nhau một đồng |
| Khách/nhân viên rỗng | Giữ nhóm Khách vãng lai / Chưa gán nhân viên | Không loại giao dịch khỏi tổng |
| Bảng quý | Một bảng mỗi năm trong A–B; chỉ tính giao dịch trong khoảng lọc, quý không phát sinh bằng 0. Nút Xem cả năm đặt A–B về 01/01–31/12 | Không mở rộng ngoài khoảng lọc ngầm định |
| Danh sách sản phẩm | Sản phẩm xuất hiện trong tập hóa đơn đã lọc; giữ dòng 0 nếu chỉ có đơn hủy/chưa hoàn thành | Chưa thêm toàn bộ sản phẩm chưa từng có giao dịch |
| Tổng bảng | Hàng Tất cả là tổng; biểu đồ bỏ hàng này nhưng giữ nhóm khách/nhân viên có mã rỗng | Không cộng tổng lần hai |

Chưa chốt: phạm vi main–sub Desktop/Web, màn hình tìm kiếm thường dùng, thời hạn tái sử dụng dữ liệu (10 phút vẫn là đề xuất), phạm vi đọc Excel và chính sách nhập. Chưa có xác nhận nghiệm thu của giảng viên.

## Phần đã triển khai

- `DuLieuBaoCao` chứa hai bảng bán hàng và nhập hàng riêng. `BaoCaoRepository.LayDuLieuBaoCao` đọc cả hai trong cùng giao dịch; không JOIN phiếu nhập vào dòng bán. Dữ liệu bán thêm mã/tên nhân viên, khách hàng. API `LayDuLieu` cũ vẫn trả bảng bán và không truy vấn thêm nhập hàng.
- `BaoCaoService` thêm tổng theo quý; sản phẩm × quý theo năm có tổng hàng/cột; nhân viên × năm và khách hàng × năm có nhóm rỗng; thu–chi × quý/năm. Service kiểm tra phạm vi ngày/loại bán cả với tập dữ liệu rộng hơn khoảng báo cáo.
- Màn thống kê thêm các tab từ kết quả chung; có chọn năm và Xem cả năm. Thu–chi dùng lưới riêng vì cộng thu, chi và chênh lệch thành một tổng biểu đồ sẽ gây hiểu sai.
- XLSX/PDF nhận toàn bộ bảng mới từ cùng kết quả; XLSX giữ kiểu số. Quy tắc xuất ghi đơn vị tiền, ngày phân kỳ và nghĩa của chi.

Phân bổ danh mục mới có thể chuyển phần dư một đồng giữa các danh mục so với cách chia độc lập trước đây. Tổng doanh thu không đổi; sản phẩm và danh mục nay dùng cùng một phép phân bổ.

## Dữ liệu đối chiếu độc lập

Mẫu trong `tests/BadmintonShop.SV4.Checks/ReportProgressChecks.cs` không kết nối DB. Đáp án dưới đây ghi tay, không lấy kết quả chương trình làm đáp án.

Hóa đơn 1 có trọng số 100:100, tổng sau giảm 101: SP1 nhận 51, SP2 nhận 50. Các hóa đơn hoàn thành còn lại: 60 (SP1, cuối Q1/2025), 90 (SP2, đầu Q2/2025), 120 (SP1, đầu Q3/2025), 40 (SP2, cuối Q4/2025), 70 (SP1, đầu 2026), 30 (SP2, Q3/2026). Đơn hủy 999, đơn chờ 888 và giao dịch đầu 2027 không tính vào doanh thu 2025–2026.

| Năm / sản phẩm | Q1 | Q2 | Q3 | Q4 | Tổng |
|---|---:|---:|---:|---:|---:|
| 2025 / SP1 | 111 | 0 | 120 | 0 | 231 |
| 2025 / SP2 | 50 | 90 | 0 | 40 | 180 |
| 2025 / Tất cả | 161 | 90 | 120 | 40 | 411 |
| 2026 / SP1 | 70 | 0 | 0 | 0 | 70 |
| 2026 / SP2 | 0 | 0 | 30 | 0 | 30 |
| 2026 / Tất cả | 70 | 0 | 30 | 0 | 100 |

Tổng doanh thu hai năm: **511**. Chi nhập: **500** năm 2025 (Q1: 200, Q2: 300), **150** năm 2026 (Q4); chênh lệch lần lượt **−89**, **−50**. Nhân viên 1: 231; chưa gán nhân viên: 120; khách vãng lai: 90. Khoảng 01/04–30/06/2025 chỉ có thu 90, chi 300. Kỳ 12/2026 chỉ có chi 150, thu 0.

## Kiểm tra và giới hạn

Chạy tại thư mục `BadmintonShop`:

```text
dotnet build BadmintonShop.sln --no-restore -v minimal -m:1 /p:UseSharedCompilation=false
dotnet run --project tests/BadmintonShop.SV4.Checks --no-build
```

R01–R07 kiểm tra sản phẩm/quý, nhiều năm, kỳ rỗng, biên ngày, trạng thái hóa đơn, phân bổ đồng nguyên, nhóm khách/nhân viên rỗng, chi riêng, lọc loại bán, khoảng ngày đảo, số tiền bảng = XLSX, mở lại PDF và vẽ biểu đồ với hàng tổng. Log mới: [minh-chung-dot-1/results.txt](minh-chung-dot-1/results.txt).

Lần chạy sau thay đổi cuối: build thành công, 0 lỗi; còn cảnh báo CS8602 đã có tại `FrmKhuyenMai.cs:278`. Bộ kiểm tra **20 đạt, 0 lỗi** (13 kiểm tra hiện có và 7 kiểm tra báo cáo mới). Không truyền cờ integration, không tạo hoặc ghi DB trong lần chạy này.

Chưa chạy SQL trên DB demo riêng; chưa đối chiếu thao tác giao diện với DB, chưa xác minh trực quan PDF hoặc PDF nhiều năm có rất nhiều cột. DB demo nhiều năm vẫn cần chuẩn bị. Mở lại PDF chưa chứng minh đầy đủ bố cục và số liệu PDF.

Tiếp theo: kiểm tra dữ liệu chung trên DB demo; bộ nhớ/làm mới và log truy vấn; main–sub; tìm nâng cao; nhập Excel; bố cục xuất nhiều năm; nghiệm thu và tài liệu bảo vệ.
