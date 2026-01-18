-- Create Database
CREATE DATABASE ThikanaTenantDB;
GO

USE ThikanaTenantDB;
GO

-- Users Table (Updated)
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NIDNumber NVARCHAR(20) UNIQUE,
    BirthCertificateNumber NVARCHAR(20),
    FullNameBN NVARCHAR(200) NOT NULL,
    FullNameEN NVARCHAR(200),
    FatherNameBN NVARCHAR(200) NOT NULL,
    FatherNameEN NVARCHAR(200),
    MotherNameBN NVARCHAR(200),
    MotherNameEN NVARCHAR(200),
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    MaritalStatus NVARCHAR(20) NOT NULL,
    Religion NVARCHAR(50) NOT NULL,
    MobileNumber NVARCHAR(15) NOT NULL,
    Email NVARCHAR(100),
    PermanentAddress NVARCHAR(500) NOT NULL,
    ProfileImage NVARCHAR(500),
    IsVerified BIT DEFAULT 0,
    VerificationStatus NVARCHAR(50) DEFAULT 'Pending',
    CompletionPercentage INT DEFAULT 0,
    RegistrationDate DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    PasswordHash NVARCHAR(255) NULL
    INDEX IX_Users_MobileNumber (MobileNumber),
    INDEX IX_Users_VerificationStatus (VerificationStatus),
    INDEX IX_Users_CreatedAt (CreatedAt DESC),
    INDEX IX_Users_IsActive_CreatedAt (IsActive, CreatedAt DESC)
);

-- Emergency Contacts Table (Updated)
CREATE TABLE EmergencyContacts (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    NameBN NVARCHAR(200) NOT NULL,
    Relationship NVARCHAR(100) NOT NULL,
    MobileNumber NVARCHAR(15) NOT NULL,
    NIDNumber NVARCHAR(20),
    Address NVARCHAR(500) NOT NULL,
    IsNIDVerified BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    INDEX IX_EmergencyContacts_UserId (UserId),
    INDEX IX_EmergencyContacts_MobileNumber (MobileNumber)
);

-- Family Members Table (Updated)
CREATE TABLE FamilyMembers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    NameBN NVARCHAR(200) NOT NULL,
    NameEN NVARCHAR(200),
    Relationship NVARCHAR(100) NOT NULL,
    Age INT NOT NULL,
    Occupation NVARCHAR(100),
    MobileNumber NVARCHAR(15),
    NIDNumber NVARCHAR(20),
    IsNIDVerified BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    INDEX IX_FamilyMembers_UserId (UserId),
    INDEX IX_FamilyMembers_Relationship (Relationship)
);

-- House Workers Table (Updated)
CREATE TABLE HouseWorkers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    WorkerType NVARCHAR(50) NOT NULL, -- Driver, Maid, Guard, etc.
    NameBN NVARCHAR(200) NOT NULL,
    NameEN NVARCHAR(200),
    NIDNumber NVARCHAR(20) NOT NULL,
    MobileNumber NVARCHAR(15) NOT NULL,
    PermanentAddress NVARCHAR(500) NOT NULL,
    IsValidFromPolice BIT DEFAULT 1,
    ValidationMessage NVARCHAR(500),
    IsDangerFlag BIT DEFAULT 0,
    PoliceVerifiedDate DATETIME NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    INDEX IX_HouseWorkers_UserId (UserId),
    INDEX IX_HouseWorkers_NIDNumber (NIDNumber),
    INDEX IX_HouseWorkers_IsDangerFlag (IsDangerFlag),
    INDEX IX_HouseWorkers_WorkerType (WorkerType)
);

-- Previous Landlords Table (Updated)
CREATE TABLE PreviousLandlords (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    NameBN NVARCHAR(200) NOT NULL,
    MobileNumber NVARCHAR(15) NOT NULL,
    Address NVARCHAR(500) NOT NULL,
    NIDNumber NVARCHAR(20),
    LeavingReason NVARCHAR(500) NOT NULL,
    StayDuration NVARCHAR(100),
    IsNIDVerified BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    INDEX IX_PreviousLandlords_UserId (UserId)
);

-- Current Landlord Table (Updated)
CREATE TABLE CurrentLandlord (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    NameBN NVARCHAR(200) NOT NULL,
    MobileNumber NVARCHAR(15) NOT NULL,
    NIDNumber NVARCHAR(20),
    Address NVARCHAR(500) NOT NULL,
    FromDate DATE NOT NULL,
    ToDate DATE NULL,
    IsVerified BIT DEFAULT 0,
    VerificationDate DATETIME NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    UNIQUE (UserId),
    INDEX IX_CurrentLandlord_MobileNumber (MobileNumber),
    INDEX IX_CurrentLandlord_IsVerified (IsVerified)
);

