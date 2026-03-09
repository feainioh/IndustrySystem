# ⚠️ 立即执行 - 资源生成器手动触发指南

## 🚨 问题

构建错误显示"命名空间中不存在'Strings'名称"。这是.NET 9的资源生成器问题，**必须在Visual Studio中手动触发**。

---

## ✅ 解决步骤（2分钟完成）

### 步骤1：关闭所有程序
```powershell
# 关闭Visual Studio（如果已打开）
# 在PowerShell中终止MSBuild进程
Get-Process -Name "MSBuild","VBCSCompiler" -ErrorAction SilentlyContinue | Stop-Process -Force
```

### 步骤2：清理构建输出
```powershell
cd D:\Code\IndustrySystem
dotnet clean
Remove-Item "src\Presentation\IndustrySystem.Presentation.Wpf\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "src\Presentation\IndustrySystem.Presentation.Wpf\obj" -Recurse -Force -ErrorAction SilentlyContinue
```

### 步骤3：在Visual Studio中操作（重要！）

1. **打开 Visual Studio 2022**

2. **打开解决方案**：`D:\Code\IndustrySystem\IndustrySystem.sln`

3. **在 Solution Explorer 中找到：**
   ```
   IndustrySystem.Presentation.Wpf
   └── Resources
       └── Strings.resx
   ```

4. **右键点击 `Strings.resx`**

5. **选择 "Run Custom Tool"（运行自定义工具）**

   ![image](https://user-images.githubusercontent.com/your-screenshot.png)

6. **等待3-5秒**，确认 `Strings.Designer.cs` 已更新

7. **Build → Rebuild Solution**

---

## 📋 验证成功

构建成功后，您应该看到：
- ✅ **0 errors**
- ⚠️ 可能有 10-15 个警告（这是正常的）

---

## 🔧 如果仍然失败

### 方案A：重启Visual Studio
1. 关闭Visual Studio
2. 删除 `.vs` 文件夹
3. 重新打开Visual Studio
4. 重复步骤3

### 方案B：修改 .resx 文件触发重新生成
1. 在Visual Studio中双击打开 `Strings.resx`
2. 按 `Ctrl+S` 保存（即使没有更改）
3. 关闭文件
4. 右键 `Strings.resx` → Run Custom Tool
5. Rebuild Solution

### 方案C：手动删除Designer文件
1. 关闭Visual Studio
2. 删除 `Strings.Designer.cs`
3. 打开Visual Studio
4. 右键 `Strings.resx` → Run Custom Tool
5. 确认 `Strings.Designer.cs` 重新生成
6. Rebuild Solution

---

## 🎯 关键点

1. **必须使用Visual Studio** - 命令行工具不够可靠
2. **必须点击 "Run Custom Tool"** - 这是触发资源生成器的唯一方法
3. **不要跳过清理步骤** - 缓存会导致问题

---

## 📞 还有2个缺失的字符串

构建成功后，我会帮您添加这2个缺失的字符串：
- `Alarm_Status_Unacknowledged` - 在 AlarmView.xaml 中
- `Tooltip_ExportLogs` - 在 OperationLogsView.xaml 中

**但首先必须解决 Strings 类无法识别的问题！**

---

## ⏱️ 预计时间：2-5分钟

现在就去Visual Studio执行步骤3吧！🚀
