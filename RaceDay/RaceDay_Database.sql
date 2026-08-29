/* =============================================
   RaceDay Database Script
   ============================================= */

-- Create Database
IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END
GO

USE RaceDayDB;
GO


/* =============================================
   DROP TABLES
   ============================================= */

IF OBJECT_ID('Results', 'U') IS NOT NULL
    DROP TABLE Results;

IF OBJECT_ID('Enrolments', 'U') IS NOT NULL
    DROP TABLE Enrolments;

IF OBJECT_ID('Categories', 'U') IS NOT NULL
    DROP TABLE Categories;

IF OBJECT_ID('Routes', 'U') IS NOT NULL
    DROP TABLE Routes;

IF OBJECT_ID('Events', 'U') IS NOT NULL
    DROP TABLE Events;

IF OBJECT_ID('Users', 'U') IS NOT NULL
    DROP TABLE Users;
GO


/* =============================================
   USERS TABLE
   ============================================= */

CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL,
    Role VARCHAR(20) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT CHK_UserRole
    CHECK (Role IN ('Organiser', 'Participant'))
);
GO


/* =============================================
   EVENTS TABLE
   ============================================= */

CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName VARCHAR(150) NOT NULL,
    EventType VARCHAR(30) NOT NULL,
    EventDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    Location VARCHAR(150) NOT NULL,
    Description VARCHAR(500),
    RegistrationDeadline DATE NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Events_Users
    FOREIGN KEY (OrganiserID)
    REFERENCES Users(UserID),

    CONSTRAINT CHK_EventType
    CHECK (EventType IN ('Running', 'Walking', 'Cycling'))
);
GO


/* =============================================
   ROUTES TABLE
   ============================================= */

CREATE TABLE Routes
(
    RouteID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL UNIQUE,
    RouteName VARCHAR(100) NOT NULL,
    DistanceKM DECIMAL(6,2) NOT NULL,
    StartLocation VARCHAR(150) NOT NULL,
    EndLocation VARCHAR(150) NOT NULL,
    RouteDescription VARCHAR(500),

    CONSTRAINT FK_Routes_Events
    FOREIGN KEY (EventID)
    REFERENCES Events(EventID),

    CONSTRAINT CHK_RouteDistance
    CHECK (DistanceKM > 0)
);
GO


/* =============================================
   CATEGORIES TABLE
   ============================================= */

CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    DistanceKM DECIMAL(6,2) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,
    MaximumParticipants INT NOT NULL,

    CONSTRAINT FK_Categories_Events
    FOREIGN KEY (EventID)
    REFERENCES Events(EventID),

    CONSTRAINT CHK_CategoryDistance
    CHECK (DistanceKM > 0),

    CONSTRAINT CHK_EntryFee
    CHECK (EntryFee >= 0),

    CONSTRAINT CHK_MaxParticipants
    CHECK (MaximumParticipants > 0)
);
GO


/* =============================================
   ENROLMENTS TABLE
   ============================================= */

CREATE TABLE Enrolments
(
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME DEFAULT GETDATE(),
    BibNumber INT UNIQUE,
    Status VARCHAR(20) NOT NULL DEFAULT 'Confirmed',

    CONSTRAINT FK_Enrolments_Participant
    FOREIGN KEY (ParticipantID)
    REFERENCES Users(UserID),

    CONSTRAINT FK_Enrolments_Event
    FOREIGN KEY (EventID)
    REFERENCES Events(EventID),

    CONSTRAINT FK_Enrolments_Category
    FOREIGN KEY (CategoryID)
    REFERENCES Categories(CategoryID),

    CONSTRAINT CHK_EnrolmentStatus
    CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled'))
);
GO


/* =============================================
   RESULTS TABLE
   ============================================= */

CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishPosition INT,
    FinishTime TIME,
    ResultStatus VARCHAR(20) NOT NULL,
    RecordedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Results_Enrolments
    FOREIGN KEY (EnrolmentID)
    REFERENCES Enrolments(EnrolmentID),

    CONSTRAINT CHK_ResultStatus
    CHECK (ResultStatus IN ('Finished', 'DNF', 'DNS')),

    CONSTRAINT CHK_FinishPosition
    CHECK (FinishPosition IS NULL OR FinishPosition > 0)
);
GO


/* =============================================
   INSERT USERS
   ============================================= */

-- Organisers
INSERT INTO Users
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    PhoneNumber,
    Role
)
VALUES
(
    'Thabo',
    'Mokoena',
    'thabo.mokoena@raceday.co.za',
    'hashedpassword123',
    '0825551234',
    'Organiser'
),
(
    'Sarah',
    'Williams',
    'sarah.williams@raceday.co.za',
    'hashedpassword456',
    '0835555678',
    'Organiser'
);


