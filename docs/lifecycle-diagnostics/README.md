# Điều tra lỗi Dispose / CreateHandle — 22/10/2026

**Chưa tái hiện được đúng exception người dùng báo, nên chưa xác định được lời gọi/file/dòng gây lỗi gốc và chưa khẳng định đã sửa dứt điểm.** Các chỉnh sửa dưới đây là bảo vệ vòng đời có kiểm tra, cùng log để lấy bằng chứng nếu lỗi tái diễn; không lấy thông báo lỗi làm bằng chứng quy kết nguyên nhân.

## Kiểm tra ban đầu và debugger

- Không tìm thấy AGENTS.md trong project hoặc thư mục cha áp dụng. Đã kiểm Git trước khi sửa. Giữ nhánh `codex/sv4-sv3-integration`, toàn bộ giao diện/POS/khuyến mãi đang làm và App.config cá nhân. Không commit/push/merge.
- Visual Studio 18 Community đang mở đúng solution, ban đầu ở Design mode. Dùng API COM DTE/Debugger3, engine **Managed (.NET Core, .NET 5+)**. Đặt Common Language Runtime Exceptions → `System.InvalidOperationException` → **BreakWhenThrown=true**; lượt attach sau đọc lại giá trị True. Khi xong đã trả giá trị về False như trước.
- Các lượt tour hoàn chỉnh chạy `BadmintonShop.Desktop.Program.Main` thật qua harness STA trong process debugger: form đăng nhập, handler xác thực fixture, vòng ShowDialog → Application.Run(FrmMain), các menu, Close và đăng xuất. Không thay code xử lý đăng nhập để chạy tour; driver nằm trong project test và chỉ chấp nhận CSDL tên `BadmintonShop_SV4_Test_*`.
- Đã attach trực tiếp `BadmintonShop.Desktop.exe` ở màn hình đăng nhập; thử cài driver bằng function evaluation gặp hạn chế native-evaluation của debugger. Vì vậy **không tính lượt attach trực tiếp đó là tour hoàn chỉnh của executable độc lập**. Bằng chứng tour hoàn chỉnh là các lượt harness gọi entry point thật nói trên.
- Tour trước và sau bảo vệ vòng đời: **mỗi lượt 2 lần đăng nhập, 144 lần điều hướng**, mở/đóng/tạo lại Sản phẩm, Danh mục, Đơn hàng, POS, Thống kê, Tổng quan, Nhà cung cấp, Phiếu nhập, Nhân viên/tài khoản. POS được giữ lại khi chuyển mục. Không có first-chance InvalidOperationException được ném trong các tour này; debugger không có lần dừng tương ứng với lỗi được báo.
- CSDL tour: `BadmintonShop_SV4_Test_20261022163229_52671bee`. Chỉ đăng nhập/đọc/điều hướng, không tạo đơn/nhập hàng/sửa dữ liệu cửa hàng.

Bằng chứng: [attach debugger](debugger-attach.txt), [tour trước](debugger-before-hardening.txt), [tour sau](debugger-after-hardening.txt). `THROWN` trong hai tour là 0. Không có stack trace lỗi gốc để đưa ra hoặc gán cho một dòng cụ thể.

## Rà soát code và chỉnh sửa cụ thể

| File / vị trí | Phát hiện và thay đổi |
| --- | --- |
| `FrmMain.Shell.cs`, `HienTrang` khoảng dòng 75 | Cache cũ giữ entry/currentPage của trang đã Close cho đến lần click kế tiếp. Giờ FormClosed/Disposed gỡ đúng instance khỏi cache và bỏ currentPage nếu khớp. Trang mới không bị callback của instance cũ xóa nhầm. Không Dispose thủ công trang còn đang được hiển thị. |
| `FrmMain.Shell.cs`, `BoTriTrang` khoảng dòng 99 | Callback Resize nay bỏ qua owner/currentPage đang Disposing/đã Dispose; không gọi Bounds/Hide trên một trang đã chết. |
| `ShopTheme.cs`, `LoadState` và `Load` khoảng dòng 17/96 | Đánh dấu Closed từ FormClosed, không đợi IsDisposed. Tác vụ đã chạy vẫn tự hoàn tất/giải phóng connection theo using, nhưng không được bind khi trang đã đóng. Không cắt transaction đang ghi. |
| `ShopTheme.cs`, `OnUi` khoảng dòng 80 | Giữ marshal qua control đích và kiểm tra trạng thái đóng trong cả callback trên UI thread. Chỉ coi lỗi invoke do đối tượng đã kết thúc là callback bị bỏ; không xếp exception chứa CreateHandle vào nhánh này. |
| `QuyenForm.cs`, `KiemTraKhiMo` khoảng dòng 20 | Giữ deferred Close có sẵn: kiểm quyền trong OnLoad, nên Close được post để bước hiển thị hiện tại trả về. Delegate nay kiểm lại disposed/disposing/handle vì owner có thể đã đóng trang trước khi nó được dispatch. Không thêm delay tùy ý. |
| `Program.cs`, `FrmMain.cs`, `FrmDangNhap.cs`, `ShopTheme.Apply` | Cài logger và đăng ký sự kiện vòng đời. Không thay quy tắc login, tính tiền, schema hoặc cấu hình kết nối. |
| `LifecycleDiagnostics.cs` (mới) | First-chance/unhandled diagnostics và ring buffer chuyển trang/tạo-hủy handle/load/close. Chỉ quan sát, không Close/Dispose, không pump message loop, không thay cơ chế xử lý exception của WinForms. |

