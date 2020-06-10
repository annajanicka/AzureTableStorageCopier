CREATE PROCEDURE spUpsetrUser @User [dbo].UserType READONLY
AS
BEGIN
MERGE [dbo].[User] AS target
USING @User AS source
ON (target.Id = source.PartitionKey)
WHEN MATCHED THEN
    UPDATE SET 
	FirstName = source.FirstName, 
	LastName = source.LastName, 
	[Timestamp] = source.[Timestamp], 
	Age = source.Age, 
	IsEmployee = source.IsEmployee,
	[Description] = source.[Description]
WHEN NOT MATCHED THEN
    INSERT (Id, [Timestamp], FirstName, LastName, Age, IsEmployee, [Description])
    VALUES (source.PartitionKey, source.[Timestamp], source.FirstName, source.LastName, source.Age, source.IsEmployee, source.[Description]);
END