-- Participants
INSERT INTO Users
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    PhoneNumber,
    Role
)
VALUES
(
    'Lerato',
    'Mahlangu',
    'lerato.mahlangu@email.com',
    'hashedpassword789',
    '0712345678',
    'Participant'
),
(
    'James',
    'Smith',
    'james.smith@email.com',
    'hashedpassword101',
    '0723456789',
    'Participant'
);
GO


/* =============================================
   INSERT EVENTS
   ============================================= */

INSERT INTO Events
(
    OrganiserID,
    EventName,
    EventType,
    EventDate,
    StartTime,
    Location,
    Description,
    RegistrationDeadline
)
VALUES
(
    1,
    'Soweto Road Run 2027',
    'Running',
    '2027-03-14',
    '06:00',
    'Soweto, Johannesburg',
    'Annual road running event through Soweto.',
    '2027-03-01'
),
(
    2,
    'Cape Town Cycle Challenge',
    'Cycling',
    '2027-04-10',
    '06:30',
    'Cape Town',
    'Cycling event featuring scenic Cape Town routes.',
    '2027-03-25'
),
(
    1,
    'Pretoria Charity Walk',
    'Walking',
    '2027-05-01',
    '07:00',
    'Pretoria',
    'Community charity walking event supporting local organisations.',
    '2027-04-20'
);
GO


/* =============================================
   INSERT ROUTES
   ============================================= */

INSERT INTO Routes
(
    EventID,
    RouteName,
    DistanceKM,
    StartLocation,
    EndLocation,
    RouteDescription
)
VALUES
(
    1,
    'Soweto City Route',
    21.10,
    'Orlando Stadium',
    'Soweto Theatre',
    'A scenic route through major Soweto landmarks.'
),
(
    2,
    'Cape Peninsula Route',
    109.00,
    'Cape Town CBD',
    'Cape Town Stadium',
    'A challenging cycling route around Cape Town.'
),
(
    3,
    'Pretoria Community Route',
    10.00,
    'Union Buildings',
    'Church Square',
    'A relaxed community walking route through Pretoria.'
);
GO


/* =============================================
   INSERT CATEGORIES
   ============================================= */

-- Categories for Event 1
INSERT INTO Categories
(
    EventID,
    CategoryName,
    DistanceKM,
    EntryFee,
    MaximumParticipants
)
VALUES
(
    1,
    '10 KM Run',
    10.00,
    150.00,
    1000
),
(
    1,
    'Half Marathon',
    21.10,
    250.00,
    2000
);


-- Categories for Event 2
INSERT INTO Categories
(
    EventID,
    CategoryName,
    DistanceKM,
    EntryFee,
    MaximumParticipants
)
VALUES
(
    2,
    '50 KM Cycle',
    50.00,
    300.00,
    1500
),
(
    2,
    '109 KM Cycle',
    109.00,
    500.00,
    3000
);


-- Categories for Event 3
INSERT INTO Categories
(
    EventID,
    CategoryName,
    DistanceKM,
    EntryFee,
    MaximumParticipants
)
VALUES
(
    3,
    '5 KM Walk',
    5.00,
    80.00,
    500
),
(
    3,
    '10 KM Walk',
    10.00,
    120.00,
    1000
);
GO


/* =============================================
   INSERT ENROLMENTS
   ============================================= */

INSERT INTO Enrolments
(
    ParticipantID,
    EventID,
    CategoryID,
    BibNumber,
    Status
)
VALUES
(
    3,
    1,
    1,
    1001,
    'Confirmed'
),
(
    4,
    1,
    2,
    1002,
    'Confirmed'
),
(
    3,
    2,
    3,
    2001,
    'Confirmed'
),
(
    4,
    3,
    5,
    3001,
    'Confirmed'
);
GO


/* =============================================
   INSERT SAMPLE RESULTS
   ============================================= */

INSERT INTO Results
(
    EnrolmentID,
    FinishPosition,
    FinishTime,
    ResultStatus
)
VALUES
(
    1,
    15,
    '00:48:32',
    'Finished'
),
(
    2,
    32,
    '01:42:15',
    'Finished'
);
GO


/* =============================================
   VIEW DATA
   ============================================= */

SELECT * FROM Users;
SELECT * FROM Events;
SELECT * FROM Routes;
SELECT * FROM Categories;
SELECT * FROM Enrolments;
SELECT * FROM Results;
GO