-- Create Database
CREATE DATABASE ThikanaTenantDB;
GO

USE ThikanaTenantDB;
GO

-- Users Table
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NIDNumber NVARCHAR(20) UNIQUE,
    BirthCertificateNumber NVARCHAR(20),
    FullNameBN NVARCHAR(200),
    FullNameEN NVARCHAR(200),
    FatherNameBN NVARCHAR(200),
    FatherNameEN NVARCHAR(200),
    MotherNameBN NVARCHAR(200),
    MotherNameEN NVARCHAR(200),
    DateOfBirth DATE,
    Gender NVARCHAR(10),
    MaritalStatus NVARCHAR(20),
    Religion NVARCHAR(50),
    MobileNumber NVARCHAR(15),
    Email NVARCHAR(100),
    PermanentAddress NVARCHAR(500),
    ProfileImage NVARCHAR(500),
    IsVerified BIT DEFAULT 0,
    VerificationStatus NVARCHAR(50) DEFAULT 'Pending',
    CompletionPercentage INT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);

-- Emergency Contacts Table
CREATE TABLE EmergencyContacts (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    NameBN NVARCHAR(200),
    Relationship NVARCHAR(100),
    MobileNumber NVARCHAR(15),
    NIDNumber NVARCHAR(20),
    Address NVARCHAR(500),
    IsNIDVerified BIT DEFAULT 0
);

-- Family Members Table
CREATE TABLE FamilyMembers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    NameBN NVARCHAR(200),
    NameEN NVARCHAR(200),
    Relationship NVARCHAR(100),
    Age INT,
    Occupation NVARCHAR(100),
    MobileNumber NVARCHAR(15),
    NIDNumber NVARCHAR(20),
    IsNIDVerified BIT DEFAULT 0
);

-- House Workers Table
CREATE TABLE HouseWorkers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    WorkerType NVARCHAR(50), -- Driver, Maid, Guard, etc.
    NameBN NVARCHAR(200),
    NameEN NVARCHAR(200),
    NIDNumber NVARCHAR(20),
    MobileNumber NVARCHAR(15),
    PermanentAddress NVARCHAR(500),
    IsValidFromPolice BIT DEFAULT 1,
    ValidationMessage NVARCHAR(500),
    IsDangerFlag BIT DEFAULT 0
);

-- Previous Landlords Table
CREATE TABLE PreviousLandlords (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    NameBN NVARCHAR(200),
    MobileNumber NVARCHAR(15),
    Address NVARCHAR(500),
    NIDNumber NVARCHAR(20),
    LeavingReason NVARCHAR(500),
    StayDuration NVARCHAR(100),
    IsNIDVerified BIT DEFAULT 0
);

-- Current Landlord Table
CREATE TABLE CurrentLandlord (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    NameBN NVARCHAR(200),
    MobileNumber NVARCHAR(15),
    NIDNumber NVARCHAR(20),
    Address NVARCHAR(500),
    FromDate DATE,
    ToDate DATE,
    UpdatedAt DATETIME DEFAULT GETDATE(),
    IsVerified BIT DEFAULT 0
);

-- Current Residence Table
CREATE TABLE CurrentResidence (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    FlatFloor NVARCHAR(100),
    HouseHolding NVARCHAR(100),
    Road NVARCHAR(200),
    Area NVARCHAR(200),
    PostCode NVARCHAR(20),
    Thana NVARCHAR(100),
    District NVARCHAR(100),
    Division NVARCHAR(100)
);

-- Verification Logs Table
CREATE TABLE VerificationLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    VerifiedBy NVARCHAR(100),
    VerificationType NVARCHAR(50),
    Status NVARCHAR(50),
    Comments NVARCHAR(500),
    VerifiedAt DATETIME DEFAULT GETDATE()
);

-- OTP Table
CREATE TABLE OTPs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    MobileNumber NVARCHAR(15),
    OTPCode NVARCHAR(10),
    ExpiryTime DATETIME,
    IsUsed BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);

-- Landlord Notifications
CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY(1,1),
    LandlordId INT,
    TenantId INT,
    MessageBN NVARCHAR(500),
    MessageEN NVARCHAR(500),
    NotificationType NVARCHAR(50),
    IsRead BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);