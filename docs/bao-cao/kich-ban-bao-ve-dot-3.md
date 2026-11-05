# Kịch bản bảo vệ và bàn giao

Thời lượng đề xuất 15–20 phút. Đây là kịch bản đã chuẩn bị, **chưa xác nhận bốn thành viên đã tự chạy trọn**. Kết quả kỹ thuật và giới hạn tại [đợt 3](quy-tac-va-tien-do-dot-3.md).

Cập nhật đợt 5: dùng [biên bản mới](quy-tac-va-tien-do-dot-5.md) để minh họa lượt #12 mới trên DB `...170422_f787c19b`: báo cáo UI 200 → 300, tồn 94 → 93, tổng hai năm 711; file xuất mới có snapshot 300 trong kỳ 01/01–05/11/2026. Mạng chậm/lỗi/phục hồi đã kiểm tra qua proxy loopback. Test host vẫn bỏ qua đăng nhập Desktop sản phẩm; các thành viên tự thực hành lượt sản phẩm và ký nhận riêng.

Cập nhật đợt 4: Codex đã xử lý UI đơn #12 (đặt ở đợt 3) qua đủ nút/MessageBox Desktop và kiểm tra báo cáo. DB UI `...161022_7f4ab55a` hiện thu 2025–2026 là **711**, #11/#12 đều Hoàn thành; không dùng số 611 cho DB này nữa. Kỳ 01/01–05/11/2026 tăng 200 → 300. Xem [biên bản đợt 4](quy-tac-va-tien-do-dot-4.md), [nguồn báo cáo 7 nhóm](bao-cao-ky-thuat-7-nhom.md), [hướng dẫn bàn giao](huong-dan-ban-giao-dot-4.md). Lượt nối hai buổi qua test host không thay cho nhóm tự chạy một lượt mới có đăng nhập Desktop sản phẩm và xuất lại file.

## Chuẩn bị

Giữ DB mẫu nguyên trạng `BadmintonShop_SV4_Test_20261105154154_a5e27e92` chỉ để đối chiếu 511/650. Tạo DB mới bằng `--acceptance --server .\SQLEXPRESS` cho thao tác ghi; ghi tên DB từ log. Cài đủ schema/migration 04/06/07. Không chạy script tổng hay 01 lên DB có sẵn. Không sửa App.config/appsettings.json.

Đặt ConnectionStrings__BadmintonShop **cho riêng từng tiến trình** Web/Desktop, cùng server/catalog; chỉ chiếu chuỗi Integrated Security của DB thử. Dùng cùng Windows user/thư mục tín hiệu. Tài khoản thử Desktop `sv4test` / `TestOnly@123`; khách trong DB acceptance `acceptance@example.invalid` / `TestOnly@123`. Không sử dụng thông tin cá nhân hoặc mật khẩu thật.

Chuẩn bị file nhập từ mẫu **của DB hiện dùng** (MaDM có thể khác), một file đúng và một file sai giá; tên sản phẩm mới để không trùng lần chạy trước. Mở sẵn phần mềm đọc XLSX/PDF.

## TV1 — Kết nối và dữ liệu, 3 phút

1. Chỉ Desktop Program.cs lấy biến môi trường trước App.config; Web Program.cs GetConnectionString qua configuration.
2. Chỉ DbHelper.TaoKetNoi, adapter Fill và ExecuteNonQuery/ExecuteTransaction; BaoCaoRepository đọc bán/nhập riêng trong transaction.
3. Đối chiếu mẫu 2025 thu 411/chi 500, 2026 thu 100/chi 150, tổng 511/650. Đơn hủy/chờ không tính doanh thu. NgayLap quyết định phân kỳ; A–B tính cả ngày cuối.
4. Trình bày giảm hóa đơn phân bổ xuống sản phẩm, tổng dòng khớp tổng hóa đơn; nhóm chưa gán nhân viên/khách vãng lai vẫn có trong tổng.

## TV2 — Báo cáo, 4 phút

1. Chọn 01/01/2025–31/12/2026, tất cả loại bán; mở sản phẩm × quý từng năm, nhân viên × năm, khách × năm và thu–chi.
2. Chỉ quý rỗng bằng 0, tổng hàng/cột; thu trừ chi -139 trên mẫu nguyên trạng. Không gọi đây là lợi nhuận kế toán.
3. Đổi loại bán Online: chi nhập không tự phân bổ theo kênh, bảng thu–chi không xuất hiện.
4. Xuất XLSX/PDF cùng snapshot, mở bằng phần mềm đọc; đối chiếu sản phẩm × quý và tổng 511 (hoặc số hiện tại nếu dùng DB đã có giao dịch).

