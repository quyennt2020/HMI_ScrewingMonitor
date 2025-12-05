@echo off
echo ================================
echo HMI Screwing Monitor - Publish Script
echo ================================
echo.

echo Creating standalone executable...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

if %errorlevel% neq 0 (
    echo ERROR: Publish failed
    pause
    exit /b 1
)

echo.
echo ================================
echo Publish completed successfully!
echo ================================
echo.
echo Executable file location:
echo bin\Release\net6.0-windows\win-x64\publish\HMI_ScrewingMonitor.exe
echo.
echo You can now distribute this .exe file without requiring .NET installation on target machines.
echo.

pause
