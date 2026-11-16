/* =====================================================================
   BadmintonShop - 10_AnhSanPham.sql
   Gan anh san pham (media/products/<ten>.jpg o goc du an) cho 40 san pham mau,
   de Web hien anh that thay vi chu cai/anh minh hoa. Chay lai nhieu lan van an toan.
   Chi cap nhat san pham chua co khoa 'products/...' (khong ghi de anh da upload).
   ===================================================================== */
USE BadmintonShop;
GO

UPDATE dbo.SanPham SET HinhAnh = 'products/astrox88d.jpg' WHERE TenSP = N'Vợt Yonex Astrox 88D Pro' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/nf700.jpg' WHERE TenSP = N'Vợt Yonex Nanoflare 700' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/tk9900.jpg' WHERE TenSP = N'Vợt Victor Thruster K 9900' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/axforce80.jpg' WHERE TenSP = N'Vợt Li-Ning Axforce 80' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/as30.jpg' WHERE TenSP = N'Cầu Yonex AS-30' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/tc77.jpg' WHERE TenSP = N'Cầu Thành Công 77' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/pc65z3.jpg' WHERE TenSP = N'Giày Yonex Power Cushion 65Z3' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/a970.jpg' WHERE TenSP = N'Giày Victor A970 ACE' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/aayu.jpg' WHERE TenSP = N'Áo thi đấu Li-Ning AAYU' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/short15048.jpg' WHERE TenSP = N'Quần short Yonex 15048' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/bg80.jpg' WHERE TenSP = N'Cước Yonex BG80' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/kg104.jpg' WHERE TenSP = N'Quấn cán Kumpoo KG-104' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/astrox99pro.jpg' WHERE TenSP = N'Vợt Yonex Astrox 99 Pro' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/arc11pro.jpg' WHERE TenSP = N'Vợt Yonex Arcsaber 11 Pro' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/auraspeed90k.jpg' WHERE TenSP = N'Vợt Victor Auraspeed 90K' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/jetspeeds12.jpg' WHERE TenSP = N'Vợt Victor Jetspeed S 12' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/windstorm72.jpg' WHERE TenSP = N'Vợt Li-Ning Windstorm 72' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/pc980.jpg' WHERE TenSP = N'Vợt Kumpoo Power Control 980' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/goldno1.jpg' WHERE TenSP = N'Cầu Victor Gold No.1' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/aplus300.jpg' WHERE TenSP = N'Cầu Li-Ning A+300' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/tc88.jpg' WHERE TenSP = N'Cầu Thành Công 88' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/mavis350.jpg' WHERE TenSP = N'Cầu nhựa Yonex Mavis 350' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/eclipsionz3.jpg' WHERE TenSP = N'Giày Yonex Eclipsion Z3' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/p9200.jpg' WHERE TenSP = N'Giày Victor P9200' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/saga.jpg' WHERE TenSP = N'Giày Li-Ning Saga' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/khe36.jpg' WHERE TenSP = N'Giày Kumpoo KH-E36' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/comfortz3.jpg' WHERE TenSP = N'Giày Yonex Comfort Z3' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/ao10504.jpg' WHERE TenSP = N'Áo thi đấu Yonex 10504' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/t33001.jpg' WHERE TenSP = N'Áo thi đấu Victor T-33001' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/aqxt.jpg' WHERE TenSP = N'Váy cầu lông Li-Ning AQXT' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/r3096.jpg' WHERE TenSP = N'Quần short Victor R-3096' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/kw2102.jpg' WHERE TenSP = N'Áo thi đấu Kumpoo KW-2102' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/aksp.jpg' WHERE TenSP = N'Quần short Li-Ning AKSP' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/vbs66n.jpg' WHERE TenSP = N'Cước Victor VBS-66N' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/lnno1.jpg' WHERE TenSP = N'Cước Li-Ning No.1' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/ac102.jpg' WHERE TenSP = N'Quấn cán Yonex AC102' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/ba82212.jpg' WHERE TenSP = N'Balo Yonex BA82212' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/br9213.jpg' WHERE TenSP = N'Túi vợt Victor BR9213' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/bangtran.jpg' WHERE TenSP = N'Băng trán Yonex' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
UPDATE dbo.SanPham SET HinhAnh = 'products/vosk.jpg' WHERE TenSP = N'Vớ Victor SK' AND (HinhAnh IS NULL OR HinhAnh NOT LIKE 'products/%');
GO
