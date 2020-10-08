

EXEC sp_configure filestream_access_level, 2
GO
RECONFIGURE
GO

--restart SERVICE at this point

--You also have to enable FileStream on the Service.  
--Open the SQL Server Configuration Manager.  
--Click SQL Server Services, then right click on the SQLEXPRESS Database Engine Instance and goto Properties.  
--Click the Filestream tab, and then check Enable FileStream for TSQL Access.

--restart SERVICE AGAIN at this point

alter database Enhancements
add filegroup Documents contains filestream;
go

alter database Enhancements
add file
  (   
  NAME = 'DocumentShare', FILENAME = 'd:\Files'
  )
to filegroup Documents;
go

-----


CREATE TABLE [dbo].[Document](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IrisId] [bigint] NULL,
	[LatestVersionId] [uniqueidentifier] NOT NULL,
	[DocumentName] [varchar](200) NULL,
	[DocumentFullPath] [varchar](max) NULL,
	[Owner] [varchar](200) NULL,
	[Status] [smallint] NULL,
 CONSTRAINT [PK_Document] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]



------------------



CREATE TABLE [dbo].[DocumentVersion](
	[DocumentId] [int] NOT NULL,
	[VersionId] [uniqueidentifier] NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[CreatedBy] [varchar](200) NOT NULL,
	[ModifiedDate] [datetime2](7) NOT NULL,
	[ModifiedBy] [varchar](200) NOT NULL,
	[Document] [varbinary](max) NULL,
	[DocumentGuid] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[DocumentFS] [varbinary](max) FILESTREAM  NULL,
	[DocumentFileStorage] [tinyint] NOT NULL,
 CONSTRAINT [PK_DocumentDetails] PRIMARY KEY CLUSTERED 
(
	[DocumentId] ASC,
	[VersionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY] FILESTREAM_ON [Documents],
UNIQUE NONCLUSTERED 
(
	[DocumentGuid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] FILESTREAM_ON [Documents]
GO

ALTER TABLE [dbo].[DocumentVersion]  WITH CHECK ADD  CONSTRAINT [FK_DocumentDetails_Document] FOREIGN KEY([DocumentId])
REFERENCES [dbo].[Document] ([Id])
GO

ALTER TABLE [dbo].[DocumentVersion] CHECK CONSTRAINT [FK_DocumentDetails_Document]
GO

ALTER TABLE [dbo].[DocumentVersion] ADD  DEFAULT (newsequentialid()) FOR [DocumentGuid]
GO


--------------------------------------




