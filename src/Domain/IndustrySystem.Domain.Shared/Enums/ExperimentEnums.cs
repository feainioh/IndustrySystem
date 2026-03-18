using System.ComponentModel;

namespace IndustrySystem.Domain.Shared.Enums;

public enum ExperimentType
{
    [Description("反应")]
    Reaction,
    [Description("旋蒸")]
    RotaryEvaporation,
    [Description("检测")]
    Detection,
    [Description("过滤")]
    Filtration,
    [Description("干燥")]
    Drying,
    [Description("淬灭")]
    Quenching,
    [Description("萃取")]
    Extraction,
    [Description("取样")]
    Sampling,
    [Description("离心")]
    Centrifugation,
    [Description("自定义检测")]
    CustomDetection
}
