# 添加最后2个缺失字符串的步骤

## ⚠️ 先决条件
确保已在Visual Studio中完成 "Run Custom Tool" 操作，并且构建成功。

---

## 📝 需要添加的字符串

### 1. Alarm_Status_Unacknowledged
- **用途**: AlarmView.xaml 中的告警状态
- **中文**: 未确认

### 2. Tooltip_ExportLogs  
- **用途**: OperationLogsView.xaml 中的导出按钮提示
- **中文**: 导出日志

---

## 🚀 添加步骤

### 方法1：在Visual Studio中添加（推荐）

1. **双击打开** `Strings.resx` 文件

2. **在最后一行添加以下两个条目：**

   | Name | Value |
   |------|-------|
   | `Alarm_Status_Unacknowledged` | `未确认` |
   | `Tooltip_ExportLogs` | `导出日志` |

3. **按 Ctrl+S 保存**

4. **右键 Strings.resx → "Run Custom Tool"**

5. **Rebuild Solution**

---

### 方法2：使用PowerShell（如果方法1不可行）

在PowerShell中运行以下命令：

```powershell
cd D:\Code\IndustrySystem

# 备份当前文件
Copy-Item "src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.resx" "src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.resx.backup"

# 添加字符串到文件末尾（在 </root> 之前）
$content = Get-Content "src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.resx" -Raw
$newContent = $content -replace '</root>', @'
 <data name="Alarm_Status_Unacknowledged" xml:space="preserve"><value>未确认</value></data>
 <data name="Tooltip_ExportLogs" xml:space="preserve"><value>导出日志</value></data>
</root>
'@
$newContent | Set-Content "src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.resx" -Encoding UTF8
```

然后在Visual Studio中：
1. 右键 Strings.resx → Run Custom Tool
2. Rebuild Solution

---

## ✅ 验证

构建成功后应该：
- ✅ **0 errors**
- ✅ 所有 XAML 文件正常编译
- ⚠️ 可能有警告（正常）

---

## 📊 完整的缺失字符串列表（已添加）

| 字符串 | 状态 | 用途 |
|--------|------|------|
| `Nav_Alarm` | ✅ 已添加 | AlarmView 标题 |
| `Desc_OperationLogs` | ✅ 已添加 | OperationLogsView 描述 |
| `Hint_Template_Name` | ✅ 已添加 | 实验模板名称提示 |
| `Hint_Template_Description` | ✅ 已添加 | 实验模板描述提示 |
| `Alarm_Status_Unacknowledged` | ⏳ 待添加 | 告警状态 |
| `Tooltip_ExportLogs` | ⏳ 待添加 | 导出日志提示 |

---

## 🎯 执行顺序

1. **首先**: 按照 `URGENT-FIX-STRINGS-NOW.md` 在Visual Studio中触发资源生成器
2. **然后**: 按照本文档添加最后2个字符串
3. **最后**: Rebuild Solution

---

## 💡 提示

完成后运行：
```powershell
cd D:\Code\IndustrySystem
.\fix-resources-en.ps1
```

这将验证所有修复是否成功。

---

⏱️ **总时间**: 3-5分钟