-- Current Residence Table (Updated)
CREATE TABLE CurrentResidence (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    FlatFloor NVARCHAR(100),
    HouseHolding NVARCHAR(100),
    Road NVARCHAR(200),
    Area NVARCHAR(200),
    PostCode NVARCHAR(20),
    Thana NVARCHAR(100),
    District NVARCHAR(100),
    Division NVARCHAR(100),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    UNIQUE (UserId),
    INDEX IX_CurrentResidence_Thana (Thana),
    INDEX IX_CurrentResidence_District (District),
    INDEX IX_CurrentResidence_Division (Division)
);

-- Verification Logs Table (Updated)
CREATE TABLE VerificationLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    VerifiedBy NVARCHAR(100) NOT NULL,
    VerifierType NVARCHAR(50) NOT NULL, -- Police, Landlord, Admin, System
    VerificationType NVARCHAR(50) NOT NULL, -- NID, Police, Landlord, Address
    Status NVARCHAR(50) NOT NULL, -- Approved, Rejected, Pending
    Comments NVARCHAR(500),
    VerifiedAt DATETIME DEFAULT GETDATE(),
    NextVerificationDate DATETIME NULL,
    
    INDEX IX_VerificationLogs_UserId (UserId),
    INDEX IX_VerificationLogs_VerifiedAt (VerifiedAt DESC),
    INDEX IX_VerificationLogs_Status (Status),
    INDEX IX_VerificationLogs_VerifierType (VerifierType),
    INDEX IX_VerificationLogs_UserId_VerifiedAt (UserId, VerifiedAt DESC)
);

-- Verification Requests Table (New)
CREATE TABLE VerificationRequests (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    RequestType NVARCHAR(50) NOT NULL, -- New, Update, Renew
    Status NVARCHAR(50) DEFAULT 'Pending', -- Pending, InProgress, Completed, Rejected
    Priority NVARCHAR(20) DEFAULT 'Normal', -- Low, Normal, High, Urgent
    Description NVARCHAR(500),
    RequestedAt DATETIME DEFAULT GETDATE(),
    ProcessedAt DATETIME NULL,
    ProcessedBy NVARCHAR(100),
    AdminComments NVARCHAR(500),
    
    INDEX IX_VerificationRequests_UserId (UserId),
    INDEX IX_VerificationRequests_Status (Status),
    INDEX IX_VerificationRequests_RequestedAt (RequestedAt DESC)
);

-- Police Verification Requests Table (New)
CREATE TABLE PoliceVerificationRequests (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    NIDNumber NVARCHAR(20) NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Pending', -- Pending, Processing, Verified, Rejected
    PoliceStation NVARCHAR(200) NOT NULL,
    RequestDate DATETIME DEFAULT GETDATE(),
    ResponseDate DATETIME NULL,
    PoliceId NVARCHAR(50),
    Comments NVARCHAR(500),
    HasCriminalRecord BIT DEFAULT 0,
    CriminalRecordDetails NVARCHAR(MAX),
    OverallStatus NVARCHAR(50) DEFAULT 'Clean', -- Clean, Warning, Risky
    
    INDEX IX_PoliceVerificationRequests_UserId (UserId),
    INDEX IX_PoliceVerificationRequests_NIDNumber (NIDNumber),
    INDEX IX_PoliceVerificationRequests_Status (Status),
    INDEX IX_PoliceVerificationRequests_OverallStatus (OverallStatus),
    INDEX IX_PoliceVerificationRequests_Status_OverallStatus (Status, OverallStatus)
);

-- OTP Table (Updated)
CREATE TABLE OTPs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    MobileNumber NVARCHAR(15) NOT NULL,
    OTPCode NVARCHAR(10) NOT NULL,
    ExpiryTime DATETIME NOT NULL,
    IsUsed BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    INDEX IX_OTPs_MobileNumber (MobileNumber),
    INDEX IX_OTPs_ExpiryTime (ExpiryTime),
    INDEX IX_OTPs_IsUsed (IsUsed)
);

-- Landlords Table (New)
CREATE TABLE Landlords (
    Id INT PRIMARY KEY IDENTITY(1,1),
    NameBN NVARCHAR(200) NOT NULL,
    MobileNumber NVARCHAR(15) NOT NULL UNIQUE,
    NIDNumber NVARCHAR(20) UNIQUE,
    Email NVARCHAR(100),
    Address NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME NULL,
    
    INDEX IX_Landlords_MobileNumber (MobileNumber),
    INDEX IX_Landlords_IsActive (IsActive)
);

