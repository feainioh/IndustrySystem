# IndustrySystem 项目代码分析报告

> 分析日期：2026-06-20  
> 分析范围：整个解决方案（Presentation.Wpf + MotionDesigner）  
> 分析维度：UI样式一致性、ViewModel架构规范、弹窗机制统一性

---

## 一、项目概览

| 项目 | 类型 | View数量 | ViewModel数量 | 备注 |
|------|------|----------|---------------|------|
| IndustrySystem.Presentation.Wpf | 主WPF应用 | 22个View | 59个ViewModel | Prism + DryIoc + MaterialDesign |
| IndustrySystem.MotionDesigner | 运动设计器 | 4个View | 21个ViewModel | 独立的WPF应用 |

---

## 二、ViewModel 架构问题分析

### 2.1 现有基类能力矩阵

| 基类 | 提供的能力 | 适用场景 |
|------|-----------|---------|
| `BaseViewModel` | `IsBusy` + `RefreshCommand` + `OnRefreshAsync()` | 需要异步数据加载 + 繁忙状态管理的 VM |
| `NagetiveViewModel` | 继承 Base + `INavigationAware` + `Title` | Prism Region 导航目标页面 |
| `NagetiveCurdVeiwModel<T>` | 继承 Nav + 搜索/分页/列表 CRUD | 带搜索+分页的数据管理列表页 |
| `DialogViewModel` | 继承 Base + `IDialogAware` + `SaveCommand`/`CancelCommand` | Prism 模态弹窗 |
| `ExperimentParameterEditorViewModelBase` | 继承 Nav + 实验参数编辑逻辑 | Region 内子视图（参数编辑器） |

### 2.2 继承体系现状（含问题标注）

```
BindableBase (Prism.Mvvm)
│
├── BaseViewModel (abstract)
│   │  场景：需要异步数据加载 + 繁忙状态管理
│   │  提供：IsBusy, RefreshCommand, OnRefreshAsync()
│   │
│   ├── NagetiveViewModel (abstract)              ← ⚠️ 命名拼写错误
│   │   │  场景：Prism Region 导航目标的顶层页面
│   │   │  提供：+ INavigationAware, Title
│   │   │
│   │   ├── NagetiveCurdVeiwModel<TItem> (abstract) ← ⚠️ 双重拼写错误
│   │   │   │  场景：带搜索+分页的数据管理列表页
│   │   │   │  提供：+ SearchText, PageIndex, PageSize, 分页命令
│   │   │   │
│   │   │   └── (派生) UsersViewModel, PermissionsViewModel, RoleManageViewModel,
│   │   │            AlarmViewModel, ExperimentTemplateViewModel, MaterialInfoViewModel,
│   │   │            InventoryViewModel, ExperimentHistoryViewModel, ExperimentGroupsViewModel
│   │   │
│   │   └── (直接派生) ExperimentsViewModel, ExperimentConfigViewModel,
│   │                  HardwareDebugViewModel, RealtimeDataViewModel,
│   │                  RunExperimentViewModel, OperationLogsViewModel,
│   │                  ShelfInfoViewModel, MotionProgramRunViewModel,
│   │                  DeviceParamsViewModel, PeripheralDebugViewModel
│   │
│   ├── ExperimentParameterEditorViewModelBase
│   │   │  场景：Region 内子视图（参数编辑器）——继承 NavigationViewModel 是正确的，
│   │   │        因为它通过 Prism Region 导航加载，需要 INavigationAware 接收父 VM 参数
│   │   │  ⚠️ 但位于 Dialogs/ 命名空间，与行为不符
│   │   │
│   │   └── (派生) ReactionParameterEditDialogViewModel 等 10 个
│   │
│   └── DialogViewModel (abstract)
│       │  场景：Prism IDialogService 模态弹窗
│       │  提供：+ IDialogAware, Title, SaveCommand, CancelCommand
│       │
│       └── (派生) 16 个对话框 VM（含 LoginViewModel）
│
├── ShellViewModel : BindableBase                   ← ✅ 合理：Shell 是导航宿主，不是导航目标
│   │  说明：管理导航、主题切换、登出。不需要 INavigationAware（不被别人导航），
│   │        不需要 RefreshCommand（无数据列表加载）。命令均为独立的 DelegateCommand。
│   │
├── SensorMetricViewModel : BindableBase            ← ✅ 合理：纯数据展示模型
│   │  说明：RealtimeDataView 中的传感器数据项，无命令、无加载逻辑
│   │
├── ProgramNodeDisplayModel : BindableBase          ← ✅ 合理：纯数据展示模型
│   │  说明：MotionProgramViewer 中的程序节点展示
│   │
├── MotionProgramViewerViewModel : BindableBase     ← ⚠️ 应继承 BaseViewModel
│   │  说明：加载和解析 JSON 运动程序文件，需要 IsBusy 管理加载状态
│   │
└── MotionDesigner 全部 21 个 VM : BindableBase     ← ⚠️ 无基类体系
       说明：独立应用，未复用任何项目基类
```

