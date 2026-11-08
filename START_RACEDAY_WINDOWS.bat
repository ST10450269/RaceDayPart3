@echo off
cd /d "%~dp0"
where docker >nul 2>&1
if errorlevel 1 (
 echo Docker not found. Install and start Docker Desktop with Linux containers.
 pause
 exit /b 1
)
echo Starting RaceDay API, MVC, SQL Server and local Blob emulator...
docker compose up --build -d
if errorlevel 1 (
 echo Docker could not start the RaceDay stack. Check Docker Desktop and logs.
 pause
 exit /b 1
)
echo.
echo Website: http://localhost:8080
 echo Swagger: http://localhost:5080/swagger
echo If the database is initialising, inspect: docker compose logs -f api
pause
