using System.Collections.ObjectModel;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels;

/// <summary>
/// 变量定义
/// </summary>
public class VariableDefinition : BindableBase
{
    private string _name = string.Empty;
    private string _dataType = "String";
    private object? _value;
    private string _description = string.Empty;
    private VariableScope _scope = VariableScope.Local;
    private bool _isReadOnly;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string DataType
    {
        get => _dataType;
        set => SetProperty(ref _dataType, value);
    }

    public object? Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public VariableScope Scope
    {
        get => _scope;
        set => SetProperty(ref _scope, value);
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }

    /// <summary>
    /// 获取显示值
    /// </summary>
    public string DisplayValue => Value?.ToString() ?? "(null)";
}

/// <summary>
/// 变量作用域
/// </summary>
public enum VariableScope
{
    /// <summary>
    /// 全局变量 - 整个项目可用
    /// </summary>
    Global,

    /// <summary>
    /// 子项目变量 - 子项目内可用
    /// </summary>
    SubProject,

    /// <summary>
    /// 局部变量 - 子程序内可用
    /// </summary>
    Local,

    /// <summary>
    /// 系统变量 - 只读系统变量
    /// </summary>
    System
}

/// <summary>
/// 变量管理器 ViewModel
/// </summary>
public class VariableManagerViewModel : BindableBase
{
    private VariableDefinition? _selectedVariable;
    private string _filterText = string.Empty;

    /// <summary>
    /// 全局变量
    /// </summary>
    public ObservableCollection<VariableDefinition> GlobalVariables { get; } = [];

    /// <summary>
    /// 子项目变量
    /// </summary>
    public ObservableCollection<VariableDefinition> SubProjectVariables { get; } = [];

    /// <summary>
    /// 局部变量
    /// </summary>
    public ObservableCollection<VariableDefinition> LocalVariables { get; } = [];

    /// <summary>
    /// 系统变量
    /// </summary>
    public ObservableCollection<VariableDefinition> SystemVariables { get; } = [];

    /// <summary>
    /// 所有变量（用于搜索）
    /// </summary>
    public ObservableCollection<VariableDefinition> AllVariables { get; } = [];

    /// <summary>
    /// 选中的变量
    /// </summary>
    public VariableDefinition? SelectedVariable
    {
        get => _selectedVariable;
        set => SetProperty(ref _selectedVariable, value);
    }

    /// <summary>
    /// 过滤文本
    /// </summary>
    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
            {
                ApplyFilter();
            }
        }
    }

    /// <summary>
    /// 可用的数据类型
    /// </summary>
    public string[] DataTypes { get; } = ["String", "Int32", "Double", "Boolean", "DateTime", "Object"];

    public ICommand AddGlobalVariableCommand { get; }
    public ICommand AddLocalVariableCommand { get; }
    public ICommand DeleteVariableCommand { get; }
    public ICommand EditVariableCommand { get; }

    public VariableManagerViewModel()
    {
        AddGlobalVariableCommand = new DelegateCommand(AddGlobalVariable);
        AddLocalVariableCommand = new DelegateCommand(AddLocalVariable);
        DeleteVariableCommand = new DelegateCommand(DeleteVariable, () => SelectedVariable != null)
            .ObservesProperty(() => SelectedVariable);
        EditVariableCommand = new DelegateCommand(EditVariable, () => SelectedVariable != null)
            .ObservesProperty(() => SelectedVariable);

        // 初始化系统变量
        InitializeSystemVariables();
    }

    private void InitializeSystemVariables()
    {
        SystemVariables.Add(new VariableDefinition
        {
            Name = "$CurrentTime",
            DataType = "DateTime",
            Value = DateTime.Now,
            Description = "当前系统时间",
            Scope = VariableScope.System,
            IsReadOnly = true
        });

        SystemVariables.Add(new VariableDefinition
        {
            Name = "$ExecutionState",
            DataType = "String",
            Value = "Idle",
            Description = "当前执行状态",
            Scope = VariableScope.System,
            IsReadOnly = true
        });

        SystemVariables.Add(new VariableDefinition
        {
            Name = "$LoopIndex",
            DataType = "Int32",
            Value = 0,
            Description = "当前循环索引",
            Scope = VariableScope.System,
            IsReadOnly = true
        });

        SystemVariables.Add(new VariableDefinition
        {
            Name = "$LastError",
            DataType = "String",
            Value = string.Empty,
            Description = "最后一个错误信息",
            Scope = VariableScope.System,
            IsReadOnly = true
        });

        RefreshAllVariables();
    }

    private void AddGlobalVariable()
    {
        var variable = new VariableDefinition
        {
            Name = $"GlobalVar{GlobalVariables.Count + 1}",
            DataType = "String",
            Scope = VariableScope.Global
        };
        GlobalVariables.Add(variable);
        RefreshAllVariables();
        SelectedVariable = variable;
    }

    private void AddLocalVariable()
    {
        var variable = new VariableDefinition
        {
            Name = $"LocalVar{LocalVariables.Count + 1}",
            DataType = "String",
            Scope = VariableScope.Local
        };
        LocalVariables.Add(variable);
        RefreshAllVariables();
        SelectedVariable = variable;
    }

    private void DeleteVariable()
    {
        if (SelectedVariable == null || SelectedVariable.IsReadOnly) return;

        switch (SelectedVariable.Scope)
        {
            case VariableScope.Global:
                GlobalVariables.Remove(SelectedVariable);
                break;
            case VariableScope.SubProject:
                SubProjectVariables.Remove(SelectedVariable);
                break;
            case VariableScope.Local:
                LocalVariables.Remove(SelectedVariable);
                break;
        }

        RefreshAllVariables();
        SelectedVariable = null;
    }

    private void EditVariable()
    {
        // 变量编辑在属性面板中进行
    }

    private void RefreshAllVariables()
    {
        AllVariables.Clear();
        foreach (var v in SystemVariables) AllVariables.Add(v);
        foreach (var v in GlobalVariables) AllVariables.Add(v);
        foreach (var v in SubProjectVariables) AllVariables.Add(v);
        foreach (var v in LocalVariables) AllVariables.Add(v);
    }

    private void ApplyFilter()
    {
        // 实现变量过滤逻辑
    }

    /// <summary>
    /// 获取变量值
    /// </summary>
    public object? GetVariable(string name)
    {
        var variable = AllVariables.FirstOrDefault(v => v.Name == name);
        return variable?.Value;
    }

    /// <summary>
    /// 设置变量值
    /// </summary>
    public void SetVariable(string name, object? value)
    {
        var variable = AllVariables.FirstOrDefault(v => v.Name == name);
        if (variable != null && !variable.IsReadOnly)
        {
            variable.Value = value;
        }
    }

    /// <summary>
    /// 更新系统变量
    /// </summary>
    public void UpdateSystemVariable(string name, object? value)
    {
        var variable = SystemVariables.FirstOrDefault(v => v.Name == name);
        if (variable != null)
        {
            variable.Value = value;
        }
    }
}