### 2.3 发现的各问题详细分析

#### 问题1：命名拼写错误（严重）

| 当前名称 | 应改为 | 影响文件数 |
|----------|--------|-----------|
| `NagetiveViewModel` | `NavigationViewModel` | 基类 + 22个派生类 |
| `NagetiveCurdVeiwModel<T>` | `CrudViewModel<T>` | 基类 + 9个派生类 |

"Nagetive" 不是任何英文单词（疑似 "Negative" 的误拼，但实际含义是 Navigation）；"Curd" 应为 "Crud"（Create/Read/Update/Delete），"Veiw" 应为 "View"。

#### 问题2：ShellViewModel 继承 BindableBase —— 经分析，这是正确的

**文件**: `ViewModels/ShellViewModel.cs:12`
```csharp
public class ShellViewModel : BindableBase  // ✅ 合理，不需要改
```
**分析**：ShellViewModel 是导航宿主而非导航目标。它的命令（`ToggleVisualThemeCommand`、`LogoutCommand`、`NavSelectionChangedCommand`）都是独立的 `DelegateCommand`，不需要 `IsBusy` 保护。它不需要 `INavigationAware`（Shell 自己管理 Region，不被别人导航）。强行继承 `NavigationViewModel` 会产生一个永远不用的 `RefreshCommand` 和永远为 `false` 的 `IsBusy`——这是**过度继承**。

#### 问题3：MotionProgramViewerViewModel 应继承 BaseViewModel

**文件**: `ViewModels/MotionProgramViewerViewModel.cs:76`
```csharp
public class MotionProgramViewerViewModel : BindableBase  // ⚠️ 应改为继承 BaseViewModel
```
该 VM 需要加载和解析 JSON 运动程序文件（`OpenFileDialog` + 文件读取 + JSON 反序列化），应通过 `BaseViewModel.IsBusy` 管理加载状态。

#### 问题4：SensorMetricViewModel 和 ProgramNodeDisplayModel 继承正确

```csharp
public class SensorMetricViewModel : BindableBase { }       // ✅ 纯数据展示
public class ProgramNodeDisplayModel : BindableBase { }      // ✅ 纯数据展示
```
这两个是列表中的**数据项模型**，不是页面 VM。它们只有属性（Label、Value、IsExecuting 等），没有命令、没有加载逻辑。继承 `BaseViewModel` 会给每个实例都带上 `IsBusy` 和 `RefreshCommand`，造成不必要的内存开销。**直接继承 BindableBase 是正确的选择**。

#### 问题5：整个 MotionDesigner 项目无基类体系

MotionDesigner 的 21 个 ViewModel **全部直接继承 `BindableBase`**，完全没有使用基类体系。具体分析：