-- Notifications Table (Updated)
CREATE TABLE Notifications (
    Id INT PRIMARY KEY IDENTITY(1,1),
    LandlordId INT NULL FOREIGN KEY REFERENCES Landlords(Id) ON DELETE CASCADE,
    TenantId INT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    MessageBN NVARCHAR(500) NOT NULL,
    MessageEN NVARCHAR(500),
    Subject NVARCHAR(200),
    NotificationType NVARCHAR(50) NOT NULL, -- Verification, Update, Alert
    IsRead BIT DEFAULT 0,
    IsImportant BIT DEFAULT 0,
    ActionLink NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    
    INDEX IX_Notifications_LandlordId (LandlordId),
    INDEX IX_Notifications_TenantId (TenantId),
    INDEX IX_Notifications_IsRead (IsRead),
    INDEX IX_Notifications_CreatedAt (CreatedAt DESC),
    INDEX IX_Notifications_NotificationType (NotificationType),
    INDEX IX_Notifications_IsRead_IsImportant (IsRead, IsImportant)
);

-- Document Attachments Table (New)
CREATE TABLE DocumentAttachments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    DocumentType NVARCHAR(50) NOT NULL, -- NID, Photo, Agreement, UtilityBill
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FileSize BIGINT NOT NULL,
    MimeType NVARCHAR(100) NOT NULL,
    IsVerified BIT DEFAULT 0,
    VerificationDate DATETIME NULL,
    Comments NVARCHAR(500),
    UploadedAt DATETIME DEFAULT GETDATE(),
    
    INDEX IX_DocumentAttachments_UserId (UserId),
    INDEX IX_DocumentAttachments_DocumentType (DocumentType),
    INDEX IX_DocumentAttachments_IsVerified (IsVerified)
);

-- System Admins Table (New)
CREATE TABLE SystemAdmins (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Designation NVARCHAR(100) NOT NULL,
    MobileNumber NVARCHAR(15) NOT NULL,
    NIDNumber NVARCHAR(20),
    PoliceStation NVARCHAR(200),
    Thana NVARCHAR(100),
    District NVARCHAR(100),
    Division NVARCHAR(100),
    AccessLevel NVARCHAR(50) NOT NULL, -- Station, Thana, District, Division, Super
    PasswordResetToken NVARCHAR(255),
    TokenExpiry DATETIME NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME NULL,
    
    INDEX IX_SystemAdmins_Email (Email),
    INDEX IX_SystemAdmins_AccessLevel (AccessLevel),
    INDEX IX_SystemAdmins_IsActive (IsActive)
);

-- Audit Logs Table (New)
CREATE TABLE AuditLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId NVARCHAR(100),
    Action NVARCHAR(100) NOT NULL,
    Entity NVARCHAR(100) NOT NULL,
    EntityId INT NOT NULL,
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX),
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    Timestamp DATETIME DEFAULT GETDATE(),
    
    INDEX IX_AuditLogs_UserId (UserId),
    INDEX IX_AuditLogs_Entity (Entity),
    INDEX IX_AuditLogs_Timestamp (Timestamp DESC),
    INDEX IX_AuditLogs_Action (Action)
);

-- System Settings Table (New)
CREATE TABLE SystemSettings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SettingKey NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue NVARCHAR(MAX),
    Description NVARCHAR(500),
    Category NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    INDEX IX_SystemSettings_SettingKey (SettingKey),
    INDEX IX_SystemSettings_Category (Category)
);

-- Message Templates Table (New)
CREATE TABLE MessageTemplates (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TemplateName NVARCHAR(100) NOT NULL UNIQUE,
    TemplateType NVARCHAR(50) NOT NULL, -- SMS, Email, Notification
    SubjectBN NVARCHAR(200),
    SubjectEN NVARCHAR(200),
    BodyBN NVARCHAR(MAX) NOT NULL,
    BodyEN NVARCHAR(MAX),
    Variables NVARCHAR(500), -- Comma separated variables like {name}, {otp}
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);

-- Add Foreign Key Constraints
ALTER TABLE CurrentLandlord
ADD CONSTRAINT FK_CurrentLandlord_Users FOREIGN KEY (UserId) 
REFERENCES Users(Id) ON DELETE CASCADE;

ALTER TABLE CurrentResidence
ADD CONSTRAINT FK_CurrentResidence_Users FOREIGN KEY (UserId) 
REFERENCES Users(Id) ON DELETE CASCADE;
GO
-- Create Views

-- View for Tenant Verification Summary
CREATE VIEW TenantVerificationSummary AS
SELECT 
    u.Id,
    u.FullNameBN,
    u.NIDNumber,
    u.MobileNumber,
    u.VerificationStatus,
    u.CompletionPercentage,
    u.CreatedAt,
    cr.Thana,
    cr.District,
    cr.Division,
    cl.NameBN AS LandlordName,
    cl.MobileNumber AS LandlordMobile,
    (SELECT COUNT(*) FROM EmergencyContacts ec WHERE ec.UserId = u.Id) AS EmergencyContactsCount,
    (SELECT COUNT(*) FROM FamilyMembers fm WHERE fm.UserId = u.Id) AS FamilyMembersCount,
    (SELECT COUNT(*) FROM HouseWorkers hw WHERE hw.UserId = u.Id) AS HouseWorkersCount
