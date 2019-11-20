CREATE PROCEDURE [dbo].[spSale_Insert]
	@Id INT OUTPUT,
	@CashierId NVARCHAR(128),
	@SaleDate datetime2,
	@Subtotal money,
	@Tax money,
	@Total money
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.Sale(CashierId, SaleDate, SubTotal, Tax, Total)
	VALUES(@CashierId, @SaleDate, @Subtotal, @Tax, @Total);

	SELECT @Id = Scope_Identity();
END
