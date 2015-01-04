CREATE TABLE [dbo].[UnidadGeopolitica] (
    [ID]      INT                 IDENTITY (1, 1) NOT NULL,
    [Nodo]    [sys].[hierarchyid] NULL,
    [IDPadre] INT                 NULL,
    CONSTRAINT [PK__UnidadGe__3214EC27B8B13CFF] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FKB7970132F029CD2E] FOREIGN KEY ([IDPadre]) REFERENCES [dbo].[UnidadGeopolitica] ([ID])
);