FROM Users u
LEFT JOIN CurrentResidence cr ON u.Id = cr.UserId
LEFT JOIN CurrentLandlord cl ON u.Id = cl.UserId
WHERE u.IsActive = 1;
GO
--SELECT * FROM TenantVerificationSummary
-- View for Landlord Dashboard
CREATE VIEW LandlordDashboard AS
SELECT 
    l.Id AS LandlordId,
    l.NameBN AS LandlordName,
    l.MobileNumber AS LandlordMobile,
    COUNT(DISTINCT cl.UserId) AS TotalTenants,
    SUM(CASE WHEN u.VerificationStatus = 'Approved' THEN 1 ELSE 0 END) AS VerifiedTenants,
    SUM(CASE WHEN u.VerificationStatus = 'Pending' THEN 1 ELSE 0 END) AS PendingTenants,
    COUNT(DISTINCT n.Id) AS UnreadNotifications
FROM Landlords l
LEFT JOIN CurrentLandlord cl ON l.MobileNumber = cl.MobileNumber
LEFT JOIN Users u ON cl.UserId = u.Id
LEFT JOIN Notifications n ON l.Id = n.LandlordId AND n.IsRead = 0
WHERE l.IsActive = 1
GROUP BY l.Id, l.NameBN, l.MobileNumber;
GO
--SELECT * FROM LandlordDashboard

-- View for Admin Dashboard
CREATE VIEW AdminDashboard AS
SELECT 
    a.Id AS AdminId,
    a.Name AS AdminName,
    a.AccessLevel,
    a.Division,
    a.District,
    a.Thana,
    COUNT(DISTINCT u.Id) AS TotalUsers,
    COUNT(DISTINCT CASE WHEN u.VerificationStatus = 'Approved' THEN u.Id END) AS VerifiedUsers,
    COUNT(DISTINCT CASE WHEN u.VerificationStatus = 'Pending' THEN u.Id END) AS PendingUsers,
    COUNT(DISTINCT pvr.Id) AS PoliceVerifications,
    COUNT(DISTINCT CASE WHEN pvr.OverallStatus = 'Risky' THEN pvr.Id END) AS RiskyVerifications
FROM SystemAdmins a
LEFT JOIN Users u ON 
    (a.AccessLevel = 'Super') OR
    (a.AccessLevel = 'Division' AND a.Division IS NOT NULL) OR
    (a.AccessLevel = 'District' AND a.District IS NOT NULL) OR
    (a.AccessLevel = 'Thana' AND a.Thana IS NOT NULL) OR
    (a.AccessLevel = 'Station' AND a.PoliceStation IS NOT NULL)
LEFT JOIN PoliceVerificationRequests pvr ON u.Id = pvr.UserId
WHERE a.IsActive = 1
GROUP BY a.Id, a.Name, a.AccessLevel, a.Division, a.District, a.Thana;
GO
--SELECT * FROM AdminDashboard

-- Create Stored Procedures

-- Stored Procedure for User Completion Percentage
CREATE PROCEDURE CalculateUserCompletionPercentage
    @UserId INT
AS
BEGIN
    DECLARE @TotalFields INT = 15;
    DECLARE @CompletedFields INT = 0;
    DECLARE @Percentage INT;

    -- Check user basic info (5 fields)
    IF EXISTS (SELECT 1 FROM Users WHERE Id = @UserId AND 
               FullNameBN IS NOT NULL AND 
               FatherNameBN IS NOT NULL AND 
               MobileNumber IS NOT NULL AND 
               PermanentAddress IS NOT NULL AND
               DateOfBirth IS NOT NULL)
        SET @CompletedFields = @CompletedFields + 5;

    -- Check emergency contacts
    IF EXISTS (SELECT 1 FROM EmergencyContacts WHERE UserId = @UserId)
        SET @CompletedFields = @CompletedFields + 1;

    -- Check family members
    IF EXISTS (SELECT 1 FROM FamilyMembers WHERE UserId = @UserId)
        SET @CompletedFields = @CompletedFields + 1;

    -- Check current residence
    IF EXISTS (SELECT 1 FROM CurrentResidence WHERE UserId = @UserId)
        SET @CompletedFields = @CompletedFields + 1;

    -- Check current landlord
    IF EXISTS (SELECT 1 FROM CurrentLandlord WHERE UserId = @UserId)
        SET @CompletedFields = @CompletedFields + 1;

    -- Check house workers
    IF EXISTS (SELECT 1 FROM HouseWorkers WHERE UserId = @UserId)
        SET @CompletedFields = @CompletedFields + 1;

    -- Check previous landlords
    IF EXISTS (SELECT 1 FROM PreviousLandlords WHERE UserId = @UserId)
        SET @CompletedFields = @CompletedFields + 1;

    -- Check documents (NID)
    IF EXISTS (SELECT 1 FROM DocumentAttachments WHERE UserId = @UserId AND DocumentType = 'NID')
        SET @CompletedFields = @CompletedFields + 1;

    -- Check documents (Photo)
    IF EXISTS (SELECT 1 FROM DocumentAttachments WHERE UserId = @UserId AND DocumentType = 'Photo')
        SET @CompletedFields = @CompletedFields + 1;

    -- Calculate percentage
    SET @Percentage = (@CompletedFields * 100) / @TotalFields;

    -- Update user
    UPDATE Users 
    SET CompletionPercentage = @Percentage,
        UpdatedAt = GETDATE()
    WHERE Id = @UserId;

    SELECT @Percentage AS CompletionPercentage;
