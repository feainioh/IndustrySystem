# 流程设计程序重构 - 完成报告

## 概述

本次重构完成了以下主要任务：
1. **项目结构重构** - 建立项目-子项目-子程序的层级结构
2. **界面更新** - 创建项目资源管理器、子程序标签页
3. **功能完善** - 实现子程序调用、变量管理

---

## 1. 项目结构设计

### 1.1 层级结构

```
MotionProject (项目)
├── DeviceConfig (设备配置)
├── GlobalVariables (全局变量)
└── SubProjects[] (子项目集合)
    ├── Variables (子项目变量)
    └── SubPrograms[] (子程序集合)
        ├── Nodes[] (节点)
        ├── Connections[] (连接)
        ├── LocalVariables (局部变量)
        ├── InputParameters[] (输入参数)
        └── OutputParameters[] (输出参数)
```

### 1.2 新增文件清单

| 文件路径 | 说明 |
|---------|------|
| `Models/ProjectStructure.cs` | 项目结构数据模型 |
| `Services/ProjectService.cs` | 项目服务实现 |
| `ViewModels/ProjectExplorerViewModel.cs` | 项目资源管理器 ViewModel |
| `ViewModels/VariableManagerViewModel.cs` | 变量管理器 ViewModel |
| `ViewModels/CallSubProgramParameterViewModel.cs` | 子程序调用参数编辑器 |
| `Views/ProjectExplorerView.xaml` | 项目资源管理器视图 |
| `Views/ProjectExplorerView.xaml.cs` | 项目资源管理器代码 |
| `Controls/SubProgramTabControl.xaml` | 子程序标签页控件 |
| `Controls/SubProgramTabControl.xaml.cs` | 子程序标签页代码 |

---

## 2. 界面更新

### 2.1 项目资源管理器 (ProjectExplorerView)

功能特性：
- 树形结构显示项目-子项目-子程序
- 支持右键上下文菜单操作
- 支持双击打开子程序
- 显示程序类型图标（Main/Initialize/ErrorHandler/Cleanup）
- 显示修改状态标记（*）

### 2.2 MainWindow 更新

新增功能：
- 左侧项目资源管理器面板
- Project 切换按钮控制面板显示/隐藏
- 响应式布局

### 2.3 子程序标签页 (SubProgramTabControl)

功能特性：
- 多标签页切换
- 显示程序类型图标
- 显示修改状态
- 支持关闭标签页

---

## 3. 功能完善

### 3.1 变量管理器 (VariableManagerViewModel)

支持的变量类型：
- **全局变量** - 整个项目可用
- **子项目变量** - 子项目内可用
- **局部变量** - 子程序内可用
- **系统变量** - 只读系统变量

系统变量列表：
| 变量名 | 类型 | 说明 |
|--------|------|------|
| `$CurrentTime` | DateTime | 当前系统时间 |
| `$ExecutionState` | String | 当前执行状态 |
| `$LoopIndex` | Int32 | 当前循环索引 |
| `$LastError` | String | 最后一个错误信息 |

### 3.2 子程序调用 (CallSubProgramParameterViewModel)

功能特性：
- 选择目标子程序
- 输入参数映射（变量或常量）
- 输出参数映射
- 等待完成选项
- 超时设置

---

## 4. 事件系统

### 4.1 项目事件

```csharp
// 子程序选中事件
public class SubProgramSelectedEvent : PubSubEvent<SubProgram> { }

// 子程序激活事件（在设计器中打开）
public class SubProgramActivatedEvent : PubSubEvent<SubProgram> { }

// 子程序修改事件
public class SubProgramModifiedEvent : PubSubEvent<SubProgram> { }
```

---

## 5. 使用指南

### 5.1 创建新项目

```csharp
// 通过 ProjectService 创建
var project = _projectService.CreateProject("我的项目");

// 自动创建默认子项目和主程序
// project.SubProjects[0].SubPrograms[0] 是 Main 程序
```

### 5.2 添加子程序

1. 在项目资源管理器中选择子项目
2. 右键选择"添加子程序"
3. 双击新建的子程序在设计器中打开

### 5.3 调用子程序

1. 在工具箱中拖拽"Call SubProgram"节点到画布
2. 在属性面板中选择目标子程序
3. 配置输入/输出参数映射
4. 设置超时和等待选项

### 5.4 使用变量

```csharp
// 获取变量值
var value = _variableManager.GetVariable("MyVariable");

// 设置变量值
_variableManager.SetVariable("MyVariable", 123);

// 使用系统变量
var loopIndex = _variableManager.GetVariable("$LoopIndex");
```

---

## 6. 项目文件格式

项目文件 (*.mproj) 使用 JSON 格式：

```json
{
  "id": "guid",
  "name": "项目名称",
  "description": "项目描述",
  "version": "1.0.0",
  "author": "作者",
  "createdTime": "2025-01-01T00:00:00",
  "modifiedTime": "2025-01-01T00:00:00",
  "deviceConfigPath": "deviceconfig.json",
  "globalVariables": {
    "MyGlobalVar": "value"
  },
  "subProjects": [
    {
      "id": "guid",
      "name": "主流程",
      "subPrograms": [
        {
          "id": "guid",
          "name": "Main",
          "programType": "Main",
          "nodes": [...],
          "connections": [...],
          "inputParameters": [...],
          "outputParameters": [...]
        }
      ]
    }
  ]
}
```

---

## 7. DI 注册

已在 `App.xaml.cs` 中注册的服务：

```csharp
// 项目服务
containerRegistry.RegisterSingleton<Services.IProjectService, Services.ProjectService>();

// ViewModels
containerRegistry.Register<ViewModels.ProjectExplorerViewModel>();
containerRegistry.Register<ViewModels.VariableManagerViewModel>();
```

---

## 8. 编译状态

✅ **编译成功** - 所有代码已通过编译测试

---

## 9. 后续工作建议

### 9.1 待完成功能
- [ ] 变量管理器 UI 视图
- [ ] 子程序参数编辑器 UI
- [ ] 子程序执行引擎集成
- [ ] 项目导入/导出向导

### 9.2 优化建议
- [ ] 添加撤销/重做功能
- [ ] 实现子程序搜索
- [ ] 添加项目模板功能

---

**完成时间**: 2025-01-XX  
**状态**: ✅ 完成  
**编译**: ✅ 成功
