using SqlSugar;

namespace IndustrySystem.Domain.Entities;

/// <summary>
/// 操作审计日志实体，持久化到数据库供操作记录界面查询。
/// </summary>
public class OperationLog
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime Timestamp { get; set; } = DateTime.Now;

    [SugarColumn(Length = 32)]
    public string Level { get; set; } = "Info";

    [SugarColumn(Length = 64)]
    public string OperationType { get; set; } = string.Empty;

    [SugarColumn(Length = 128)]
    public string Operator { get; set; } = string.Empty;

    [SugarColumn(Length = 2048)]
    public string Description { get; set; } = string.Empty;

    [SugarColumn(Length = 64)]
    public string IPAddress { get; set; } = "-";

    [SugarColumn(Length = 128)]
    public string Logger { get; set; } = string.Empty;

    public long ElapsedMs { get; set; }

    public bool IsSuccess { get; set; } = true;

    [SugarColumn(Length = 2048, IsNullable = true)]
    public string? ErrorMessage { get; set; }
}
