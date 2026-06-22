# IndustrySystem 代码整改计划

> 基于 `docs/code-analysis-report.md` 的分析结果  
> 原则：按优先级分阶段执行，每阶段完成后编译验证，不修改功能行为

---

## 第一阶段：P0 严重问题（2项）

### 任务 1.1：重命名基类拼写错误

**范围**：2个基类文件 + 31个引用文件，纯机械重命名

#### 步骤

**A. 重命名基类文件及类名**

| 文件 | 旧名 | 新名 |
|------|------|------|
| `ViewModels/NagetiveViewModel.cs` | `NagetiveViewModel` | `NavigationViewModel` |
| `ViewModels/NagetiveCurdVeiwModel.cs` | `NagetiveCurdVeiwModel<TItem>` | `CrudViewModel<TItem>` |

同时修正文件名：
- `NagetiveViewModel.cs` → `NavigationViewModel.cs`
- `NagetiveCurdVeiwModel.cs` → `CrudViewModel.cs`

**B. 更新所有派生类引用（31个文件）**

继承 `NagetiveViewModel`（需改 13个文件）：
```
DeviceParamsViewModel.cs, ExperimentsViewModel.cs, ExperimentConfigViewModel.cs,
HardwareDebugViewModel.cs, MotionProgramRunViewModel.cs, OperationLogsViewModel.cs,
PeripheralDebugViewModel.cs, RealtimeDataViewModel.cs, RolesViewModel.cs,
RunExperimentViewModel.cs, ShelfInfoViewModel.cs,
ExperimentParameterEditorViewModel.cs (基类),
NagetiveCurdVeiwModel.cs (基类自身)
```

继承 `NagetiveCurdVeiwModel<T>`（需改 9个文件）：
```
AlarmViewModel.cs, ExperimentGroupsViewModel.cs, ExperimentHistoryViewModel.cs,
ExperimentTemplateViewModel.cs, InventoryViewModel.cs, MaterialInfoViewModel.cs,
PermissionsViewModel.cs, RoleManageViewModel.cs, UsersViewModel.cs
```

**C. 编译验证**
```bash
dotnet build IndustrySystem.sln
```

**风险评估**：✅ 低风险。纯重命名，编译器会捕获遗漏的引用。

---

### 任务 1.2：重构 ShelfInfoViewModel 弹窗机制

**问题**：`ShelfInfoViewModel` 通过 `DialogHost.Show()` 直接显示 `ContainerListDialog` 和 `ShelfListDialog`，同时这两个 Dialog 在 DI 中也注册了 Prism Dialog。关闭按钮用 code-behind 的 `DialogHost.Close()`。

**目标**：全部走 `IDialogService`，移除 `DialogHost` 直接调用。

#### 1.2.1 分析当前架构

当前 `ContainerListDialog` 和 `ShelfListDialog` 的 DataContext **被直接设为 `ShelfInfoViewModel`**（`new ContainerListDialog { DataContext = this }`），所有绑定（`PagedContainers`、`AddContainerCommand` 等）都指向 ShelfInfoViewModel。对应的 `ContainerListDialogViewModel` 和 `ShelfListDialogViewModel` **是空壳类**。

ShelfInfoViewModel 中与列表弹窗相关的代码：

| 成员 | 用途 |
|------|------|
| `PagedContainers` / `ContainerTotalCount` / `ContainerPageSize` 等 | 容器列表分页数据 |
| `ContainerFirstPageCommand` / `ContainerNextPageCommand` 等 | 容器分页命令 |
| `ContainerPageSizes` | 每页条数选项 |
| `AddContainerCommand` / `EditContainerCommand` / `DeleteContainerCommand` | 容器 CRUD 命令 |
| `LoadContainersAsync()` | 加载容器数据 |
| `OpenContainerListDialogAsync()` | 通过 DialogHost.Show 打开 |
| `OpenContainerDialogAsync(Guid?)` | 通过 IDialogService 打开编辑弹窗 |
| `DeleteContainerAsync(Guid)` | 删除确认 |
| `_containerListOpen` | 列表弹窗状态标志 |
| `CloseListDialogIfOpen()` | 通过 DialogHost.Close 关闭 |
| `PagedShelves` / `ShelfTotalCount` / `ShelfPageSize` 等 | 货架列表分页数据 |
| `ShelfFirstPageCommand` / `ShelfNextPageCommand` 等 | 货架分页命令 |
| `ShelfPageSizes` | 每页条数选项 |
| `AddShelfCommand` / `EditShelfCommand` / `DeleteShelfCommand` | 货架 CRUD 命令 |
| `LoadShelvesAsync()` | 加载货架数据 |
| `OpenShelfListDialogAsync()` | 通过 DialogHost.Show 打开 |
| `OpenShelfDialogAsync(Guid?)` | 通过 IDialogService 打开编辑弹窗 |
| `DeleteShelfAsync(Guid)` | 删除确认 |
| `_shelfListOpen` | 列表弹窗状态标志 |

