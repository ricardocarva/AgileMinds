CREATE DATABASE IF NOT EXISTS tobenamed;
USE tobenamed;

CREATE TABLE IF NOT EXISTS givemeaname (
    placeholder VARCHAR(255) PRIMARY KEY,
    DateCreated DATETIME,
    CreatedBy VARCHAR(100)
);

INSERT INTO givemeaname (placeholder, DateCreated, CreatedBy) VALUES
('placeholder1', NOW(), 'user1'),
('placeholder2', NOW(), 'user2');
