# Sơ đồ kiến trúc hệ thống

Hệ thống gồm hai ứng dụng: **BadmintonShop.Web** (ASP.NET Core MVC) cho khách hàng mua hàng qua trình duyệt và **BadmintonShop.Desktop** (WinForms) cho nhân viên/quản lý vận hành cửa hàng.
Cả hai không tự viết SQL mà cùng tham chiếu thư viện **BadmintonShop.Core** (Models, DataAccess/Repository, Services), thư viện này truy cập **một CSDL SQL Server duy nhất** bằng ADO.NET.
Tồn kho và tổng tiền do **trigger** trong CSDL cập nhật, nên dù đơn đến từ web hay từ POS thì số liệu vẫn đúng.
Web gửi email xác nhận đơn hàng qua **Gmail SMTP**.

```mermaid
flowchart LR
    KH(["Khách hàng"])
    NV(["Nhân viên / Quản lý"])
    TD["Trình duyệt web"]

    subgraph WEB["BadmintonShop.Web – ASP.NET Core MVC"]
        direction TB
        CTRL["Controllers<br/>SV2: tài khoản, sản phẩm, đánh giá<br/>SV3: giỏ hàng, đặt hàng, thanh toán"]
        VIEW["Views – Razor"]
        AUTHW["Cookie Authentication<br/>SV1: cơ chế đăng nhập khách hàng"]
        CTRL --> VIEW
    end

    subgraph DESK["BadmintonShop.Desktop – WinForms"]
        direction TB
        FDN["FrmDangNhap, FrmMain – phân quyền<br/>SV1"]
        F1["NCC, phiếu nhập, nhân viên và tài khoản, sao lưu<br/>SV1"]
        F2["Sản phẩm, danh mục, đơn hàng, POS, thống kê<br/>SV4"]
        F3["Khuyến mãi – SV3 · Duyệt bình luận – SV2"]
    end

    subgraph CORE["BadmintonShop.Core – Class Library dùng chung · SV1"]
        direction TB
        MOD["Models<br/>15 lớp ứng với 15 bảng"]
        DAL["DataAccess<br/>DbHelper + XxxRepository<br/>mỗi SV viết repository cho bảng mình"]
        SVC["Services<br/>MatKhauHelper – BCrypt, PhienDangNhap,<br/>PhanQuyen, TinhKhuyenMaiService – SV3"]
    end

    subgraph DB["SQL Server – CSDL BadmintonShop · SV1"]
        direction TB
        TBL[("15 bảng<br/>PK, FK, CHECK, chỉ mục")]
        TRG["Trigger: trg_CTPN_TangTon,<br/>trg_CTHD_GiamTon, trg_HoaDon_CapNhat<br/>Thủ tục: sp_KiemTraTonKho"]
        TRG --- TBL
    end

    SMTP[["Gmail SMTP"]]

    KH --> TD -->|"HTTP/HTTPS"| WEB
    NV --> DESK
    WEB -->|"tham chiếu project"| CORE
    DESK -->|"tham chiếu project"| CORE
    DAL -->|"ADO.NET – Microsoft.Data.SqlClient<br/>SQL có tham số, transaction"| DB
    WEB -->|"email xác nhận đơn – SV3"| SMTP
    SMTP -.-> KH

    classDef sv1 fill:#dbeafe,stroke:#1d4ed8,color:#0f172a
    classDef sv2 fill:#dcfce7,stroke:#15803d,color:#0f172a
    classDef sv3 fill:#fef3c7,stroke:#b45309,color:#0f172a
    classDef sv4 fill:#fce7f3,stroke:#be185d,color:#0f172a
    class AUTHW,FDN,F1,MOD,DAL,SVC,TBL,TRG sv1
    class CTRL,VIEW sv2
    class SMTP,F3 sv3
    class F2 sv4
```

**Chú thích màu:** xanh dương = SV1 · xanh lá = SV2/SV3 (Web) · vàng = SV3 · hồng = SV4.

## Vì sao tách thư viện dùng chung BadmintonShop.Core?

- **Viết SQL một lần, hai ứng dụng cùng dùng.** Ví dụ `SanPhamRepository.LayTheoMa` được cả trang chi tiết sản phẩm trên web lẫn màn hình POS trên desktop gọi; sửa câu truy vấn ở một chỗ thì cả hai cùng được sửa.
- **Logic nghiệp vụ không lệch nhau.** Băm/kiểm mật khẩu (`MatKhauHelper`), tính khuyến mãi (`TinhKhuyenMaiService`), lưu hóa đơn trong transaction… chỉ có một bản cài đặt, tránh tình trạng web tính một kiểu, desktop tính một kiểu.
- **Tách lớp rõ ràng, dễ phân công và dễ kiểm thử.** Controller/Form chỉ lo giao diện, không chứa SQL; mỗi thành viên viết `XxxRepository` cho bảng mình phụ trách và có thể kiểm thử Core độc lập với giao diện.
- **Ràng buộc quan trọng đặt ở CSDL** (khóa, CHECK, trigger tồn kho), nên dù dữ liệu đến từ ứng dụng nào cũng không thể bán quá tồn hay làm sai tổng tiền.