END
GO

-- Stored Procedure for Dashboard Statistics
CREATE PROCEDURE GetDashboardStats
    @UserId INT
AS
BEGIN
    SELECT 
        (SELECT COUNT(*) FROM EmergencyContacts WHERE UserId = @UserId) AS EmergencyContactsCount,
        (SELECT COUNT(*) FROM FamilyMembers WHERE UserId = @UserId) AS FamilyMembersCount,
        (SELECT COUNT(*) FROM HouseWorkers WHERE UserId = @UserId) AS HouseWorkersCount,
        (SELECT COUNT(*) FROM PreviousLandlords WHERE UserId = @UserId) AS PreviousLandlordsCount,
        (SELECT COUNT(*) FROM DocumentAttachments WHERE UserId = @UserId) AS DocumentsCount,
        (SELECT COUNT(*) FROM VerificationLogs WHERE UserId = @UserId AND Status = 'Approved') AS ApprovedVerificationsCount,
        (SELECT COUNT(*) FROM Notifications WHERE TenantId = @UserId AND IsRead = 0) AS UnreadNotificationsCount,
        (SELECT CompletionPercentage FROM Users WHERE Id = @UserId) AS CompletionPercentage;
END
GO

-- Stored Procedure for Admin Statistics
CREATE PROCEDURE GetAdminDashboardStats
    @AdminId INT
AS
BEGIN
    DECLARE @AccessLevel NVARCHAR(50);
    DECLARE @Division NVARCHAR(100);
    DECLARE @District NVARCHAR(100);
    DECLARE @Thana NVARCHAR(100);
    DECLARE @PoliceStation NVARCHAR(200);

    -- Get admin details
    SELECT 
        @AccessLevel = AccessLevel,
        @Division = Division,
        @District = District,
        @Thana = Thana,
        @PoliceStation = PoliceStation
    FROM SystemAdmins WHERE Id = @AdminId;

    -- Build dynamic query based on access level
    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @WhereClause NVARCHAR(MAX) = '';

    IF @AccessLevel = 'Division' AND @Division IS NOT NULL
        SET @WhereClause = 'WHERE cr.Division = @Division';
    ELSE IF @AccessLevel = 'District' AND @District IS NOT NULL
        SET @WhereClause = 'WHERE cr.District = @District';
    ELSE IF @AccessLevel = 'Thana' AND @Thana IS NOT NULL
        SET @WhereClause = 'WHERE cr.Thana = @Thana';
    ELSE IF @AccessLevel = 'Station' AND @PoliceStation IS NOT NULL
        SET @WhereClause = 'WHERE pvr.PoliceStation = @PoliceStation';

    SET @SQL = N'
    SELECT 
        COUNT(DISTINCT u.Id) AS TotalUsers,
        COUNT(DISTINCT CASE WHEN u.VerificationStatus = ''Approved'' THEN u.Id END) AS VerifiedUsers,
        COUNT(DISTINCT CASE WHEN u.VerificationStatus = ''Pending'' THEN u.Id END) AS PendingUsers,
        COUNT(DISTINCT CASE WHEN u.VerificationStatus = ''Rejected'' THEN u.Id END) AS RejectedUsers,
        COUNT(DISTINCT pvr.Id) AS TotalPoliceVerifications,
        COUNT(DISTINCT CASE WHEN pvr.OverallStatus = ''Clean'' THEN pvr.Id END) AS CleanVerifications,
        COUNT(DISTINCT CASE WHEN pvr.OverallStatus = ''Warning'' THEN pvr.Id END) AS WarningVerifications,
        COUNT(DISTINCT CASE WHEN pvr.OverallStatus = ''Risky'' THEN pvr.Id END) AS RiskyVerifications,
        COUNT(DISTINCT hw.Id) AS TotalHouseWorkers,
        COUNT(DISTINCT CASE WHEN hw.IsDangerFlag = 1 THEN hw.Id END) AS RiskyHouseWorkers
    FROM Users u
    LEFT JOIN CurrentResidence cr ON u.Id = cr.UserId
    LEFT JOIN PoliceVerificationRequests pvr ON u.Id = pvr.UserId
    LEFT JOIN HouseWorkers hw ON u.Id = hw.UserId
    ' + @WhereClause;

    EXEC sp_executesql @SQL, 
        N'@Division NVARCHAR(100), @District NVARCHAR(100), @Thana NVARCHAR(100), @PoliceStation NVARCHAR(200)',
        @Division, @District, @Thana, @PoliceStation;
