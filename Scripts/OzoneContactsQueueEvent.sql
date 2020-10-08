PRINT 'CREATE TABLE [dbo].[OzoneContactsQueueEvent]'
GO

/* History:
  Created by DMitchell
*/

IF OBJECT_ID ('dbo.OzoneContactsQueueEvent') IS NULL
BEGIN 
  CREATE TABLE [dbo].[OzoneContactsQueueEvent] 
  (
	[Id] [int] IDENTITY(1,1) NOT NULL,
	QueueId [int] not null,
	[DateTime] [datetime2](7) NOT NULL,
	[Message] [varchar](max) NULL,
	[Status] [tinyint] NOT NULL,
 CONSTRAINT [PK_OzoneContactsQueueEvent] PRIMARY KEY CLUSTERED 
 (
	[Id] ASC
 )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
 ) 

ALTER TABLE [dbo].[OzoneContactsQueueEvent] ADD  CONSTRAINT [DF_OzoneContactsQueueEvent_DateTime]  DEFAULT (getdate()) FOR [DateTime]

ALTER TABLE [dbo].[OzoneContactsQueueEvent] ADD CONSTRAINT [FK_OzoneContactsQueueEvent_QueueId] FOREIGN KEY([QueueId]) REFERENCES [dbo].[OzoneContactsQueue]([Id])


END
GO

