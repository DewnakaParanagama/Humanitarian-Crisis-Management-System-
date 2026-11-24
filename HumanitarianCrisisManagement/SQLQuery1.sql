SELECT Id, FullName, Email, Role, Status
FROM dbo.Users
WHERE Email = 'frontendtest01@example.com';

UPDATE dbo.Users
SET Role = 1
WHERE Email = 'frontendtest01@example.com'
  AND Role = 3;

SELECT FullName, Email, Role
FROM dbo.Users
WHERE Email = 'frontendtest01@example.com';