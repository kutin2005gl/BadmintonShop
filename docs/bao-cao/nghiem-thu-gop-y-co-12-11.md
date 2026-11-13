# Nghiệm thu góp ý của cô ngày 12/11 (CO-01 đến CO-06)

Ngày thực hiện: **13/11/2026**. Tài liệu này ghi **chính sách đã chốt trong mã nguồn** và **bằng chứng kiểm tra** cho các mục `CO-01` đến `CO-06` của [checklist](checklist-theo-phan-cong-sv1-sv4.md). `CO-07` (từng thành viên tự thử và luyện demo) cần cả nhóm tự thao tác nên không nằm ở đây.

Các cột "Người kiểm", "Ngày" trong checklist do thành viên nhóm tự điền sau khi chạy lại các bước dưới đây trên máy mình.

## CO-01 Mua hàng không cần đăng nhập

- Trang đặt hàng cho khách chưa đăng nhập nhập **họ tên, số điện thoại, địa chỉ**; khách đã đăng nhập giữ nguyên luồng cũ.
- Số điện thoại được chuẩn hóa về dạng `0xxxxxxxxx` (bỏ khoảng trắng, dấu chấm, `+84`). Có khách trùng SĐT thì **dùng lại**, không tạo khách trùng và không ghi đè họ tên, địa chỉ hồ sơ đã có.
- Chưa có khách thì tạo `KhachHang` mới với email giả `khach<SĐT>@khach.invalid` và mật khẩu ngẫu nhiên: không đăng nhập được, không nhận email xác nhận.
- Khách vãng lai chỉ xem lại được **đơn vừa đặt trong phiên đó**; đơn khác cùng SĐT và trang lịch sử `/DonHang` bị chặn.
- Bằng chứng: đặt 2 đơn bằng `0900 000 777` và `+84900000777` chỉ tạo 1 khách; SĐT `12` bị từ chối; xem đơn từ phiên khác trả 404.

## CO-02 Chính sách voucher

| Loại | Bảng | Giới hạn tổng | Giới hạn mỗi khách | Thời hạn | Điều kiện hóa đơn |
|---|---|---|---|---|---|
| **Voucher (mã)** `VOUCHER` | `KM_TheoVoucher` | `SoLuong` lượt, trừ `DaDung` | **1 lần mỗi khách** cho mỗi mã | `NgayBatDau`–`NgayKetThuc`, `TrangThai` bật | không |
| **Theo hóa đơn** `HOADON` | `KM_TheoHD` | không giới hạn | không giới hạn, áp dụng cho mọi đơn đủ điều kiện | như trên | `TongTienToiThieu` (bậc cao nhất đạt được) |
| **Theo sản phẩm** `SANPHAM` | `KM_TheoSP` | không giới hạn | không giới hạn | như trên | không |

- Voucher sai, chưa mở, hết hạn, đã tắt, hết lượt đều bị từ chối với thông báo riêng.
- "Mỗi khách một lần" tính theo các đơn **chưa hủy** đã gắn voucher đó (`HoaDon.MaCTKM`), nên hủy đơn thì khách được dùng lại mã. Kiểm tra chạy trong transaction đặt hàng, sau khi giữ khóa dòng voucher, nên hai đơn đồng thời của cùng một khách bị xếp hàng; bị từ chối thì rollback, không mất lượt.
- Khách đã đăng nhập thấy lỗi ngay ở giỏ hàng; khách vãng lai bị chặn lúc bấm Đặt hàng (lúc đó mới biết khách qua SĐT).
- **POS không chọn khách**, nên chỉ áp giới hạn tổng lượt, ngày và trạng thái; không giới hạn mỗi khách.
- **Hủy đơn không hoàn `DaDung`** (đúng như hành vi cũ, đã ghi ở checklist bàn giao SV4); chỉ lượt "mỗi khách" được giải phóng.
- Ví dụ trong lời cô (hóa đơn trên 300 nghìn giảm 15 nghìn) chỉ minh họa cho loại `HOADON`, không cố định ngưỡng.
- Bằng chứng (web, DB cục bộ, voucher thử 5 lượt): khách A dùng lần 1 được; lần 2 bị từ chối, `DaDung` không đổi; khách B dùng được; A hủy đơn rồi dùng lại được.

