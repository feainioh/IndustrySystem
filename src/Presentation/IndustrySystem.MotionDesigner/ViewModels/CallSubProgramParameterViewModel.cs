using System.Collections.ObjectModel;
using System.Windows.Input;
using IndustrySystem.MotionDesigner.Models;
using IndustrySystem.MotionDesigner.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels;

/// <summary>
/// 子程序调用参数 ViewModel
/// 用于 CallSubProgram 节点的参数编辑
/// </summary>
public class CallSubProgramParameterViewModel : BindableBase
{
    private readonly IProjectService _projectService;
    private readonly IEventAggregator _eventAggregator;

    private SubProgram? _selectedSubProgram;
    private bool _waitForCompletion = true;
    private int _timeout = 30000;

    /// <summary>
    /// 可调用的子程序列表
    /// </summary>
    public ObservableCollection<SubProgramItem> AvailableSubPrograms { get; } = [];

    /// <summary>
    /// 输入参数映射
    /// </summary>
    public ObservableCollection<ParameterMapping> InputMappings { get; } = [];

    /// <summary>
    /// 输出参数映射
    /// </summary>
    public ObservableCollection<ParameterMapping> OutputMappings { get; } = [];

    /// <summary>
    /// 选中的子程序
    /// </summary>
    public SubProgram? SelectedSubProgram
    {
        get => _selectedSubProgram;
        set
        {
            if (SetProperty(ref _selectedSubProgram, value))
            {
                OnSelectedSubProgramChanged();
            }
        }
    }

    /// <summary>
    /// 是否等待子程序完成
    /// </summary>
    public bool WaitForCompletion
    {
        get => _waitForCompletion;
        set => SetProperty(ref _waitForCompletion, value);
    }

    /// <summary>
    /// 超时时间（毫秒）
    /// </summary>
    public int Timeout
    {
        get => _timeout;
        set => SetProperty(ref _timeout, value);
    }

    public ICommand RefreshSubProgramsCommand { get; }

    public CallSubProgramParameterViewModel(IProjectService projectService, IEventAggregator eventAggregator)
    {
        _projectService = projectService;
        _eventAggregator = eventAggregator;

        RefreshSubProgramsCommand = new DelegateCommand(RefreshSubPrograms);

        // 订阅项目变化事件
        _eventAggregator.GetEvent<SubProgramModifiedEvent>().Subscribe(_ => RefreshSubPrograms());
    }

    /// <summary>
    /// 刷新可用子程序列表
    /// </summary>
    public void RefreshSubPrograms()
    {
        AvailableSubPrograms.Clear();

        var project = _projectService.CurrentProject;
        if (project == null) return;

        foreach (var subProject in project.SubProjects)
        {
            foreach (var subProgram in subProject.SubPrograms)
            {
                AvailableSubPrograms.Add(new SubProgramItem
                {
                    SubProgram = subProgram,
                    DisplayName = $"{subProject.Name}/{subProgram.Name}",
                    FullPath = $"{project.Name}/{subProject.Name}/{subProgram.Name}"
                });
            }
        }
    }

    private void OnSelectedSubProgramChanged()
    {
        InputMappings.Clear();
        OutputMappings.Clear();

        if (SelectedSubProgram == null) return;

        // 创建输入参数映射
        foreach (var param in SelectedSubProgram.InputParameters)
        {
            InputMappings.Add(new ParameterMapping
            {
                ParameterName = param.Name,
                ParameterType = param.DataType,
                IsRequired = param.IsRequired,
                Description = param.Description,
                DefaultValue = param.DefaultValue?.ToString()
            });
        }

        // 创建输出参数映射
        foreach (var param in SelectedSubProgram.OutputParameters)
        {
            OutputMappings.Add(new ParameterMapping
            {
                ParameterName = param.Name,
                ParameterType = param.DataType,
                Description = param.Description
            });
        }
    }

