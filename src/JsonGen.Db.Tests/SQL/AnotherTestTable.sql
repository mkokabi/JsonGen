CREATE TABLE [dbo].[AnotherTestTable] (
    [Id]    INT           NOT NULL,
    [NameA] NVARCHAR (20) NULL,
    [NameB] NVARCHAR (20) NULL,
	[DOB] DATETIME NULL
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO

INSERT INTO [dbo].[AnotherTestTable] ([Id], [NameA], [NameB], [DOB]) VALUES (1, N'MK', N'AF', N'2018-01-02 00:00:00')
INSERT INTO [dbo].[AnotherTestTable] ([Id], [NameA], [NameB], [DOB]) VALUES (2, N'AK', N'GG', N'2018-02-02 00:00:00')
INSERT INTO [dbo].[AnotherTestTable] ([Id], [NameA], [NameB], [DOB]) VALUES (3, N'DK', N'SS', N'2018-03-02 11:58:00')
