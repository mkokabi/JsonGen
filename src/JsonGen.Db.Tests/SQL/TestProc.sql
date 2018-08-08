CREATE PROCEDURE [dbo].[TestProc]
	@param1 int = 0,
	@param2 VARCHAR(10)
AS
	SELECT * FROM dbo.TestTable WHERE Id = @param1 OR Name = @param2
RETURN 0
