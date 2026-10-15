-- Dữ liệu mẫu cho tblUsers, tblUserRoles, tblProducts
-- Chạy SAU KHI đã tạo 3 bảng (MVC7_2_Schema.sql hoặc tự tạo bằng tay).
-- Mật khẩu của tất cả tài khoản: 12345678
-- (PasswordHashed là chuỗi đã băm PBKDF2-SHA256 theo đúng định dạng của Services/PasswordHasher.cs)
USE MVC7_2_AuthorizationDb;
GO

-- 1. tblUsers
INSERT INTO dbo.tblUsers (Username, PasswordHashed, FullName, Email, AccessFailedCount, LockoutEnd, IsLocked) VALUES
(N'manager', N'100000.vxWJtZlqF/vKqbHxWmfZjg==.yrysnnyXW2lEUXDiGuuJhPB7pdFCF00KYUC5ZJkxiR8=', N'Nguyễn Văn Quản Lý', N'manager@shop.vn', 0, NULL, 0),
(N'staff1', N'100000.RR7BwMmf0t90RhddiSwwcg==.se4MaD41fBmxqvmdvU7j7/c233MfjzLzFDEzXoFTGgc=', N'Trần Thị Nhân Viên', N'staff1@shop.vn', 0, NULL, 0),
(N'staff2', N'100000.WJoLbWYBh9zfoc6DvfZZRg==.D+RxspSL7pOqRNX2tFcKkmW/hhxNSddsHysgRFJZfOY=', N'Lê Văn Nhân Viên', N'staff2@shop.vn', 0, NULL, 0),
(N'user1', N'100000.tMTVa8Ps6L4o2faAVaWtZA==.M6Rrb/MSzZr9WgUupWCTOJot9wXAZ/G9BvuLvuYZRXM=', N'Phạm Văn Khách', N'user1@shop.vn', 0, NULL, 0);

-- 2. tblUserRoles (lấy UserId theo Username nên không phụ thuộc giá trị IDENTITY)
INSERT INTO dbo.tblUserRoles (UserId, RoleName)
SELECT UserId, N'MANAGER' FROM dbo.tblUsers WHERE Username = N'manager'
UNION ALL SELECT UserId, N'STAFF' FROM dbo.tblUsers WHERE Username = N'staff1'
UNION ALL SELECT UserId, N'STAFF' FROM dbo.tblUsers WHERE Username = N'staff2'
UNION ALL SELECT UserId, N'NORMAL' FROM dbo.tblUsers WHERE Username = N'user1';

-- 3. tblProducts (CreatedByUserId lấy theo Username người tạo)
INSERT INTO dbo.tblProducts (ProductName, Price, Quantity, CreatedByUserId)
SELECT N'Laptop Dell Inspiron 15', 15990000, 10, UserId FROM dbo.tblUsers WHERE Username = N'manager'
UNION ALL SELECT N'Chuột Logitech M331', 350000, 50, UserId FROM dbo.tblUsers WHERE Username = N'manager'
UNION ALL SELECT N'Bàn phím cơ Akko 3087', 1290000, 25, UserId FROM dbo.tblUsers WHERE Username = N'staff1'
UNION ALL SELECT N'Màn hình LG 24 inch', 3490000, 15, UserId FROM dbo.tblUsers WHERE Username = N'staff1'
UNION ALL SELECT N'Tai nghe Sony WH-1000XM5', 7990000, 8, UserId FROM dbo.tblUsers WHERE Username = N'staff2'
UNION ALL SELECT N'USB Kingston 64GB', 150000, 100, UserId FROM dbo.tblUsers WHERE Username = N'staff2';
GO

-- Kiểm tra
SELECT u.UserId, u.Username, r.RoleName FROM dbo.tblUsers u JOIN dbo.tblUserRoles r ON r.UserId = u.UserId;
SELECT p.*, u.Username AS NguoiTao FROM dbo.tblProducts p JOIN dbo.tblUsers u ON u.UserId = p.CreatedByUserId;
