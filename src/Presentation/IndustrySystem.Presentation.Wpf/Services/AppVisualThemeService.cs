using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ModernWpf;

namespace IndustrySystem.Presentation.Wpf.Services;

public enum AppVisualTheme
{
    Classic,
    LiquidGlass
}

public static class AppVisualThemeService
{
    public const string IsLiquidGlassThemeResourceKey = "IsLiquidGlassTheme";

    private static readonly Uri LiquidGlassThemeUri = new(
        "pack://application:,,,/IndustrySystem.Presentation.Wpf;component/Resources/Styles/LiquidGlassTheme.xaml",
        UriKind.Absolute);

    private static readonly object MissingResource = new();
    private static readonly Dictionary<object, object> ClassicValues = new();
    private static ResourceDictionary? _liquidGlassResources;

    public static AppVisualTheme Current { get; private set; } = AppVisualTheme.Classic;

    public static AppVisualTheme Toggle()
    {
        var next = Current == AppVisualTheme.LiquidGlass
            ? AppVisualTheme.Classic
            : AppVisualTheme.LiquidGlass;
        Apply(next);
        return next;
    }

    public static void Apply(AppVisualTheme theme)
    {
        if (System.Windows.Application.Current is null)
        {
            return;
        }

        if (theme == AppVisualTheme.LiquidGlass)
        {
            ApplyLiquidGlassTheme();
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
        }
        else
        {
            RestoreClassicTheme();
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
        }

        System.Windows.Application.Current.Resources[IsLiquidGlassThemeResourceKey] = theme == AppVisualTheme.LiquidGlass;
        Current = theme;
    }

    private static void ApplyLiquidGlassTheme()
    {
        var appResources = System.Windows.Application.Current.Resources;
        var liquidResources = GetLiquidGlassResources();

        foreach (var key in liquidResources.Keys.Cast<object>().ToList())
        {
            if (!ClassicValues.ContainsKey(key))
            {
                ClassicValues[key] = appResources.Contains(key)
                    ? appResources[key]
                    : MissingResource;
            }

            appResources[key] = liquidResources[key];
        }
    }

    private static void RestoreClassicTheme()
    {
        var appResources = System.Windows.Application.Current.Resources;
        foreach (var item in ClassicValues)
        {
            if (ReferenceEquals(item.Value, MissingResource))
            {
                appResources.Remove(item.Key);
                continue;
            }

            appResources[item.Key] = item.Value;
        }
    }

    private static ResourceDictionary GetLiquidGlassResources()
    {
        _liquidGlassResources ??= new ResourceDictionary { Source = LiquidGlassThemeUri };
        return _liquidGlassResources;
    }
}