| 文件 | 类名 | 当前基类 | 建议基类 | 理由 |
|------|------|---------|---------|------|
| DesignerViewModel.cs | DesignerViewModel | BindableBase | NavigationViewModel | 设计器主页面，导航目标 |
| DeviceDebugViewModel.cs | DeviceDebugViewModel | BindableBase | NavigationViewModel | 设备调试页面，导航目标 |
| ProjectExplorerViewModel.cs | ProjectExplorerViewModel | BindableBase | BaseViewModel | 项目树，需要加载数据 |
| PositionSettingsViewModel.cs | PositionSettingsViewModel | BindableBase | NavigationViewModel | 位置设置页面，导航目标 |
| VariableManagerViewModel.cs | VariableManagerViewModel | BindableBase | BaseViewModel | 变量管理，需要加载数据 |
| DeviceDebug/* (11个) | 各调试VM | BindableBase | BaseViewModel | 设备控件 VM，需要加载/发送数据 |
| Dialogs/* (2个) | 对话框VM | BindableBase | DialogViewModel | 应使用标准对话框模式 |
| ActionNodeViewModel.cs | ActionNodeViewModel | BindableBase | BindableBase ✅ | 画布节点展示模型 |
| ConnectionViewModel.cs | ConnectionViewModel | BindableBase | BindableBase ✅ | 连接线展示模型 |
| CallSubProgramParameterViewModel.cs | CallSubProgramParameterViewModel | BindableBase | BindableBase ✅ | 子程序参数模型 |

#### 问题6：孤立的 ViewModel（死代码）

**文件**: `ViewModels/RolesViewModel.cs:6`
```csharp
public class RolesViewModel : NagetiveViewModel  // ⚠️ 无对应View，无DI注册，完全未使用
```
实际角色管理使用的是 `RoleManageViewModel : NagetiveCurdVeiwModel<RoleDto>`，`RolesViewModel` 是遗留的死代码。

#### 问题7：空壳 ViewModel

| ViewModel | View | 状态 |
|-----------|------|------|
| `DeviceParamsViewModel : NagetiveViewModel { }` | DeviceParamsView | 占位页面，显示"功能正在建设中" |
| `PeripheralDebugViewModel : NagetiveViewModel { }` | PeripheralDebugView | 占位页面，显示"Feature in progress..." |

#### 问题8：ExperimentParameterEditorViewModelBase 位置不当

**文件**: `ViewModels/Dialogs/ExperimentParameters/ExperimentParameterEditorViewModelBase.cs`

该类位于 `Dialogs` 命名空间，文件名包含 "Dialog"，但其继承 `NavigationViewModel`（而非 `DialogViewModel`）且通过 `RegisterForNavigation` 注册（而非 `RegisterDialog`）。实际上它是作为 Region 内嵌子视图运行，**继承 NavigationViewModel 是正确的**——它需要 `INavigationAware.OnNavigatedTo` 来接收父 VM 传递的参数。问题仅在于**命名空间位置误导**：应从 `ViewModels/Dialogs/` 移到 `ViewModels/ExperimentParameters/`。

#### 问题9：AutoWireViewModel 使用不一致

以下 4 个 View **缺少** `prism:ViewModelLocator.AutoWireViewModel="True"`：

| View | 注册方式 | 影响 |
|------|---------|------|
| DeviceParamsView | RegisterForNavigation | 依赖导航系统的隐式 VM 绑定 |
| HardwareDebugView | RegisterForNavigation | 同上 |
| PeripheralDebugView | RegisterForNavigation | 同上 |
| ExperimentsView | RegisterForNavigation | 同上 |

虽然 `RegisterForNavigation` 在导航时会自动设置 DataContext，但缺少 `AutoWireViewModel` 会导致视图在非导航场景下无法自动绑定 ViewModel，且与其他页面不一致。

---

## 三、弹窗机制问题分析

### 3.1 当前使用情况

项目中同时存在 **三种** 弹窗/对话框机制：

#### 机制A：Prism IDialogService（注册为 Prism Dialog）

在 `App.xaml.cs` 中通过 `RegisterDialog` 注册的对话框（共17个）：

| 对话框 | ViewModel |
|--------|-----------|
| LoginView | LoginViewModel |
| UserEditDialog | UserEditDialogViewModel |
| PermissionEditDialog | PermissionEditDialogViewModel |
| RoleEditDialog | RoleEditDialogViewModel |
| MaterialEditDialog | MaterialEditDialogViewModel |
| ShelfListDialog | ShelfListDialogViewModel |
| ShelfEditDialog | ShelfEditDialogViewModel |
| SlotConfigDialog | SlotConfigDialogViewModel |
| InventoryEditDialog | InventoryEditDialogViewModel |
| InventoryOutboundDialog | InventoryOutboundDialogViewModel |
| ContainerListDialog | ContainerListDialogViewModel |
| ContainerEditDialog | ContainerEditDialogViewModel |
| ExperimentTemplateEditDialog | ExperimentTemplateEditDialogViewModel |
| ExperimentGroupEditDialog | ExperimentGroupEditDialogViewModel |
| ExperimentEditDialog | ExperimentEditDialogViewModel |
| ConfirmDialog | ConfirmDialogViewModel |

**调用方式**（在 ViewModel 中）:
```csharp
private readonly IDialogService _dialogService;
_dialogService.ShowDialog("DialogName", parameters, callback);
```

#### 机制B：MaterialDesign DialogHost

Shell.xaml 中定义了 `materialDesign:DialogHost Identifier="RootDialogHost"`，仅在以下位置使用：

| 调用位置 | 使用方式 |
|----------|---------|
| `ShelfInfoViewModel.cs:576` | `await DialogHost.Show(dialog, "RootDialogHost")` |
| `ShelfInfoViewModel.cs:621` | `await DialogHost.Show(dialog, "RootDialogHost")` |
| `ShelfInfoViewModel.cs:489` | `DialogHost.Close("RootDialogHost")` |
| `ContainerListDialog.xaml.cs:16` | `DialogHost.Close("RootDialogHost")` |
| `ShelfListDialog.xaml.cs:16` | `DialogHost.Close("RootDialogHost")` |

#### 机制C：Region Navigation（区域导航——伪装成对话框）

10个实验参数编辑器通过 `RegisterForNavigation` 注册，作为**内嵌区域视图**而非弹出对话框：

| 视图 | 注册方式 |
|------|---------|
| ReactionParameterEditDialog | RegisterForNavigation |
| RotaryEvaporationParameterEditDialog | RegisterForNavigation |
| DetectionParameterEditDialog | RegisterForNavigation |
| FiltrationParameterEditDialog | RegisterForNavigation |
| DryingParameterEditDialog | RegisterForNavigation |
| QuenchingParameterEditDialog | RegisterForNavigation |
| ExtractionParameterEditDialog | RegisterForNavigation |
| SamplingParameterEditDialog | RegisterForNavigation |
| CentrifugationParameterEditDialog | RegisterForNavigation |
| CustomDetectionParameterEditDialog | RegisterForNavigation |

这些视图被导航到 `ExperimentConfigView` 中的 `ParameterEditorRegionName` 区域。

### 3.2 发现的问题

#### 问题9：ShelfInfoViewModel 混用两种弹窗机制（严重）

`ShelfInfoViewModel` 同时使用了 **IDialogService** 和 **DialogHost**：

- 通过构造函数注入 `IDialogService` 用于某些弹窗
- 但也直接调用 `DialogHost.Show(dialog, "RootDialogHost")` 显示 ContainerListDialog 和 ShelfListDialog
- 这些对话框的 code-behind 调用 `DialogHost.Close("RootDialogHost")`

而 `ContainerListDialog` 和 `ShelfListDialog` **同时也在 DI 中注册为 Prism Dialog**（`RegisterDialog`），造成了双重身份。

**影响**: 同一对话框有两种打开/关闭路径，行为不一致；DialogHost 方式不经过 Prism 的 DialogService 生命周期管理。

#### 问题10：实验参数编辑器"对话框"实际是内嵌视图

文件名和路径都在 `Dialogs/` 下，命名包含 "Dialog"，但技术实现是区域导航而非弹窗。这会导致：
- 新开发者误以为是弹窗而尝试用 `IDialogService` 打开
- 没有标准的 OK/Cancel 按钮（因为不是真正的对话框）
- 与真正的 Prism Dialog 行为不一致

#### 问题11：MotionDesigner 对话框无标准模式

MotionDesigner 的 `AddDeviceDialog` 和 `AddPositionDialog` 的 ViewModel 继承 `BindableBase`，没有实现 `IDialogAware`，缺少统一的对话框生命周期管理。

---

## 四、UI 样式一致性问题分析

### 4.1 样式资源文件结构

| 文件 | 内容 |
|------|------|
| `Resources/Styles/ViewStyles.xaml` | 主样式定义（颜色、布局、卡片、按钮、DataGrid等，约1012行） |
| `Resources/Styles/AdditionalStyles.xaml` | 辅助样式（页面容器、Section标题、对话框样式，约165行） |
| `Resources/Styles/Controls.xaml` | 控件样式（CompactButton等，约110行） |
| `Resources/Styles/MaterialTheme.xaml` | MaterialDesign主题配置 |
| `Resources/Styles/LiquidGlassTheme.xaml` | LiquidGlass主题配置 |

### 4.2 发现的问题

#### 问题12：图标库混用

项目中同时使用了 **两套图标系统**：

| 图标系统 | 用法示例 | 使用的页面 |
|----------|---------|-----------|
| `ui:FontIcon` (Segoe MDL2 Assets) | `<ui:FontIcon Glyph="&#xE950;"/>` | Shell, OperationLogsView, RealtimeDataView, DeviceParamsView, HardwareDebugView |
| `materialDesign:PackIcon` | `<materialDesign:PackIcon Kind="Account"/>` | LoginView, ExperimentConfigView, MotionProgramRunView |

部分页面甚至在同一文件中混用两种图标：
- **ExperimentConfigView**: header 用 PackIcon，但其他区域用 Segoe MDL2
- **RunExperimentView**: 全部使用内联 TextBlock FontIcon 方式

#### 问题13：样式命名体系不一致

| 风格 | 使用页面 | 示例 |
|------|---------|------|
| `HeaderActionOutlinedButton` / `HeaderActionAccentButton` | ExperimentConfigView | 编辑/保存操作按钮组 |
| `AppInteractiveButtonPrimary` / `AppInteractiveButtonNeutral` | RunExperimentView, OperationLogsView | Start/Stop 按钮组 |
| `PrimaryCommandButton` / `SecondaryCommandButton` | HardwareDebugView, OperationLogsView | 别名（指向 InteractiveButton） |
| 直接使用 MaterialDesign 样式 | MotionProgramRunView, LoginView | `MaterialDesignRaisedButton` 等 |

虽然 `PrimaryCommandButton` 是 `AppInteractiveButtonPrimary` 的别名，但命名不统一会让开发者困惑应该用哪个。

#### 问题14：页面留白过多

以下页面存在大量留白或空内容：

| 页面 | 问题描述 |
|------|---------|
| **PeripheralDebugView** | 整个页面只有一行文字 "Feature in progress..."，99% 空白 |
| **DeviceParamsView** | 页面只有标题 + "设备参数功能正在建设中。" 卡片 |
| **ExperimentsView** | 页面只有一个标题卡片，无实际内容 |
| **HardwareDebugView** | 多个 Height="12"、Height="16" 的空白行分隔，视觉稀疏 |
| **RealtimeDataView** | 温度趋势图区域使用硬编码的 Polyline 占位符 |

#### 问题15：字体大小不一致

| 位置 | 使用的字号 | 应有标准 |
|------|-----------|---------|
| 页面标题 | 24 (PageHeaderTitle) / 26 (PageTitle) | 应统一为24 |
| 副标题 | 12 (PageHeaderSubtitle) / 14 (PageSubtitle) | 应统一 |
| MotionProgramRunView 标题 | FontWeight="Light" | 其他页面使用 SemiBold |
| LoginView 标题 | FontSize="28" | 远大于其他页面 |

#### 问题16：过度自定义的页面样式

**LoginView** 拥有完全独立的设计系统（12个专属画刷 + 渐变 + 阴影），与其他页面风格完全不同。虽然有设计意图（品牌登录页），但色值、字号、间距等都未遵循项目全局样式。

**RunExperimentView** 拥有大量内联样式（DataTrigger 驱动的颜色切换、自定义 ListBoxItem 模板、自定义状态栏样式），包含约 60+ 处内联样式定义。

#### 问题17：间距系统不统一

ViewStyles.xaml 定义了间距标记：
```xml
<System:Double x:Key="SmallSpacing">8</System:Double>
<System:Double x:Key="MediumSpacing">12</System:Double>
<System:Double x:Key="StandardSpacing">16</System:Double>
<System:Double x:Key="LargeSpacing">24</System:Double>
```

但实际页面中几乎**从未使用**这些资源，而是使用硬编码的 Margin/Padding 值（如 `Margin="12"`, `Margin="16"`, `Margin="8"`）。

#### 问题18：重复定义本地样式资源

**OperationLogsView** 在 `<UserControl.Resources>` 中定义了 `HeaderGradient`，而这个渐变色在 `ViewStyles.xaml` 中已经有 `DataGradient`、`PermissionGradient` 等同类定义。

#### 问题19：MotionDesigner 样式体系完全独立

MotionDesigner 有自己的 `MotionDesignerStyles.xaml`，定义了独特的配色（`MotionDesignerBorderBrush` 等），与主 WPF 项目的 `ViewStyles.xaml` 完全独立。两个项目间没有共享任何样式资源。

---

## 五、问题汇总与严重程度分级

### 严重（P0 - 应立即修复）

| 编号 | 问题 | 影响范围 |
|------|------|---------|
| #1 | `NagetiveViewModel` / `NagetiveCurdVeiwModel` 命名拼写错误 | 2个基类 + 31个派生类 |
| #9 | `ShelfInfoViewModel` 混用 `IDialogService` 和 `DialogHost` | 货架管理 + 2个对话框 |

### 重要（P1 - 本迭代修复）

| 编号 | 问题 | 影响范围 |
|------|------|---------|
| #3 | `MotionProgramViewerViewModel` 应继承 `BaseViewModel` 以支持加载状态 | 运动程序查看器 |
| #5 | MotionDesigner 全部 ViewModel 无基类体系（页面VM需引入基类） | 整个 MotionDesigner 项目 |
| #8 | 实验参数编辑器命名空间位置不当（应在 ExperimentParameters/ 而非 Dialogs/） | 10个参数编辑器 + 基类 |
| #12 | 图标库混用（`ui:FontIcon` vs `materialDesign:PackIcon`） | 全部页面 |
| #13 | 按钮样式命名不统一 | 全部页面 |

### 一般（P2 - 下迭代修复）

| 编号 | 问题 | 影响范围 |
|------|------|---------|
| #6 | 孤立的 `RolesViewModel` 死代码 | 代码清洁度 |
| #7 | 空壳 ViewModel（DeviceParams/PeripheralDebug） | 2个页面 |
| #9 | AutoWireViewModel 不一致（4个View缺失） | 4个View |
| #11 | MotionDesigner 对话框无标准模式 | 2个对话框 |
| #14 | 页面留白过多（5个占位页面） | 5个页面 |
| #15 | 字体大小不一致 | 多个页面 |
| #16 | 过度自定义页面样式（LoginView, RunExperimentView） | 2个页面 |
| #17 | 间距系统已定义但未使用 | 全部页面 |
| #18 | 重复定义本地样式资源 | OperationLogsView |
| #19 | MotionDesigner 样式体系独立 | 整个 MotionDesigner |

### 无需修复（经分析确认合理）

| 编号 | 项 | 理由 |
|------|-----|------|
| — | `ShellViewModel : BindableBase` | Shell 是导航宿主，不是导航目标。不需要 `INavigationAware`，不需要 `RefreshCommand`。独立命令足够。 |
| — | `SensorMetricViewModel : BindableBase` | 纯数据展示模型，无命令、无加载逻辑 |
| — | `ProgramNodeDisplayModel : BindableBase` | 纯数据展示模型，无命令、无加载逻辑 |
| — | `ActionNodeViewModel : BindableBase` | MotionDesigner 画布节点展示模型 |
| — | `ConnectionViewModel : BindableBase` | MotionDesigner 连接线展示模型 |

---

## 六、建议的规范

### 6.1 ViewModel 继承规范（按职责决策）

#### 6.1.1 基类定义

```csharp
// 基础 ViewModel - 需要异步数据加载的 VM
public abstract class BaseViewModel : BindableBase
{
    bool IsBusy { get; }                    // 繁忙状态，自动管理 RefreshCommand 可执行性
    ICommand RefreshCommand { get; }        // 刷新命令，IsBusy=true 时不可执行
    protected virtual Task OnRefreshAsync(); // 子类重写此方法实现刷新逻辑
}

// 导航页面 ViewModel - Prism Region 导航目标
public abstract class NavigationViewModel : BaseViewModel, INavigationAware
{
    string Title { get; set; }              // 页面标题
}

// CRUD 管理页面 ViewModel - 带搜索、分页的列表管理页
public abstract class CrudViewModel<TItem> : NavigationViewModel
{
    string SearchText { get; set; }         // 搜索文本
    int PageIndex { get; set; }             // 当前页码
    int PageSize { get; set; }              // 每页条数
    int TotalCount { get; }                 // 总记录数
    ObservableCollection<TItem> Items { get; }
    // 分页命令：FirstPageCommand, PreviousPageCommand, NextPageCommand, LastPageCommand
}

// 对话框 ViewModel - Prism IDialogService 模态弹窗
public abstract class DialogViewModel : BaseViewModel, IDialogAware
{
    string Title { get; set; }
    ICommand SaveCommand { get; }           // 保存并关闭（返回 OK）
    ICommand CancelCommand { get; }         // 取消并关闭（返回 Cancel）
}
```

#### 6.1.2 继承决策树

```
问自己 4 个问题来决定 VM 应该继承哪个基类：

┌─ Q1: 这个 VM 是否需要异步加载数据（API调用、文件读取）？
│   ├─ 否 → BindableBase（纯展示模型 / 数据项模型 / Shell）
│   └─ 是 → 继承 BaseViewModel，继续 Q2
│
├─ Q2: 这个 VM 是 Prism 模态弹窗吗？
│   ├─ 是 → DialogViewModel（停止）
│   └─ 否 → 继续 Q3
│
├─ Q3: 这个 VM 是 Region 导航目标（页面或子视图）吗？
│   ├─ 否 → BaseViewModel（如嵌入的控件 VM）
│   └─ 是 → NavigationViewModel，继续 Q4
│
└─ Q4: 这个 VM 管理一个带搜索+分页的数据列表吗？
    ├─ 是 → CrudViewModel<T>
    └─ 否 → 保持 NavigationViewModel
```

#### 6.1.3 每个 ViewModel 的继承决策表

**▸ 继承 `CrudViewModel<T>` 的 VM（9个）**

| ViewModel | 泛型参数 | 理由 |
|-----------|---------|------|
| UsersViewModel | `UserDto` | 用户列表搜索+分页 |
| PermissionsViewModel | `PermissionDto` | 权限列表搜索+分页 |
| RoleManageViewModel | `RoleDto` | 角色列表搜索+分页 |
| AlarmViewModel | `AlarmItem` | 报警列表搜索+分页 |
| ExperimentTemplateViewModel | `ExperimentTemplateDto` | 模板列表搜索+分页 |
| MaterialInfoViewModel | `MaterialDto` | 物料列表搜索+分页 |
| InventoryViewModel | `InventoryRecordDto` | 库存列表搜索+分页 |
| ExperimentHistoryViewModel | `ExperimentHistoryDto` | 历史列表搜索+分页 |
| ExperimentGroupsViewModel | `ExperimentGroupDto` | 实验组列表搜索+分页 |

**▸ 继承 `NavigationViewModel` 的 VM（12个）**

| ViewModel | 理由 |
|-----------|------|
| ExperimentConfigViewModel | 导航页面，有数据加载但非列表分页 |
| ExperimentsViewModel | 导航页面 |
| HardwareDebugViewModel | 导航页面，设备调试 |
| RealtimeDataViewModel | 导航页面，实时数据展示 |
| RunExperimentViewModel | 导航页面，实验执行 |
| OperationLogsViewModel | 导航页面（虽然手动实现了分页，未复用 CrudViewModel） |
| ShelfInfoViewModel | 导航页面，货架信息 |
| MotionProgramRunViewModel | 导航页面，运动程序执行 |
| DeviceParamsViewModel | 导航页面（占位） |
| PeripheralDebugViewModel | 导航页面（占位） |
| ExperimentParameterEditorViewModelBase | Region 子视图基类，通过导航参数接收父 VM |
| MotionDesigner: DesignerViewModel | 设计器主页，导航目标 |

**▸ 继承 `BaseViewModel` 的 VM（2个 + MotionDesigner）**

| ViewModel | 理由 |
|-----------|------|
| MotionProgramViewerViewModel | 嵌入控件，加载JSON文件，需要 IsBusy |
| MotionDesigner: DeviceDebugViewModel | 设备调试页面，加载/发送数据 |
| MotionDesigner: DeviceDebug/* (11个) | 设备控件 VM，需要异步通信 |

**▸ 继承 `DialogViewModel` 的 VM（16个）**

所有通过 `RegisterDialog` 注册的弹窗 VM，按 Q2 决策。

**▸ 继承 `BindableBase` 的 VM（经分析合理，不改）**

| ViewModel | 理由 |
|-----------|------|
| ShellViewModel | 导航宿主，不是导航目标；命令均为独立 DelegateCommand；不需要 IsBusy/RefreshCommand |
| SensorMetricViewModel | RealtimeDataView 中的传感器数据项，纯属性，无命令 |
| ProgramNodeDisplayModel | MotionProgramViewer 中的程序节点，纯属性 |
| MotionDesigner: ActionNodeViewModel | 画布节点展示模型 |
| MotionDesigner: ConnectionViewModel | 连接线展示模型 |
| MotionDesigner: CallSubProgramParameterViewModel | 子程序参数模型 |

> **关键原则**：基类继承不是"越多越好"。给纯数据展示模型加 `IsBusy` 和 `RefreshCommand` 是**过度继承**，会给每个列表项实例带来不必要的内存开销。继承的正确标准是**这个 VM 实际需要哪些能力**，而不是"所有 VM 必须有基类"。

### 6.2 View-ViewModel 绑定规范

1. 所有 View 必须在 XAML 中添加 `prism:ViewModelLocator.AutoWireViewModel="True"`
2. View 的 code-behind 不应手动设置 `DataContext`
3. View-ViewModel 的注册统一在 `App.xaml.cs:RegisterTypes()` 中进行
4. 导航页面使用 `RegisterForNavigation<View, ViewModel>(nameof(View))`
5. 对话框使用 `RegisterDialog<View, ViewModel>()`

### 6.3 弹窗/对话框规范

**统一使用 Prism IDialogService**：

```csharp
// ✅ 正确：通过 IDialogService 打开对话框
private readonly IDialogService _dialogService;
_dialogService.ShowDialog(nameof(UserEditDialog), parameters, result => {
    if (result.Result == ButtonResult.OK) { /* 处理结果 */ }
});
```

**禁用 MaterialDesign DialogHost 直接调用**：
```csharp
// ❌ 禁止：直接使用 DialogHost
await DialogHost.Show(dialog, "RootDialogHost");
DialogHost.Close("RootDialogHost");
```

**对话框 ViewModel 必须实现 IDialogAware**（通过继承 DialogViewModel）：
```csharp
// ✅ 正确
public class MyDialogViewModel : DialogViewModel { }

