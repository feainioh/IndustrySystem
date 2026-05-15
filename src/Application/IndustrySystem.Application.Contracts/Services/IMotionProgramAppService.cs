using IndustrySystem.Application.Contracts.Dtos.MotionProgram;

namespace IndustrySystem.Application.Contracts.Services;

/// <summary>
/// </summary>
public interface IMotionProgramAppService
{
    Task<List<MotionProgramListItemDto>> GetListAsync(string? category = null);
    
    Task<MotionProgramDto?> GetAsync(Guid id);
    
    Task<MotionProgramDto?> GetByNameAsync(string name);
    
    Task<MotionProgramDto> SaveAsync(SaveMotionProgramRequest request);
    
    Task DeleteAsync(Guid id);
    
    Task<string> ExportToJsonAsync(Guid id);
    
    Task<MotionProgramDto> ImportFromJsonAsync(string json);
    
    Task<MotionProgramDto> CopyAsync(Guid id, string newName);
    
    Task<List<MotionProgramListItemDto>> GetSubProgramsAsync();
}