**嵌套弹窗逻辑**：从列表弹窗可以打开编辑弹窗（ContainerEditDialog / ShelfEditDialog）：
1. 关闭列表弹窗 → 等待50ms → 打开编辑弹窗
2. 编辑弹窗关闭后 → 重新打开列表弹窗

#### 1.2.2 重构方案

**A. 实现 ContainerListDialogViewModel**

将容器列表相关属性/命令从 `ShelfInfoViewModel` 移动到 `ContainerListDialogViewModel`：

```csharp
public class ContainerListDialogViewModel : DialogViewModel
{
    private readonly IShelfAppService _svc;
    private readonly IDialogService _dialogService;
    
    // 分页数据
    public ObservableCollection<ContainerDto> PagedContainers { get; }
    public int ContainerTotalCount { get; }
    public int ContainerCurrentPage { get; set; }
    public int ContainerTotalPages { get; }
    public int ContainerPageSize { get; set; }
    public ObservableCollection<int> ContainerPageSizes { get; }
    
    // 分页命令
    public ICommand ContainerFirstPageCommand { get; }
    public ICommand ContainerPreviousPageCommand { get; }
    public ICommand ContainerNextPageCommand { get; }
    public ICommand ContainerLastPageCommand { get; }
    
    // CRUD 命令
    public ICommand AddContainerCommand { get; }     // 打开 ContainerEditDialog
    public ICommand EditContainerCommand { get; }    // 打开 ContainerEditDialog
    public ICommand DeleteContainerCommand { get; }  // 删除确认 + 刷新
    
    // 私有：容器数据列表
    private List<ContainerDto> _allContainers;
    
    public override void OnDialogOpened(IDialogParameters parameters)
    {
        _ = LoadContainersAsync();
    }
    
    // 编辑弹窗打开 → 关闭自身 → 等编辑弹窗结果 → 重新打开自身
    private async Task OpenEditDialogAsync(Guid? id) { ... }
}
```

**B. 实现 ShelfListDialogViewModel**

同理，将货架列表相关属性/命令移到 `ShelfListDialogViewModel`。

**C. 修改 XAML 关闭按钮**

两个 Dialog 的关闭按钮从 code-behind `Click` 事件改为命令绑定：

```xml
<!-- 旧 -->
<Button Click="CloseButton_Click" ... />

<!-- 新 -->
<Button Command="{Binding CancelCommand}" ... />
```

删除 `ContainerListDialog.xaml.cs` 和 `ShelfListDialog.xaml.cs` 中的 `CloseButton_Click` 方法。

**D. 清理 ShelfInfoViewModel**

- 移除所有容器列表分页属性/命令（约15个成员）
- 移除所有货架列表分页属性/命令（约15个成员）
- 移除 `_containerListOpen`、`_shelfListOpen` 标志
- 移除 `CloseListDialogIfOpen()` 方法
- 移除 `OpenContainerListDialogAsync()`、`OpenShelfListDialogAsync()` 方法
- 移除 `LoadContainersAsync()`、`LoadShelvesAsync()` 方法（移到对应 DialogVM）
- 保留 `OpenSlotConfigAsync()`（它已经正确使用 IDialogService）
- 保留 `RefreshSlotsQuietAsync()`（槽位相关，属于 ShelfInfoViewModel 自己的职责）

**E. 处理嵌套弹窗场景**

编辑弹窗从列表弹窗中打开的逻辑：
1. 列表 VM 调用 `_dialogService.ShowDialog("ContainerEditDialog", ...)`
2. 在回调中，如果 result.OK → 刷新自身列表 → 列表 VM 继续存在
3. 用户关闭编辑弹窗后自动回到列表弹窗