// ❌ 错误
public class MyDialogViewModel : BindableBase { }
```

**区域嵌入的子视图不应命名为 "Dialog"**：
- 如果视图是通过 `RegisterForNavigation` 注册并在 Region 中使用的，应从 `Dialogs/` 文件夹移出，去掉 "Dialog" 后缀
- 只有通过 `RegisterDialog` 注册的才是对话框

### 6.4 UI 样式规范

**图标使用规范：**
- 统一使用 `ui:FontIcon`（Segoe MDL2 Assets），**禁止混用** `materialDesign:PackIcon`
- MDL2 图标字符参考：https://learn.microsoft.com/windows/apps/design/style/segoe-ui-symbol-font

**按钮样式规范：**
- 主要操作按钮：`Style="{StaticResource AppInteractiveButtonPrimary}"`
- 次要操作按钮：`Style="{StaticResource AppInteractiveButtonNeutral}"`
- 危险操作按钮：`Style="{StaticResource AppInteractiveButtonDanger}"`
- 图标工具栏按钮：`Style="{StaticResource ToolbarIconButton}"`

**页面布局规范：**
- 根容器使用 `<Grid Style="{StaticResource PageRootGrid}">`（统一 Margin=12）
- 卡片使用 `FormCard`（带内边距）或 `ListCard`（无内边距）
- 页面标题使用 `PageHeaderTitle`（FontSize=24, FontWeight=SemiBold）
- 页面副标题使用 `PageHeaderSubtitle`（FontSize=12）
- 使用间距资源：`{StaticResource SmallSpacing}`(8) / `{StaticResource MediumSpacing}`(12) / `{StaticResource StandardSpacing}`(16)

**禁用内联样式和重复定义：**
- 禁止在 View 的 `<UserControl.Resources>` 中定义已在全局样式中存在的画刷/样式
- 禁止硬编码色值（如 `#667085`），统一使用 `{DynamicResource AppMutedTextBrush}` 等

