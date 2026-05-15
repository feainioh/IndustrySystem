using System.Collections.ObjectModel;
using IndustrySystem.Presentation.Wpf.Resources;
using Prism.Mvvm;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class SensorMetricViewModel : BindableBase
{
	private double _value;

	public string Label { get; init; } = string.Empty;
	public string UnitSmall { get; init; } = string.Empty;

	public double Value
	{
		get => _value;
		init => _value = value;
	}

	public string ValueText => $"{Value:F2}";
}

public class RealtimeDataViewModel : NagetiveViewModel
{
	private string _currentMonitor = Strings.Mock_Experiment_FilterValidation;

	public string CurrentMonitor
	{
		get => _currentMonitor;
		set => SetProperty(ref _currentMonitor, value);
	}

	public ObservableCollection<SensorMetricViewModel> Sensors { get; } = new()
	{
		new SensorMetricViewModel { Label = Strings.Metric_PressureSensorA, UnitSmall = "MPa", Value = 0.74 },
		new SensorMetricViewModel { Label = Strings.Metric_TemperatureProbeT1, UnitSmall = "°C", Value = 25.26 },
		new SensorMetricViewModel { Label = Strings.Metric_FlowMeterF1, UnitSmall = "L/min", Value = 1.22 },
		new SensorMetricViewModel { Label = Strings.Metric_ConductivityMeter, UnitSmall = "μS/cm", Value = 11.83 },
	};
}
