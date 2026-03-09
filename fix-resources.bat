@echo off
chcp 65001 >nul
echo ========================================
echo Resource File Auto-Fix Script
echo ========================================
echo.

echo [Step 1/5] Cleaning build output...
dotnet clean --verbosity minimal >nul 2>&1
rmdir /s /q "src\Presentation\IndustrySystem.Presentation.Wpf\bin" 2>nul
rmdir /s /q "src\Presentation\IndustrySystem.Presentation.Wpf\obj" 2>nul
echo OK - Cleaned
echo.

echo [Step 2/5] Checking files...
if not exist "src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.resx" (
    echo ERROR: Strings.resx not found!
    pause
    exit /b 1
)
echo OK - Strings.resx exists
echo.

echo [Step 3/5] Checking .NET SDK...
dotnet --version
echo.

echo [Step 4/5] Restoring NuGet packages...
dotnet restore --verbosity minimal >nul 2>&1
echo OK - NuGet packages restored
echo.

echo [Step 5/5] Rebuilding project...
dotnet build "src\Presentation\IndustrySystem.Presentation.Wpf\IndustrySystem.Presentation.Wpf.csproj" --no-incremental
echo.

echo ========================================
echo Script Completed
echo ========================================
echo.

if exist "src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.Designer.cs" (
    echo SUCCESS - Strings.Designer.cs exists
    dir "src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.Designer.cs"
) else (
    echo ERROR - Strings.Designer.cs not generated
    echo.
    echo Please open Visual Studio and:
    echo 1. Find Resources\Strings.resx
    echo 2. Right-click -^> Run Custom Tool
)

echo.
pause
