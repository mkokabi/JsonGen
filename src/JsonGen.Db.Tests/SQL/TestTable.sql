CREATE TABLE [dbo].[TestTable] (
    [Id]   INT            NULL,
    [Name] NVARCHAR (100) NULL
);

GO

INSERT INTO [dbo].[TestTable] ([Id], [Name]) VALUES (1, N'MK')
INSERT INTO [dbo].[TestTable] ([Id], [Name]) VALUES (2, N'AK')

