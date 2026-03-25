-- MySQL Setup for Task Management Platform

-- 1. Create Databases
CREATE DATABASE IF NOT EXISTS UserDb;
CREATE DATABASE IF NOT EXISTS TaskDb;

-- 2. Setup UserDb
USE UserDb;

CREATE TABLE IF NOT EXISTS Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(255) NOT NULL,
    PasswordHash TEXT NOT NULL,
    Role VARCHAR(50) NOT NULL
);

-- Seed Users (Password is 'admin')
INSERT IGNORE INTO Users (Id, Username, PasswordHash, Role) VALUES 
(1, 'admin', '$2a$11$ghvKM208pBmjdIBrJnXlGuiSNzwRpPwHPOARhYSd.m9GS2zmoPDb.', 'Admin'),
(2, 'manager', '$2a$11$ghvKM208pBmjdIBrJnXlGuiSNzwRpPwHPOARhYSd.m9GS2zmoPDb.', 'Manager'),
(3, 'engineer', '$2a$11$ghvKM208pBmjdIBrJnXlGuiSNzwRpPwHPOARhYSd.m9GS2zmoPDb.', 'Engineer');

-- 3. Setup TaskDb
USE TaskDb;

CREATE TABLE IF NOT EXISTS Tasks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Description TEXT,
    Priority VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL,
    AssigneeId INT,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6) NOT NULL,
    DueDate DATETIME(6)
);

CREATE TABLE IF NOT EXISTS ActivityLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TaskId INT NOT NULL,
    StatusChangedTo VARCHAR(50),
    ChangedByUserId INT NOT NULL,
    Timestamp DATETIME(6) NOT NULL,
    FOREIGN KEY (TaskId) REFERENCES Tasks(Id)
);

-- Seed Tasks
INSERT IGNORE INTO Tasks (Id, Title, Description, Priority, Status, AssigneeId, CreatedAt, UpdatedAt, DueDate) VALUES 
(1, 'Initial Task', 'Setup microservices', 'High', 'In Progress', 3, '2024-03-24 10:00:00', '2024-03-25 10:00:00', '2024-03-27 10:00:00'),
(2, 'Overdue Task', 'This task should trigger an SLA breach', 'Medium', 'Open', 1, '2024-03-20 10:00:00', '2024-03-20 10:00:00', '2024-03-24 10:00:00'),
(3, 'UI Implementation', 'Build dashboard using Angular', 'High', 'Blocked', 2, '2024-03-23 10:00:00', '2024-03-25 12:00:00', '2024-03-30 10:00:00'),
(4, 'Documentation', 'Write internal API documentation', 'Low', 'Completed', 3, '2024-03-15 10:00:00', '2024-03-23 10:00:00', '2024-03-20 10:00:00');

-- Seed ActivityLogs
INSERT IGNORE INTO ActivityLogs (Id, TaskId, StatusChangedTo, ChangedByUserId, Timestamp) VALUES 
(1, 1, 'In Progress', 3, '2024-03-24 10:00:00'),
(2, 3, 'Open', 2, '2024-03-23 10:00:00'),
(3, 3, 'Blocked', 1, '2024-03-25 12:00:00');
