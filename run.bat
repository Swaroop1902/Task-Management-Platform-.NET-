@echo off
start "UserService" cmd /c "cd TaskManagement.UserService && dotnet run"
start "TaskService" cmd /c "cd TaskManagement.TaskService && dotnet run"
start "ReportingService" cmd /c "cd TaskManagement.ReportingService && dotnet run"
start "Frontend" cmd /c "cd TaskManagement.Frontend && npm start"
