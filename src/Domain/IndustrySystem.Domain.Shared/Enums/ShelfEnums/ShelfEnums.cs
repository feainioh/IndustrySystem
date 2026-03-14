using System.ComponentModel;

namespace IndustrySystem.Domain.Shared.Enums.ShelfEnums;

public enum ContainerType
{
    [Description("Shelf_Container_CentrifugeBottle")]
    CentrifugeBottle,
    [Description("Shelf_Container_SamplingBottle")]
    SamplingBottle,
    [Description("Shelf_Container_StockBottle")]
    StockBottle,
    [Description("Shelf_Container_ReagentKit")]
    ReagentKit,
    [Description("Shelf_Container_PowderCylinder")]
    PowderCylinder,
    [Description("Shelf_Container_Consumable")]
    Consumable,
    [Description("Shelf_Container_Other")]
    Other
}
