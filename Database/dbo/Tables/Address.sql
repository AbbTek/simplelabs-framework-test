CREATE TABLE [dbo].[Address] (
    [ID]            INT              IDENTITY (1, 1) NOT NULL,
    [Street]        NVARCHAR (250)   NOT NULL,
    [ZipCode]       NVARCHAR (50)    NULL,
    [Coordinates]   [sys].[geometry] NOT NULL,
    [TextReference] NVARCHAR (MAX)   NULL,
    [Date]          DATETIME         NOT NULL,
    CONSTRAINT [PK_Direccion] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20141225-223713]
    ON [dbo].[Address]([ZipCode] ASC);


GO
CREATE SPATIAL INDEX [SpatialIndex-20141230-125344]
    ON [dbo].[Address] ([Coordinates])
    WITH  (
            BOUNDING_BOX = (XMAX = 70, XMIN = 60, YMAX = 30, YMIN = 20),
            CELLS_PER_OBJECT = 16
          );