END
GO

-- Stored Procedure for User Registration with OTP
CREATE PROCEDURE RegisterUserWithOTP
    @NIDNumber NVARCHAR(20),
    @MobileNumber NVARCHAR(15),
    @OTPCode NVARCHAR(10),
    @FullNameBN NVARCHAR(200),
    @FatherNameBN NVARCHAR(200),
    @DateOfBirth DATE,
    @Gender NVARCHAR(10),
    @MaritalStatus NVARCHAR(20),
    @Religion NVARCHAR(50),
    @PermanentAddress NVARCHAR(500)
AS
BEGIN
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Verify OTP
        IF NOT EXISTS (
            SELECT 1 FROM OTPs 
            WHERE MobileNumber = @MobileNumber 
            AND OTPCode = @OTPCode 
            AND ExpiryTime > GETDATE() 
            AND IsUsed = 0
        )
        BEGIN
            ROLLBACK;
            SELECT 'INVALID_OTP' AS Result;
            RETURN;
        END

        -- Check if user already exists
        IF EXISTS (SELECT 1 FROM Users WHERE NIDNumber = @NIDNumber OR MobileNumber = @MobileNumber)
        BEGIN
            ROLLBACK;
            SELECT 'USER_EXISTS' AS Result;
            RETURN;
        END

        -- Create user
        INSERT INTO Users (
            NIDNumber, MobileNumber, FullNameBN, FatherNameBN, 
            DateOfBirth, Gender, MaritalStatus, Religion, PermanentAddress,
            CreatedAt, UpdatedAt, IsActive
        ) VALUES (
            @NIDNumber, @MobileNumber, @FullNameBN, @FatherNameBN,
            @DateOfBirth, @Gender, @MaritalStatus, @Religion, @PermanentAddress,
            GETDATE(), GETDATE(), 1
        );

        DECLARE @UserId INT = SCOPE_IDENTITY();

        -- Mark OTP as used
        UPDATE OTPs SET IsUsed = 1 
        WHERE MobileNumber = @MobileNumber AND OTPCode = @OTPCode;

        -- Create verification log
        INSERT INTO VerificationLogs (UserId, VerifiedBy, VerifierType, VerificationType, Status, VerifiedAt)
        VALUES (@UserId, 'System', 'System', 'Registration', 'Pending', GETDATE());

        -- Create verification request
        INSERT INTO VerificationRequests (UserId, RequestType, Status, Priority, RequestedAt)
        VALUES (@UserId, 'New', 'Pending', 'Normal', GETDATE());

        COMMIT;
        SELECT 'SUCCESS' AS Result, @UserId AS UserId;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SELECT 'ERROR' AS Result, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

-- Stored Procedure for Police Verification
CREATE PROCEDURE ProcessPoliceVerification
    @UserId INT,
    @NIDNumber NVARCHAR(20),
    @PoliceStation NVARCHAR(200),
    @PoliceId NVARCHAR(50),
    @HasCriminalRecord BIT,
    @CriminalRecordDetails NVARCHAR(MAX),
    @OverallStatus NVARCHAR(50),
    @Comments NVARCHAR(500)
AS
BEGIN
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Update or insert police verification
        IF EXISTS (SELECT 1 FROM PoliceVerificationRequests WHERE UserId = @UserId AND NIDNumber = @NIDNumber)
        BEGIN
            UPDATE PoliceVerificationRequests
            SET Status = 'Verified',
                ResponseDate = GETDATE(),
                PoliceId = @PoliceId,
                HasCriminalRecord = @HasCriminalRecord,
                CriminalRecordDetails = @CriminalRecordDetails,
                OverallStatus = @OverallStatus,
                Comments = @Comments
            WHERE UserId = @UserId AND NIDNumber = @NIDNumber;
        END
        ELSE
        BEGIN
            INSERT INTO PoliceVerificationRequests (
                UserId, NIDNumber, Status, PoliceStation, 
                RequestDate, ResponseDate, PoliceId,
                HasCriminalRecord, CriminalRecordDetails, OverallStatus, Comments
            ) VALUES (
                @UserId, @NIDNumber, 'Verified', @PoliceStation,
                GETDATE(), GETDATE(), @PoliceId,
                @HasCriminalRecord, @CriminalRecordDetails, @OverallStatus, @Comments
            );
        END

        -- Update house workers if any
        UPDATE hw
        SET hw.IsValidFromPolice = CASE WHEN @OverallStatus = 'Clean' THEN 1 ELSE 0 END,
            hw.ValidationMessage = @Comments,
            hw.IsDangerFlag = CASE WHEN @OverallStatus = 'Risky' THEN 1 ELSE 0 END,
            hw.PoliceVerifiedDate = GETDATE(),
            hw.UpdatedAt = GETDATE()
        FROM HouseWorkers hw
        WHERE hw.UserId = @UserId AND hw.NIDNumber = @NIDNumber;

        -- Create verification log
        INSERT INTO VerificationLogs (UserId, VerifiedBy, VerifierType, VerificationType, Status, Comments, VerifiedAt)
        VALUES (@UserId, @PoliceId, 'Police', 'PoliceVerification', 
                CASE WHEN @OverallStatus = 'Clean' THEN 'Approved' ELSE 'Rejected' END,
                @Comments, GETDATE());

        COMMIT;
        SELECT 'SUCCESS' AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK;
        SELECT 'ERROR' AS Result, ERROR_MESSAGE() AS ErrorMessage;
    END CATCH
