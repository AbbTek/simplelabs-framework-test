CREATE TABLE [TestSchema].[Unidad] (
    [IDUnidad] INT            IDENTITY (1, 1) NOT NULL,
    [Nombre]   NVARCHAR (512) NULL,
    [Estado]   TINYINT        NULL,
    CONSTRAINT [PK_Unidad] PRIMARY KEY CLUSTERED ([IDUnidad] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_TestSchema_Unidad_1]
    ON [TestSchema].[Unidad]([Nombre] ASC);