关键：需要使用 Prism dialog 的嵌套支持。Prism 的 IDialogService 默认支持嵌套——在 dialog 回调中再 ShowDialog 即可。

实际上查看 Prism 源码，`IDialogService.ShowDialog` 创建的是模态窗口，不能在同一线程中嵌套。方案是：
- 在列表弹窗中点击"编辑" → 先让列表弹窗返回一个特定的 DialogResult
- 父 VM (ShelfInfoViewModel) 收到结果后打开编辑弹窗
- 编辑完成后再重新打开列表弹窗

这需要用 DialogParameters 传递状态。简化方案：列表弹窗关闭时传回"用户想编辑哪个ID"，父 VM 打开编辑弹窗后再打开列表弹窗。

**F. 移除 DialogHost 引用**

- `ShelfInfoViewModel.cs`: 删除所有 `DialogHost.Show()` / `DialogHost.Close()` 调用
- `ContainerListDialog.xaml.cs`: 删除 `using MaterialDesignThemes.Wpf` 和 `CloseButton_Click`
- `ShelfListDialog.xaml.cs`: 同上

**风险评估**：⚠️ 中风险。涉及 ShelfInfoViewModel 的重构，需要仔细测试弹窗的打开/关闭/嵌套场景。建议先在一个 Dialog 上验证模式，再复制到另一个。

---

## 第二阶段：P1 重要问题（5项）

### 任务 2.1：MotionProgramViewerViewModel 继承 BaseViewModel

**文件**：`ViewModels/MotionProgramViewerViewModel.cs`

**改动**：
```csharp
// 旧
public class MotionProgramViewerViewModel : BindableBase

// 新
public class MotionProgramViewerViewModel : BaseViewModel
```

利用继承的 `IsBusy` 属性包裹文件加载逻辑：
```csharp
protected override async Task OnRefreshAsync()
{
    IsBusy = true;
    try
    {
        // 现有的加载和解析 JSON 逻辑
        await LoadProgramAsync();
    }
    finally
    {
        IsBusy = false;
    }
}
```

**风险评估**：✅ 低风险。仅改变基类，利用了现有的模板方法模式。

---

### 任务 2.2：实验参数编辑器移出 Dialogs 命名空间

**范围**：11个文件

| 操作 | 文件 |
|------|------|
| 移动 | `ViewModels/Dialogs/ExperimentParameters/ExperimentParameterEditorViewModel.cs` → `ViewModels/ExperimentParameters/ExperimentParameterEditorViewModel.cs` |
| 移动 | `ViewModels/Dialogs/ExperimentParameters/ExperimentParameterDialogViewModels.cs` → `ViewModels/ExperimentParameters/ExperimentParameterDialogViewModels.cs` |
| 更新命名空间 | 2个文件 `namespace ...ViewModel.Dialogs` → `namespace ...ViewModel.ExperimentParameters` |
| 更新 using | `ExperimentConfigViewModel.cs`、`ExperimentTemplateViewModel.cs`（引用该基类的文件） |

对应的 View 文件（`Views/Dialogs/ExperimentParameters/*.xaml`）保持在 `Views/Dialogs/ExperimentParameters/` 不变——它们在 XAML 中是内容控件，folder 路径不影响功能。但如果要完全一致，也可以移到 `Views/ExperimentParameters/`。

**注意**：View 和 ViewModel 的文件夹不需要完全一致，但命名空间一致更便于维护。本次仅移动 ViewModel 文件。

**风险评估**：✅ 低风险。纯移动 + 命名空间更新。

---

### 任务 2.3：统一按钮样式别名

**目标**：废弃冗余别名，ViewStyles.xaml 中保留主样式名，将别名标记为 `[Obsolete]` 或直接替换。

**ViewStyles.xaml 中的别名关系**：

| 别名（废弃） | 实际指向 | 使用位置 |
|-------------|---------|---------|
| `PrimaryCommandButton` | `AppInteractiveButtonPrimary` | HardwareDebugView, OperationLogsView |
| `SecondaryCommandButton` | `AppInteractiveButtonNeutral` | HardwareDebugView |
| `HeaderActionOutlinedButton` | `AppInteractiveButtonNeutral` | ExperimentConfigView, OperationLogsView |
| `HeaderActionAccentButton` | `AppInteractiveButtonPrimary` | ExperimentConfigView, OperationLogsView |
| `HeaderActionRaisedButton` | `AppInteractiveButtonPrimary` | UsersView |

