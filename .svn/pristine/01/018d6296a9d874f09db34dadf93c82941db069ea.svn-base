
PRINT 'CREATE TABLE [dbo].[OzoneContactsQueue]'
GO

/* History:
  Created by DMitchell
*/

IF OBJECT_ID ('dbo.OzoneContactsQueue') IS NULL
BEGIN 
  CREATE TABLE [dbo].[OzoneContactsQueue] 
  (
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DateTime] [datetime2](7) NOT NULL,
	[Message] [varchar](max) NULL,
	[Status] [tinyint] NOT NULL,
	[IrisId] [bigint] NOT NULL,
 CONSTRAINT [PK_OzoneContactsQueue] PRIMARY KEY CLUSTERED 
 (
	[Id] ASC
 )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
 ) 

ALTER TABLE [dbo].[OzoneContactsQueue] ADD  CONSTRAINT [DF_OzoneContactsQueue_DateTime]  DEFAULT (getdate()) FOR [DateTime]

END
GO

