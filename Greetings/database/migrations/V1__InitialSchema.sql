CREATE TABLE [dbo].[Greetings] (
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [Message]      NVARCHAR (MAX)   NOT NULL,
    [GeneratedOn]  DATETIME2 (7)    NOT NULL
    );

ALTER TABLE [dbo].[Greetings]
    ADD CONSTRAINT [PK_Greetings] PRIMARY KEY CLUSTERED ([Id] ASC);