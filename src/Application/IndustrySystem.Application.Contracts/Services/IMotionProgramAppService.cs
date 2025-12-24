using IndustrySystem.Application.Contracts.Dtos.MotionProgram;

namespace IndustrySystem.Application.Contracts.Services;

/// <summary>
/// 动作程序服务接口
/// </summary>
public interface IMotionProgramAppService
{
    /// <summary>获取程序列表</summary>
    Task<List<MotionProgramListItemDto>> GetListAsync(string? category = null);
    
    /// <summary>获取单个程序</summary>
    Task<MotionProgramDto?> GetAsync(Guid id);
    
    /// <summary>根据名称获取程序</summary>
    Task<MotionProgramDto?> GetByNameAsync(string name);
    
    /// <summary>保存程序（创建或更新）</summary>
    Task<MotionProgramDto> SaveAsync(SaveMotionProgramRequest request);
    
    /// <summary>删除程序</summary>
    Task DeleteAsync(Guid id);
    
    /// <summary>导出程序为JSON</summary>
    Task<string> ExportToJsonAsync(Guid id);
    
    /// <summary>从JSON导入程序</summary>
    Task<MotionProgramDto> ImportFromJsonAsync(string json);
    
    /// <summary>复制程序</summary>
    Task<MotionProgramDto> CopyAsync(Guid id, string newName);
    
    /// <summary>获取子程序列表</summary>
    Task<List<MotionProgramListItemDto>> GetSubProgramsAsync();
}
