CREATE TABLE [dbo].[AnotherTestTable] (
    [Id]    INT           NOT NULL,
    [NameA] NVARCHAR (20) NULL,
    [NameB] NVARCHAR (20) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO

INSERT INTO [dbo].[AnotherTestTable] ([Id], [NameA], [NameB]) VALUES (1, N'MK', N'AF')
INSERT INTO [dbo].[AnotherTestTable] ([Id], [NameA], [NameB]) VALUES (2, N'AK', N'GG')
INSERT INTO [dbo].[AnotherTestTable] ([Id], [NameA], [NameB]) VALUES (3, N'DK', N'SS')