**方案**：保留所有别名（不删除，避免 breaking change），但在 ViewStyles.xaml 中加注释标记推荐名称：

```xml
<!-- Recommended: use AppInteractiveButtonPrimary directly -->
<Style x:Key="PrimaryCommandButton" TargetType="Button" BasedOn="{StaticResource AppInteractiveButtonPrimary}"/>
```

后续新代码统一使用 `AppInteractiveButton*` 系列。

**风险评估**：✅ 零风险。仅添加注释，不修改任何 XAML 引用。

---

### 任务 2.4：统一图标库

**目标**：统一使用 `ui:FontIcon`（Segoe MDL2 Assets），逐步替换 `materialDesign:PackIcon`。

**当前混用情况**：

| 页面 | 当前图标 | 替换方案 |
|------|---------|---------|
| ExperimentConfigView | `materialDesign:PackIcon Kind="TuneVariant"` | 改为 `ui:FontIcon Glyph="&#xE90F;"` |
| ExperimentConfigView | 其他 PackIcon（Refresh, Plus, ContentSaveOutline） | 对应 MDL2 glyph |
| ExperimentTemplateView | PackIcon | 对应 MDL2 glyph |
| ExperimentGroupsView | PackIcon | 对应 MDL2 glyph |
| MotionProgramRunView | PackIcon Kind="RobotIndustrial" | 无直接对应 → 保留或找替代 |
| LoginView | PackIcon Kind="Account" | 改为 `ui:FontIcon Glyph="&#xE77B;"` |
| InventoryView | PackIcon | 对应 MDL2 glyph |
| MaterialInfoView | PackIcon | 对应 MDL2 glyph |
| PermissionsView | PackIcon | 对应 MDL2 glyph |
| RoleManageView | PackIcon | 对应 MDL2 glyph |
| UsersView | PackIcon | 对应 MDL2 glyph |
| ShelfInfoView | PackIcon (ViewGrid, Cancel, TrayRemove) | 对应 MDL2 glyph |

**PackIcon → MDL2 对照表**（常用）：

| PackIcon Kind | MDL2 Glyph | 含义 |
|---------------|-----------|------|
| Account | `` | 用户 |
| Plus | `` | 添加 |
| Pencil | `` | 编辑 |
| Delete | `` | 删除 |
| Refresh | `` | 刷新 |
| ContentSaveOutline | `` | 保存 |
| TuneVariant | `` | 配置 |
| RobotIndustrial | N/A | 保留 PackIcon |
| PackageVariant | `` | 容器/货架 |
| ViewGrid | `` | 网格视图 |

**注意**：部分 MaterialDesign PackIcon 没有完全对应的 MDL2 字符（如 RobotIndustrial），这类可以保留 PackIcon，但应加注释说明原因。

**方案**：分两批
- 第一批：所有管理页面（Users, Roles, Permissions, Material, Inventory, Shelf, Experiment*）——替换量最大但图标都有对应
- 第二批：特殊页面（LoginView 的 Account、MotionProgramRunView 的 RobotIndustrial）——确认替代或保留

**风险评估**：⚠️ 中低风险。图标替换可能导致视觉上的细微差异（字形风格不同），需要逐个确认。

---

### 任务 2.5：为 MotionDesigner 的页面 VM 引入基类

**范围**：MotionDesigner 项目中约 4 个页面级 VM + 11 个设备控件 VM

**方案**：MotionDesigner 引用 Wpf 项目的 ViewModel 基类（通过项目引用已存在），或创建自己的基类

| VM | 建议基类 | 理由 |
|----|---------|------|
| `DesignerViewModel` | `NavigationViewModel` | 设计器主页面，Prism Region 导航目标 |
| `DeviceDebugViewModel` | `NavigationViewModel` | 设备调试页面 |
| `PositionSettingsViewModel` | `NavigationViewModel` | 位置设置页面（已有 `AutoWireViewModel`） |
| `ProjectExplorerViewModel` | `BaseViewModel` | 项目树，需要加载项目数据 |
| `VariableManagerViewModel` | `BaseViewModel` | 变量管理，需要加载数据 |
| `DeviceDebug/*` (11个) | `BaseViewModel` | 设备控件，需要 IsBusy 管理通信状态 |
| `AddDeviceDialogViewModel` | `DialogViewModel` | 应实现 IDialogAware |
| `AddPositionDialogViewModel` | `DialogViewModel` | 应实现 IDialogAware |
| `ActionNodeViewModel` | `BindableBase` ✅ | 画布节点展示模型，不改 |
| `ConnectionViewModel` | `BindableBase` ✅ | 连接线展示模型，不改 |
| `CallSubProgramParameterViewModel` | `BindableBase` ✅ | 子程序参数模型，不改 |

