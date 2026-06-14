/*=========================================================
  GSTRULES TABLE
=========================================================*/

IF OBJECT_ID('dbo.GSTRULES', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.GSTRULES
    (
        Id INT IDENTITY(1,1) NOT NULL,
        ProductId INT NOT NULL,
        GstValue DECIMAL(18,2) NULL,
        StartRange DECIMAL(18,2) NOT NULL,
        EndRange DECIMAL(18,2) NULL,

        CONSTRAINT PK_GSTRULES
            PRIMARY KEY CLUSTERED (Id)
    );
END
GO

/*=========================================================
  PRIMARY KEY
=========================================================*/

IF NOT EXISTS
(
    SELECT 1
    FROM sys.key_constraints
    WHERE name = 'PK_GSTRULES'
)
BEGIN
    ALTER TABLE dbo.GSTRULES
    ADD CONSTRAINT PK_GSTRULES
        PRIMARY KEY CLUSTERED (Id);
END
GO

/*=========================================================
  FOREIGN KEY -> PRODUCTS(PRODID)
=========================================================*/

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_GSTRULES_PRODUCTS'
)
BEGIN
    ALTER TABLE dbo.GSTRULES
    ADD CONSTRAINT FK_GSTRULES_PRODUCTS
        FOREIGN KEY (ProductId)
        REFERENCES dbo.PRODUCTS (PRODID);
END
GO

/*=========================================================
  INDEX FOR FK
=========================================================*/

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_GSTRULES_ProductId'
      AND object_id = OBJECT_ID('dbo.GSTRULES')
)
BEGIN
    CREATE INDEX IX_GSTRULES_ProductId
        ON dbo.GSTRULES(ProductId);
END
GO

/*=========================================================
  ADDITIONALCHARGES TABLE
=========================================================*/

IF OBJECT_ID('dbo.ADDITIONALCHARGES', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ADDITIONALCHARGES
    (
        Id INT IDENTITY(1,1) NOT NULL,
        ProductId INT NOT NULL,
        GstValue DECIMAL(18,2) NULL,
        StartRange DECIMAL(18,2) NOT NULL,
        EndRange DECIMAL(18,2) NULL,

        CONSTRAINT PK_ADDITIONALCHARGES
            PRIMARY KEY CLUSTERED (Id)
    );
END
GO

/*=========================================================
  FOREIGN KEY -> PRODUCTS(PRODID)
=========================================================*/

IF NOT EXISTS
(
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_ADDITIONALCHARGES_PRODUCTS'
)
BEGIN
    ALTER TABLE dbo.ADDITIONALCHARGES
    ADD CONSTRAINT FK_ADDITIONALCHARGES_PRODUCTS
        FOREIGN KEY (ProductId)
        REFERENCES dbo.PRODUCTS (PRODID);
END
GO

/*=========================================================
  INDEX FOR ProductId
=========================================================*/

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_ADDITIONALCHARGES_ProductId'
      AND object_id = OBJECT_ID('dbo.ADDITIONALCHARGES')
)
BEGIN
    CREATE INDEX IX_ADDITIONALCHARGES_ProductId
        ON dbo.ADDITIONALCHARGES(ProductId);
END
GO