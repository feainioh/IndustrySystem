# Fix Strings Resource Generation Issue
# This script will clean and regenerate resource files

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Resource File Auto-Fix Script" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Set project paths
$projectPath = "src\Presentation\IndustrySystem.Presentation.Wpf"
$resourcesPath = "$projectPath\Resources"

# Step 1: Clean build output
Write-Host "[Step 1/5] Cleaning build output..." -ForegroundColor Yellow
dotnet clean --verbosity minimal 2>&1 | Out-Null
Remove-Item "$projectPath\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$projectPath\obj" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "OK - Cleaned`n" -ForegroundColor Green

# Step 2: Verify files exist
Write-Host "[Step 2/5] Verifying resource files..." -ForegroundColor Yellow
$resxFile = "$resourcesPath\Strings.resx"
$designerFile = "$resourcesPath\Strings.Designer.cs"

if (-not (Test-Path $resxFile)) {
    Write-Host "ERROR: Strings.resx file not found!" -ForegroundColor Red
    exit 1
}

Write-Host "OK - Strings.resx exists" -ForegroundColor Green
Write-Host "  Path: $resxFile" -ForegroundColor Gray

if (Test-Path $designerFile) {
    $fileSize = (Get-Item $designerFile).Length
    Write-Host "OK - Strings.Designer.cs exists (Size: $fileSize bytes)" -ForegroundColor Green
} else {
    Write-Host "WARNING - Strings.Designer.cs not found (will auto-generate)" -ForegroundColor Yellow
}
Write-Host ""

# Step 3: Check .NET version
Write-Host "[Step 3/5] Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
Write-Host "OK - .NET SDK Version: $dotnetVersion`n" -ForegroundColor Green

# Step 4: Restore NuGet packages
Write-Host "[Step 4/5] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore --verbosity minimal 2>&1 | Out-Null
Write-Host "OK - NuGet packages restored`n" -ForegroundColor Green

# Step 5: Rebuild project
Write-Host "[Step 5/5] Rebuilding project..." -ForegroundColor Yellow
$buildOutput = dotnet build "$projectPath\IndustrySystem.Presentation.Wpf.csproj" --no-incremental 2>&1

# Analyze build results
$errors = $buildOutput | Select-String "error" | Measure-Object
$warnings = $buildOutput | Select-String "warning" | Measure-Object

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Build Results" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($errors.Count -eq 0) {
    Write-Host "OK - Build succeeded! No errors." -ForegroundColor Green
} else {
    Write-Host "FAILED - Build failed with $($errors.Count) error(s)" -ForegroundColor Red
    Write-Host "`nError details:" -ForegroundColor Red
    $buildOutput | Select-String "error" | Select-Object -First 10 | ForEach-Object { 
        Write-Host "  $_" -ForegroundColor Red 
    }
}

if ($warnings.Count -gt 0) {
    Write-Host "WARNING - Warning count: $($warnings.Count)" -ForegroundColor Yellow
}

# Final verification
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Verify Strings.Designer.cs" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if (Test-Path $designerFile) {
    $fileInfo = Get-Item $designerFile
    Write-Host "OK - File exists" -ForegroundColor Green
    Write-Host "  Size: $($fileInfo.Length) bytes" -ForegroundColor Gray
    Write-Host "  Last modified: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
    
    # Check key properties
    $content = Get-Content $designerFile -Raw
    $keyProperties = @(
        "Lbl_NotLoggedIn",
        "Log_AlarmViewModel_Initialized",
        "Nav_Users",
        "Nav_Roles",
        "Nav_Permissions"
    )
    
    Write-Host "`nKey property check:" -ForegroundColor Cyan
    foreach ($prop in $keyProperties) {
        if ($content -match $prop) {
            Write-Host "  OK - $prop" -ForegroundColor Green
        } else {
            Write-Host "  MISSING - $prop" -ForegroundColor Red
        }
    }
} else {
    Write-Host "ERROR - Strings.Designer.cs still missing!" -ForegroundColor Red
    Write-Host "`nRecommended actions:" -ForegroundColor Yellow
    Write-Host "1. Open project in Visual Studio" -ForegroundColor Gray
    Write-Host "2. Find Resources\Strings.resx" -ForegroundColor Gray
    Write-Host "3. Right-click -> 'Run Custom Tool'" -ForegroundColor Gray
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Script Completed" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

if ($errors.Count -eq 0) {
    Write-Host "SUCCESS - Fix completed! You can now run the application." -ForegroundColor Green
    exit 0
} else {
    Write-Host "FAILED - Errors still exist. Please see fix-resource-strings.md for manual steps." -ForegroundColor Red
    exit 1
}
