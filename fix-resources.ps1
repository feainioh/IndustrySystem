# 自动修复 Strings资源生成问题
# 此脚本将清理并重新生成资源文件

# 设置控制台编码为 UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
chcp 65001 | Out-Null

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "资源文件自动修复脚本" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# 设置项目路径
$projectPath = "src\Presentation\IndustrySystem.Presentation.Wpf"
$resourcesPath = "$projectPath\Resources"

# 步骤1：清理构建输出
Write-Host "[步骤 1/5] 清理构建输出..." -ForegroundColor Yellow
dotnet clean --verbosity minimal 2>&1 | Out-Null
Remove-Item "$projectPath\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$projectPath\obj" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "✓ 清理完成`n" -ForegroundColor Green

# 步骤2：验证文件存在
Write-Host "[步骤 2/5] 验证资源文件..." -ForegroundColor Yellow
$resxFile = "$resourcesPath\Strings.resx"
$designerFile = "$resourcesPath\Strings.Designer.cs"

if (-not (Test-Path $resxFile)) {
    Write-Host "✗ 错误: Strings.resx 文件不存在！" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Strings.resx 存在" -ForegroundColor Green
Write-Host "  路径: $resxFile" -ForegroundColor Gray

if (Test-Path $designerFile) {
    $fileSize = (Get-Item $designerFile).Length
    Write-Host "✓ Strings.Designer.cs 存在 (大小: $fileSize bytes)" -ForegroundColor Green
} else {
    Write-Host "⚠ Strings.Designer.cs 不存在（将自动生成）" -ForegroundColor Yellow
}
Write-Host ""

# 步骤3：检查.NET版本
Write-Host "[步骤 3/5] 检查.NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
Write-Host "✓ .NET SDK 版本: $dotnetVersion`n" -ForegroundColor Green

# 步骤4：恢复NuGet包
Write-Host "[步骤 4/5] 恢复NuGet包..." -ForegroundColor Yellow
dotnet restore --verbosity minimal 2>&1 | Out-Null
Write-Host "✓ NuGet包恢复完成`n" -ForegroundColor Green

# 步骤5：重新构建项目
Write-Host "[步骤 5/5] 重新构建项目..." -ForegroundColor Yellow
$buildOutput = dotnet build "$projectPath\IndustrySystem.Presentation.Wpf.csproj" --no-incremental 2>&1

# 分析构建结果
$errors = $buildOutput | Select-String "error" | Measure-Object
$warnings = $buildOutput | Select-String "warning" | Measure-Object

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "构建结果" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if ($errors.Count -eq 0) {
    Write-Host "✓ 构建成功！没有错误。" -ForegroundColor Green
} else {
    Write-Host "✗ 构建失败：发现 $($errors.Count) 个错误" -ForegroundColor Red
    Write-Host "`n错误详情:" -ForegroundColor Red
    $buildOutput | Select-String "error" | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
}

if ($warnings.Count -gt 0) {
    Write-Host "⚠ 警告数量: $($warnings.Count)" -ForegroundColor Yellow
}

# 最终验证
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "验证 Strings.Designer.cs" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if (Test-Path $designerFile) {
    $fileInfo = Get-Item $designerFile
    Write-Host "✓ 文件存在" -ForegroundColor Green
    Write-Host "  大小: $($fileInfo.Length) bytes" -ForegroundColor Gray
    Write-Host "  最后修改: $($fileInfo.LastWriteTime)" -ForegroundColor Gray
    
    # 检查关键属性
    $content = Get-Content $designerFile -Raw
    $keyProperties = @(
        "Lbl_NotLoggedIn",
        "Log_AlarmViewModel_Initialized",
        "Nav_Users",
        "Nav_Roles",
        "Nav_Permissions"
    )
    
    Write-Host "`n关键属性检查:" -ForegroundColor Cyan
    foreach ($prop in $keyProperties) {
        if ($content -match $prop) {
            Write-Host "  ✓ $prop" -ForegroundColor Green
        } else {
            Write-Host "  ✗ $prop (缺失)" -ForegroundColor Red
        }
    }
} else {
    Write-Host "✗ Strings.Designer.cs 仍然不存在！" -ForegroundColor Red
    Write-Host "`n建议操作:" -ForegroundColor Yellow
    Write-Host "1. 在Visual Studio中打开项目" -ForegroundColor Gray
    Write-Host "2. 找到 Resources\Strings.resx" -ForegroundColor Gray
    Write-Host "3. 右键点击 → 'Run Custom Tool'" -ForegroundColor Gray
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "脚本执行完成" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

if ($errors.Count -eq 0) {
    Write-Host "✓ 修复成功！您现在可以运行应用程序了。" -ForegroundColor Green
    exit 0
} else {
    Write-Host "✗ 仍有错误需要手动修复。请参考 fix-resource-strings.md 获取详细说明。" -ForegroundColor Red
    exit 1
}
