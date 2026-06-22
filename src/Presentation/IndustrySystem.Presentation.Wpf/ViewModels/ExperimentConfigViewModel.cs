using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums;
using IndustrySystem.Presentation.Wpf.Resources;
using Prism;
using Prism.Commands;
using Prism.Navigation;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ExperimentConfigViewModel : NavigationViewModel, IParameterEditorHost
{
	private static string T(string key) => LocalizationProvider.Instance[key];

	public const string ParameterEditorRegionName = "ExperimentConfigParameterRegion";

	private readonly IExperimentAppService _experimentSvc;
	private readonly IExperimentTemplateAppService _templateSvc;
	private readonly IExperimentParameterAppService _parameterSvc;
	private readonly IRegionManager _regionManager;
	private bool _isLoadingEditor;

	public ObservableCollection<ExperimentSummaryDto> Experiments { get; } = new();
	public ObservableCollection<ExperimentTemplateDto> Templates { get; } = new();
	public ObservableCollection<ExperimentType> ExperimentTypes { get; } =
		Enum.GetValues(typeof(ExperimentType)).Cast<ExperimentType>().ToObservableCollection();
	public ObservableCollection<ExperimentParameterOptionDto> ParameterOptions { get; } = new();

	private ExperimentSummaryDto? _selectedExperiment;
	public ExperimentSummaryDto? SelectedExperiment
	{
		get => _selectedExperiment;
		set
		{
			if (SetProperty(ref _selectedExperiment, value) && !_isLoadingEditor)
			{
				_ = LoadSelectedExperimentAsync(value?.Id);
			}
		}
	}

	private ExperimentTemplateDto? _selectedTemplate;
	public ExperimentTemplateDto? SelectedTemplate
	{
		get => _selectedTemplate;
		set
		{
			if (SetProperty(ref _selectedTemplate, value) && !_isLoadingEditor)
			{
				_ = ApplyTemplateAsync(value);
			}
		}
	}

	private Guid _editingId;
	public Guid EditingId
	{
		get => _editingId;
		private set
		{
			if (SetProperty(ref _editingId, value))
			{
				RaisePropertyChanged(nameof(IsNewExperiment));
			}
		}
	}

	public bool IsNewExperiment => EditingId == Guid.Empty;

	private string _experimentName = string.Empty;
	public string ExperimentName
	{
		get => _experimentName;
		set => SetProperty(ref _experimentName, value);
	}

	private ExperimentType _selectedType = ExperimentType.Reaction;
	public ExperimentType SelectedType
	{
		get => _selectedType;
		set
		{
			if (SetProperty(ref _selectedType, value) && !_isLoadingEditor)
			{
				_ = LoadParameterOptionsAndSelectAsync(null);
			}
		}
	}

	private ExperimentParameterOptionDto? _selectedParameter;
	public ExperimentParameterOptionDto? SelectedParameter
	{
		get => _selectedParameter;
		set
		{
			if (SetProperty(ref _selectedParameter, value) && !_isLoadingEditor)
			{
				_ = LoadParameterIntoEditorAsync(SelectedType, value?.Id);
			}
		}
	}

	private DateTime _createdAt;
	public DateTime CreatedAt
	{
		get => _createdAt;
		set => SetProperty(ref _createdAt, value);
	}

	private DateTime? _updatedAt;
	public DateTime? UpdatedAt
	{
		get => _updatedAt;
		set => SetProperty(ref _updatedAt, value);
	}

	private ExperimentParameterItemDto? _currentParameterDetail;
	public ExperimentParameterItemDto? CurrentParameterDetail
	{
		get => _currentParameterDetail;
		set => SetProperty(ref _currentParameterDetail, value);
	}

	public ICommand NewCommand { get; }
	public ICommand SaveCommand { get; }

	public ExperimentConfigViewModel(
		IExperimentAppService experimentSvc,
		IExperimentTemplateAppService templateSvc,
		IExperimentParameterAppService parameterSvc,
		IRegionManager regionManager)
	{
		_experimentSvc = experimentSvc;
		_templateSvc = templateSvc;
		_parameterSvc = parameterSvc;
		_regionManager = regionManager;

		NewCommand = new AsyncDelegateCommand(NewExperimentAsync);
		SaveCommand = new AsyncDelegateCommand(SaveAsync);

		_ = LoadAsync();
	}

	protected override async Task OnRefreshAsync()
	{
		await LoadAsync(SelectedExperiment?.Id);
	}

	public void NavigateToCurrentParameterEditor()
	{
		NavigateParameterEditor(SelectedType);
	}

	public async Task LoadAsync(Guid? selectId = null)
	{
		await LoadTemplatesAsync();

		var list = await _experimentSvc.GetListAsync();
		var targetId = selectId ?? SelectedExperiment?.Id;

		_isLoadingEditor = true;
		try
		{
			Experiments.Clear();
			foreach (var item in list)
			{
				Experiments.Add(item);
			}

			SelectedExperiment = targetId.HasValue
				? Experiments.FirstOrDefault(x => x.Id == targetId.Value)
				: Experiments.FirstOrDefault();
		}
		finally
		{
			_isLoadingEditor = false;
		}

		if (SelectedExperiment is not null)
		{
			await LoadSelectedExperimentAsync(SelectedExperiment.Id);
		}
		else
		{
			await NewExperimentAsync();
		}
	}

	private async Task LoadTemplatesAsync()
	{
		var list = await _templateSvc.GetListAsync();
		var selectedTemplateId = SelectedTemplate?.Id;

		_isLoadingEditor = true;
		try
		{
			Templates.Clear();
			foreach (var item in list.OrderBy(x => x.Name))
			{
				Templates.Add(item);
			}

			SelectedTemplate = selectedTemplateId.HasValue
				? Templates.FirstOrDefault(x => x.Id == selectedTemplateId.Value)
				: null;
		}
		finally
		{
			_isLoadingEditor = false;
		}
	}

	private async Task LoadSelectedExperimentAsync(Guid? id)
	{
		if (!id.HasValue)
		{
			return;
		}

		var dto = await _experimentSvc.GetAsync(id.Value);
		if (dto is null)
		{
			return;
		}

		_isLoadingEditor = true;
		try
		{
			EditingId = dto.Id;
			ExperimentName = dto.Name;
			CreatedAt = dto.CreatedAt.ToLocalTime();
			UpdatedAt = dto.UpdatedAt?.ToLocalTime();
			SelectedTemplate = null;
			SetProperty(ref _selectedType, dto.Type, nameof(SelectedType));
		}
		finally
		{
			_isLoadingEditor = false;
		}

		await LoadParameterOptionsAndSelectAsync(dto.ParameterId);
	}

	private async Task NewExperimentAsync()
	{
		_isLoadingEditor = true;
		try
		{
			SelectedExperiment = null;
			EditingId = Guid.Empty;
			ExperimentName = string.Empty;
			CreatedAt = DateTime.Now;
			UpdatedAt = null;
			SelectedTemplate = null;
			SetProperty(ref _selectedType, ExperimentType.Reaction, nameof(SelectedType));
		}
		finally
		{
			_isLoadingEditor = false;
		}

		await LoadParameterOptionsAndSelectAsync(null);
	}

	private async Task ApplyTemplateAsync(ExperimentTemplateDto? template)
	{
		if (template is null || !IsNewExperiment)
		{
			return;
		}

		_isLoadingEditor = true;
		try
		{
			if (string.IsNullOrWhiteSpace(ExperimentName))
			{
				ExperimentName = string.Format(T("ExperimentConfig_NewExperimentNameFromTemplateFormat"), template.Name);
			}

			SetProperty(ref _selectedType, template.Type, nameof(SelectedType));
		}
		finally
		{
			_isLoadingEditor = false;
		}

		await LoadParameterOptionsAndSelectAsync(template.ParameterId);
	}

	private async Task LoadParameterOptionsAndSelectAsync(Guid? preferredParameterId)
	{
		var options = await _parameterSvc.GetOptionsAsync(SelectedType);
		if (options.Count == 0)
		{
			await _parameterSvc.CreateAsync(new ExperimentParameterItemDto
			{
				Id = Guid.NewGuid(),
				Type = SelectedType,
				Name = string.Format(T("DefaultParameterNameFormat"), GetTypeDisplayName(SelectedType)),
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now
			});

			options = await _parameterSvc.GetOptionsAsync(SelectedType);
		}

		_isLoadingEditor = true;
		try
		{
			ParameterOptions.Clear();
			foreach (var option in options)
			{
				ParameterOptions.Add(option);
			}

			if (preferredParameterId.HasValue)
			{
				SelectedParameter = ParameterOptions.FirstOrDefault(x => x.Id == preferredParameterId.Value)
					?? ParameterOptions.FirstOrDefault();
			}
			else
			{
				SelectedParameter = ParameterOptions.FirstOrDefault();
			}
		}
		finally
		{
			_isLoadingEditor = false;
		}

		await LoadParameterIntoEditorAsync(SelectedType, SelectedParameter?.Id);
	}

	private async Task LoadParameterIntoEditorAsync(ExperimentType type, Guid? parameterId)
	{
		NavigateParameterEditor(type);

		if (!parameterId.HasValue)
		{
			CurrentParameterDetail = null;
			return;
		}

		CurrentParameterDetail = await _parameterSvc.GetAsync(type, parameterId.Value);
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

		if (_regionManager.Regions.ContainsRegionWithName(ParameterEditorRegionName))
		{
			var parameters = new NavigationParameters
			{
				{ "ExperimentType", type },
				{ "ParentVm", this }
			};

			_regionManager.RequestNavigate(ParameterEditorRegionName, target, parameters);
		}
	}

	private async Task SaveAsync()
	{
		if (string.IsNullOrWhiteSpace(ExperimentName))
		{
			MessageBox.Show(T("Msg_ExperimentNameRequired"), T("Msg_InfoTitle"), MessageBoxButton.OK, MessageBoxImage.Warning);
			return;
		}

		var input = new ExperimentConfigUpsertDto(
			EditingId,
			ExperimentName.Trim(),
			SelectedType,
			SelectedParameter?.Id);

		var saved = EditingId == Guid.Empty
			? await _experimentSvc.CreateAsync(input)
			: await _experimentSvc.UpdateAsync(input);

		await LoadAsync(saved.Id);

		MessageBox.Show(T("Msg_SaveSuccess"), T("Msg_InfoTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
	}

	private static string GetTypeDisplayName(ExperimentType type) => type switch
	{
		ExperimentType.Reaction => T("ExperimentType_Reaction"),
		ExperimentType.RotaryEvaporation => T("ExperimentType_RotaryEvaporation"),
		ExperimentType.Detection => T("ExperimentType_Detection"),
		ExperimentType.Filtration => T("ExperimentType_Filtration"),
		ExperimentType.Drying => T("ExperimentType_Drying"),
		ExperimentType.Quenching => T("ExperimentType_Quenching"),
		ExperimentType.Extraction => T("ExperimentType_Extraction"),
		ExperimentType.Sampling => T("ExperimentType_Sampling"),
		ExperimentType.Centrifugation => T("ExperimentType_Centrifugation"),
		ExperimentType.CustomDetection => T("ExperimentType_CustomDetection"),
		_ => type.ToString()
	};
}
