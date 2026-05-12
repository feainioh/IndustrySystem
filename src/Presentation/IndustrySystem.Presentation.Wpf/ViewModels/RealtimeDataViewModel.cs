using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Mvvm;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

/// <summary>单个传感器指标数据（含 OxyPlot 迷你图）</summary>
public class SensorMetricViewModel : BindableBase
{
    private double _value;
    private PlotModel _miniPlotModel;

    public string Label { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public string UnitSmall { get; init; } = string.Empty;
    public double Value { get => _value; set => SetProperty(ref _value, value); }
    public string ValueText => $"{Value:F2}";

    public PlotModel MiniPlotModel
    {
        get => _miniPlotModel;
        private set => SetProperty(ref _miniPlotModel, value);
    }

    private readonly double[] _history = new double[20];
    private readonly BarSeries _barSeries;

    public SensorMetricViewModel()
    {
        _miniPlotModel = BuildMiniPlot(out _barSeries);
    }

    public void PushValue(double newVal)
    {
        Value = newVal;
        Array.Copy(_history, 1, _history, 0, _history.Length - 1);
        _history[_history.Length - 1] = newVal;
        RaisePropertyChanged(nameof(ValueText));
        RefreshMiniItems();
    }

    public void InitHistory(double baseVal, Random rng)
    {
        double v = baseVal;
        for (int i = 0; i < _history.Length; i++)
        {
            v += (rng.NextDouble() - 0.5) * 0.1 * Math.Abs(baseVal);
            _history[i] = Math.Max(0, v);
        }
        RefreshMiniItems();
    }

    private void RefreshMiniItems()
    {
        _barSeries.Items.Clear();
        double minV = _history.Min();
        double maxV = _history.Max();
        double range = maxV - minV;
        if (range < 1e-9) range = 1;
        foreach (var v in _history)
            _barSeries.Items.Add(new BarItem { Value = v });

        // 更新 Y 轴范围留一点 padding
        var yAxis = _miniPlotModel.Axes.OfType<LinearAxis>().FirstOrDefault(a => a.Position == AxisPosition.Left);
        if (yAxis != null)
        {
            yAxis.Minimum = minV - range * 0.1;
            yAxis.Maximum = maxV + range * 0.3;
        }
        _miniPlotModel.InvalidatePlot(true);
    }

    private static PlotModel BuildMiniPlot(out BarSeries series)
    {
        var model = new PlotModel
        {
            Background = OxyColors.Transparent,
            PlotAreaBackground = OxyColors.Transparent,
            PlotMargins = new OxyThickness(0),
            Padding = new OxyThickness(0),
        };

        model.Axes.Add(new CategoryAxis
        {
            Position = AxisPosition.Bottom,
            IsAxisVisible = false,
            IsPanEnabled = false,
            IsZoomEnabled = false,
        });
        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            IsAxisVisible = false,
            IsPanEnabled = false,
            IsZoomEnabled = false,
        });

        series = new BarSeries
        {
            FillColor = OxyColor.FromArgb(200, 77, 166, 232),
            StrokeThickness = 0,
            BarWidth = 0.85,
        };
        model.Series.Add(series);
        return model;
    }
}

public class RealtimeDataViewModel : NagetiveViewModel
{
    private readonly Random _rng = new(42);
    private CancellationTokenSource? _cts;
    private SynchronizationContext? _uiContext;

    public string CurrentMonitor { get; } = "过滤工艺验证";

    // 温度趋势图（OxyPlot）
    private readonly LineSeries _t1Series;
    private readonly LineSeries _t2Series;

    private PlotModel _trendModel;
    public PlotModel TrendModel
    {
        get => _trendModel;
        private set => SetProperty(ref _trendModel, value);
    }

    public ObservableCollection<SensorMetricViewModel> Sensors { get; } = new()
    {
        new SensorMetricViewModel { Label = "压力传感器 A", Unit = "MPa",   UnitSmall = "MPa",   Value = 0.74 },
        new SensorMetricViewModel { Label = "温度探头 T1",  Unit = "°C",    UnitSmall = "°C",    Value = 25.26 },
        new SensorMetricViewModel { Label = "流量计 F1",    Unit = "L/min", UnitSmall = "L/min", Value = 1.22 },
        new SensorMetricViewModel { Label = "电导率仪",     Unit = "μS/cm", UnitSmall = "μS/cm", Value = 11.83 },
        new SensorMetricViewModel { Label = "温度探头 T2",  Unit = "°C",    UnitSmall = "°C",    Value = 25.38 },
        new SensorMetricViewModel { Label = "阀门开度 V1",  Unit = "%",     UnitSmall = "%",     Value = 44.25 },
    };

    public RealtimeDataViewModel()
    {
        _trendModel = BuildTrendModel(out _t1Series, out _t2Series);
    }

    public override Task OnNavigatedToAsync()
    {
        _uiContext = SynchronizationContext.Current;
        InitData();
        StartSimulation();
        return Task.CompletedTask;
    }

