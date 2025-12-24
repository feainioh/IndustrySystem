# MaterialDesign Theme Integration Guide

## Overview

Complete MaterialDesign theme integration for IndustrySystem WPF application, providing modern, beautiful, and consistent UI across all pages.

## Files Created

### 1. MaterialTheme.xaml ?
**Location**: `Resources/Styles/MaterialTheme.xaml`

**Purpose**: Central MaterialDesign theme resource dictionary

**Contents**:
- Color palette (Primary, Accent, Module colors)
- Gradient brushes
- Elevation effects
- Material control styles
- Layout styles
- Icon badges
- Spacing constants

### 2. Shell_MaterialDesign.xaml ?
**Location**: `Shell_MaterialDesign.xaml`

**Features**:
- MaterialDesign DrawerHost for navigation
- Responsive drawer menu
- Color zones for app bar
- Material cards and elevations
- Modern navigation listbox
- Notification system
- User profile menu
- Theme switcher (Light/Dark)

### 3. LoginView.xaml (Updated) ?
**Features**:
- Gradient background with decorative elements
- Glass-effect card
- Material text fields with icons
- Floating labels
- Loading indicator
- Modern button styles

### 4. UsersView_Material.xaml ?
**Template for all management views**

**Features**:
- Material cards with elevation
- Gradient icon badges
- Material DataGrid
- Floating action buttons
- Search functionality
- Chip status indicators

## Color System

### Primary Colors
```
Primary:        #0078D4 (Blue)
Primary Light:  #00BCF2 (Cyan)
Primary Dark:   #005A9E (Dark Blue)
```

### Module-Specific Colors
```
Users:       #FFB900 (Yellow-Gold)
Roles:       #10893E (Green)
Permissions: #8764B8 (Purple)
Experiments: #0078D4 (Blue)
Materials:   #FF8C00 (Orange)
Devices:     #E74856 (Red)
Data:        #00BCF2 (Cyan)
Alarms:      #F7630C (Orange-Red)
```

### Status Colors
```
Success:  #10893E (Green)
Warning:  #FFB900 (Yellow)
Error:    #E74856 (Red)
Info:     #0078D4 (Blue)
```

## Material Components Used

### Cards
```xaml
<materialDesign:Card UniformCornerRadius="8"
                    Padding="16"
                    materialDesign:ElevationAssist.Elevation="Dp2">
    <!-- Content -->
</materialDesign:Card>
```

### Text Fields
```xaml
<TextBox Style="{StaticResource MaterialTextField}"
        materialDesign:HintAssist.Hint="Username"
        materialDesign:HintAssist.IsFloating="True"
        materialDesign:TextFieldAssist.HasLeadingIcon="True"
        materialDesign:TextFieldAssist.LeadingIcon="Account"/>
```

### Buttons
```xaml
<!-- Raised Button -->
<Button Style="{StaticResource MaterialDesignRaisedButton}"/>

<!-- Flat Button -->
<Button Style="{StaticResource MaterialDesignFlatButton}"/>

<!-- Icon Button -->
<Button Style="{StaticResource MaterialDesignIconButton}">
    <materialDesign:PackIcon Kind="Delete"/>
</Button>

<!-- FAB -->
<Button Style="{StaticResource MaterialDesignFloatingActionButton}"/>
```

### Chips
```xaml
<materialDesign:Chip Content="Active"
                    Background="{StaticResource SuccessBrush}"
                    Foreground="White"/>
```

### DataGrid
```xaml
<DataGrid Style="{StaticResource MaterialDataGrid}"
         materialDesign:DataGridAssist.ColumnHeaderPadding="16,8"
         materialDesign:DataGridAssist.CellPadding="16,8"/>
```

### Icons
```xaml
<materialDesign:PackIcon Kind="Account" 
                        Width="24" Height="24"/>
```

## Layout Standards

