using System.Collections.ObjectModel;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.Models;

/// <summary>
/// 运动程序项目 - 顶层结构
/// 包含设备配置和多个子项目
/// </summary>
public class MotionProject : BindableBase
{
    private Guid _id = Guid.NewGuid();
    private string _name = "新项目";
    private string _description = string.Empty;
    private string _version = "1.0.0";
    private DateTime _createdTime = DateTime.Now;
    private DateTime _modifiedTime = DateTime.Now;
    private string _author = string.Empty;
    private string _filePath = string.Empty;
    private bool _isModified;

    /// <summary>
    /// 项目唯一标识
    /// </summary>
    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>
    /// 项目名称
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
                IsModified = true;
        }
    }

    /// <summary>
    /// 项目描述
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value))
                IsModified = true;
        }
    }

    /// <summary>
    /// 项目版本
    /// </summary>
    public string Version
    {
        get => _version;
        set
        {
            if (SetProperty(ref _version, value))
                IsModified = true;
        }
    }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime
    {
        get => _createdTime;
        set => SetProperty(ref _createdTime, value);
    }

    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime ModifiedTime
    {
        get => _modifiedTime;
        set => SetProperty(ref _modifiedTime, value);
    }

    /// <summary>
    /// 作者
    /// </summary>
    public string Author
    {
        get => _author;
        set
        {
            if (SetProperty(ref _author, value))
                IsModified = true;
        }
    }

    /// <summary>
    /// 项目文件路径
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    /// <summary>
    /// 是否已修改
    /// </summary>
    public bool IsModified
    {
        get => _isModified;
        set
        {
            if (SetProperty(ref _isModified, value) && value)
                ModifiedTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 设备配置文件路径（相对于项目文件）
    /// </summary>
    public string DeviceConfigPath { get; set; } = "deviceconfig.json";

    /// <summary>
    /// 设备配置（运行时加载）
    /// </summary>
    public Services.DeviceConfigDto? DeviceConfig { get; set; }

    /// <summary>
    /// 子项目集合
    /// </summary>
    public ObservableCollection<SubProject> SubProjects { get; } = [];

    /// <summary>
    /// 全局变量
    /// </summary>
    public Dictionary<string, object> GlobalVariables { get; set; } = [];
}

/// <summary>
/// 子项目 - 包含多个子程序
/// </summary>
public class SubProject : BindableBase
{
    private Guid _id = Guid.NewGuid();
    private string _name = "新子项目";
    private string _description = string.Empty;
    private bool _isExpanded = true;
    private bool _isSelected;
    private int _order;

    /// <summary>
    /// 子项目唯一标识
    /// </summary>
    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>
    /// 子项目名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// 子项目描述
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>
    /// 是否展开（用于树视图）
    /// </summary>
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    /// <summary>
    /// 是否选中
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }

    /// <summary>
    /// 子程序集合
    /// </summary>
    public ObservableCollection<SubProgram> SubPrograms { get; } = [];

    /// <summary>
    /// 子项目变量
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = [];
}

/// <summary>
/// 子程序 - 对应一个设计器窗口
/// </summary>
public class SubProgram : BindableBase
{
    private Guid _id = Guid.NewGuid();
    private string _name = "新子程序";
    private string _description = string.Empty;
    private bool _isSelected;
    private bool _isModified;
    private bool _isOpen;
    private int _order;
    private SubProgramType _programType = SubProgramType.Normal;

    /// <summary>
    /// 子程序唯一标识
    /// </summary>
    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    /// <summary>
    /// 子程序名称
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// 子程序描述
    /// </summary>
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>
    /// 是否选中
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// 是否已修改
    /// </summary>
    public bool IsModified
    {
        get => _isModified;
        set => SetProperty(ref _isModified, value);
    }

    /// <summary>
    /// 是否已打开（在设计器中）
    /// </summary>
    public bool IsOpen
    {
        get => _isOpen;
        set => SetProperty(ref _isOpen, value);
    }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int Order
    {
        get => _order;
        set => SetProperty(ref _order, value);
    }

    /// <summary>
    /// 程序类型
    /// </summary>
    public SubProgramType ProgramType
    {
        get => _programType;
        set => SetProperty(ref _programType, value);
    }

    /// <summary>
    /// 节点集合
    /// </summary>
    public ObservableCollection<ViewModels.ActionNodeViewModel> Nodes { get; } = [];

    /// <summary>
    /// 连接集合
    /// </summary>
    public ObservableCollection<ViewModels.ConnectionViewModel> Connections { get; } = [];

    /// <summary>
    /// 子程序局部变量
    /// </summary>
    public Dictionary<string, object> LocalVariables { get; set; } = [];

    /// <summary>
    /// 输入参数定义
    /// </summary>
    public List<ProgramParameter> InputParameters { get; set; } = [];

    /// <summary>
    /// 输出参数定义
    /// </summary>
    public List<ProgramParameter> OutputParameters { get; set; } = [];
}

/// <summary>
/// 子程序类型
/// </summary>
public enum SubProgramType
{
    /// <summary>
    /// 普通程序
    /// </summary>
    Normal,

    /// <summary>
    /// 主程序（入口点）
    /// </summary>
    Main,

    /// <summary>
    /// 初始化程序
    /// </summary>
    Initialize,

    /// <summary>
    /// 错误处理程序
    /// </summary>
    ErrorHandler,

    /// <summary>
    /// 清理程序
    /// </summary>
    Cleanup
}

/// <summary>
/// 程序参数定义
/// </summary>
public class ProgramParameter
{
    /// <summary>
    /// 参数名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 参数类型
    /// </summary>
    public string DataType { get; set; } = "string";

    /// <summary>
    /// 默认值
    /// </summary>
    public object? DefaultValue { get; set; }

    /// <summary>
    /// 参数描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 是否必需
    /// </summary>
    public bool IsRequired { get; set; } = true;
}
