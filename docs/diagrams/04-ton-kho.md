# Sơ đồ tuần tự – Lập phiếu nhập hàng (Desktop)

Nhân viên lập phiếu nhập trên `FrmPhieuNhap`; form tự tính thành tiền/tổng tiền để xem trước, rồi gọi `PhieuNhapRepository.Them`.
Repository ghi `PhieuNhap` và từng dòng `ChiTietPhieuNhap` trong **một transaction** thông qua `DbHelper.ExecuteTransaction`.
Code **không tự cộng tồn kho**: mỗi lần INSERT chi tiết, trigger `trg_CTPN_TangTon` tăng `SanPham.SoLuongTon` và tính lại `PhieuNhap.TongTien`.
Có lỗi ở bất kỳ bước nào thì toàn bộ bị rollback – không có phiếu "nửa vời", tồn kho giữ nguyên.

```mermaid
sequenceDiagram
    autonumber
    actor NV as Nhân viên
    participant F as FrmPhieuNhap
    participant R as PhieuNhapRepository
    participant H as DbHelper.ExecuteTransaction
    participant DB as SQL Server

    NV->>F: Chọn nhà cung cấp, thêm dòng sản phẩm, số lượng, đơn giá nhập
    F->>F: Gộp dòng trùng sản phẩm, tính ThanhTien và TongTien để hiển thị
    NV->>F: Bấm Lưu
    F->>R: Them(pn, chiTiet) với pn.MaNV = PhienDangNhap.MaNV
    R->>R: Kiểm tra có ít nhất 1 dòng và không trùng MaSP

    alt Dữ liệu không hợp lệ
        R-->>F: ArgumentException
        F-->>NV: Báo lỗi, không ghi gì vào CSDL
    else Dữ liệu hợp lệ
        R->>H: ExecuteTransaction(congViec)
        H->>DB: Mở kết nối và BEGIN TRANSACTION
        H->>R: Gọi congViec(conn, tran)
        R->>DB: INSERT PhieuNhap (NgayNhap, MaNCC, MaNV) rồi SELECT SCOPE_IDENTITY()
        DB-->>R: MaPN mới

        loop Mỗi dòng chi tiết
            R->>DB: INSERT ChiTietPhieuNhap (MaPN, MaSP, SoLuong, DonGia)
            activate DB
            Note over DB: trg_CTPN_TangTon (AFTER INSERT)<br/>SanPham.SoLuongTon tăng thêm SoLuong<br/>PhieuNhap.TongTien = tổng ThanhTien
            deactivate DB
        end

        alt Tất cả lệnh thành công
            R-->>H: MaPN
            H->>DB: COMMIT
            H-->>R: MaPN
            R-->>F: MaPN
            F-->>NV: Thông báo lưu thành công, làm mới danh sách
        else Lỗi SQL – vi phạm FK hoặc CHECK, mất kết nối
            DB-->>H: SqlException
            H->>DB: ROLLBACK
            Note over DB: Không có PhieuNhap mới,<br/>SoLuongTon giữ nguyên
            H-->>R: ném lại SqlException
            R-->>F: SqlException
            F-->>NV: Báo lỗi, phiếu chưa được lưu
        end
    end
```

> Ghi chú: `FrmPhieuNhap` là form dự kiến của tuần 3 (ROADMAP 3.1); `PhieuNhapRepository.Them` và `DbHelper.ExecuteTransaction` đã có trong Core.
