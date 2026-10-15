-- Script tạo CSDL & 3 bảng: tblUsers 1 - N tblUserRoles, tblUsers 1 - N tblProducts
-- (Tùy chọn: khi chạy, ứng dụng tự tạo các bảng bằng EnsureCreated và tự điền dữ liệu mẫu qua DbSeeder.
--  Dữ liệu mẫu được điền bằng code vì mật khẩu phải được băm PBKDF2 trước khi lưu.)
IF DB_ID(N'MVC7_2_AuthorizationDb') IS NULL
    CREATE DATABASE MVC7_2_AuthorizationDb;
GO
USE MVC7_2_AuthorizationDb;
GO

CREATE TABLE dbo.tblUsers
(
    UserId            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Username          NVARCHAR(50)  NOT NULL,
    PasswordHashed    NVARCHAR(256) NOT NULL,
    FullName          NVARCHAR(100) NOT NULL,
    Email             NVARCHAR(100) NULL,
    AccessFailedCount INT           NOT NULL CONSTRAINT DF_tblUsers_AccessFailedCount DEFAULT (0),
    LockoutEnd        DATETIME2     NULL,
    IsLocked          BIT           NOT NULL CONSTRAINT DF_tblUsers_IsLocked DEFAULT (0),
    CONSTRAINT UQ_tblUsers_Username UNIQUE (Username)
);
GO

CREATE TABLE dbo.tblUserRoles
(
    RoleId   INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId   INT          NOT NULL,
    RoleName NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_tblUserRoles_tblUsers FOREIGN KEY (UserId) REFERENCES dbo.tblUsers(UserId) ON DELETE CASCADE,
    CONSTRAINT CK_tblUserRoles_RoleName CHECK (RoleName IN ('NORMAL', 'STAFF', 'MANAGER')),
    CONSTRAINT UQ_tblUserRoles_User_Role UNIQUE (UserId, RoleName)
);
GO

CREATE TABLE dbo.tblProducts
(
    ProductId       INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ProductName     NVARCHAR(150)  NOT NULL,
    Price           DECIMAL(18,2)  NOT NULL,
    Quantity        INT            NOT NULL,
    CreatedByUserId INT            NOT NULL,
    CONSTRAINT FK_tblProducts_tblUsers FOREIGN KEY (CreatedByUserId) REFERENCES dbo.tblUsers(UserId)
);
GO

-- Đổi vai trò cho một tài khoản (ví dụ nâng user1 lên STAFF):
-- UPDATE dbo.tblUserRoles SET RoleName = 'STAFF' WHERE UserId = (SELECT UserId FROM dbo.tblUsers WHERE Username = 'user1');
