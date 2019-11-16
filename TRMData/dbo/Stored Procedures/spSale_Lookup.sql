CREATE PROCEDURE [dbo].[spSale_Lookup]
	@CashierId NVARCHAR(128),
	@SaleDate datetime2
AS
BEGIN
	SET NOCOUNT ON;

	SELECT Id
	FROM dbo.Sale
	WHERE CashierId = @CashierId and SaleDate = @SaleDate;
END