    public override Task OnNavigatedFromAsync()
    {
        _cts?.Cancel();
        return Task.CompletedTask;
    }

    private void InitData()
    {
        // 初始化趋势数据（100 点）
        _t1Series.Points.Clear();
        _t2Series.Points.Clear();
        double v1 = 25.0, v2 = 25.4;
        for (int i = 0; i < 100; i++)
        {
            v1 += (_rng.NextDouble() - 0.5) * 0.3;
            v2 += (_rng.NextDouble() - 0.5) * 0.3;
            _t1Series.Points.Add(new DataPoint(i, Math.Round(v1, 2)));
            _t2Series.Points.Add(new DataPoint(i, Math.Round(v2, 2)));
        }

        // 更新图例标签
        UpdateLegend();
        _trendModel.InvalidatePlot(true);

        // 初始化迷你图
        foreach (var s in Sensors)
            s.InitHistory(s.Value, _rng);
    }

    private void StartSimulation()
    {
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(800, token).ConfigureAwait(false);
                if (token.IsCancellationRequested) break;

                // 滚动趋势数据
                double nextX = _t1Series.Points.Count > 0 ? _t1Series.Points[^1].X + 1 : 100;
                double newT1 = Math.Round(_t1Series.Points[^1].Y + (_rng.NextDouble() - 0.5) * 0.25, 2);
                double newT2 = Math.Round(_t2Series.Points[^1].Y + (_rng.NextDouble() - 0.5) * 0.25, 2);

                _t1Series.Points.Add(new DataPoint(nextX, newT1));
                _t2Series.Points.Add(new DataPoint(nextX, newT2));
                if (_t1Series.Points.Count > 100)
                {
                    _t1Series.Points.RemoveAt(0);
                    _t2Series.Points.RemoveAt(0);
                }

                // 更新传感器值
                UpdateSensor(0, 0.74, 0.05);
                UpdateSensor(1, 25.26, 0.15);
                UpdateSensor(2, 1.22, 0.08);
                UpdateSensor(3, 11.83, 0.2);
                UpdateSensor(4, 25.38, 0.15);
                UpdateSensor(5, 44.25, 0.5);

                UpdateLegend();
                _trendModel.InvalidatePlot(true);
            }
        }, token);
    }

    private void UpdateSensor(int index, double baseVal, double noise)
    {
        var newVal = Math.Round(baseVal + (_rng.NextDouble() - 0.5) * noise * 2, 2);
        Sensors[index].PushValue(Math.Max(0, newVal));
    }

    private void UpdateLegend()
    {
        if (_t1Series.Points.Count > 0)
            _t1Series.Title = $"T1 (核心): {_t1Series.Points[^1].Y:F1}°C";
        if (_t2Series.Points.Count > 0)
            _t2Series.Title = $"T2 (环境): {_t2Series.Points[^1].Y:F1}°C";
    }

    private static PlotModel BuildTrendModel(out LineSeries t1, out LineSeries t2)
    {
        var model = new PlotModel
        {
            Background = OxyColors.Transparent,
            PlotAreaBackground = OxyColors.Transparent,
            IsLegendVisible = true,
        };

        model.Legends.Add(new OxyPlot.Legends.Legend
        {
            LegendPosition = OxyPlot.Legends.LegendPosition.TopRight,
            LegendBackground = OxyColor.FromArgb(180, 255, 255, 255),
            LegendBorder = OxyColors.LightGray,
            LegendBorderThickness = 0.5,
            LegendTextColor = OxyColors.DimGray,
        });

        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Bottom,
            IsAxisVisible = true,
            AxislineColor = OxyColors.LightGray,
            TextColor = OxyColors.Gray,
            TicklineColor = OxyColors.LightGray,
            MajorGridlineStyle = LineStyle.Dot,
            MajorGridlineColor = OxyColor.FromArgb(60, 180, 180, 180),
            IsPanEnabled = false,
            IsZoomEnabled = false,
        });
        model.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            IsAxisVisible = true,
            AxislineColor = OxyColors.LightGray,
            TextColor = OxyColors.Gray,
            TicklineColor = OxyColors.LightGray,
            MajorGridlineStyle = LineStyle.Dot,
            MajorGridlineColor = OxyColor.FromArgb(60, 180, 180, 180),
            IsPanEnabled = false,
            IsZoomEnabled = false,
        });

        t1 = new LineSeries
        {
            Title = "T1 (核心)",
            Color = OxyColor.FromRgb(232, 85, 85),
            StrokeThickness = 2,
            MarkerType = MarkerType.None,
            RenderInLegend = true,
        };
        t2 = new LineSeries
        {
            Title = "T2 (环境)",
            Color = OxyColor.FromRgb(59, 141, 224),
            StrokeThickness = 2,
            MarkerType = MarkerType.None,
            RenderInLegend = true,
        };
        model.Series.Add(t1);
        model.Series.Add(t2);
        return model;
    }
}

