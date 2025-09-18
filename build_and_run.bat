@echo off
echo ================================
echo HMI Screwing Monitor - Build Script
echo ================================
echo.

echo Checking .NET installation...
dotnet --version
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found! Please install .NET 6.0 SDK
    echo Download from: https://dotnet.microsoft.com/download/dotnet/6.0
    pause
    exit /b 1
)

echo.
echo Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)

echo.
echo Building project...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo ================================
echo Build completed successfully!
echo ================================
echo.

echo Do you want to run the application? (Y/N)
set /p choice=
if /i "%choice%"=="Y" (
    echo.
    echo Starting HMI application...
    dotnet run
) else (
    echo.
    echo Build completed. You can run the application manually with: dotnet run
)

pause
