-- Prepared before SQL recovery; executed after authorized restart on 06/11/2026.
-- Use sqlcmd -f 65001 for this UTF-8 script; see sql-dot5-postrestart-utf8.txt.
-- Run only after a successful read-only master connection, with sqlcmd -d
-- BadmintonShop_SV4_Test_20261105170422_f787c19b. No writes or configuration changes.
IF DB_NAME() <> N'BadmintonShop_SV4_Test_20261105170422_f787c19b'
    THROW 50001, N'Wrong catalog for historical read-only reconciliation.', 1;

SELECT DB_NAME() AS DatabaseName, SYSDATETIMEOFFSET() AS ObservedAt;
SELECT MaHD, NgayLap, TrangThai, TongTien
FROM dbo.HoaDon WHERE MaHD IN (11,12) ORDER BY MaHD;
SELECT MaSP, TenSP, SoLuongTon FROM dbo.SanPham
WHERE TenSP = N'Vợt thử' ORDER BY MaSP;
SELECT N'2025-2026' AS Period,
       COALESCE(SUM(TongTien),0) AS CompletedRevenue
FROM dbo.HoaDon
WHERE TrangThai=N'Hoàn thành' AND NgayLap>='20250101' AND NgayLap<'20270101';
SELECT N'2026-01-01 to 2026-11-05 inclusive' AS Period,
       COALESCE(SUM(TongTien),0) AS CompletedRevenue
FROM dbo.HoaDon
WHERE TrangThai=N'Hoàn thành' AND NgayLap>='20260101' AND NgayLap<'20261106';
-- Historical expectations: #11/#12 completed, racket stock 93, revenue 711 / 300.
-- A difference must be investigated; do not UPDATE to force these values.
