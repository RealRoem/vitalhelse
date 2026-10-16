@echo off
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo Fant ikke 'dotnet'. Installer .NET 9 SDK eller nyere: https://dotnet.microsoft.com/download
  exit /b 1
)
set ASPNETCORE_ENVIRONMENT=Development
echo Starter VitalHelse pa http://localhost:1515 (Ctrl+C for a avslutte)
dotnet run --project vitalhelse/VitalHelse.csproj --urls http://localhost:1515
