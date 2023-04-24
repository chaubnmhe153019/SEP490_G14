use master
GO

DROP DATABASE IF EXISTS attas
GO

CREATE DATABASE attas
ON (NAME = 'attas_data', FILENAME = 'D:\SQL\data\attas_data.mdf') 
LOG ON (NAME = 'attas_log', FILENAME = 'D:\SQL\data\attas_log.ldf');
GO

USE attas
GO
CREATE TABLE [token] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [tokenHash] nvarchar(255),
  [user] nvarchar(255)
)
GO

CREATE TABLE [session] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionHash] nvarchar(255),
  [statusId] int,
  [solutionCount] int,
  [maxSearchingTime] int,
  [strategyOption] int,
  [taskCount] int,
  [instructorCount] int,
  [slotCount] int,
  [dayCount] int,
  [timeCount] int,
  [segmentCount] int,
  [slotSegmentRuleCount] int,
  [subjectCount] int,
  [areaCount] int,
  [backupCount] int
)
GO

CREATE TABLE [settingObjective] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [obj] int,
  [value] int
)
GO

CREATE TABLE [status] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [name] nvarchar(255)
)
GO

CREATE TABLE [time] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [businessId] nvarchar(255),
  [order] int
)
GO

CREATE TABLE [instructor] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [businessId] nvarchar(255),
  [order] int,
  [minQuota] int,
  [maxQuota] int
)
GO

CREATE TABLE [task] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [businessId] nvarchar(255),
  [order] int
)
GO

CREATE TABLE [result] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [solutionId] int,
  [taskOrder] int,
  [instructorOrder] int,
  [timeOrder] int
)
GO

CREATE TABLE [solution] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [no] int,
  [taskAssigned] int,
  [workingDay] int,
  [workingTime] int,
  [waitingTime] int,
  [subjectDiversity] int,
  [quotaAvailable] int,
  [walkingDistance] int,
  [subjectPreference] int,
  [slotPreference] int
)
GO

CREATE TABLE [slotConflict] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [timeOrderR] int,
  [timeOrderC] int
)
GO

CREATE TABLE [slotDay] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [timeOrderR] int,
  [dayOrderC] int
)
GO

CREATE TABLE [slotTime] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [timeOrderR] int,
  [timeOrderC] int
)
GO

CREATE TABLE [slotSegment] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [timeOrder] int,
  [dayOrder] int,
  [segmentOrder] int
)
GO

CREATE TABLE [patternCost] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [order] int,
  [value] int
)
GO

CREATE TABLE [instructorSubject] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [instructorOrder] int,
  [subjectOrder] int,
  [value] int
)
GO

CREATE TABLE [instructorTime] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [instructorOrder] int,
  [timeOrder] int,
  [value] int
)
GO

CREATE TABLE [preassign] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [instructorOrder] int,
  [taskOrder] int,
  [value] int
)
GO

CREATE TABLE [areaDistance] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [areaOrderR] int,
  [areaOrderC] int,
  [value] int
)
GO

CREATE TABLE [areaCoefficient] (
  [id] int IDENTITY(1,1) PRIMARY KEY,
  [sessionId] int,
  [timeOrderR] int,
  [timeOrderC] int,
  [value] int
)
GO

ALTER TABLE [slotConflict] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [slotDay] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [slotTime] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [slotSegment] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [patternCost] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [instructorSubject] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [instructorTime] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [preassign] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [areaDistance] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [areaCoefficient] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [time] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [instructor] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [task] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO

ALTER TABLE [result] ADD FOREIGN KEY ([solutionId]) REFERENCES [solution] ([id])
GO


ALTER TABLE [session] ADD FOREIGN KEY ([statusId]) REFERENCES [status] ([id])
GO

ALTER TABLE [solution] ADD FOREIGN KEY ([sessionId]) REFERENCES [session] ([id])
GO


INSERT INTO token (tokenHash,[user]) VALUES ('token','FPT');

INSERT INTO [status] (name) VALUES ('PENDING')
INSERT INTO [status] (name) VALUES ('UNKNOWN')
INSERT INTO [status] (name) VALUES ('INFEASIBLE')
INSERT INTO [status] (name) VALUES ('FEASIBLE')
INSERT INTO [status] (name) VALUES ('OPTIMAL')


select * from [session]
select * from solution
select * from result
select * from task
select * from [time]
select * from instructor
select * from [status]
select * from token
select * from settingObjective