Đã kiểm các `using` trong Program và form thêm/sửa/đổi mật khẩu/sao lưu: đó là scope của cửa sổ modal hoặc root Application.Run; trang cache trong HienTrang không nằm trong using. Không có bằng chứng để quy kết chúng là nguồn lỗi. Các stack/frame thật của lỗi gốc vẫn cần lấy ở lần tái diễn.

Không thay BeginInvoke bằng Task.Delay hoặc nuốt exception để làm biến mất thông báo. Không đặt catch rỗng. Mọi sửa bảo vệ kể trên **không được coi là chứng minh nguyên nhân gốc**.

## Kết quả kiểm tra sau sửa

- Build toàn solution qua OutDir riêng: **0 warning, 0 error**.
- [31 kiểm tra unit/vòng đời/UI đạt](lifecycle-tests.txt): gỡ cache khi đóng, tạo instance mới, đóng trang khi query đang chờ, đăng xuất bắt buộc trong khi tải, bỏ callback của owner cũ, hide trong khi tải vẫn bind trên UI thread, Dispose trang ẩn khi query chưa xong, giữ giỏ và bản nháp khi chuyển menu, quyền nhân viên và mở các màn hình SV4. Bật `Control.CheckForIllegalCrossThreadCalls=true` trong kiểm tra vòng đời.
- Các truy vấn chậm trong test được điều khiển bằng ManualResetEventSlim, không dùng thời gian trễ ngẫu nhiên làm cách sửa. Timer 150 ms chỉ thuộc **driver kiểm thử**, không phải trì hoãn vòng đời production.
- L03 kiểm **đăng xuất bắt buộc** khi tải; chưa tự động hóa mọi nhánh Yes/No của hộp thoại đăng xuất thường khi đang có giỏ chưa lưu. Không nhận đã kiểm toàn bộ thao tác tay/Designer/DPI Windows thực tế.
- [68 kiểm tra hồi quy POS/khuyến mãi/Web–Desktop đạt](integration-regression.txt) sau thay đổi. Chạy trên CSDL mới `BadmintonShop_SV4_Test_20261022170125_cf1aea86`, không phải CSDL cửa hàng.
- [Log probe](SYNTHETIC-diagnostic-probe.log) kiểm chứng logger lưu đầy đủ first-chance/throw stack. **Đây là exception chủ động tạo trong test L06, không phải lỗi Dispose/CreateHandle người dùng gặp, không dùng để nhận đã tái hiện lỗi.**

## Lấy stack trace ở lần lỗi tái diễn

Ứng dụng tự ghi exception InvalidOperationException có thông báo chứa `CreateHandle` vào:

```text
%LOCALAPPDATA%\BadmintonShop\Diagnostics\lifecycle-<thời điểm>-<pid>.log
```

Log gồm exception đầy đủ, stack tại lúc ném, thread ID, MVID xác định build và 120 sự kiện gần nhất: navigation, Load/Shown, HandleCreated/HandleDestroyed, FormClosing/FormClosed/Disposed, bắt đầu/kết thúc tải. Cờ `cancel` ở FormClosing là giá trị tại thời điểm logger được gọi, không phải kết quả cuối của các handler phía sau. Không ghi ô mật khẩu hay chuỗi kết nối.

Nếu cần lấy cả các InvalidOperationException xảy ra trước lỗi cuối, đặt biến **trong phiên PowerShell dùng để khởi chạy ứng dụng**:

```powershell
$env:BadmintonShop__LifecycleTrace='1'
& 'D:\Desktop\BadmintonShop-Lifecycle-build\BadmintonShop.Desktop.exe'
```

Không cần sửa App.config. Có thể đặt thêm `BadmintonShop__LifecycleTraceDirectory` trước khi chạy để chọn thư mục log có quyền ghi. Nếu không có exception phù hợp thì logger không tạo tệp.

Trong Visual Studio: đặt Desktop làm Startup Project, mở **Debug → Windows → Exception Settings** (Ctrl+Alt+E), tìm `System.InvalidOperationException`, bật **Thrown**, chạy/attach ứng dụng. Khi dừng, giữ nguyên trạng thái để xem `$exception.ToString()` và Call Stack (hiện external code nếu cần). Đối chiếu log cùng PID/build trước khi sửa dòng Dispose/Close. Không chỉ lấy exception xuất hiện cuối cùng nếu đang unwind một lỗi khởi tạo trước đó.

Lệnh lặp lại kiểm tra vòng đời chỉ trên CSDL test đã có:

```powershell
dotnet build BadmintonShop.sln --no-restore -p:OutDir=D:\Desktop\BadmintonShop-Lifecycle-build\
$env:ConnectionStrings__BadmintonShop='Server=.;Database=BadmintonShop_SV4_Test_20261022163229_52671bee;Integrated Security=True;TrustServerCertificate=True;'
dotnet D:\Desktop\BadmintonShop-Lifecycle-build\BadmintonShop.SV4.Checks.dll --lifecycle-checks
```

Các biến ở ví dụ chỉ có hiệu lực trong process/shell hiện tại. Không ghi đè cấu hình cá nhân, không chạy migration lên CSDL cửa hàng. Chưa xác định nguyên nhân gốc là giới hạn còn lại của đợt điều tra này.
