using System;
using System.Globalization;
using System.Windows.Data;
using IndustrySystem.Domain.Shared.Enums.ShelfEnums;
using MaterialDesignThemes.Wpf;

namespace IndustrySystem.Presentation.Wpf.Converters;

/// <summary>
/// Converts ContainerType? to a MaterialDesign PackIconKind for slot display.
/// </summary>
public class ContainerTypeToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ContainerType ct)
        {
            return ct switch
            {
                ContainerType.CentrifugeBottle => PackIconKind.TestTube,
                ContainerType.SamplingBottle => PackIconKind.Eyedropper,
                ContainerType.StockBottle => PackIconKind.BottleWine,
                ContainerType.ReagentKit => PackIconKind.GridLarge,
                ContainerType.PowderCylinder => PackIconKind.CylinderOff,
                ContainerType.Consumable => PackIconKind.Recycle,
                _ => PackIconKind.PackageVariant
            };
        }
        return PackIconKind.TrayRemove;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