## TV3 — Bộ nhớ, lọc và main–sub, 4 phút

1. Dùng SQL diagnostics của bộ Checks: danh sách sản phẩm nạp 1 truy vấn; tìm 10 từ khóa/chọn ảnh thêm 0; tải lại thêm 1. Không mở SQL Profiler trên DB cửa hàng.
2. Báo cáo cùng bộ lọc thêm 0, Làm mới thêm 2. TTL đúng biên 10 phút được kiểm tra bằng đồng hồ tiêm, không nói đã đợi thật 10 phút.
3. Chọn ô bất kỳ của hóa đơn/phiếu nhập Desktop; chi tiết đúng mã. Web Đơn hàng của tôi: chọn ô/Enter hiện chi tiết trong cùng trang, không thấy đơn người khác.
4. Tìm nâng cao: tổng >=1.000.000 AND <=2.000.000 gồm hai biên; địa chỉ TP.HCM OR Hà Nội; NOT trạng thái Đã hủy; nêu các toán tử còn lại. Địa chỉ là so toàn bộ chuỗi, không tách thành phố.

## TV4 — Nhập/xuất, 3 phút

1. Tải mẫu qua Save As, chọn file lỗi: chỉ rõ dòng/cột, xác nhận khóa và chưa ghi.
2. Chọn file đúng → preview → kiểm tra giá/danh mục → xác nhận. Tồn ban đầu 0; mã tự sinh; không cập nhật sản phẩm trùng.
3. Chỉ minh chứng rollback toàn file và quyền nhập trong DB-X01/02; không dùng một file đúng để chứng minh mọi lỗi.
4. Mở XLSX/PDF, chỉ đơn vị, bộ lọc, tổng và PDF chia phần nhiều năm; Tổng toàn kỳ lặp ở mỗi phần không cộng lại.

## Cả nhóm — Luồng hai ứng dụng, 4–6 phút

1. Mở Web và Desktop riêng trên **cùng DB thử ghi**, ghi tên DB và phiên bản mã. Mở thống kê trước khi khách đặt để có snapshot nền B.
2. Khách đặt một sản phẩm COD giá cuối P qua Web. Ghi MaHD, tồn trước/sau; Web hiện Chờ xác nhận. Doanh thu vẫn B.
3. Desktop tải/lọc, chọn đúng MaHD rồi bấm Xác nhận, Giao hàng, Hoàn thành; đọc từng MessageBox trước khi đồng ý. Kiểm tra Web theo dõi từng trạng thái.
4. Xem báo cáo cùng điều kiện sau ghi: tổng B+P; bấm lại dùng snapshot, Làm mới vẫn B+P. Nếu đơn nằm ngoài khoảng ngày hoặc chưa Hoàn thành, giải thích tại sao không tăng.
5. Xuất lại và đối chiếu file với snapshot mới; lưu ảnh Web, Desktop, báo cáo, file và log. Điền Đạt/Chưa đạt, người thực hiện/ngày, tên DB. **Chỉ lúc hoàn tất bước này mới xem xét đánh dấu CHUNG-02.**

## Câu hỏi dự kiến và điều kiện bàn giao

| Câu hỏi | Trả lời cần giữ nhất quán |
|---|---|
| Chi là gì? | Tiền nhập theo NgayNhap trong kỳ, không suy từ lương và không phải giá vốn bán. |
| Có thể dùng tồn trong cache để bán không? | Không; lúc ghi phải kiểm tra giá/tồn hiện hành dưới khóa/giao dịch SQL. |
| SQL bên ngoài ứng dụng có báo tín hiệu không? | Không; cần Làm mới/TTL. Khác máy cần thư mục chung, chưa nghiệm thu share mạng. |
| Đã xong toàn môn chưa? | Chưa: chính sách/phạm vi, bàn giao, lượt mới của thành viên với đăng nhập Desktop sản phẩm/xuất snapshot mới và tập bảo vệ còn mở. Phần UI Desktop #12 đã hoàn tất đợt 4. |
| 96 + 3 là 99 kiểm tra độc lập? | Không. Các lần có phần trùng, ghi riêng theo phạm vi và mã nguồn. |

TV1/TV2 bàn giao schema kết quả, bộ lọc và đáp án; TV3 bàn giao log SQL/quyền; TV4 bàn giao mẫu/file xuất/ảnh. Bên nhận tự chạy và ký kết quả. Chỉ xuất bộ báo cáo Word/PDF/hình cuối sau chốt nội dung; không dùng kết quả lịch sử thay nghiệm thu bản mới.
