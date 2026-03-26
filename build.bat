@echo off
setlocal
cd /d "%~dp0"

dotnet publish "ACNH.LookupBot.csproj" -c Release -o "publish"
if errorlevel 1 (
    echo Build failed.
    exit /b 1
)

echo Published to "%~dp0publish"
PAUSE