    /// <summary>
    /// 获取参数字典（用于保存到节点参数）
    /// </summary>
    public Dictionary<string, object> GetParameters()
    {
        var parameters = new Dictionary<string, object>
        {
            ["SubProgramId"] = SelectedSubProgram?.Id ?? Guid.Empty,
            ["SubProgramName"] = SelectedSubProgram?.Name ?? string.Empty,
            ["WaitForCompletion"] = WaitForCompletion,
            ["Timeout"] = Timeout
        };

        // 输入参数
        var inputs = new Dictionary<string, string>();
        foreach (var mapping in InputMappings)
        {
            if (!string.IsNullOrEmpty(mapping.MappedVariable))
            {
                inputs[mapping.ParameterName] = mapping.MappedVariable;
            }
        }
        parameters["InputMappings"] = inputs;

        // 输出参数
        var outputs = new Dictionary<string, string>();
        foreach (var mapping in OutputMappings)
        {
            if (!string.IsNullOrEmpty(mapping.MappedVariable))
            {
                outputs[mapping.ParameterName] = mapping.MappedVariable;
            }
        }
        parameters["OutputMappings"] = outputs;

        return parameters;
    }

    /// <summary>
    /// 从参数字典加载
    /// </summary>
    public void LoadFromParameters(Dictionary<string, object> parameters)
    {
        RefreshSubPrograms();

        if (parameters.TryGetValue("SubProgramId", out var idObj) && idObj is Guid subProgramId)
        {
            var item = AvailableSubPrograms.FirstOrDefault(sp => sp.SubProgram?.Id == subProgramId);
            if (item != null)
            {
                SelectedSubProgram = item.SubProgram;
            }
        }

        if (parameters.TryGetValue("WaitForCompletion", out var waitObj))
        {
            WaitForCompletion = Convert.ToBoolean(waitObj);
        }

        if (parameters.TryGetValue("Timeout", out var timeoutObj))
        {
            Timeout = Convert.ToInt32(timeoutObj);
        }

        // 加载输入映射
        if (parameters.TryGetValue("InputMappings", out var inputObj) && inputObj is Dictionary<string, string> inputs)
        {
            foreach (var mapping in InputMappings)
            {
                if (inputs.TryGetValue(mapping.ParameterName, out var variable))
                {
                    mapping.MappedVariable = variable;
                }
            }
        }

        // 加载输出映射
        if (parameters.TryGetValue("OutputMappings", out var outputObj) && outputObj is Dictionary<string, string> outputs)
        {
            foreach (var mapping in OutputMappings)
            {
                if (outputs.TryGetValue(mapping.ParameterName, out var variable))
                {
                    mapping.MappedVariable = variable;
                }
            }
        }
    }
}

/// <summary>
/// 子程序项
/// </summary>
public class SubProgramItem
{
    public SubProgram? SubProgram { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
}

/// <summary>
/// 参数映射
/// </summary>
public class ParameterMapping : BindableBase
{
    private string _parameterName = string.Empty;
    private string _parameterType = "String";
    private string? _mappedVariable;
    private string? _constantValue;
    private bool _useConstant;
    private bool _isRequired;
    private string _description = string.Empty;
    private string? _defaultValue;

    /// <summary>
    /// 参数名称
    /// </summary>
    public string ParameterName
    {
        get => _parameterName;
        set => SetProperty(ref _parameterName, value);
    }

    /// <summary>
    /// 参数类型
    /// </summary>
    public string ParameterType
    {
        get => _parameterType;
        set => SetProperty(ref _parameterType, value);
    }

    /// <summary>
    /// 映射的变量名
    /// </summary>
    public string? MappedVariable
    {
        get => _mappedVariable;
        set => SetProperty(ref _mappedVariable, value);
    }

    /// <summary>
    /// 常量值
    /// </summary>
    public string? ConstantValue
    {
        get => _constantValue;
        set => SetProperty(ref _constantValue, value);
    }

    /// <summary>
    /// 是否使用常量
    /// </summary>
    public bool UseConstant
    {
        get => _useConstant;
        set => SetProperty(ref _useConstant, value);
    }

    /// <summary>
    /// 是否必需
    /// </summary>
    public bool IsRequired
    {
        get => _isRequired;
        set => SetProperty(ref _isRequired, value);
    }

    /// <summary>
    /// 描述
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>
    /// 默认值
    /// </summary>
    public string? DefaultValue
    {
        get => _defaultValue;
        set => SetProperty(ref _defaultValue, value);
    }
}
