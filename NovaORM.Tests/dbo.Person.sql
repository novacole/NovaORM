USE TestDB
GO
IF EXISTS (select * from sys.tables t where t.name ='Person')
BEGIN
DROP TABLE Person
END 
GO
CREATE TABLE [dbo].[Person]
(
	[Id] INT NOT NULL IDENTITY PRIMARY KEY,
	FirstName NVARCHAR(200),
	LastName NVARCHAR(200)
)
GO
INSERT INTO Person VALUES ('Bob','Sanders');
INSERT INTO Person VALUES ('Frank','Sanders');
INSERT INTO Person VALUES ('George','Sanders');
INSERT INTO Person VALUES ('Sam','Sanders');
INSERT INTO Person VALUES ('Jose','Sanders');
INSERT INTO Person VALUES ('Mike','Sanders');
INSERT INTO Person VALUES ('Donald','Sanders');
INSERT INTO Person VALUES ('Ronald','Sanders');
INSERT INTO Person VALUES ('Dan','Sanders');
INSERT INTO Person VALUES ('Smith','Sanders');