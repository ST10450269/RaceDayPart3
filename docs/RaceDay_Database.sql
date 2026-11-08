-- RACEDAY | PART 1 SQL SERVER SCHEMA AND SAMPLE DATA
-- Run ONCE against a fresh SQL Server instance in SSMS. If RaceDayDb exists, do not run.
-- Equivalent EF Core Code-First mapping: src/RaceDay.Api/Data/RaceDayDbContext.cs
-- PasswordHash values are ASP.NET Core Identity V3 PBKDF2 hashes; not plain-text passwords.
CREATE DATABASE RaceDayDb;
GO
USE RaceDayDb;
GO
CREATE TABLE dbo.Roles (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    Name NVARCHAR(30) NOT NULL,
    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
);
CREATE TABLE dbo.Users (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    Phone NVARCHAR(30) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    ProfileImageName NVARCHAR(260) NULL,
    RoleId INT NOT NULL,
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT FK_Users_Roles FOREIGN KEY(RoleId) REFERENCES dbo.Roles(Id)
);
CREATE TABLE dbo.Events (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY,
    OrganiserId INT NOT NULL,
    Name NVARCHAR(120) NOT NULL,
    Description NVARCHAR(1500) NOT NULL,
    DateUtc DATETIME2 NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    DistanceKm DECIMAL(8,2) NOT NULL,
    EventType NVARCHAR(12) NOT NULL,
    BannerImageName NVARCHAR(260) NULL,
    CONSTRAINT FK_Events_Users FOREIGN KEY(OrganiserId) REFERENCES dbo.Users(Id)
);
CREATE TABLE dbo.Categories (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
    EventId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    MinAge INT NULL,
    MaxAge INT NULL,
    DistanceKm DECIMAL(8,2) NULL,
    CONSTRAINT UQ_Categories_Event_Name UNIQUE(EventId,Name),
    CONSTRAINT FK_Categories_Events FOREIGN KEY(EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE
);
CREATE TABLE dbo.Enrolments (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Enrolments PRIMARY KEY,
    EventId INT NOT NULL,
    ParticipantId INT NOT NULL,
    CategoryId INT NOT NULL,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Enrolments_Status DEFAULT ('Confirmed'),
    EnteredAtUtc DATETIME2 NOT NULL,
    CONSTRAINT UQ_Enrolments_Event_Participant UNIQUE(EventId,ParticipantId),
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY(EventId) REFERENCES dbo.Events(Id),
    CONSTRAINT FK_Enrolments_Users FOREIGN KEY(ParticipantId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY(CategoryId) REFERENCES dbo.Categories(Id)
);
CREATE TABLE dbo.Results (
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Results PRIMARY KEY,
    EnrolmentId INT NOT NULL,
    FinishTimeSeconds INT NOT NULL,
    Position INT NOT NULL,
    TotalFinishers INT NOT NULL,
    PublishedAtUtc DATETIME2 NOT NULL,
    CONSTRAINT UQ_Results_Enrolment UNIQUE(EnrolmentId),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY(EnrolmentId) REFERENCES dbo.Enrolments(Id) ON DELETE CASCADE
);
GO
INSERT dbo.Roles(Name) VALUES ('Organiser'),('Participant');
DECLARE @org INT=(SELECT Id FROM dbo.Roles WHERE Name='Organiser');
DECLARE @ath INT=(SELECT Id FROM dbo.Roles WHERE Name='Participant');
INSERT dbo.Users(FullName,Email,PasswordHash,Phone,City,RoleId) VALUES
('Thandi Mokoena','organiser@raceday.test','AQAAAAIAAYagAAAAEP2K1Wf0eHlWAu/3YOqlbLLc/ZZlcBHnQ2OEZHmjkt0AIuycDInBIw+58pXYrl8+NA==','0820000000','Durban',@org),
('Peter Jacobs','organiser2@raceday.test','AQAAAAIAAYagAAAAEP2K1Wf0eHlWAu/3YOqlbLLc/ZZlcBHnQ2OEZHmjkt0AIuycDInBIw+58pXYrl8+NA==','0820000000','Cape Town',@org),
('Nomsa Dlamini','runner@raceday.test','AQAAAAIAAYagAAAAEIh3krOBQeOdyuKaciKBHI940p3Qyx2xe6G8DrPQ5qq0AIpkMrg5WrAekenR0RzfKA==','0820000000','Pietermaritzburg',@ath),
('Sizwe Nkosi','runner2@raceday.test','AQAAAAIAAYagAAAAEIh3krOBQeOdyuKaciKBHI940p3Qyx2xe6G8DrPQ5qq0AIpkMrg5WrAekenR0RzfKA==','0820000000','Johannesburg',@ath);
DECLARE @org1 INT=(SELECT Id FROM dbo.Users WHERE Email='organiser@raceday.test');
DECLARE @org2 INT=(SELECT Id FROM dbo.Users WHERE Email='organiser2@raceday.test');
DECLARE @ath1 INT=(SELECT Id FROM dbo.Users WHERE Email='runner@raceday.test');
DECLARE @ath2 INT=(SELECT Id FROM dbo.Users WHERE Email='runner2@raceday.test');
INSERT dbo.Events (OrganiserId,Name,Description,DateUtc,Location,DistanceKm,EventType) VALUES
(@org1,'Heritage Road Race','Community road race through KwaZulu-Natal.',DATEADD(day,-30,SYSUTCDATETIME()),'Pietermaritzburg',10,'Run'),
(@org1,'Durban Coastal Run','A scenic coastal race for all fitness levels.',DATEADD(day,21,SYSUTCDATETIME()),'Durban',21,'Run'),
(@org2,'Cape Pedal Challenge','A road cycling challenge along beautiful coastal roads.',DATEADD(day,40,SYSUTCDATETIME()),'Cape Town',50,'Cycle');
DECLARE @past INT=(SELECT Id FROM dbo.Events WHERE Name='Heritage Road Race');
DECLARE @upcoming INT=(SELECT Id FROM dbo.Events WHERE Name='Durban Coastal Run');
DECLARE @cycle INT=(SELECT Id FROM dbo.Events WHERE Name='Cape Pedal Challenge');
INSERT dbo.Categories(EventId,Name,DistanceKm,MinAge,MaxAge) VALUES
(@past,'10 km Open',10,NULL,NULL),
(@upcoming,'21 km Open',21,NULL,NULL),
(@upcoming,'Under 20',NULL,12,19),
(@cycle,'50 km Open',50,NULL,NULL);
DECLARE @pCat INT=(SELECT Id FROM dbo.Categories WHERE EventId=@past AND Name='10 km Open');
DECLARE @uCat INT=(SELECT Id FROM dbo.Categories WHERE EventId=@upcoming AND Name='21 km Open');
DECLARE @cCat INT=(SELECT Id FROM dbo.Categories WHERE EventId=@cycle AND Name='50 km Open');
INSERT dbo.Enrolments(EventId,ParticipantId,CategoryId,Status,EnteredAtUtc) VALUES
(@past,@ath1,@pCat,'Confirmed',DATEADD(day,-50,SYSUTCDATETIME())),
(@upcoming,@ath1,@uCat,'Confirmed',SYSUTCDATETIME()),
(@cycle,@ath2,@cCat,'Pending',SYSUTCDATETIME());
INSERT dbo.Results(EnrolmentId,FinishTimeSeconds,Position,TotalFinishers,PublishedAtUtc)
SELECT Id,3234,47,312,DATEADD(day,-29,SYSUTCDATETIME()) FROM dbo.Enrolments WHERE EventId=@past AND ParticipantId=@ath1;
GO
SELECT 'Roles' AS Entity, COUNT(*) AS NumberOfRecords FROM dbo.Roles
UNION ALL SELECT 'Users',COUNT(*) FROM dbo.Users
UNION ALL SELECT 'Events',COUNT(*) FROM dbo.Events
UNION ALL SELECT 'Categories',COUNT(*) FROM dbo.Categories
UNION ALL SELECT 'Enrolments',COUNT(*) FROM dbo.Enrolments
UNION ALL SELECT 'Results',COUNT(*) FROM dbo.Results;
-- Demo credentials (not for production): Organiser = organiser@raceday.test / DemoOrg123!
-- Participant = runner@raceday.test / DemoRunner123!