**LoginView 特殊处理：**
- LoginView 作为品牌登录页可以有独立的设计风格，但应将样式定义提取到独立的 LoginStyles.xaml 文件

### 6.5 命名规范

| 类别 | 规范 | 示例 |
|------|------|------|
| View | `{Feature}View` | `UsersView` |
| ViewModel | `{Feature}ViewModel` | `UsersViewModel` |
| Dialog View | `{Feature}Dialog` | `UserEditDialog` |
| Dialog ViewModel | `{Feature}DialogViewModel` | `UserEditDialogViewModel` |
| 导航基类 | `NavigationViewModel` | — |
| CRUD基类 | `CrudViewModel<T>` | — |
| 弹窗基类 | `DialogViewModel` | — |

### 6.6 MotionDesigner 规范

1. MotionDesigner 应创建自己的 ViewModel 基类（可共享 Wpf 项目的 `BaseViewModel` 通过共享项目）
2. 对话框 ViewModel 应实现 `IDialogAware` 接口
3. 样式资源应与主项目统一配色方案（通过共享 ResourceDictionary 或明确的主题 Token）

---

## 七、整改优先级路线图

### 第一阶段：修基类命名 + 清理混用弹窗（P0）

| 步骤 | 内容 | 影响范围 |
|------|------|---------|
| 1 | 重命名 `NagetiveViewModel` → `NavigationViewModel` | 1个基类文件 |
| 2 | 重命名 `NagetiveCurdVeiwModel<T>` → `CrudViewModel<T>` | 1个基类文件 |
| 3 | 更新所有派生类的基类引用 | 22 + 9 = 31个文件 |
| 4 | 重构 `ShelfInfoViewModel`，移除 `DialogHost` 直接调用，统一使用 `IDialogService` | 1个VM + 2个Dialog code-behind |
| 5 | 移除 `ContainerListDialog.xaml.cs` 和 `ShelfListDialog.xaml.cs` 中的 `DialogHost.Close()` 调用 | 2个文件 |

