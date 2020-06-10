-- the order of these columns MATTERS!

CREATE TYPE [dbo].[UserType] AS TABLE(
    PartitionKey uniqueidentifier NOT NULL,
    RowKey uniqueidentifier NOT NULL,
    Timestamp datetimeoffset(7) NOT NULL,
	Age int NOT NULL,
	[Description] nvarchar(500) NULL,
	FirstName nvarchar(50) NOT NULL,
	IsEmployee bit NOT NULL,
	LastName nvarchar(50) NOT NULL
)
