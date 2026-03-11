using System.ComponentModel;

namespace IndustrySystem.Domain.Shared.Enums.ShelfEnums;

public enum ContainerType
{
    [Description("离心瓶")]
    CentrifugeBottle,
    [Description("取样瓶")]
    SamplingBottle,
    [Description("原液瓶")]
    StockBottle,
    [Description("试剂盒")]
    ReagentKit,
    [Description("粉筒")]
    PowderCylinder,
    [Description("耗材")]
    Consumable,
    [Description("其他")]
    Other
}
