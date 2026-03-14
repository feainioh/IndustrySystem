using IndustrySystem.Domain.Shared.Enums.MaterialEnums;
using IndustrySystem.Domain.Shared.Enums.ShelfEnums;

namespace IndustrySystem.Application.Contracts.Dtos;

public record ExperimentTemplateDto(Guid Id, string Name, string? Description);
public record ExperimentGroupDto(Guid Id, string Name);
public record ExperimentDto(Guid Id, Guid TemplateId, Guid GroupId, string Name, DateTime CreatedAt);

public record InventoryRecordDto(
    Guid Id,
    Guid MaterialId,
    string MaterialCode,
    string MaterialName,
    string BatchNo,
    decimal Quantity,
    decimal SafetyStock,
    string Unit,
    DateTime? InboundDate,
    DateTime? ExpiryDate,
    string Location,
    int WellRow,
    int WellColumn,
    Guid? ShelfSlotId,
    string Remark);


public record MaterialDto(
    Guid Id,
    string MaterialCode,
    string Name,
    string FullName,
    string MolecularFormula,
    MaterialCategory Category,
    MaterialType MaterialType,
    string CasNo,
    string Purity,
    string Density,
    string Unit,
    MaterialHazardLevel HazardLevel,
    MaterialStorageCondition StorageCondition,
    string Precautions,
    string Brand,
    string Supplier);


// Shelf DTOs
public record ContainerInfoDto(
    Guid Id,
    string Name,
    ContainerType ContainerType,
    int Rows,
    int Columns,
    string Description);

public record ShelfConfigDto(
    Guid Id,
    string ShelfCode,
    string Name,
    int Rows,
    int Columns,
    string Description);

public record ShelfSlotDto(
    Guid Id,
    Guid ShelfId,
    int Row,
    int Column,
    IReadOnlyList<ContainerType> AllowedContainerTypes,
    Guid? ContainerId,
    Guid? InventoryRecordId,
    bool IsDisabled,
    string Remark,
    string? ContainerName,
    ContainerType? ContainerTypeValue,
    int? ContainerRows,
    int? ContainerColumns,
    string? MaterialName,
    decimal? Quantity,
    string? Unit,
    IReadOnlyList<WellOccupancyDto> OccupiedWells,
    int InventoryRecordCount = 0);

/// <summary>容器内单个孔位的占用信息</summary>
public record WellOccupancyDto(
    int WellRow,
    int WellColumn,
    string MaterialName,
    decimal Quantity,
    string Unit);

/// <summary>按物料+批号汇总的库存概览</summary>
public record InventorySummaryDto(
    Guid MaterialId,
    string MaterialCode,
    string MaterialName,
    string BatchNo,
    string Unit,
    decimal TotalQuantity,
    decimal SafetyStock,
    int RecordCount);