## CO-03 Main–sub đơn hàng

- **Desktop** (`FrmDonHang`): lưới chính/phụ, chọn bất kỳ ô nào của dòng thì nạp đúng chi tiết đơn; danh sách rỗng thì lưới phụ bị xóa. Bài kiểm tra `DB-M01` bấm từng ô của các dòng đầu và so với DB: đạt.
- **Web** (`/DonHang`): bấm dòng hoặc Enter/Space thì nạp chi tiết ngay bên dưới; bấm nhanh nhiều dòng thì chỉ giữ kết quả của dòng cuối; chi tiết đơn người khác trả 404.
- Đã sửa: bản nhúng không còn lặp tiêu đề `h1`, link "← Đơn hàng của tôi" và `container` lồng nhau; **hủy đơn từ panel nhúng quay về danh sách** và hiện thông báo, không nhảy sang trang chi tiết.

## CO-04 Quan hệ khách hàng–hóa đơn

- `HoaDon` chỉ có **`MaKH`** (khóa ngoại `FK_HoaDon_KH` tới `KhachHang`, cho phép NULL) và `DiaChiGiao` là địa chỉ **của riêng đơn đó**; không có cột sao chép họ tên, SĐT, email.
- Họ tên, SĐT, email được **ghép bằng `JOIN KhachHang`** khi hiển thị (`HoaDonRepository.LayDanhSachHienThi`); `MaKH` NULL hiển thị "Khách vãng lai".
- `MaKH` NULL chỉ còn dùng cho **đơn POS tại quầy**; đơn web của khách chưa đăng nhập (CO-01) có `MaKH` của khách tạo theo SĐT.
- Lưới đơn hàng Desktop chỉ hiện tên và SĐT khách; email và địa chỉ hồ sơ xem ở dòng thông tin khi chọn đơn (không đổ cả hồ sơ khách ra lưới).
- Giữ `DiaChiGiao` vì khách có thể giao đến địa chỉ khác hồ sơ.
- `docs/ERD.md` đã cập nhật ghi chú `MaKH`. Hình ERD PNG/Word vẫn là bản nền, cần xuất lại trước khi nộp (đã ghi trong ERD).

## CO-05 Tồn kho sau nhập và bán

Kịch bản chạy trên DB thật trong một transaction rồi `ROLLBACK` (SP 1, tồn đầu 20):

| Bước | Tồn |
|---|---|
| Nhập 10 (phiếu nhập) | 30 |
| Bán Web 3 | 27 |
| Bán POS 2 | 25 |
| Hủy đơn Web (hoàn 3) | 28 |
| Bán vượt tồn | bị chặn: "Sản phẩm không đủ tồn kho" |
| Sau `ROLLBACK` | 20 (DB không đổi) |

Tồn kho do trigger `trg_CTPN_TangTon`, `trg_CTHD_GiamTon`, `trg_HoaDon_CapNhat` cập nhật nên nhất quán giữa Web, POS và Desktop. Màn Sản phẩm có nút **Tải lại**; bộ nhớ đệm bị vô hiệu khi dữ liệu đổi (bài `DB-C05`, `DB-C04`).

## CO-06 Thống kê

- Mở màn hình tự nạp **từ đầu năm hiện tại đến hôm nay**, có ô chọn năm ("Xem cả năm").
- Dòng tổng trên đầu màn hình hiện **Doanh thu, Thu, Chi nhập hàng, Chênh lệch** khi chọn "Tất cả loại bán".
- Bảng **Bán chạy** sắp giảm dần theo số lượng.
- Tab **Sản phẩm × quý {năm}**: **chọn một sản phẩm thì biểu đồ đổi sang doanh số Q1–Q4 của đúng sản phẩm đó**; dòng "Tất cả" là toàn cửa hàng. Đổi năm hoặc khoảng lọc thì bảng và biểu đồ dựng lại, không giữ lựa chọn cũ; năm không có giao dịch hiển thị bằng 0.
- Bằng chứng: bài `U11b` kiểm tra Q1–Q4 của từng dòng khớp bảng và tổng 4 quý bằng cột Tổng.