### 第二阶段：统一 ViewModel 继承体系（P1）

| 步骤 | 内容 | 影响范围 |
|------|------|---------|
| 1 | `MotionProgramViewerViewModel` 改为继承 `BaseViewModel`（Q1=是, Q2=否, Q3=否） | 1个文件 |
| 2 | 实验参数编辑器基类及子类从 `Dialogs/` 命名空间移到 `ExperimentParameters/` | 11个文件 |
| 3 | 为 MotionDesigner 的关键 VM 引入基类（页面VM→NavigationViewModel, 设备VM→BaseViewModel, 对话框VM→需实现IDialogAware） | ~15个文件 |
| 4 | 统一图标库为 `ui:FontIcon`（Segoe MDL2 Assets） | 所有View文件 |
| 5 | 统一按钮样式别名，废弃冗余别名（`PrimaryCommandButton` → `AppInteractiveButtonPrimary` 等） | 所有View文件 |

### 第三阶段：样式和布局优化（P2）

| 步骤 | 内容 | 影响范围 |
|------|------|---------|
| 1 | 补充占位页面内容（DeviceParamsView, PeripheralDebugView, ExperimentsView） | 3个View + VM |
| 2 | 清理死代码 `RolesViewModel` | 删除1个文件 |
| 3 | 统一4个缺失 `AutoWireViewModel` 的 View | 4个View |
| 4 | 消除页面留白，将硬编码间距替换为全局间距资源 | 所有View |
| 5 | 清理重复的本地样式定义（OperationLogsView 的 HeaderGradient 等） | 1个View |
| 6 | 统一字体大小（标题24px、副标题12px）和排版 | 多个View |
| 7 | 提取 RunExperimentView 内联样式为独立资源 | 1个View |
| 8 | 提取 LoginView 样式到独立 LoginStyles.xaml | 1个View + 1个新文件 |
| 9 | MotionDesigner 样式体系与主项目配色对齐 | MotionDesigner |