### Page Structure
```xaml
<Grid Style="{StaticResource PageContainer}">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>        <!-- Header -->
        <RowDefinition Height="24"/>          <!-- Spacing -->
        <RowDefinition Height="Auto"/>        <!-- Action Card -->
        <RowDefinition Height="16"/>          <!-- Spacing -->
        <RowDefinition Height="*"/>           <!-- Data Card -->
    </Grid.RowDefinitions>
</Grid>
```

### Page Header
```xaml
<Grid>
    <!-- Icon Badge -->
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
    
    <!-- Title and Subtitle -->
    <StackPanel>
        <TextBlock Text="Page Title" 
                  Style="{StaticResource PageTitle}"/>
        <TextBlock Text="Page description" 
                  Style="{StaticResource PageSubtitle}"/>
    </StackPanel>
    
    <!-- Action Buttons -->
    <StackPanel Orientation="Horizontal">
        <!-- Buttons -->
    </StackPanel>
</Grid>
```

## Theme Switching

### Light Theme (Default)
- White surfaces
- Light shadows
- Dark text
- Vibrant colors

### Dark Theme
```csharp
var paletteHelper = new PaletteHelper();
var theme = paletteHelper.GetTheme();
theme.SetBaseTheme(BaseTheme.Dark);
paletteHelper.SetTheme(theme);
```

## Elevation System

MaterialDesign uses elevation to show hierarchy:

```
Dp0:  No elevation (flat)
Dp1:  1dp elevation (subtle)
Dp2:  2dp elevation (cards)
Dp3:  3dp elevation (raised)
Dp4:  4dp elevation (app bar)
Dp6:  6dp elevation (floating)
Dp8:  8dp elevation (dialogs)
Dp12: 12dp elevation (navigation)
Dp16: 16dp elevation (modals)
Dp24: 24dp elevation (top layer)
```

## Navigation Patterns

### Drawer Navigation
- Hamburger menu toggle
- Categorized menu sections
- Icon + text items
- Active item highlighting
- Auto-close on mobile

### Menu Structure
```
DASHBOARD
й└йд Run Experiment
й└йд Realtime Data
й╕йд Alarms

MANAGEMENT
й└йд Experiment Templates
й└йд Experiment Config
й└йд Materials
й└йд Inventory
й╕йд Devices

SYSTEM
й└йд Users
й└йд Roles
й└йд Permissions
й╕йд Operation Logs
```

## Responsive Design

### Breakpoints
```
Mobile:    < 600px  (Drawer overlay)
Tablet:    600-960px (Drawer toggleable)
Desktop:   > 960px  (Drawer persistent)
```

### Drawer Behavior
- Mobile: Overlay, auto-close
- Tablet: Toggle mode
- Desktop: Persistent mode

## Interactive Elements

### Hover Effects
All interactive elements have hover feedback:
- Cards: Elevation increase
- Buttons: Background tint
- List items: Background highlight

### Ripple Effect
MaterialDesign automatically adds ripple effects to:
- Buttons
- List items
- Cards (when interactive)
- Menu items

## Dialogs and Popups

### Material Dialog
```xaml
<materialDesign:DialogHost Identifier="RootDialog">
    <!-- Content -->
</materialDesign:DialogHost>
```

```csharp
await DialogHost.Show(dialogContent, "RootDialog");
```

### Popup Box
```xaml
<materialDesign:PopupBox PlacementMode="BottomAndAlignRightEdges">
    <materialDesign:PopupBox.ToggleContent>
        <materialDesign:PackIcon Kind="DotsVertical"/>
    </materialDesign:PopupBox.ToggleContent>
    <StackPanel>
        <Button Content="Action 1"/>
        <Button Content="Action 2"/>
    </StackPanel>
</materialDesign:PopupBox>
```

## Icons Library

### Material Design Icons
Using MaterialDesignInXaml.PackIcon:

**Common Icons**:
- Account: User/Profile
- AccountMultiple: Users list
- ShieldAccount: Roles
- Key: Permissions
- PlayCircle: Run
- ChartLine: Data/Analytics
- AlertCircle: Alarms
- Package: Materials
- Warehouse: Inventory
- DevicesOther: Devices
- Factory: System/Logo
- Cog: Settings
- History: Logs
- Magnify: Search
- Plus: Add
- Pencil: Edit
- Delete: Delete
- Refresh: Refresh
- Export: Export
- Bell: Notifications