END
GO

-- Insert Default System Settings
INSERT INTO SystemSettings (SettingKey, SettingValue, Description, Category) VALUES
('OTPExpiryMinutes', '5', 'OTP expiration time in minutes', 'Security'),
('MaxLoginAttempts', '3', 'Maximum login attempts before lockout', 'Security'),
('LockoutMinutes', '15', 'Account lockout duration in minutes', 'Security'),
('VerificationThreshold', '90', 'Minimum completion percentage for verification', 'Verification'),
('DefaultThana', N'ধানমন্ডি', 'Default thana for new registrations', 'Location'),
('DefaultDistrict', N'ঢাকা', 'Default district for new registrations', 'Location'),
('DefaultDivision', N'ঢাকা', 'Default division for new registrations', 'Location'),
('SMSProvider', 'Demo', 'SMS provider name', 'Communication'),
('EmailProvider', 'SMTP', 'Email provider name', 'Communication'),
('MaxDocumentSizeMB', '5', 'Maximum document upload size in MB', 'Document'),
('AllowedDocumentTypes', 'jpg,jpeg,png,pdf', 'Allowed document file types', 'Document'),
('SystemMaintenance', 'false', 'System maintenance mode', 'System'),
('DataRetentionDays', '365', 'Days to keep audit logs', 'System'),
('PoliceVerificationRequired', 'true', 'Police verification required for house workers', 'Verification'),
('AutoNotificationEnabled', 'true', 'Enable automatic notifications', 'Notification');

-- Insert Default Message Templates
INSERT INTO MessageTemplates (TemplateName, TemplateType, SubjectBN, BodyBN, Variables) VALUES
('OTPVerification', 'SMS', NULL, N'আপনার OTP কোড: {otp}. এটি {minutes} মিনিটের জন্য বৈধ।', '{otp},{minutes}'),
('RegistrationSuccess', 'SMS', NULL, N'স্বাগতম {name}! আপনার রেজিস্ট্রেশন সফলভাবে সম্পন্ন হয়েছে।', '{name}'),
('VerificationPending', 'Notification', N'যাচাইকরণ পেন্ডিং', 'ভাড়াটিয়া {tenantName} এর যাচাইকরণ অনুরোধ পেন্ডিং রয়েছে।', '{tenantName}'),
('VerificationCompleted', 'Email', N'যাচাইকরণ সম্পন্ন', 'প্রিয় {landlordName}, ভাড়াটিয়া {tenantName} এর যাচাইকরণ সম্পন্ন হয়েছে।', '{landlordName},{tenantName}'),
('PoliceVerificationAlert', 'Notification', N'পুলিশ যাচাইকরণ প্রয়োজন', 'গৃহকর্মী {workerName} এর পুলিশ যাচাইকরণ প্রয়োজন।', '{workerName}'),
('DocumentUploadReminder', 'Notification', N'ডকুমেন্ট আপলোড করুন', 'আপনার {documentType} আপলোড করুন যাচাইকরণ সম্পন্ন করতে।', '{documentType}');

-- Insert Default Super Admin (Password: Admin@123)
INSERT INTO SystemAdmins (Name, Email, PasswordHash, Designation, MobileNumber, AccessLevel) 
VALUES (N'সুপার এডমিন', 'admin@thikana.gov.bd', 
        '$2a$11$YOUR_HASHED_PASSWORD_HERE', -- Use BCrypt to hash 'Admin@123'
        N'সুপার এডমিনিস্ট্রেটর', '01700000000', 'Super');

-- Insert Sample Landlord
INSERT INTO Landlords (NameBN, MobileNumber, Email, Address) 
VALUES (N'মোঃ রফিকুল ইসলাম', '01712345678', 'rafiq@example.com', '১২৩, গুলশান, ঢাকা');

