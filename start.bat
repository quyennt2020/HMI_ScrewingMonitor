@echo off
echo ========================================
echo    HMI SCREWING MONITOR - QUICK START
echo ========================================
echo.

cd /d "%~dp0"

echo Current directory: %CD%
echo.

echo [1/4] Checking .NET installation...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ ERROR: .NET SDK not found!
    echo 📥 Please install .NET 6.0 SDK from:
    echo    https://dotnet.microsoft.com/download/dotnet/6.0
    echo.
    pause
    exit /b 1
) else (
    echo ✅ .NET SDK found
)

echo.
echo [2/4] Restoring packages...
dotnet restore --verbosity quiet
if %errorlevel% neq 0 (
    echo ❌ ERROR: Package restore failed
    pause
    exit /b 1
) else (
    echo ✅ Packages restored
)

echo.
echo [3/4] Building project...
dotnet build --configuration Release --verbosity quiet
if %errorlevel% neq 0 (
    echo ❌ ERROR: Build failed
    echo 💡 Check the error messages above
    pause
    exit /b 1
) else (
    echo ✅ Build successful
)

echo.
echo [4/4] Starting application...
echo ========================================
echo.
echo 🚀 Launching HMI Screwing Monitor...
echo 📝 Check PROJECT_STRUCTURE.md for documentation
echo ⚙️  Modify Config/devices.json to configure devices
echo.

start /b dotnet run --configuration Release

echo.
echo ✅ Application started successfully!
echo 🔧 To stop: Close the application window or press Ctrl+C
echo.

pause
