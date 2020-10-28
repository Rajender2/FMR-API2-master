USE [FMRDBNew]
GO

/****** Object:  Table [dbo].[JobMCQuestion]    Script Date: 22-10-2020 1.18.36 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[JobMCQuestion](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[JobOrderId] [int] NOT NULL,
	[QuestionId] [int] NULL,
	[OrderById] [int] NULL,
	[AddedOn] [datetime] NULL,
	[AddedById] [bigint] NULL,
 CONSTRAINT [PK_JobMCQuestion] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[JobMCQuestion]  WITH CHECK ADD  CONSTRAINT [FK_JobMCQuestion_FormTemplate] FOREIGN KEY([QuestionId])
REFERENCES [dbo].[FormTemplate] ([Id])
GO

ALTER TABLE [dbo].[JobMCQuestion] CHECK CONSTRAINT [FK_JobMCQuestion_FormTemplate]
GO

/****** Object:  Table [dbo].[JobOrderDocuments]    Script Date: 22-10-2020 1.19.02 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[JobOrderDocuments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[JobOrderId] [int] NULL,
	[DocumentId] [int] NULL,
 CONSTRAINT [PK_JobOrderDocuments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[JobOrderDocuments]  WITH CHECK ADD  CONSTRAINT [FK_JobOrderDocuments_DocumentTemplate1] FOREIGN KEY([DocumentId])
REFERENCES [dbo].[DocumentTemplate] ([Id])
GO

ALTER TABLE [dbo].[JobOrderDocuments] CHECK CONSTRAINT [FK_JobOrderDocuments_DocumentTemplate1]
GO

ALTER TABLE [dbo].[JobOrderDocuments]  WITH CHECK ADD  CONSTRAINT [FK_JobOrderDocuments_JobOrder1] FOREIGN KEY([JobOrderId])
REFERENCES [dbo].[JobOrder] ([Id])
GO

ALTER TABLE [dbo].[JobOrderDocuments] CHECK CONSTRAINT [FK_JobOrderDocuments_JobOrder1]
GO


ALTER TABLE [dbo].[FormTemplate] ADD CompanyId int NULL
GO
ALTER TABLE [dbo].[FormTemplate] ADD CreatedBy int NULL
GO
ALTER TABLE [dbo].[FormTemplate] ADD CreatedOn datetime NULL
GO
ALTER TABLE [dbo].[FormTemplate] ADD IsActive int NULL
GO
ALTER TABLE [dbo].[FormTemplate]  WITH CHECK ADD  CONSTRAINT [FK_FormTemplate_Company] FOREIGN KEY([CompanyId])
REFERENCES [dbo].[Company] ([Id])
GO

ALTER TABLE [dbo].[FormTemplate] CHECK CONSTRAINT [FK_FormTemplate_Company]
GO
