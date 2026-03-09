# 修复 Strings.resx 资源生成问题

## 问题描述

编译错误显示"命名空间中不存在'Strings'名称"，这是因为.NET 9的资源生成器没有正确生成`Strings.Designer.cs`文件或Visual Studio缓存问题。

## ✅ 完整修复步骤

### 方案1：使用Visual Studio（推荐）

#### 步骤1：清理项目

1. 在Visual Studio中，选择菜单：**Build → Clean Solution**
2. 关闭Visual Studio

#### 步骤2：删除生成缓存

在PowerShell中运行以下命令：

```powershell
# 删除所有bin和obj文件夹
Remove-Item ".\src\Presentation\IndustrySystem.Presentation.Wpf\bin\" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item ".\src\Presentation\IndustrySystem.Presentation.Wpf\obj\" -Recurse -Force -ErrorAction SilentlyContinue
```

#### 步骤3：修改项目文件

打开 `src\Presentation\IndustrySystem.Presentation.Wpf\IndustrySystem.Presentation.Wpf.csproj`

确保包含以下配置：

```xml
<PropertyGroup>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>enable</Nullable>
    <GenerateResourceUsePreserializedResources>true</GenerateResourceUsePreserializedResources>
</PropertyGroup>

<ItemGroup>
    <PackageReference Include="System.Resources.Extensions" Version="9.0.0" />
</ItemGroup>

<ItemGroup>
    <EmbeddedResource Update="Resources\Strings.resx">
        <Generator>PublicResXFileCodeGenerator</Generator>
        <LastGenOutput>Strings.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <Compile Update="Resources\Strings.Designer.cs">
        <DesignTime>True</DesignTime>
        <AutoGen>True</AutoGen>
        <DependentUpon>Strings.resx</DependentUpon>
    </Compile>
</ItemGroup>
```

#### 步骤4：重新打开Visual Studio

1. 打开 Visual Studio
2. 打开解决方案
3. 在Solution Explorer中找到：`Resources → Strings.resx`
4. **右键点击 `Strings.resx`**
5. **选择 "Run Custom Tool"（运行自定义工具）**
6. 等待几秒钟

#### 步骤5：验证生成

检查 `Strings.Designer.cs` 文件：

```powershell
code ".\src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.Designer.cs"
```

确认文件中包含以下属性：
- `Lbl_NotLoggedIn`
- `Log_AlarmViewModel_Initialized`
- `Log_HardwareDebug_Connected`
- 等等...

#### 步骤6：重新构建

在Visual Studio中：
1. **Build → Rebuild Solution**
2. 等待构建完成

---

### 方案2：使用命令行

如果Visual Studio方案不起作用，使用此命令行方法：

#### 步骤1：清理

```powershell
cd D:\Code\IndustrySystem

# 清理所有构建输出
dotnet clean

# 删除bin和obj
Get-ChildItem -Path . -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force
```

#### 步骤2：恢复NuGet包

```powershell
dotnet restore
```

#### 步骤3：构建项目

```powershell
# 构建WPF项目
dotnet build ".\src\Presentation\IndustrySystem.Presentation.Wpf\IndustrySystem.Presentation.Wpf.csproj" --no-incremental

# 如果上面失败，尝试添加详细输出
dotnet build ".\src\Presentation\IndustrySystem.Presentation.Wpf\IndustrySystem.Presentation.Wpf.csproj" --no-incremental -v detailed
```

---

### 方案3：手动重新生成 Strings.Designer.cs

如果上述方法都失败，手动触发资源生成：

#### 步骤1：安装ResGen工具

```powershell
dotnet tool install -g dotnet-t4
```

#### 步骤2：生成Designer文件

```powershell
cd ".\src\Presentation\IndustrySystem.Presentation.Wpf\Resources\"

# 使用ResGen生成资源
resgen Strings.resx Strings.resources
```

#### 步骤3：在Visual Studio中

