# MaterialDesign Quick Reference

## ?? 已完成的优化

### 1. MaterialTheme.xaml ?
统一的 Material 主题资源字典
- 完整色彩系统
- 渐变画刷
- 阴影效果
- 控件样式

### 2. LoginView.xaml ?
```
- 渐变背景 (#667eea → #764ba2)
- 玻璃效果卡片
- Material 输入框（带图标和浮动标签）
- 加载指示器
- 现代化按钮
```

### 3. Shell_MaterialDesign.xaml ?
```
- DrawerHost 抽屉导航
- 彩色 AppBar
- 分类菜单
- 通知系统
- 用户菜单
- 主题切换
```

### 4. UsersView_Material.xaml ?
```
- Material 卡片布局
- 渐变图标徽章
- Material DataGrid
- Chip 状态指示器
- 搜索功能
```

## ?? 快速使用指南

### Card（卡片）
```xaml
<materialDesign:Card UniformCornerRadius="8" 
                    Padding="16"
                    materialDesign:ElevationAssist.Elevation="Dp2">
    <StackPanel>
        <!-- 内容 -->
    </StackPanel>
</materialDesign:Card>
```

### TextField（文本框）
```xaml
<TextBox Style="{StaticResource MaterialTextField}"
        materialDesign:HintAssist.Hint="用户名"
        materialDesign:HintAssist.IsFloating="True"
        materialDesign:TextFieldAssist.HasLeadingIcon="True"
        materialDesign:TextFieldAssist.LeadingIcon="Account"/>
```

### Button（按钮）
```xaml
<!-- 主要按钮 -->
<Button Style="{StaticResource MaterialPrimaryButton}" Content="保存"/>

<!-- Accent 按钮 -->
<Button Style="{StaticResource MaterialAccentButton}" Content="提交"/>

<!-- 扁平按钮 -->
<Button Style="{StaticResource MaterialDesignFlatButton}" Content="取消"/>

<!-- 图标按钮 -->
<Button Style="{StaticResource MaterialIconButton}">
    <materialDesign:PackIcon Kind="Delete"/>
</Button>
```

### Icon（图标）
```xaml
<materialDesign:PackIcon Kind="Account" Width="24" Height="24"/>
```

### Chip（标签）
```xaml
<materialDesign:Chip Content="Active" 
                    Background="{StaticResource SuccessBrush}"/>
```

### DataGrid（数据表格）
```xaml
<DataGrid Style="{StaticResource MaterialDataGrid}"
         ItemsSource="{Binding Items}"/>
```

## ?? 色彩方案

### 主色
```
Primary:       #0078D4 (蓝色)
Primary Light: #00BCF2 (青色)
```

### 模块色
```
?? Users:       #FFB900 (黄金)
?? Roles:       #10893E (绿色)
?? Permissions: #8764B8 (紫色)
?? Experiments: #0078D4 (蓝色)
?? Materials:   #FF8C00 (橙色)
?? Devices:     #E74856 (红色)
?? Data:        #00BCF2 (青色)
```

### 状态色
```
? Success: #10893E (绿色)
?? Warning: #FFB900 (黄色)
? Error:   #E74856 (红色)
?? Info:    #0078D4 (蓝色)
```

## ?? 布局标准

### 页面结构
```xaml
<Grid Margin="24">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>   <!-- 标题 -->
        <RowDefinition Height="24"/>     <!-- 间距 -->
        <RowDefinition Height="Auto"/>   <!-- 操作卡片 -->
        <RowDefinition Height="16"/>     <!-- 间距 -->
        <RowDefinition Height="*"/>      <!-- 数据卡片 -->
    </Grid.RowDefinitions>
</Grid>
```

### 页面标题
```xaml
<Border Width="56" Height="56" CornerRadius="28"
        Background="{StaticResource UserGradientBrush}">
    <Border.Effect>
        <DropShadowEffect Color="#FFB900" BlurRadius="16" 
                        ShadowDepth="0" Opacity="0.6"/>
    </Border.Effect>
    <materialDesign:PackIcon Kind="AccountMultiple" 
                            Width="32" Height="32"
                            Foreground="White"/>
</Border>

<StackPanel>
    <TextBlock Text="标题" Style="{StaticResource PageTitle}"/>
    <TextBlock Text="副标题" Style="{StaticResource PageSubtitle}"/>
</StackPanel>
```

