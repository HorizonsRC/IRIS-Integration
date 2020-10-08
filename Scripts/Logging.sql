

CREATE TABLE [dbo].[Logging](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DateTime] [datetime] NULL,
	[Event] [varchar](50) NULL,
	[Description] [varchar](100) NULL,
	[WindowsUser] [varchar](50) NULL,
	[Comment] [varchar](200) NULL,
	[Exception] [varchar](max) NULL,
 CONSTRAINT [PK_Logging] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


