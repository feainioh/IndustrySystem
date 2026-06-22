# IndustrySystem 遗留项整改计划

> 基于三轮整改后的剩余问题，按投入产出比排序

---

## 一、LoginView 样式提取

**目标**：将 LoginView 的 12 个专属画刷从 `ViewStyles.xaml` 移到独立 `LoginStyles.xaml`

**当前状态**：`ViewStyles.xaml` 第 55-81 行定义了 `Login*` 系列画刷，与页面布局样式混在一起

**方案**：
1. 新建 `Resources/Styles/LoginStyles.xaml`
2. 从 `ViewStyles.xaml` 剪切 `Login*` 画刷定义到新文件
3. 在 `App.xaml` 的资源合并中引入 `LoginStyles.xaml`
4. 验证 LoginView 显示正常

**文件**：`ViewStyles.xaml`（改）、`LoginStyles.xaml`（新）、`App.xaml`（改）
**风险**：低，纯移动

---

## 二、RunExperimentView 内联样式提取

**目标**：将 60+ 处硬编码颜色和内联样式提取到 `ViewStyles.xaml` 中的命名样式

**当前状态**：`RunExperimentView.xaml` 中存在大量 `#1D2939`、`#667085`、`#94A3B8` 等硬编码颜色

**方案**：
1. 统计所有硬编码颜色出现频率
2. 在 `ViewStyles.xaml` 中补充缺失的颜色 Token（如 `AppTextPrimaryColor`、`AppTextSecondaryColor` 已存在则引用）
3. 分批替换：
   - 第一批：设备状态指示器样式（Ellipse Style with DataTrigger）
   - 第二批：DrawerContent 布局颜色
   - 第三批：步骤管道指示器样式
4. 每批替换后编译验证

**文件**：`RunExperimentView.xaml`（改）、`ViewStyles.xaml`（改）
**风险**：中，大量内联样式，需分批

---

## 三、页面间距统一使用全局资源

**目标**：将硬编码 `Margin="12"`、`Margin="16"` 等替换为 `{StaticResource StandardSpacing}` 等

**当前状态**：`ViewStyles.xaml` 已定义 `SmallSpacing(8)`、`MediumSpacing(12)`、`StandardSpacing(16)`、`LargeSpacing(24)`，但无页面使用

**方案**：
1. 在 `ViewStyles.xaml` 中将 `PageRootGrid` 的 `Margin` 改为 `{StaticResource StandardSpacing}`
2. 在 RowDefinition 间距中逐步替换（需逐个文件改，约 18 个 View）
3. 优先改管理视图（9 个 CRUD 页面），再改特殊页面

**文件**：18 个 View XAML + `ViewStyles.xaml`
**风险**：低，但工作量大，可分批

---

## 四、PageHeaderPackIcon → PageHeaderMdl2Icon 替换

**目标**：逐步将页面头部的 `materialDesign:PackIcon` 替换为内联 `TextBlock` + Segoe MDL2

**涉及页面**：

| 页面 | 当前 PackIcon Kind | 替换为 MDL2 Glyph |
|------|-------------------|-------------------|
| ExperimentConfigView | TuneVariant | `&#xE90F;` |
| ExperimentTemplateView | FlaskOutline | `&#xE9D6;` |
| ExperimentGroupsView | FormatListChecks | `&#xE7C5;` |
| InventoryView | Warehouse | `&#xE8B7;` |
| MaterialInfoView | PackageVariant | `&#xE81E;` |
| PermissionsView | LockOutline | `&#xE72E;` |
| RoleManageView | ShieldAccount | `&#xEE2B;`（或保留） |
| UsersView | AccountMultiple | `&#xE716;` |
| ShelfInfoView | ViewGrid | `&#xE8F1;` |

**方案**：
```xml
<!-- 改前 -->
<materialDesign:PackIcon Kind="TuneVariant" Style="{StaticResource PageHeaderPackIcon}"/>

<!-- 改后 -->
<TextBlock Text="&#xE90F;" Style="{StaticResource PageHeaderMdl2Icon}"/>
```
每页改一行，逐页验证。

**文件**：9 个 View XAML
**风险**：低，仅图标字形变化

---

## 五、Dialog 表单中 PackIcon → FontIcon 替换

**目标**：Dialog 头部的表单图标从 PackIcon 替换为 FontIcon

**涉及页面**：

| Dialog | 当前 PackIcon | 替换 MDL2 |
|--------|-------------|-----------|
| ContainerEditDialog | PackageVariant | `&#xE81E;` |
| ContainerListDialog | PackageVariant, Plus | `&#xE81E;`, `&#xE710;` |
| ShelfEditDialog | ViewGrid | `&#xE8F1;` |
| ShelfListDialog | ViewGrid, Plus | `&#xE8F1;`, `&#xE710;` |
| SlotConfigDialog | CogOutline | `&#xE713;` |
| UserEditDialog | AccountEditOutline | `&#xE77B;`（近似） |
| PermissionEditDialog | KeyOutline | `&#xE75E;` |
| RoleEditDialog | ShieldEditOutline | `&#xEE2B;`（近似） |
| MaterialEditDialog | FlaskOutline | `&#xE9D6;` |
| InventoryEditDialog | ArrowDownBoldCircleOutline | `&#xE896;`（近似） |
| InventoryOutboundDialog | ArrowUpBoldCircleOutline | `&#xE898;`（近似） |
| ExperimentEditDialog | — | — |
| ExperimentGroupEditDialog | FormatListChecks | `&#xE7C5;` |
| ExperimentTemplateEditDialog | — | — |

**注意**：部分 PackIcon 无精确 MDL2 对应（如 `InformationOutline`、`CalendarClock`、`ShieldCheckOutline`），这些保留 PackIcon 并加注释。

**方案**：逐文件替换，无精确对应的保留并注释原因

**文件**：约 15 个 Dialog XAML
**风险**：低-中，部分图标语义可能略有偏差

---

## 六、MotionDesigner 样式配色对齐

**目标**：MotionDesigner 的配色与主 WPF 项目使用一致的 Token 名称

**当前状态**：MotionDesigner 有独立的 `MotionDesignerStyles.xaml`，使用 `MotionDesignerBorderBrush` 等自定义名称

**方案**：
1. 在 MotionDesigner 中引用 `ViewStyles.xaml` 的配色 Token（通过共享 ResourceDictionary 或复制色值）
2. 统一命名：`MotionDesignerBorderBrush` → `AppBorderBrush`（色值相同 #E2E8F0）
3. 将通用颜色定义提取到共享文件

**文件**：`MotionDesignerStyles.xaml`（改），可能需要创建共享 ResourceDictionary
**风险**：中，涉及跨项目资源引用

---

## 执行优先级建议

```
第一批（1-2小时）
├── 一、LoginView 样式提取       ← 简单移动
├── 四、PageHeader 图标替换      ← 9 页，每页改一行
└── 五、Dialog 表单图标替换      ← 15 页，每页改 1-3 行

第二批（2-4小时）
├── 三、页面间距统一             ← 工作量大但机械
└── 二、RunExperimentView 提取   ← 最复杂，需仔细

第三批（需要跨项目协调）
└── 六、MotionDesigner 配色对齐  ← 需要确认共享策略
```