## ?? 主题切换

```csharp
var paletteHelper = new PaletteHelper();
var theme = paletteHelper.GetTheme();

// 切换到暗色主题
theme.SetBaseTheme(BaseTheme.Dark);

// 切换到亮色主题
theme.SetBaseTheme(BaseTheme.Light);

paletteHelper.SetTheme(theme);
```

## ?? 常用图标

```
Account           - 用户
AccountMultiple   - 用户列表
ShieldAccount     - 角色
Key               - 权限
PlayCircle        - 运行
ChartLine         - 数据
AlertCircle       - 告警
Package           - 物料
Warehouse         - 库存
DevicesOther      - 设备
Factory           - 系统
Cog               - 设置
History           - 日志
Magnify           - 搜索
Plus              - 添加
Pencil            - 编辑
Delete            - 删除
Refresh           - 刷新
Export            - 导出
Bell              - 通知
```

## ?? 阴影层级

```
Dp0:  无阴影
Dp1:  微妙阴影
Dp2:  卡片阴影 ? 推荐
Dp3:  抬起阴影
Dp4:  应用栏阴影
Dp6:  浮动阴影
Dp8:  对话框阴影
Dp12: 导航阴影
Dp16: 模态阴影
Dp24: 顶层阴影
```

## ?? 响应式设计

```
Mobile:  < 600px  (抽屉覆盖)
Tablet:  600-960px (抽屉可切换)
Desktop: > 960px  (抽屉持久化)
```

## ? 常用控件速查

### DialogHost
```xaml
<materialDesign:DialogHost Identifier="RootDialog">
    <ContentControl Content="{Binding CurrentContent}"/>
</materialDesign:DialogHost>
```

```csharp
await DialogHost.Show(content, "RootDialog");
```

### PopupBox
```xaml
<materialDesign:PopupBox>
    <materialDesign:PopupBox.ToggleContent>
        <materialDesign:PackIcon Kind="DotsVertical"/>
    </materialDesign:PopupBox.ToggleContent>
    <StackPanel>
        <Button Content="选项1"/>
        <Button Content="选项2"/>
    </StackPanel>
</materialDesign:PopupBox>
```

### Badge
```xaml
<materialDesign:Badged Badge="5">
    <materialDesign:PackIcon Kind="Bell"/>
</materialDesign:Badged>
```

### ProgressBar
```xaml
<!-- 线性进度条 -->
<ProgressBar Style="{StaticResource MaterialDesignLinearProgressBar}"
            IsIndeterminate="True"/>

<!-- 圆形进度条 -->
<ProgressBar Style="{StaticResource MaterialDesignCircularProgressBar}"
            IsIndeterminate="True"/>
```

### ColorZone
```xaml
<materialDesign:ColorZone Mode="PrimaryMid" Padding="16">
    <TextBlock Text="App Bar"/>
</materialDesign:ColorZone>
```

## ?? 下一步

### 需要更新的视图
1. ? LoginView - 完成
2. ? Shell - 完成
3. ? UsersView - 模板完成
4. ? RoleManageView - 待更新
5. ? PermissionsView - 待更新
6. ? ExperimentTemplateView - 待更新
7. ? MaterialInfoView - 待更新
8. ? InventoryView - 待更新
9. ? DeviceParamsView - 待更新
10. ? RealtimeDataView - 待更新

### 更新模板
使用 `UsersView_Material.xaml` 作为模板：
1. 复制页面结构
2. 修改模块色（图标渐变背景）
3. 更新图标 Kind
4. 调整 DataGrid 列
5. 绑定对应 ViewModel

## ?? 参考资源

- MaterialDesignInXaml: https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit
- Material Icons: https://materialdesignicons.com/
- Color Tool: https://material.io/resources/color/

---

**提示**: 所有新建视图直接参考 `UsersView_Material.xaml` 模板创建！