1. 删除当前的 `Strings.Designer.cs`
2. 右键点击 `Strings.resx`
3. 选择 "Properties"
4. 设置：
   - **Custom Tool**: `PublicResXFileCodeGenerator`
   - **Custom Tool Namespace**: `IndustrySystem.Presentation.Wpf.Resources`
5. 右键点击 `Strings.resx` → "Run Custom Tool"

---

## 🔍 检查清单

完成修复后，验证以下内容：

### ✅ 文件存在性检查

```powershell
# 检查Designer文件是否存在
Test-Path ".\src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.Designer.cs"

# 检查Designer文件大小（应该 > 50KB）
(Get-Item ".\src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.Designer.cs").Length
```

### ✅ 内容验证

```powershell
# 搜索关键属性
Select-String -Path ".\src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.Designer.cs" -Pattern "Lbl_NotLoggedIn"
Select-String -Path ".\src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.Designer.cs" -Pattern "Log_AlarmViewModel_Initialized"
```

### ✅ 编译验证

```powershell
dotnet build ".\src\Presentation\IndustrySystem.Presentation.Wpf\IndustrySystem.Presentation.Wpf.csproj" 2>&1 | Select-String -Pattern "Strings"
```

---

## 🚨 常见问题

### Q1: "Strings"仍然无法识别

**原因**: Visual Studio缓存问题

**解决**:
1. 关闭Visual Studio
2. 删除 `.vs` 文件夹
3. 删除所有 `bin` 和 `obj` 文件夹
4. 重新打开Visual Studio

### Q2: 编译时出现"资源文件锁定"错误

**原因**: MSBuild进程占用

**解决**:
```powershell
# 终止所有MSBuild进程
Get-Process -Name "MSBuild" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "VBCSCompiler" -ErrorAction SilentlyContinue | Stop-Process -Force
```

### Q3: Designer.cs文件生成但属性缺失

**原因**: .resx文件格式错误

**解决**:
1. 在Visual Studio中打开 `Strings.resx`
2. 手动检查所有条目
3. 确保没有XML格式错误
4. 保存并重新运行自定义工具

---

## 📋 最终验证步骤

完成所有修复后：

```powershell
# 1. 完全清理
dotnet clean
Remove-Item -Path ".\src\Presentation\IndustrySystem.Presentation.Wpf\bin",".\src\Presentation\IndustrySystem.Presentation.Wpf\obj" -Recurse -Force -ErrorAction SilentlyContinue

# 2. 恢复依赖
dotnet restore

# 3. 构建项目
dotnet build ".\src\Presentation\IndustrySystem.Presentation.Wpf\IndustrySystem.Presentation.Wpf.csproj"

# 4. 检查错误数量
$errors = dotnet build ".\src\Presentation\IndustrySystem.Presentation.Wpf\IndustrySystem.Presentation.Wpf.csproj" 2>&1 | Select-String "error"
Write-Host "错误数量: $($errors.Count)"
```

---

## ⏱️ 预计时间

- **方案1（Visual Studio）**: 5-10分钟
- **方案2（命令行）**: 3-5分钟
- **方案3（手动）**: 10-15分钟

---

## 💡 提示

1. **优先使用方案1**，它最可靠
2. 如果方案1失败，尝试**重启计算机**后再试
3. 确保.NET 9 SDK已正确安装：`dotnet --version`
4. 确保Visual Studio已更新到最新版本

---

## 📞 如果仍然失败

如果所有方案都失败，请提供以下信息：

```powershell
# 收集诊断信息
@"
.NET版本: $(dotnet --version)
Visual Studio版本: $(Get-Item "C:\Program Files\Microsoft Visual Studio\2022\*\Common7\IDE\devenv.exe" | ForEach-Object { $_.VersionInfo.FileVersion })
Strings.resx大小: $((Get-Item ".\src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.resx").Length)
Strings.Designer.cs存在: $(Test-Path ".\src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.Designer.cs")
"@ | Out-File -FilePath "diagnostic-info.txt"

Write-Host "诊断信息已保存到 diagnostic-info.txt"
```

将此文件内容提供给技术支持。
