# Sơ đồ hệ thống BadmintonShop

Các sơ đồ viết bằng [Mermaid](https://mermaid.js.org/) – GitHub và VS Code (extension Markdown Preview Mermaid Support) hiển thị trực tiếp.
Bản PNG để dán vào báo cáo Word nằm trong thư mục [`png/`](png/).

| # | Sơ đồ | Loại | Nội dung |
|:-:|---|---|---|
| 1 | [Kiến trúc hệ thống](01-kien-truc.md) | flowchart | Web – Desktop – Core – SQL Server – Gmail SMTP, phân công SV1–SV4, lý do tách thư viện dùng chung |
| 2 | [ERD](02-erd.md) | erDiagram | 16 bảng: `01_CreateDatabase.sql` + `07_SV2_DanhGiaSanPham.sql`, cột HoaDon mở rộng theo `BadmintonShop.sql` + bảng so sánh với ERD vẽ tay |
| 3 | [Lược đồ quan hệ](03-luoc-do-quan-he.md) | văn bản | Mỗi bảng 1 dòng, PK in đậm, FK in nghiêng, bảng tóm tắt khóa ngoại |
| 4 | [Lập phiếu nhập – tồn kho](04-ton-kho.md) | sequenceDiagram | FrmPhieuNhap → PhieuNhapRepository → DbHelper → SQL Server, trigger tăng tồn, rollback khi lỗi |
| 5 | [Đăng nhập Desktop](05-dang-nhap.md) | flowchart | Kiểm tra kết nối, BCrypt, tài khoản bị khóa, PhienDangNhap, phân quyền menu |

**Xuất lại PNG** (cần Node.js): tách khối ` ```mermaid ` ra file `.mmd` rồi chạy
`npx -y @mermaid-js/mermaid-cli -i <file.mmd> -o docs/diagrams/png/<ten>.png -b white -s 2`.
