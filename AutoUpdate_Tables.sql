USE MyDB
GO

-- ************************************************
-- < Drop tables if they exist >

IF (OBJECT_ID('dbo.AutoUpdate_Programs', 'U') IS NOT NULL)
	DROP TABLE dbo.AutoUpdate_Programs

IF (OBJECT_ID('dbo.AutoUpdate_Paths', 'U') IS NOT NULL)
	DROP TABLE dbo.AutoUpdate_Paths

IF (OBJECT_ID('dbo.AutoUpdate_Areas', 'U') IS NOT NULL)
	DROP TABLE dbo.AutoUpdate_Areas

IF (OBJECT_ID('dbo.AutoUpdate_Users', 'U') IS NOT NULL)
	DROP TABLE dbo.AutoUpdate_Users

-- < Drop tables if they exist >
-- ************************************************



-- ************************************************
-- < Create table for areas >

CREATE TABLE dbo.AutoUpdate_Areas(
	Id INT IDENTITY(1,1) NOT NULL,
	AreaName VARCHAR(50) NOT NULL,
 CONSTRAINT PK_AutoUpdate_Areas PRIMARY KEY CLUSTERED 
(
	Id ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- < /Create table for areas>
-- ************************************************



-- ************************************************
-- < Create table for paths >

CREATE TABLE dbo.AutoUpdate_Paths(
	Id INT IDENTITY(1,1) NOT NULL,
	AreaId INT NOT NULL,
	DirPath VARCHAR(50) NOT NULL,
	SrcPath VARCHAR(150) NOT NULL,
	Comments VARCHAR(150) NULL,
 CONSTRAINT PK_AutoUpdate_Paths PRIMARY KEY CLUSTERED 
(
	Id ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE dbo.AutoUpdate_Paths  WITH CHECK ADD  CONSTRAINT FK_AutoUpdate_Path_AutoUpdate_Areas FOREIGN KEY(AreaId)
REFERENCES dbo.AutoUpdate_Areas (Id)
GO

ALTER TABLE dbo.AutoUpdate_Paths CHECK CONSTRAINT FK_AutoUpdate_Path_AutoUpdate_Areas
GO

-- </ Create table for paths >
-- ************************************************



-- ************************************************
-- < Create table for programs >

CREATE TABLE dbo.AutoUpdate_Programs(
	Id INT IDENTITY(1,1) NOT NULL,
	AreaId INT NOT NULL,
	AppName VARCHAR(50) NOT NULL,
	ProcessName VARCHAR(50) NOT NULL,
	IsSecure BIT NULL,
 CONSTRAINT PK_AutoUpdate_Programs PRIMARY KEY CLUSTERED 
(
	Id ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE dbo.AutoUpdate_Programs ADD  CONSTRAINT DF_AutoUpdate_Programs_IsSecure  DEFAULT ((0)) FOR IsSecure
GO

ALTER TABLE dbo.AutoUpdate_Programs  WITH CHECK ADD  CONSTRAINT FK_AutoUpdate_Programs_AutoUpdate_Areas FOREIGN KEY(AreaId)
REFERENCES dbo.AutoUpdate_Areas (Id)
GO

ALTER TABLE dbo.AutoUpdate_Programs CHECK CONSTRAINT FK_AutoUpdate_Programs_AutoUpdate_Areas
GO

-- </ Create table for programs >
-- ************************************************



-- ************************************************
-- < Create table for users >

CREATE TABLE dbo.AutoUpdate_Users(
	Id INT IDENTITY(1,1) NOT NULL,
	FullName VARCHAR(50) NOT NULL,
	ScanCode VARCHAR(50) NOT NULL,	
 CONSTRAINT PK_AutoUpdate_Logins PRIMARY KEY CLUSTERED 
(
	Id ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-- </ Create table for users >
-- ************************************************



-- ************************************************
-- < Load data for new tables >

INSERT INTO dbo.AutoUpdate_Areas (AreaName)
	VALUES ('Area 1'), 
		   ('Area 2'), 
		   ('Area 3'), 
		   ('Area 4'), 
		   ('Area 5')

INSERT INTO dbo.AutoUpdate_Paths (AreaId, DirPath, SrcPath)
	VALUES (1, 'C:\Area 1 Apps', '\\IPG220-6211-07\AutoUpdater\Area 1 Apps'),
		   (2, 'C:\Area 2 Apps', '\\IPG220-6211-07\AutoUpdater\Area 2 Apps'),
		   (3, 'C:\Area 3 Apps', '\\IPG220-6211-07\AutoUpdater\Area 3 Apps'),
		   (4, 'C:\Area 4 Apps', '\\IPG220-6211-07\AutoUpdater\Area 4 Apps'),
		   (5, 'C:\Area 5 Apps', '\\IPG220-6211-07\AutoUpdater\Area 5 Apps')

INSERT INTO dbo.AutoUpdate_Programs (AreaId, AppName, ProcessName, IsSecure)
	VALUES (1, 'App 1', 'Proc 1', 0), (1, 'App 2', 'Proc 2', 0), (1, 'App 3', 'Proc 3', 0), (1, 'App 4', 'Proc 4', 0), (1, 'App 5', 'Proc 5', 0),
		   (2, 'App 1', 'Proc 1', 0), (2, 'App 2', 'Proc 2', 0), (2, 'App 3', 'Proc 3', 0), (2, 'App 4', 'Proc 4', 1), (2, 'App 5', 'Proc 5', 0),
		   (3, 'App 1', 'Proc 1', 0), (3, 'App 2', 'Proc 2', 0), (3, 'App 3', 'Proc 3', 0), (3, 'App 4', 'Proc 4', 0), (3, 'App 5', 'Proc 5', 1),
		   (4, 'App 1', 'Proc 1', 0), (4, 'App 2', 'Proc 2', 1), (4, 'App 3', 'Proc 3', 0), (4, 'App 4', 'Proc 4', 0), (4, 'App 5', 'Proc 5', 0),
		   (5, 'App 1', 'Proc 1', 1), (5, 'App 2', 'Proc 2', 0), (5, 'App 3', 'Proc 3', 1), (5, 'App 4', 'Proc 4', 0), (5, 'App 5', 'Proc 5', 0)

INSERT INTO dbo.AutoUpdate_Users (FullName, ScanCode)
	VALUES ('Zdeno Chara', 'chara123')

-- </ Load data for new tables >
-- ************************************************