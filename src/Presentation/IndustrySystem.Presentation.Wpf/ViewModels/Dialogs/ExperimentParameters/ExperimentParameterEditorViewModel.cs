using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ExperimentParameterEditorViewModel :NagetiveViewModel
{
    private readonly IExperimentParameterAppService _svc;
    private ExperimentTemplateViewModel? _parentVm;

    public ExperimentType Type { get; private set; }

    private string _typeName = string.Empty;
    public string TypeName { get => _typeName; private set => SetProperty(ref _typeName, value); }

    public ObservableCollection<ExperimentParameterItemDto> Items { get; } = new();

    private ExperimentParameterItemDto? _selectedItem;
    public ExperimentParameterItemDto? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                if (value is not null) LoadEditor(value);
            }
        }
    }

    // --- Common ---
    private Guid _id;
    public Guid Id { get => _id; set => SetProperty(ref _id, value); }

    private string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    // --- Reaction ---
    private string _rawMaterial = string.Empty;
    public string RawMaterial { get => _rawMaterial; set => SetProperty(ref _rawMaterial, value); }

    private string _stockSolution = string.Empty;
    public string StockSolution { get => _stockSolution; set => SetProperty(ref _stockSolution, value); }

    private decimal _temperatureC;
    public decimal TemperatureC { get => _temperatureC; set => SetProperty(ref _temperatureC, value); }

    private decimal _pressureKpa;
    public decimal PressureKpa { get => _pressureKpa; set => SetProperty(ref _pressureKpa, value); }

    private int _durationMinutes;
    public int DurationMinutes { get => _durationMinutes; set => SetProperty(ref _durationMinutes, value); }

    private int _stirSpeedRpm;
    public int StirSpeedRpm { get => _stirSpeedRpm; set => SetProperty(ref _stirSpeedRpm, value); }

    private decimal _liquidAddSpeedMlMin;
    public decimal LiquidAddSpeedMlMin { get => _liquidAddSpeedMlMin; set => SetProperty(ref _liquidAddSpeedMlMin, value); }

    private decimal _powderAddSpeedGMin;
    public decimal PowderAddSpeedGMin { get => _powderAddSpeedGMin; set => SetProperty(ref _powderAddSpeedGMin, value); }

    // --- Shared: Detergent ---
    private string _detergent = string.Empty;
    public string Detergent { get => _detergent; set => SetProperty(ref _detergent, value); }

    private decimal _detergentVolumeMl;
    public decimal DetergentVolumeMl { get => _detergentVolumeMl; set => SetProperty(ref _detergentVolumeMl, value); }

    // --- RotaryEvaporation ---
    private decimal _bathTemperatureC;
    public decimal BathTemperatureC { get => _bathTemperatureC; set => SetProperty(ref _bathTemperatureC, value); }

    private decimal _vaporTemperatureC;
    public decimal VaporTemperatureC { get => _vaporTemperatureC; set => SetProperty(ref _vaporTemperatureC, value); }

    private decimal _vacuumKpa;
    public decimal VacuumKpa { get => _vacuumKpa; set => SetProperty(ref _vacuumKpa, value); }

    private int _rotationRpm;
    public int RotationRpm { get => _rotationRpm; set => SetProperty(ref _rotationRpm, value); }

    private decimal _liftStrokeMm;
    public decimal LiftStrokeMm { get => _liftStrokeMm; set => SetProperty(ref _liftStrokeMm, value); }

    private decimal _coolantTemperatureC;
    public decimal CoolantTemperatureC { get => _coolantTemperatureC; set => SetProperty(ref _coolantTemperatureC, value); }

    private bool _collectCondensate;
    public bool CollectCondensate { get => _collectCondensate; set => SetProperty(ref _collectCondensate, value); }

    private bool _continuousFeed;
    public bool ContinuousFeed { get => _continuousFeed; set => SetProperty(ref _continuousFeed, value); }

    // --- Detection ---
    private string _method = string.Empty;
    public string Method { get => _method; set => SetProperty(ref _method, value); }

    private int _wavelengthNm;
    public int WavelengthNm { get => _wavelengthNm; set => SetProperty(ref _wavelengthNm, value); }

    private string _notes = string.Empty;
    public string Notes { get => _notes; set => SetProperty(ref _notes, value); }

    private string _parameterJson = string.Empty;
    public string ParameterJson { get => _parameterJson; set => SetProperty(ref _parameterJson, value); }

    // --- Drying ---
    private Guid? _desiccantId;
    public Guid? DesiccantId { get => _desiccantId; set => SetProperty(ref _desiccantId, value); }

    private decimal _desiccantVolumeMl;
    public decimal DesiccantVolumeMl { get => _desiccantVolumeMl; set => SetProperty(ref _desiccantVolumeMl, value); }

    private int _shakeSpeedRpm;
    public int ShakeSpeedRpm { get => _shakeSpeedRpm; set => SetProperty(ref _shakeSpeedRpm, value); }

    private int _shakeDurationMinutes;
    public int ShakeDurationMinutes { get => _shakeDurationMinutes; set => SetProperty(ref _shakeDurationMinutes, value); }

    // --- Quenching ---
    private string _quenchingAgent = string.Empty;
    public string QuenchingAgent { get => _quenchingAgent; set => SetProperty(ref _quenchingAgent, value); }

    private decimal _quenchingAgentVolumeMl;
    public decimal QuenchingAgentVolumeMl { get => _quenchingAgentVolumeMl; set => SetProperty(ref _quenchingAgentVolumeMl, value); }

    private decimal _quenchingAgentDripSpeedMlMin;
    public decimal QuenchingAgentDripSpeedMlMin { get => _quenchingAgentDripSpeedMlMin; set => SetProperty(ref _quenchingAgentDripSpeedMlMin, value); }

    private bool _addQuenchingAgentFirst;
    public bool AddQuenchingAgentFirst { get => _addQuenchingAgentFirst; set => SetProperty(ref _addQuenchingAgentFirst, value); }

    private decimal _preTemperatureC;
    public decimal PreTemperatureC { get => _preTemperatureC; set => SetProperty(ref _preTemperatureC, value); }

    private decimal _maxTemperatureC;
    public decimal MaxTemperatureC { get => _maxTemperatureC; set => SetProperty(ref _maxTemperatureC, value); }

    private decimal _totalProductVolumeMl;
    public decimal TotalProductVolumeMl { get => _totalProductVolumeMl; set => SetProperty(ref _totalProductVolumeMl, value); }

    // --- Extraction ---
    private string _extractAgent = string.Empty;
    public string ExtractAgent { get => _extractAgent; set => SetProperty(ref _extractAgent, value); }

    private decimal _extractAgentVolumeMl;
    public decimal ExtractAgentVolumeMl { get => _extractAgentVolumeMl; set => SetProperty(ref _extractAgentVolumeMl, value); }

    private int _stirDurationMinutes;
    public int StirDurationMinutes { get => _stirDurationMinutes; set => SetProperty(ref _stirDurationMinutes, value); }

    private int _settlingMinutes;
    public int SettlingMinutes { get => _settlingMinutes; set => SetProperty(ref _settlingMinutes, value); }

    // --- Sampling ---
    private decimal _sampleVolumeMl;
    public decimal SampleVolumeMl { get => _sampleVolumeMl; set => SetProperty(ref _sampleVolumeMl, value); }

    // --- Centrifugation ---
    private int _speedRpm;
    public int SpeedRpm { get => _speedRpm; set => SetProperty(ref _speedRpm, value); }

    // --- Commands ---
    public ICommand RefreshCommand { get; }
    public ICommand NewCommand { get; }
    public new ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    public ExperimentParameterEditorViewModel(IExperimentParameterAppService svc)
    {
        _svc = svc;

        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        NewCommand = new DelegateCommand(NewItem);
        SaveCommand = new DelegateCommand(async () => await SaveAsync());
        DeleteCommand = new DelegateCommand(async () => await DeleteAsync());
    }

    /// <summary>Prism region 导航到此 VM 时调用，接收实验类型和父 VM</summary>
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.TryGetValue<ExperimentType>("ExperimentType", out var type))
        {
            Type = type;
            TypeName = GetTypeName(type);
        }

        // 取消旧的父 VM 订阅
        if (_parentVm is not null)
            _parentVm.PropertyChanged -= OnParentPropertyChanged;

        NewItem();
        _ = LoadAsync();

        if (navigationContext.Parameters.TryGetValue<ExperimentTemplateViewModel>("ParentVm", out var parentVm))
        {
            _parentVm = parentVm;
            _parentVm.PropertyChanged += OnParentPropertyChanged;

            // 如果父VM已经有选中的参数详情，立即加载
            if (_parentVm.CurrentParameterDetail is { } detail && detail.Type == Type)
                LoadParameterDetail(detail);
        }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        if (_parentVm is not null)
            _parentVm.PropertyChanged -= OnParentPropertyChanged;
    }

    private void OnParentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ExperimentTemplateViewModel.CurrentParameterDetail))
        {
            var detail = _parentVm?.CurrentParameterDetail;
            if (detail is not null && detail.Type == Type)
                LoadParameterDetail(detail);
        }
    }

    public async Task LoadAsync()
    {
        var list = await _svc.GetListAsync(Type);
        Items.Clear();
        foreach (var item in list) Items.Add(item);
    }

    /// <summary>
    /// 外部调用：加载参数列表并选中指定ID的参数。
    /// 如果找不到匹配项则选第一个。
    /// </summary>
    public async Task LoadAndSelectAsync(Guid? parameterId)
    {
        await LoadAsync();
        if (parameterId.HasValue)
        {
            var match = Items.FirstOrDefault(x => x.Id == parameterId.Value);
            SelectedItem = match ?? Items.FirstOrDefault();
        }
        else
        {
            SelectedItem = Items.FirstOrDefault();
        }
    }

    /// <summary>外部调用：直接加载一个参数DTO到编辑器</summary>
    public void LoadParameterDetail(ExperimentParameterItemDto? dto)
    {
        if (dto is null) return;
        // 确保Items列表里有此项
        if (!Items.Any(x => x.Id == dto.Id))
        {
            Items.Add(dto);
        }
        SelectedItem = Items.FirstOrDefault(x => x.Id == dto.Id);
    }

    private void NewItem()
    {
        Id = Guid.Empty;
        Name = string.Empty;
        // Reaction
        RawMaterial = string.Empty; StockSolution = string.Empty; TemperatureC = 0; PressureKpa = 0;
        DurationMinutes = 0; StirSpeedRpm = 0; LiquidAddSpeedMlMin = 0; PowderAddSpeedGMin = 0;
        // Shared
        Detergent = string.Empty; DetergentVolumeMl = 0;
        // RotaryEvaporation
        BathTemperatureC = 0; VaporTemperatureC = 0; VacuumKpa = 0; RotationRpm = 0;
        LiftStrokeMm = 0; CoolantTemperatureC = 0; CollectCondensate = false; ContinuousFeed = false;
        // Detection
        Method = string.Empty; WavelengthNm = 0; Notes = string.Empty; ParameterJson = string.Empty;
        // Drying
        DesiccantId = Guid.NewGuid(); DesiccantVolumeMl = 0; ShakeSpeedRpm = 0; ShakeDurationMinutes = 0;
        // Quenching
        QuenchingAgent = string.Empty; QuenchingAgentVolumeMl = 0; QuenchingAgentDripSpeedMlMin = 0;
        AddQuenchingAgentFirst = false; PreTemperatureC = 0; MaxTemperatureC = 0; TotalProductVolumeMl = 0;
        // Extraction
        ExtractAgent = string.Empty; ExtractAgentVolumeMl = 0; StirDurationMinutes = 0; SettlingMinutes = 0;
        // Sampling
        SampleVolumeMl = 0;
        // Centrifugation
        SpeedRpm = 0;
        SelectedItem = null;
    }

    private void LoadEditor(ExperimentParameterItemDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        // Reaction
        RawMaterial = dto.RawMaterial; StockSolution = dto.StockSolution;
        TemperatureC = dto.TemperatureC; PressureKpa = dto.PressureKpa;
        DurationMinutes = dto.DurationMinutes; StirSpeedRpm = dto.StirSpeedRpm;
        LiquidAddSpeedMlMin = dto.LiquidAddSpeedMlMin; PowderAddSpeedGMin = dto.PowderAddSpeedGMin;
        // Shared
        Detergent = dto.Detergent; DetergentVolumeMl = dto.DetergentVolumeMl;
        // RotaryEvaporation
        BathTemperatureC = dto.BathTemperatureC; VaporTemperatureC = dto.VaporTemperatureC;
        VacuumKpa = dto.VacuumKpa; RotationRpm = dto.RotationRpm;
        LiftStrokeMm = dto.LiftStrokeMm; CoolantTemperatureC = dto.CoolantTemperatureC;
        CollectCondensate = dto.CollectCondensate; ContinuousFeed = dto.ContinuousFeed ;
        // Detection
        Method = dto.Method; WavelengthNm = dto.WavelengthNm; Notes = dto.Notes; ParameterJson = dto.ParameterJson;
        // Drying
        DesiccantId = dto.DesiccantId; DesiccantVolumeMl = dto.DesiccantVolumeMl;
        ShakeSpeedRpm = dto.ShakeSpeedRpm; ShakeDurationMinutes = dto.ShakeDurationMinutes;
        // Quenching
        QuenchingAgent = dto.QuenchingAgent; QuenchingAgentVolumeMl = dto.QuenchingAgentVolumeMl;
        QuenchingAgentDripSpeedMlMin = dto.QuenchingAgentDripSpeedMlMin;
        AddQuenchingAgentFirst = dto.AddQuenchingAgentFirst ;
        PreTemperatureC = dto.PreTemperatureC; MaxTemperatureC = dto.MaxTemperatureC;
        TotalProductVolumeMl = dto.TotalProductVolumeMl;
        // Extraction
        ExtractAgent = dto.ExtractAgent; ExtractAgentVolumeMl = dto.ExtractAgentVolumeMl;
        StirDurationMinutes = dto.StirDurationMinutes; SettlingMinutes = dto.SettlingMinutes;
        // Sampling
        SampleVolumeMl = dto.SampleVolumeMl;
        // Centrifugation
        SpeedRpm = dto.SpeedRpm;
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            MessageBox.Show("参数名称不能为空", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dto = new ExperimentParameterItemDto
        {
            Id = Id,
            Type = Type,
            Name = Name.Trim(),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            // Reaction
            RawMaterial = RawMaterial,
            StockSolution = StockSolution,
            TemperatureC = TemperatureC,
            PressureKpa = PressureKpa,
            DurationMinutes = DurationMinutes,
            StirSpeedRpm = StirSpeedRpm,
            LiquidAddSpeedMlMin = LiquidAddSpeedMlMin,
            PowderAddSpeedGMin = PowderAddSpeedGMin,
            // Shared
            Detergent = Detergent,
            DetergentVolumeMl = DetergentVolumeMl,
            // RotaryEvaporation
            BathTemperatureC = BathTemperatureC,
            VaporTemperatureC = VaporTemperatureC,
            VacuumKpa = VacuumKpa,
            RotationRpm = RotationRpm,
            LiftStrokeMm = LiftStrokeMm,
            CoolantTemperatureC = CoolantTemperatureC,
            CollectCondensate = CollectCondensate,
            ContinuousFeed = ContinuousFeed,
            // Detection
            Method = Method,
            WavelengthNm = WavelengthNm,
            Notes = Notes,
            ParameterJson = ParameterJson,
            // Drying
            DesiccantId = DesiccantId,
            DesiccantVolumeMl = DesiccantVolumeMl,
            ShakeSpeedRpm = ShakeSpeedRpm,
            ShakeDurationMinutes = ShakeDurationMinutes,
            // Quenching
            QuenchingAgent = QuenchingAgent,
            QuenchingAgentVolumeMl = QuenchingAgentVolumeMl,
            QuenchingAgentDripSpeedMlMin = QuenchingAgentDripSpeedMlMin,
            AddQuenchingAgentFirst = AddQuenchingAgentFirst,
            PreTemperatureC = PreTemperatureC,
            MaxTemperatureC = MaxTemperatureC,
            TotalProductVolumeMl = TotalProductVolumeMl,
            // Extraction
            ExtractAgent = ExtractAgent,
            ExtractAgentVolumeMl = ExtractAgentVolumeMl,
            StirDurationMinutes = StirDurationMinutes,
            SettlingMinutes = SettlingMinutes,
            // Sampling
            SampleVolumeMl = SampleVolumeMl,
            // Centrifugation
            SpeedRpm = SpeedRpm,
        };

        var saved = Id == Guid.Empty
            ? await _svc.CreateAsync(dto)
            : await _svc.UpdateAsync(dto);

        await LoadAsync();
        SelectedItem = Items.FirstOrDefault(x => x.Id == saved.Id);
    }

    private async Task DeleteAsync()
    {
        if (Id == Guid.Empty) return;

        var r = MessageBox.Show("确认删除参数？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (r != MessageBoxResult.Yes) return;

        await _svc.DeleteAsync(Type, Id);
        await LoadAsync();
        NewItem();
    }

    private static string GetTypeName(ExperimentType type) => type switch
    {
        ExperimentType.Reaction => "反应实验参数",
        ExperimentType.RotaryEvaporation => "旋蒸参数",
        ExperimentType.Detection => "检测参数",
        ExperimentType.Filtration => "过滤参数",
        ExperimentType.Drying => "干燥参数",
        ExperimentType.Quenching => "淬灭参数",
        ExperimentType.Extraction => "萃取参数",
        ExperimentType.Sampling => "取样参数",
        ExperimentType.Centrifugation => "离心参数",
        ExperimentType.CustomDetection => "自定义检测参数",
        _ => type.ToString()
    };
}
