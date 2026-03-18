using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ExperimentParameterEditorViewModel : BindableBase
{
    private readonly IExperimentParameterAppService _svc;

    public ExperimentType Type { get; }
    public string TypeName { get; }

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
    private string? _rawMaterial;
    public string? RawMaterial { get => _rawMaterial; set => SetProperty(ref _rawMaterial, value); }

    private string? _stockSolution;
    public string? StockSolution { get => _stockSolution; set => SetProperty(ref _stockSolution, value); }

    private decimal? _temperatureC;
    public decimal? TemperatureC { get => _temperatureC; set => SetProperty(ref _temperatureC, value); }

    private decimal? _pressureKpa;
    public decimal? PressureKpa { get => _pressureKpa; set => SetProperty(ref _pressureKpa, value); }

    private int? _durationMinutes;
    public int? DurationMinutes { get => _durationMinutes; set => SetProperty(ref _durationMinutes, value); }

    private int? _stirSpeedRpm;
    public int? StirSpeedRpm { get => _stirSpeedRpm; set => SetProperty(ref _stirSpeedRpm, value); }

    private decimal? _liquidAddSpeedMlMin;
    public decimal? LiquidAddSpeedMlMin { get => _liquidAddSpeedMlMin; set => SetProperty(ref _liquidAddSpeedMlMin, value); }

    private decimal? _powderAddSpeedGMin;
    public decimal? PowderAddSpeedGMin { get => _powderAddSpeedGMin; set => SetProperty(ref _powderAddSpeedGMin, value); }

    // --- Shared: Detergent ---
    private string? _detergent;
    public string? Detergent { get => _detergent; set => SetProperty(ref _detergent, value); }

    private decimal? _detergentVolumeMl;
    public decimal? DetergentVolumeMl { get => _detergentVolumeMl; set => SetProperty(ref _detergentVolumeMl, value); }

    // --- RotaryEvaporation ---
    private decimal? _bathTemperatureC;
    public decimal? BathTemperatureC { get => _bathTemperatureC; set => SetProperty(ref _bathTemperatureC, value); }

    private decimal? _vaporTemperatureC;
    public decimal? VaporTemperatureC { get => _vaporTemperatureC; set => SetProperty(ref _vaporTemperatureC, value); }

    private decimal? _vacuumKpa;
    public decimal? VacuumKpa { get => _vacuumKpa; set => SetProperty(ref _vacuumKpa, value); }

    private int? _rotationRpm;
    public int? RotationRpm { get => _rotationRpm; set => SetProperty(ref _rotationRpm, value); }

    private decimal? _liftStrokeMm;
    public decimal? LiftStrokeMm { get => _liftStrokeMm; set => SetProperty(ref _liftStrokeMm, value); }

    private decimal? _coolantTemperatureC;
    public decimal? CoolantTemperatureC { get => _coolantTemperatureC; set => SetProperty(ref _coolantTemperatureC, value); }

    private bool _collectCondensate;
    public bool CollectCondensate { get => _collectCondensate; set => SetProperty(ref _collectCondensate, value); }

    private bool _continuousFeed;
    public bool ContinuousFeed { get => _continuousFeed; set => SetProperty(ref _continuousFeed, value); }

    // --- Detection ---
    private string? _method;
    public string? Method { get => _method; set => SetProperty(ref _method, value); }

    private int? _wavelengthNm;
    public int? WavelengthNm { get => _wavelengthNm; set => SetProperty(ref _wavelengthNm, value); }

    private string? _notes;
    public string? Notes { get => _notes; set => SetProperty(ref _notes, value); }

    private string? _parameterJson;
    public string? ParameterJson { get => _parameterJson; set => SetProperty(ref _parameterJson, value); }

    // --- Drying ---
    private Guid? _desiccantId;
    public Guid? DesiccantId { get => _desiccantId; set => SetProperty(ref _desiccantId, value); }

    private decimal? _desiccantVolumeMl;
    public decimal? DesiccantVolumeMl { get => _desiccantVolumeMl; set => SetProperty(ref _desiccantVolumeMl, value); }

    private int? _shakeSpeedRpm;
    public int? ShakeSpeedRpm { get => _shakeSpeedRpm; set => SetProperty(ref _shakeSpeedRpm, value); }

    private int? _shakeDurationMinutes;
    public int? ShakeDurationMinutes { get => _shakeDurationMinutes; set => SetProperty(ref _shakeDurationMinutes, value); }

    // --- Quenching ---
    private string? _quenchingAgent;
    public string? QuenchingAgent { get => _quenchingAgent; set => SetProperty(ref _quenchingAgent, value); }

    private decimal? _quenchingAgentVolumeMl;
    public decimal? QuenchingAgentVolumeMl { get => _quenchingAgentVolumeMl; set => SetProperty(ref _quenchingAgentVolumeMl, value); }

    private decimal? _quenchingAgentDripSpeedMlMin;
    public decimal? QuenchingAgentDripSpeedMlMin { get => _quenchingAgentDripSpeedMlMin; set => SetProperty(ref _quenchingAgentDripSpeedMlMin, value); }

    private bool _addQuenchingAgentFirst;
    public bool AddQuenchingAgentFirst { get => _addQuenchingAgentFirst; set => SetProperty(ref _addQuenchingAgentFirst, value); }

    private decimal? _preTemperatureC;
    public decimal? PreTemperatureC { get => _preTemperatureC; set => SetProperty(ref _preTemperatureC, value); }

    private decimal? _maxTemperatureC;
    public decimal? MaxTemperatureC { get => _maxTemperatureC; set => SetProperty(ref _maxTemperatureC, value); }

    private decimal? _totalProductVolumeMl;
    public decimal? TotalProductVolumeMl { get => _totalProductVolumeMl; set => SetProperty(ref _totalProductVolumeMl, value); }

    // --- Extraction ---
    private string? _extractAgent;
    public string? ExtractAgent { get => _extractAgent; set => SetProperty(ref _extractAgent, value); }

    private decimal? _extractAgentVolumeMl;
    public decimal? ExtractAgentVolumeMl { get => _extractAgentVolumeMl; set => SetProperty(ref _extractAgentVolumeMl, value); }

    private int? _stirDurationMinutes;
    public int? StirDurationMinutes { get => _stirDurationMinutes; set => SetProperty(ref _stirDurationMinutes, value); }

    private int? _settlingMinutes;
    public int? SettlingMinutes { get => _settlingMinutes; set => SetProperty(ref _settlingMinutes, value); }

    // --- Sampling ---
    private decimal? _sampleVolumeMl;
    public decimal? SampleVolumeMl { get => _sampleVolumeMl; set => SetProperty(ref _sampleVolumeMl, value); }

    // --- Centrifugation ---
    private int? _speedRpm;
    public int? SpeedRpm { get => _speedRpm; set => SetProperty(ref _speedRpm, value); }

    // --- Commands ---
    public ICommand RefreshCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    public ExperimentParameterEditorViewModel(IExperimentParameterAppService svc, ExperimentType type)
    {
        _svc = svc;
        Type = type;
        TypeName = GetTypeName(type);

        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        NewCommand = new DelegateCommand(NewItem);
        SaveCommand = new DelegateCommand(async () => await SaveAsync());
        DeleteCommand = new DelegateCommand(async () => await DeleteAsync());

        NewItem();
        _ = LoadAsync();
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
        RawMaterial = null; StockSolution = null; TemperatureC = null; PressureKpa = null;
        DurationMinutes = null; StirSpeedRpm = null; LiquidAddSpeedMlMin = null; PowderAddSpeedGMin = null;
        // Shared
        Detergent = null; DetergentVolumeMl = null;
        // RotaryEvaporation
        BathTemperatureC = null; VaporTemperatureC = null; VacuumKpa = null; RotationRpm = null;
        LiftStrokeMm = null; CoolantTemperatureC = null; CollectCondensate = false; ContinuousFeed = false;
        // Detection
        Method = null; WavelengthNm = null; Notes = null; ParameterJson = null;
        // Drying
        DesiccantId = null; DesiccantVolumeMl = null; ShakeSpeedRpm = null; ShakeDurationMinutes = null;
        // Quenching
        QuenchingAgent = null; QuenchingAgentVolumeMl = null; QuenchingAgentDripSpeedMlMin = null;
        AddQuenchingAgentFirst = false; PreTemperatureC = null; MaxTemperatureC = null; TotalProductVolumeMl = null;
        // Extraction
        ExtractAgent = null; ExtractAgentVolumeMl = null; StirDurationMinutes = null; SettlingMinutes = null;
        // Sampling
        SampleVolumeMl = null;
        // Centrifugation
        SpeedRpm = null;
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
        CollectCondensate = dto.CollectCondensate ?? false; ContinuousFeed = dto.ContinuousFeed ?? false;
        // Detection
        Method = dto.Method; WavelengthNm = dto.WavelengthNm; Notes = dto.Notes; ParameterJson = dto.ParameterJson;
        // Drying
        DesiccantId = dto.DesiccantId; DesiccantVolumeMl = dto.DesiccantVolumeMl;
        ShakeSpeedRpm = dto.ShakeSpeedRpm; ShakeDurationMinutes = dto.ShakeDurationMinutes;
        // Quenching
        QuenchingAgent = dto.QuenchingAgent; QuenchingAgentVolumeMl = dto.QuenchingAgentVolumeMl;
        QuenchingAgentDripSpeedMlMin = dto.QuenchingAgentDripSpeedMlMin;
        AddQuenchingAgentFirst = dto.AddQuenchingAgentFirst ?? false;
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
            Id = Id, Type = Type, Name = Name.Trim(),
            CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now,
            // Reaction
            RawMaterial = RawMaterial, StockSolution = StockSolution,
            TemperatureC = TemperatureC, PressureKpa = PressureKpa,
            DurationMinutes = DurationMinutes, StirSpeedRpm = StirSpeedRpm,
            LiquidAddSpeedMlMin = LiquidAddSpeedMlMin, PowderAddSpeedGMin = PowderAddSpeedGMin,
            // Shared
            Detergent = Detergent, DetergentVolumeMl = DetergentVolumeMl,
            // RotaryEvaporation
            BathTemperatureC = BathTemperatureC, VaporTemperatureC = VaporTemperatureC,
            VacuumKpa = VacuumKpa, RotationRpm = RotationRpm,
            LiftStrokeMm = LiftStrokeMm, CoolantTemperatureC = CoolantTemperatureC,
            CollectCondensate = CollectCondensate, ContinuousFeed = ContinuousFeed,
            // Detection
            Method = Method, WavelengthNm = WavelengthNm, Notes = Notes, ParameterJson = ParameterJson,
            // Drying
            DesiccantId = DesiccantId, DesiccantVolumeMl = DesiccantVolumeMl,
            ShakeSpeedRpm = ShakeSpeedRpm, ShakeDurationMinutes = ShakeDurationMinutes,
            // Quenching
            QuenchingAgent = QuenchingAgent, QuenchingAgentVolumeMl = QuenchingAgentVolumeMl,
            QuenchingAgentDripSpeedMlMin = QuenchingAgentDripSpeedMlMin,
            AddQuenchingAgentFirst = AddQuenchingAgentFirst,
            PreTemperatureC = PreTemperatureC, MaxTemperatureC = MaxTemperatureC,
            TotalProductVolumeMl = TotalProductVolumeMl,
            // Extraction
            ExtractAgent = ExtractAgent, ExtractAgentVolumeMl = ExtractAgentVolumeMl,
            StirDurationMinutes = StirDurationMinutes, SettlingMinutes = SettlingMinutes,
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
