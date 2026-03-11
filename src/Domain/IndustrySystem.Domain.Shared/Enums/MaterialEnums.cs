using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndustrySystem.Domain.Shared.Enums.MaterialEnums
{

    public enum MaterialCategory
    {
        [Description("Material_Category_Solid")]
        Solid,
        [Description("Material_Category_Liquid")]
        Liquid,
        [Description("Material_Category_Gas")]
        Gas
    }

    public enum MaterialHazardLevel
    {
        [Description("Material_Hazard_None")]
        None,
        [Description("Material_Hazard_Low")]
        Low,
        [Description("Material_Hazard_Medium")]
        Medium,
        [Description("Material_Hazard_High")]
        High
    }

    public enum MaterialType
    {
        [Description("Material_Type_RawMaterial")]
        RawMaterial,
        [Description("Material_Type_Consumable")]
        Consumable,
        [Description("Material_Type_Intermediate")]
        Intermediate,
        [Description("Material_Type_Product")]
        Product,
        [Description("Material_Type_Other")]
        Other
    }

    public enum MaterialStorageCondition
    {
        [Description("Material_Storage_RoomTemperature")]
        RoomTemperature,
        [Description("Material_Storage_Refrigerated")]
        Refrigerated,
        [Description("Material_Storage_LightProtected")]
        LightProtected
    }
}
