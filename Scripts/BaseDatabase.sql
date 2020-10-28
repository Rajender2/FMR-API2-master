ALTER TABLE [City] DROP CONSTRAINT [FK_City_State_Stateid];

GO

ALTER TABLE [State] DROP CONSTRAINT [FK_State_Country_Countryid];

GO

ALTER TABLE [UserAddress] DROP CONSTRAINT [FK_UserAddress_Address_Addressid];

GO

EXEC sp_rename N'[City].[Stateid]', N'StateId', N'COLUMN';

GO

EXEC sp_rename N'[City].[IX_City_Stateid]', N'IX_City_StateId', N'INDEX';

GO

DROP INDEX [IX_UserAddress_Addressid] ON [UserAddress];
DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[UserAddress]') AND [c].[name] = N'Addressid');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [UserAddress] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [UserAddress] ALTER COLUMN [Addressid] int NOT NULL;
CREATE INDEX [IX_UserAddress_Addressid] ON [UserAddress] ([Addressid]);

GO

DROP INDEX [IX_State_Countryid] ON [State];
DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[State]') AND [c].[name] = N'Countryid');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [State] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [State] ALTER COLUMN [Countryid] int NOT NULL;
CREATE INDEX [IX_State_Countryid] ON [State] ([Countryid]);

GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[FormTemplate]') AND [c].[name] = N'IsActive');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [FormTemplate] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [FormTemplate] ALTER COLUMN [IsActive] int NULL;

GO

ALTER TABLE [FormTemplate] ADD [CompanyId] int NULL DEFAULT 0;

GO

DROP INDEX [IX_City_StateId] ON [City];
DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[City]') AND [c].[name] = N'StateId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [City] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [City] ALTER COLUMN [StateId] int NOT NULL;
CREATE INDEX [IX_City_StateId] ON [City] ([StateId]);

GO

ALTER TABLE [Assessment] ADD [ResumePath] nvarchar(max) NULL;

GO

ALTER TABLE [Address] ADD [CountryId] int NOT NULL DEFAULT 0;

GO

CREATE TABLE [JobMCQuestion] (
    [Id] int NOT NULL IDENTITY,
    [JobOrderId] int NOT NULL,
    [AddedOn] datetime2 NOT NULL,
    [AddedById] bigint NOT NULL,
    [OrderById] int NULL,
    [QuestionId] int NOT NULL,
    CONSTRAINT [PK_JobMCQuestion] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JobMCQuestion_FormTemplate_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [FormTemplate] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [JobOrderDocuments] (
    [Id] int NOT NULL IDENTITY,
    [JobOrderId] int NOT NULL,
    [DocumentId] int NOT NULL,
    CONSTRAINT [PK_JobOrderDocuments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JobOrderDocuments_DocumentTemplate_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [DocumentTemplate] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_JobOrderDocuments_JobOrder_JobOrderId] FOREIGN KEY ([JobOrderId]) REFERENCES [JobOrder] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_FormTemplate_CompanyId] ON [FormTemplate] ([CompanyId]);

GO

CREATE INDEX [IX_JobMCQuestion_QuestionId] ON [JobMCQuestion] ([QuestionId]);

GO

CREATE INDEX [IX_JobOrderDocuments_DocumentId] ON [JobOrderDocuments] ([DocumentId]);

GO

CREATE INDEX [IX_JobOrderDocuments_JobOrderId] ON [JobOrderDocuments] ([JobOrderId]);

GO

ALTER TABLE [City] ADD CONSTRAINT [FK_City_State_StateId] FOREIGN KEY ([StateId]) REFERENCES [State] ([id]) ON DELETE CASCADE;

GO

ALTER TABLE [FormTemplate] ADD CONSTRAINT [FK_FormTemplate_Company_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Company] ([Id]) ON DELETE CASCADE;

GO

ALTER TABLE [State] ADD CONSTRAINT [FK_State_Country_Countryid] FOREIGN KEY ([Countryid]) REFERENCES [Country] ([id]) ON DELETE CASCADE;

GO

ALTER TABLE [UserAddress] ADD CONSTRAINT [FK_UserAddress_Address_Addressid] FOREIGN KEY ([Addressid]) REFERENCES [Address] ([id]) ON DELETE CASCADE;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20201027100200_Updated02', N'2.2.6-servicing-10079');

GO