**关键**：MotionDesigner 已经引用了 Wpf 项目，可以直接使用 `BaseViewModel` / `NavigationViewModel` / `DialogViewModel`。

**风险评估**：⚠️ 中风险。需要确认 MotionDesigner 的 DI 注册方式是否兼容 Prism 导航体系。

---

## 第三阶段：P2 一般问题（9项）

### 任务 3.1：清理死代码 RolesViewModel
- 删除 `ViewModels/RolesViewModel.cs`

### 任务 3.2：补充 4 个 View 缺失的 AutoWireViewModel
- 在 `DeviceParamsView.xaml`、`HardwareDebugView.xaml`、`PeripheralDebugView.xaml`、`ExperimentsView.xaml` 的 `<UserControl>` 标签中添加 `prism:ViewModelLocator.AutoWireViewModel="True"`

### 任务 3.3：清理 OperationLogsView 重复的本地样式定义
- 删除 `OperationLogsView.xaml` 的 `<UserControl.Resources>` 中已在全局定义的 `HeaderGradient`、`InfoBrush`、`WarnBrush`、`ErrorBrush`、`SuccessBrush`，改为使用全局资源

### 任务 3.4：补充占位页面
- `PeripheralDebugView`、`DeviceParamsView`、`ExperimentsView` 填充实际功能或至少显示有意义的信息

### 任务 3.5：统一间距系统
- 在现有的 `<UserControl.Resources>` 中添加间距别名（如在 PageRootGrid 中使用 `Margin="{StaticResource StandardSpacing}"` 替代 `Margin="12"`）

### 任务 3.6：提取 RunExperimentView 内联样式
- 将 60+ 处内联样式和硬编码颜色提取到 ViewStyles.xaml 中的新命名样式

### 任务 3.7：提取 LoginView 样式到独立文件
- 创建 `Resources/Styles/LoginStyles.xaml`
- 将 LoginView 的专属画刷和样式移入

### 任务 3.8：统一字体大小
- PageTitle（26px）→ 废弃，统一用 PageHeaderTitle（24px）
- PageSubtitle（14px）→ 废弃，统一用 PageHeaderSubtitle（12px）

### 任务 3.9：MotionDesigner 样式对齐
- 统一使用 Wpf 项目的颜色 Token（AppPrimaryTextBrush 等），或至少对齐色值

---

## 验证策略

每个阶段完成后执行：

```bash
# 编译验证
dotnet build IndustrySystem.sln

# 检查是否有编译警告（关注 CS0108, CS0114 等继承相关警告）
dotnet build IndustrySystem.sln -warnaserror
```

功能验证（手动）：
1. 启动应用 → 登录 → 导航到各页面确认页面正常显示
2. ShelfInfoView：点击容器列表/货架列表 → 确认弹窗正常打开/关闭 → 点击编辑 → 确认嵌套弹窗正常工作
3. 其他管理页面：确认 CRUD 操作正常

---

## 执行顺序建议

```
第一阶段（预计 4-6 小时）
├── 1.1 重命名基类        ← 先做，纯机械操作
└── 1.2 重构弹窗机制      ← 后做，涉及逻辑变更

第二阶段（预计 6-8 小时）
├── 2.1 MotionProgramViewer 基类  ← 简单
├── 2.2 参数编辑器移出 Dialogs    ← 简单
├── 2.3 按钮样式别名注释          ← 零风险
├── 2.4 统一图标库               ← 逐个页面改
└── 2.5 MotionDesigner 基类      ← 需要验证 DI

第三阶段（预计 8-12 小时）
├── 3.1~3.3  清理类（死代码、AutoWire、重复样式）← 简单
├── 3.4      补充占位页面                        ← 需要产品设计
├── 3.5~3.9  样式系统优化                         ← 细碎但重要
```
