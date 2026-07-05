/*=====================================================
  dbo.USERS - Supplier Discount Column
  Re-runnable Script
=====================================================*/

IF COL_LENGTH('dbo.USERS', 'DISCOUNT') IS NULL
BEGIN
    ALTER TABLE dbo.USERS
    ADD DISCOUNT DECIMAL(18, 2) NULL;
END;
GO
