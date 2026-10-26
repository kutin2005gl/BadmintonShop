/* SV4: preserve the exact confirmed request for idempotent POS retry, including a losing voucher.
   Run after 04_SV4_Migration.sql. Additive, repeatable. Never automatically run on a store database. */
USE BadmintonShop;
GO
IF COL_LENGTH('dbo.HoaDon','DauVanTayPOS') IS NULL
    ALTER TABLE dbo.HoaDon ADD DauVanTayPOS VARCHAR(64) NULL;
GO
