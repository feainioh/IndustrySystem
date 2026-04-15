using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums;
using Prism;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ExperimentTemplateViewModel : NagetiveCurdVeiwModel<ExperimentTemplateDto>
{
    public const string ParameterEditorRegionName = "ExperimentTemplateParameterRegion";

    private readonly IExperimentTemplateAppService _svc;
    private readonly IExperimentParameterAppService _parameterSvc;
    private readonly IRegionManager _regionManager;

    public ObservableCollection<ExperimentTemplateDto> Templates { get; } = new();

    public ObservableCollection<ExperimentType> ExperimentTypes { get; } =
        Enum.GetValues(typeof(ExperimentType)).Cast<ExperimentType>().ToObservableCollection();

    public ObservableCollection<ExperimentParameterOptionDto> ParameterOptions { get; } = new();

    private ExperimentTemplateDto? _selectedTemplate;
    public ExperimentTemplateDto? SelectedTemplate
    {
        get => _selectedTemplate;
        set
        {
            if (SetProperty(ref _selectedTemplate, value))
            {
                LoadEditor(value);
                RaiseCanExecuteChanged();
            }
        }
    }

    private Guid _editingId;
    public Guid EditingId { get => _editingId; set => SetProperty(ref _editingId, value); }

    private string _templateName = string.Empty;
    public string TemplateName { get => _templateName; set => SetProperty(ref _templateName, value); }

    private ExperimentType _selectedType = ExperimentType.Reaction;
    public ExperimentType SelectedType
    {
        get => _selectedType;
        set
        {
            if (SetProperty(ref _selectedType, value))
            {
                _ = LoadParameterOptionsAndSelectAsync();
                NavigateParameterEditor(value);
            }
        }
    }

    private string _parameterIdText = string.Empty;
    public string ParameterIdText { get => _parameterIdText; set => SetProperty(ref _parameterIdText, value); }

    private ExperimentParameterOptionDto? _selectedParameter;
    public ExperimentParameterOptionDto? SelectedParameter
    {
        get => _selectedParameter;
        set
        {
            if (SetProperty(ref _selectedParameter, value))
            {
                ParameterIdText = value?.Id.ToString() ?? string.Empty;
                // Notify the parameter editor region about the selected parameter
                if (value is not null)
                    _ = LoadParameterIntoEditorAsync(SelectedType, value.Id);
            }
        }
    }

    private bool _isTemplate = true;
    public bool IsTemplate { get => _isTemplate; set => SetProperty(ref _isTemplate, value); }

    private DateTime _createdAt;
    public DateTime CreatedAt { get => _createdAt; set => SetProperty(ref _createdAt, value); }

    private DateTime? _updatedAt;
    public DateTime? UpdatedAt { get => _updatedAt; set => SetProperty(ref _updatedAt, value); }

    /// <summary>当前选中参数的详细数据，供参数编辑区域绑定</summary>
    private ExperimentParameterItemDto? _currentParameterDetail;
    public ExperimentParameterItemDto? CurrentParameterDetail
    {
        get => _currentParameterDetail;
        set => SetProperty(ref _currentParameterDetail, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    /// <summary>
    /// 由 View 的 Loaded 事件调用，确保 region 存在后执行首次导航。
    /// </summary>
    public void NavigateToCurrentParameterEditor()
    {
        NavigateParameterEditor(SelectedType);
    }

    public ExperimentTemplateViewModel(IExperimentTemplateAppService svc, IExperimentParameterAppService parameterSvc, IRegionManager regionManager)
    {
        _svc = svc;
        _parameterSvc = parameterSvc;
        _regionManager = regionManager;

        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        NewCommand = new DelegateCommand(NewTemplate);
        SaveCommand = new DelegateCommand(async () => await SaveAsync());
        DeleteCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await DeleteAsync(id.Value);
        });

        NewTemplate();
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        var list = await _svc.GetListAsync();
        Templates.Clear();
        foreach (var item in list)
            Templates.Add(item);

        if (SelectedTemplate is null && Templates.Count > 0)
            SelectedTemplate = Templates[0];

        // 确保 region 导航在 View 加载后执行（构造函数阶段 region 不存在，此处补偿）
        NavigateParameterEditor(SelectedType);
    }

    /// <summary>
    /// 加载参数选项列表，并根据当前模板的 ParameterId 自动选中对应参数。
    /// 如果没有绑定的参数ID，默认选中第一个。
    /// 如果参数表中没有数据，则自动新建一个默认参数。
    /// </summary>
    private async Task LoadParameterOptionsAndSelectAsync()
    {
        var options = await _parameterSvc.GetOptionsAsync(SelectedType);

        // 如果表中没有数据，自动新建一个默认参数
        if (options.Count == 0)
        {
            var defaultParam = await _parameterSvc.CreateAsync(new ExperimentParameterItemDto
            {
                Id = Guid.NewGuid(),
                Type = SelectedType,
                Name = $"{GetTypeDisplayName(SelectedType)}-默认参数",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
            options = await _parameterSvc.GetOptionsAsync(SelectedType);
        }

        ParameterOptions.Clear();
        foreach (var option in options)
            ParameterOptions.Add(option);

        // 根据绑定的参数ID选中，没有则选第一个
        if (Guid.TryParse(ParameterIdText, out var id))
        {
            var match = ParameterOptions.FirstOrDefault(x => x.Id == id);
            SelectedParameter = match ?? ParameterOptions.FirstOrDefault();
        }
        else
        {
            SelectedParameter = ParameterOptions.FirstOrDefault();
        }
    }

    /// <summary>根据参数ID加载参数详情到编辑器</summary>
    private async Task LoadParameterIntoEditorAsync(ExperimentType type, Guid parameterId)
    {
        // 确保 region 中已导航到对应类型的参数编辑视图，再加载详情
        NavigateParameterEditor(type);
        var detail = await _parameterSvc.GetAsync(type, parameterId);
        CurrentParameterDetail = detail;
    }

    private void NewTemplate()
    {
        EditingId = Guid.Empty;
        TemplateName = string.Empty;
        SelectedType = ExperimentType.Reaction;
        ParameterIdText = string.Empty;
        SelectedParameter = null;
        IsTemplate = true;
        CreatedAt = DateTime.Now;
        UpdatedAt = null;
        SelectedTemplate = null;
        _ = LoadParameterOptionsAndSelectAsync();
        NavigateParameterEditor(SelectedType);
    }

    private void LoadEditor(ExperimentTemplateDto? dto)
    {
        if (dto is null) return;

        EditingId = dto.Id;
        TemplateName = dto.Name;

        // 先设置 ParameterIdText
        ParameterIdText = dto.ParameterId?.ToString() ?? string.Empty;
        IsTemplate = dto.IsTemplate;
        CreatedAt = dto.CreatedAt;
        UpdatedAt = dto.UpdatedAt;

        // 直接设置 backing field 并通知 UI，避免 setter 中条件触发导致同类型模板切换时不导航
        SetProperty(ref _selectedType, dto.Type, nameof(SelectedType));

        // 始终重新加载参数列表和导航参数编辑器
        _ = LoadParameterOptionsAndSelectAsync();
        NavigateParameterEditor(dto.Type);
    }

    private void NavigateParameterEditor(ExperimentType type)
    {
        var target = type switch
        {
            ExperimentType.Reaction => "ReactionParameterEditDialog",
            ExperimentType.RotaryEvaporation => "RotaryEvaporationParameterEditDialog",
            ExperimentType.Detection => "DetectionParameterEditDialog",
            ExperimentType.Filtration => "FiltrationParameterEditDialog",
            ExperimentType.Drying => "DryingParameterEditDialog",
            ExperimentType.Quenching => "QuenchingParameterEditDialog",
            ExperimentType.Extraction => "ExtractionParameterEditDialog",
            ExperimentType.Sampling => "SamplingParameterEditDialog",
            ExperimentType.Centrifugation => "CentrifugationParameterEditDialog",
            ExperimentType.CustomDetection => "CustomDetectionParameterEditDialog",
            _ => "ReactionParameterEditDialog"
        };

        // 仅在 region 已由 View 创建后才导航（构造函数阶段 region 尚未注册）
        if (_regionManager.Regions.ContainsRegionWithName(ParameterEditorRegionName))
        {
            var navParams = new NavigationParameters
            {
                { "ExperimentType", type },
                { "ParentVm", this }
            };
            _regionManager.RequestNavigate(ParameterEditorRegionName, target, navParams);
        }
    }

    private async Task SaveAsync()
    {
        Guid? parameterId = null;
        if (SelectedParameter is not null)
            parameterId = SelectedParameter.Id;
        else if (!string.IsNullOrWhiteSpace(ParameterIdText) && Guid.TryParse(ParameterIdText.Trim(), out var parsed))
            parameterId = parsed;

        var input = new ExperimentTemplateDto(
            EditingId,
            TemplateName,
            SelectedType,
            parameterId,
            true,
            EditingId == Guid.Empty ? DateTime.Now : CreatedAt,
            DateTime.Now);

        var saved = EditingId == Guid.Empty
            ? await _svc.CreateAsync(input)
            : await _svc.UpdateAsync(input);

        await LoadAsync();
        SelectedTemplate = Templates.FirstOrDefault(x => x.Id == saved.Id);

        MessageBox.Show("保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task DeleteAsync(Guid id)
    {
        var r = MessageBox.Show("确认删除该实验模板？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (r != MessageBoxResult.Yes) return;

        await _svc.DeleteAsync(id);

        var target = Templates.FirstOrDefault(x => x.Id == id);
        if (target is not null) Templates.Remove(target);

        if (SelectedTemplate?.Id == id)
            NewTemplate();
    }

    private void RaiseCanExecuteChanged()
    {
        if (DeleteCommand is DelegateCommand<Guid?> dc)
            dc.RaiseCanExecuteChanged();
    }

    private static string GetTypeDisplayName(ExperimentType type) => type switch
    {
        ExperimentType.Reaction => "反应",
        ExperimentType.RotaryEvaporation => "旋蒸",
        ExperimentType.Detection => "检测",
        ExperimentType.Filtration => "过滤",
        ExperimentType.Drying => "干燥",
        ExperimentType.Quenching => "淬灭",
        ExperimentType.Extraction => "萃取",
        ExperimentType.Sampling => "取样",
        ExperimentType.Centrifugation => "离心",
        ExperimentType.CustomDetection => "自定义检测",
        _ => type.ToString()
    };

    protected override async Task<IReadOnlyList<ExperimentTemplateDto>> LoadItemsAsync()
        => await _svc.GetListAsync();
}

static class CollectionExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        => new(source);
}
