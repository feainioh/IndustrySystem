# 🔧 修复乱码问题 - 使用指南

## 问题原因
PowerShell 脚本中的中文字符在某些系统配置下会显示为乱码。

---

## ✅ 解决方案（3种方法）

### **方法1：使用批处理文件（最简单，推荐）**

直接双击运行：
```
fix-resources.bat
```

**优点：**
- ✅ 无需任何配置
- ✅ 自动处理编码
- ✅ 双击即可运行
- ✅ 显示构建输出

**步骤：**
1. 在文件资源管理器中找到 `fix-resources.bat`
2. 双击运行
3. 等待脚本完成
4. 按任意键关闭窗口

---

### **方法2：使用英文版 PowerShell 脚本**

在 PowerShell 中运行：
```powershell
cd D:\Code\IndustrySystem
.\fix-resources-en.ps1
```

**优点：**
- ✅ 完全英文界面
- ✅ 无乱码问题
- ✅ 详细的输出信息

**如果遇到执行策略错误：**
```powershell
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
.\fix-resources-en.ps1
```

---

### **方法3：使用修复后的中文版 PowerShell 脚本**

在 PowerShell 中运行：
```powershell
cd D:\Code\IndustrySystem
.\fix-resources.ps1
```

这个版本已经添加了编码修复：
```powershell
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8
chcp 65001 | Out-Null
```

**如果仍然乱码，请使用方法1或方法2。**

---

## 📊 预期输出

### 成功的输出示例：

```
========================================
Resource File Auto-Fix Script
========================================

[Step 1/5] Cleaning build output...
OK - Cleaned

[Step 2/5] Checking files...
OK - Strings.resx exists

[Step 3/5] Checking .NET SDK...
OK - .NET SDK Version: 9.0.xxx

[Step 4/5] Restoring NuGet packages...
OK - NuGet packages restored

[Step 5/5] Rebuilding project...

========================================
Build Results
========================================
OK - Build succeeded! No errors.
WARNING - Warning count: 10

========================================
Verify Strings.Designer.cs
========================================
OK - File exists
  Size: 52341 bytes
  Last modified: 2024-xx-xx xx:xx:xx

Key property check:
  OK - Lbl_NotLoggedIn
  OK - Log_AlarmViewModel_Initialized
  OK - Nav_Users
  OK - Nav_Roles
  OK - Nav_Permissions

========================================
Script Completed
========================================

SUCCESS - Fix completed! You can now run the application.
```

---

## 🚀 快速开始（一键复制）

### Windows 命令提示符（CMD）：
```cmd
cd /d D:\Code\IndustrySystem && fix-resources.bat
```

### PowerShell（英文版）：
```powershell
cd D:\Code\IndustrySystem; Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process; .\fix-resources-en.ps1
```

### PowerShell（中文版）：
```powershell
cd D:\Code\IndustrySystem; [Console]::OutputEncoding = [System.Text.Encoding]::UTF8; .\fix-resources.ps1
```

---

## ⚠️ 如果所有自动化脚本都失败

请使用**手动修复方法**：

### 步骤1：清理
```powershell
cd D:\Code\IndustrySystem
Remove-Item "src\Presentation\IndustrySystem.Presentation.Wpf\bin" -Recurse -Force
Remove-Item "src\Presentation\IndustrySystem.Presentation.Wpf\obj" -Recurse -Force
```

### 步骤2：在 Visual Studio 中
1. **打开项目**
2. **Solution Explorer** → **IndustrySystem.Presentation.Wpf** → **Resources** → **Strings.resx**
3. **右键点击 Strings.resx**
4. **选择 "Run Custom Tool"**
5. **等待几秒钟**
6. **Build → Rebuild Solution**

---

## 📁 文件对比

| 文件 | 语言 | 编码处理 | 推荐度 |
|------|------|----------|--------|
| `fix-resources.bat` | 英文 | 自动 | ⭐⭐⭐⭐⭐ |
| `fix-resources-en.ps1` | 英文 | 自动 | ⭐⭐⭐⭐ |
| `fix-resources.ps1` | 中文 | 已修复 | ⭐⭐⭐ |

---

## 💡 建议

1. **首先尝试 `fix-resources.bat`** - 最简单，最可靠
2. 如果需要详细输出，使用 `fix-resources-en.ps1`
3. 如果想要中文界面，使用修复后的 `fix-resources.ps1`

---

## 🔍 验证修复成功

运行任意一个脚本后，检查：

```powershell
# 检查文件是否存在
Test-Path "src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.Designer.cs"

# 检查文件大小（应该大于 50KB）
(Get-Item "src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.Designer.cs").Length

# 搜索关键属性
Select-String -Path "src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.Designer.cs" -Pattern "Lbl_NotLoggedIn"
```

---

## 🎯 现在就试试！

**推荐：直接双击运行**
```
fix-resources.bat
```

或者在命令行中：
```cmd
cd /d D:\Code\IndustrySystem
fix-resources.bat
```

就这么简单！🚀