-- Insert Sample User for Testing
INSERT INTO Users (
    NIDNumber, FullNameBN, FatherNameBN, DateOfBirth, Gender, 
    MaritalStatus, Religion, MobileNumber, PermanentAddress,
    VerificationStatus, CompletionPercentage
) VALUES (
    '1991234567890', N'আহমেদ হাসান', N'মোঃ আব্দুল করিম', '1990-05-15', N'পুরুষ',
    N'বিবাহিত', N'ইসলাম', '01712345678', N'১২৩, ধানমন্ডি, ঢাকা',
    'Pending', 30
);

PRINT 'Database schema created successfully with sample data!';
GO

-- Trigger for updating UpdatedAt timestamp on Users
CREATE TRIGGER trg_Users_UpdateTimestamp
ON Users
AFTER UPDATE
AS
BEGIN
    UPDATE Users
    SET UpdatedAt = GETDATE()
    FROM Users u
    INNER JOIN inserted i ON u.Id = i.Id;
END
GO

-- Trigger for audit logging on Users table
CREATE TRIGGER trg_Users_Audit
ON Users
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    DECLARE @UserId NVARCHAR(100);
    DECLARE @IpAddress NVARCHAR(50);
    DECLARE @UserAgent NVARCHAR(500);
    
    -- Get user context (you might need to adjust based on your auth system)
    SET @UserId = SYSTEM_USER;
    
    -- Log INSERT operations
    IF EXISTS (SELECT * FROM inserted) AND NOT EXISTS (SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLogs (UserId, Action, Entity, EntityId, NewValues, Timestamp)
        SELECT @UserId, 'INSERT', 'Users', i.Id, 
               CONCAT('Created user: ', i.FullNameBN, ' (NID: ', ISNULL(i.NIDNumber, 'N/A'), ')'),
               GETDATE()
        FROM inserted i;
    END
    
    -- Log UPDATE operations
    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
    BEGIN
        INSERT INTO AuditLogs (UserId, Action, Entity, EntityId, OldValues, NewValues, Timestamp)
        SELECT @UserId, 'UPDATE', 'Users', i.Id,
               CONCAT('Previous: ', d.FullNameBN, ' Status: ', d.VerificationStatus),
               CONCAT('Current: ', i.FullNameBN, ' Status: ', i.VerificationStatus),
               GETDATE()
        FROM inserted i
        INNER JOIN deleted d ON i.Id = d.Id
        WHERE i.FullNameBN != d.FullNameBN OR i.VerificationStatus != d.VerificationStatus;
    END
    
    -- Log DELETE operations
    IF EXISTS (SELECT * FROM deleted) AND NOT EXISTS (SELECT * FROM inserted)
    BEGIN
        INSERT INTO AuditLogs (UserId, Action, Entity, EntityId, OldValues, Timestamp)
        SELECT @UserId, 'DELETE', 'Users', d.Id,
               CONCAT('Deleted user: ', d.FullNameBN, ' (NID: ', ISNULL(d.NIDNumber, 'N/A'), ')'),
               GETDATE()
        FROM deleted d;
    END
END
GO

-- Trigger for automatic notification when verification threshold reached
CREATE TRIGGER trg_Users_VerificationNotification
ON Users
AFTER UPDATE
AS
BEGIN
    IF UPDATE(CompletionPercentage)
    BEGIN
        DECLARE @UserId INT;
        DECLARE @CompletionPercentage INT;
        DECLARE @LandlordMobile NVARCHAR(15);
        DECLARE @TenantName NVARCHAR(200);
        
        SELECT @UserId = i.Id, 
               @CompletionPercentage = i.CompletionPercentage,
               @TenantName = i.FullNameBN
        FROM inserted i
        INNER JOIN deleted d ON i.Id = d.Id
        WHERE i.CompletionPercentage >= 90 AND d.CompletionPercentage < 90;
        
        IF @UserId IS NOT NULL
        BEGIN
            -- Get landlord mobile from current landlord
            SELECT @LandlordMobile = cl.MobileNumber
            FROM CurrentLandlord cl
            WHERE cl.UserId = @UserId;
            
            IF @LandlordMobile IS NOT NULL
            BEGIN
                -- Create notification for landlord
                INSERT INTO Notifications (LandlordId, TenantId, MessageBN, NotificationType, IsImportant, CreatedAt)
                SELECT l.Id, @UserId, 
                       CONCAT('ভাড়াটিয়া ', @TenantName, ' এর যাচাইকরণ প্রক্রিয়া ', @CompletionPercentage, '% সম্পন্ন হয়েছে। এখন যাচাই করুন।'),
                       'Verification', 1, GETDATE()
                FROM Landlords l
                WHERE l.MobileNumber = @LandlordMobile;
            END
        END
    END
END
GO