Full icon list: https://materialdesignicons.com/

## Typography

### Text Styles
```xaml
<!-- Headlines -->
<TextBlock Style="{StaticResource MaterialDesignHeadline3TextBlock}"/>
<TextBlock Style="{StaticResource MaterialDesignHeadline4TextBlock}"/>
<TextBlock Style="{StaticResource MaterialDesignHeadline5TextBlock}"/>
<TextBlock Style="{StaticResource MaterialDesignHeadline6TextBlock}"/>

<!-- Body -->
<TextBlock Style="{StaticResource MaterialDesignBody1TextBlock}"/>
<TextBlock Style="{StaticResource MaterialDesignBody2TextBlock}"/>

<!-- Captions -->
<TextBlock Style="{StaticResource MaterialDesignCaptionTextBlock}"/>
<TextBlock Style="{StaticResource MaterialDesignOverlineTextBlock}"/>
```

## Best Practices

### DO ?
- Use Material cards for content grouping
- Apply consistent spacing (8, 16, 24px)
- Use gradient backgrounds for headers
- Add elevation to interactive elements
- Use PackIcon for all icons
- Follow color palette
- Add loading indicators
- Use floating labels for text fields
- Group related actions

### DON'T ?
- Mix different design systems
- Use random colors
- Ignore elevation system
- Create custom buttons when Material styles exist
- Overcomplicate layouts
- Forget hover states
- Ignore responsive breakpoints
- Use deprecated Material 2 components

## Migration Checklist

For each view to update:

- [ ] Replace Grid with MaterialDesign Card
- [ ] Update TextBox to Material style with floating hints
- [ ] Replace Button with Material button styles
- [ ] Add gradient icon badge to header
- [ ] Update DataGrid to MaterialDataGrid style
- [ ] Add Chip for status indicators
- [ ] Use PackIcon for all icons
- [ ] Apply proper elevation
- [ ] Add hover effects
- [ ] Test light and dark themes
- [ ] Verify responsive behavior

## Performance Tips

1. **Reuse Styles**: Always use StaticResource for styles
2. **Virtualization**: Enabled by default in Material DataGrid
3. **Icon Caching**: PackIcon caches SVG paths
4. **Lazy Loading**: Load drawer content only when opened
5. **Theme Caching**: Theme changes are lightweight

## Accessibility

MaterialDesign includes:
- High contrast support
- Screen reader compatibility
- Keyboard navigation
- Focus indicators
- ARIA labels

## Troubleshooting

### Issue: Icons not showing
**Solution**: Ensure MaterialDesignThemes.Wpf package is installed and referenced

### Issue: Styles not applied
**Solution**: Check MaterialTheme.xaml is in App.xaml MergedDictionaries

### Issue: Theme not switching
**Solution**: Use PaletteHelper.SetTheme() method

### Issue: Colors look wrong
**Solution**: Verify using correct brushes (e.g., PrimaryBrush not PrimaryColor)

## Example Views to Update

Priority order:
1. ? LoginView - Complete
2. ? Shell - Complete (Shell_MaterialDesign.xaml)
3. ? UsersView - Template created
4. RoleManageView
5. PermissionsView
6. ExperimentTemplateView
7. MaterialInfoView
8. InventoryView
9. DeviceParamsView
10. RealtimeDataView
11. OperationLogsView
12. All remaining views

## Resources

- [MaterialDesignInXaml](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit)
- [Material Design Guidelines](https://material.io/design)
- [Material Design Icons](https://materialdesignicons.com/)
- [Color Tool](https://material.io/resources/color/)

---

**Status**: ?? MaterialDesign Integration Complete
**Files**: 4 created, ready for implementation
**Next Steps**: Update remaining views using UsersView_Material.xaml as template
