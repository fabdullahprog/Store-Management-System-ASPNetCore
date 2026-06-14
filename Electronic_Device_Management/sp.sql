-- 1. Create a Table Type for Order Details
-- This allows us to pass the entire list of products in one parameter
CREATE TYPE dbo.OrderDetailType AS TABLE (
    ProductCategoryId INT,
    ProductId INT,
    OrderQuantity INT,
    OrderUnit NVARCHAR(50),
    UnitPrice DECIMAL(18, 2),
    Amount DECIMAL(18, 2)
);
GO

-- 2. Stored Procedure: Create Order
CREATE PROCEDURE sp_InsertOrder
    @CustomerName NVARCHAR(100),
    @ContactNumber NVARCHAR(20),
    @ContactAddress NVARCHAR(MAX),
    @OrderDetails dbo.OrderDetailType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- A. Handle Customer (Create if not exists)
        DECLARE @CustomerId INT;
        SELECT @CustomerId = CustomerId FROM Customers WHERE ContactNumber = @ContactNumber;

        IF @CustomerId IS NULL
        BEGIN
            INSERT INTO Customers (CustomerName, ContactNumber, ContactAddress)
            VALUES (@CustomerName, @ContactNumber, @ContactAddress);
            SET @CustomerId = SCOPE_IDENTITY();
        END

        -- B. Insert Master Order
        DECLARE @OrderId INT;
        DECLARE @TotalAmount DECIMAL(18,2) = (SELECT SUM(Amount) FROM @OrderDetails);

        INSERT INTO Orders (CustomerId, OrderDate, TotalAmount)
        VALUES (@CustomerId, GETDATE(), @TotalAmount);
        SET @OrderId = SCOPE_IDENTITY();

        -- C. Insert Details and Update Stock
        INSERT INTO OrderDetails (OrderId, ProductCategoryId, ProductId, OrderQuantity, OrderUnit, UnitPrice, Amount)
        SELECT @OrderId, ProductCategoryId, ProductId, OrderQuantity, OrderUnit, UnitPrice, Amount
        FROM @OrderDetails;

        -- D. Update Stock
        UPDATE P
        SET P.AvailableQuantity = P.AvailableQuantity - OD.OrderQuantity
        FROM Products P
        INNER JOIN @OrderDetails OD ON P.ProductId = OD.ProductId;

        COMMIT TRANSACTION;
        SELECT @OrderId AS OrderId; -- Return the new ID
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- 3. Stored Procedure: Update Order
CREATE PROCEDURE sp_UpdateOrder
    @OrderId INT,
    @CustomerId INT,
    @CustomerName NVARCHAR(100),
    @ContactNumber NVARCHAR(20),
    @ContactAddress NVARCHAR(MAX),
    @OrderDate DATETIME,
    @OrderDetails dbo.OrderDetailType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- A. Restore Old Stock
        UPDATE P
        SET P.AvailableQuantity = P.AvailableQuantity + OD.OrderQuantity
        FROM Products P
        INNER JOIN OrderDetails OD ON P.ProductId = OD.ProductId
        WHERE OD.OrderId = @OrderId;

        -- B. Update Customer (Simplified logic based on your controller)
        UPDATE Customers 
        SET CustomerName = @CustomerName, 
            ContactNumber = @ContactNumber, 
            ContactAddress = @ContactAddress
        WHERE CustomerId = @CustomerId;

        -- C. Remove old details
        DELETE FROM OrderDetails WHERE OrderId = @OrderId;

        -- D. Insert new details
        INSERT INTO OrderDetails (OrderId, ProductCategoryId, ProductId, OrderQuantity, OrderUnit, UnitPrice, Amount)
        SELECT @OrderId, ProductCategoryId, ProductId, OrderQuantity, OrderUnit, UnitPrice, Amount
        FROM @OrderDetails;

        -- E. Update Master Order
        UPDATE Orders 
        SET OrderDate = @OrderDate,
            TotalAmount = (SELECT SUM(Amount) FROM @OrderDetails)
        WHERE OrderId = @OrderId;

        -- F. Deduct New Stock
        UPDATE P
        SET P.AvailableQuantity = P.AvailableQuantity - OD.OrderQuantity
        FROM Products P
        INNER JOIN @OrderDetails OD ON P.ProductId = OD.ProductId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- 4. Stored Procedure: Delete Order
CREATE PROCEDURE sp_DeleteOrder
    @OrderId INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        -- Restore Stock
        UPDATE P
        SET P.AvailableQuantity = P.AvailableQuantity + OD.OrderQuantity
        FROM Products P
        INNER JOIN OrderDetails OD ON P.ProductId = OD.ProductId
        WHERE OD.OrderId = @OrderId;

        DELETE FROM OrderDetails WHERE OrderId = @OrderId;
        DELETE FROM Orders WHERE OrderId = @OrderId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO
