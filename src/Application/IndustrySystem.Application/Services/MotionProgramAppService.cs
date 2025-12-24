using System.Text.Json;
using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos.MotionProgram;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace IndustrySystem.Application.Services;

/// <summary>
/// 动作程序服务实现
/// </summary>
public class MotionProgramAppService : IMotionProgramAppService
{
    private readonly ILogger<MotionProgramAppService> _logger;
    private readonly IMapper _mapper;
    
    // 使用内存存储（生产环境应改为数据库）
    private static readonly Dictionary<Guid, MotionProgramData> _programs = new();
    
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public MotionProgramAppService(ILogger<MotionProgramAppService> logger, IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
    }

    public Task<List<MotionProgramListItemDto>> GetListAsync(string? category = null)
    {
        _logger.LogDebug("Getting motion program list, category: {Category}", category);
        
        var list = _programs.Values.AsEnumerable();
        
        if (!string.IsNullOrEmpty(category))
        {
            list = list.Where(p => p.Category == category);
        }
        
        var result = list.Select(p => new MotionProgramListItemDto(
            p.Id,
            p.Name,
            p.Description,
            p.Version,
            p.Category,
            p.IsSubProgram,
            p.CreatedAt,
            p.ModifiedAt,
            p.CreatedBy,
            p.Nodes.Count
        )).ToList();
        
        return Task.FromResult(result);
    }

    public Task<MotionProgramDto?> GetAsync(Guid id)
    {
        _logger.LogDebug("Getting motion program: {Id}", id);
        
        if (!_programs.TryGetValue(id, out var data))
            return Task.FromResult<MotionProgramDto?>(null);
        
        return Task.FromResult<MotionProgramDto?>(DataToDto(data));
    }

    public Task<MotionProgramDto?> GetByNameAsync(string name)
    {
        _logger.LogDebug("Getting motion program by name: {Name}", name);
        
        var data = _programs.Values.FirstOrDefault(p => p.Name == name);
        if (data == null) return Task.FromResult<MotionProgramDto?>(null);
        
        return Task.FromResult<MotionProgramDto?>(DataToDto(data));
    }

    public Task<MotionProgramDto> SaveAsync(SaveMotionProgramRequest request)
    {
        _logger.LogInformation("Saving motion program: {Name}", request.Name);
        
        MotionProgramData data;
        
        if (request.Id.HasValue && request.Id.Value != Guid.Empty && _programs.ContainsKey(request.Id.Value))
        {
            data = _programs[request.Id.Value];
            data.Name = request.Name;
            data.Description = request.Description;
            data.Category = request.Category;
            data.IsSubProgram = request.IsSubProgram;
            data.ModifiedAt = DateTime.Now;
            data.Version = IncrementVersion(data.Version);
        }
        else
        {
            data = new MotionProgramData
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                IsSubProgram = request.IsSubProgram,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
                Version = "1.0.0"
            };
        }
        
        data.Nodes = request.Nodes;
        data.Connections = request.Connections;
        data.Variables = request.Variables;
        
        _programs[data.Id] = data;
        
        _logger.LogInformation("Motion program saved: {Id}", data.Id);
        return Task.FromResult(DataToDto(data));
    }

    public Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting motion program: {Id}", id);
        _programs.Remove(id);
        return Task.CompletedTask;
    }

    public Task<string> ExportToJsonAsync(Guid id)
    {
        _logger.LogDebug("Exporting motion program to JSON: {Id}", id);
        
        if (!_programs.TryGetValue(id, out var data))
            throw new InvalidOperationException($"Program not found: {id}");
        
        var dto = DataToDto(data);
        return Task.FromResult(JsonSerializer.Serialize(dto, _jsonOptions));
    }

    public Task<MotionProgramDto> ImportFromJsonAsync(string json)
    {
        _logger.LogInformation("Importing motion program from JSON");
        
        var dto = JsonSerializer.Deserialize<MotionProgramDto>(json, _jsonOptions)
                  ?? throw new InvalidOperationException("Invalid JSON format");
        
        var request = new SaveMotionProgramRequest(
            null,
            dto.Name + " (Imported)",
            dto.Description,
            dto.Category,
            dto.IsSubProgram,
            dto.Nodes,
            dto.Connections,
            dto.Variables
        );
        
        return SaveAsync(request);
    }

    public async Task<MotionProgramDto> CopyAsync(Guid id, string newName)
    {
        _logger.LogInformation("Copying motion program: {Id} -> {NewName}", id, newName);
        
        var dto = await GetAsync(id) 
                  ?? throw new InvalidOperationException($"Program not found: {id}");
        
        var request = new SaveMotionProgramRequest(
            null,
            newName,
            dto.Description,
            dto.Category,
            dto.IsSubProgram,
            dto.Nodes,
            dto.Connections,
            dto.Variables
        );
        
        return await SaveAsync(request);
    }

    public Task<List<MotionProgramListItemDto>> GetSubProgramsAsync()
    {
        _logger.LogDebug("Getting sub-programs");
        
        var result = _programs.Values
            .Where(p => p.IsSubProgram)
            .Select(p => new MotionProgramListItemDto(
                p.Id,
                p.Name,
                p.Description,
                p.Version,
                p.Category,
                p.IsSubProgram,
                p.CreatedAt,
                p.ModifiedAt,
                p.CreatedBy,
                p.Nodes.Count
            )).ToList();
        
        return Task.FromResult(result);
    }

    #region Private Helpers

    private static MotionProgramDto DataToDto(MotionProgramData data)
    {
        return new MotionProgramDto(
            data.Id,
            data.Name,
            data.Description,
            data.Version,
            data.CreatedAt,
            data.ModifiedAt,
            data.CreatedBy,
            data.Category,
            data.IsSubProgram,
            data.Nodes,
            data.Connections,
            data.Variables
        );
    }

    private static string IncrementVersion(string version)
    {
        var parts = version.Split('.');
        if (parts.Length != 3) return "1.0.1";
        
        if (int.TryParse(parts[2], out int patch))
        {
            return $"{parts[0]}.{parts[1]}.{patch + 1}";
        }
        
        return version;
    }

    private class MotionProgramData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0";
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsSubProgram { get; set; }
        public List<ActionNodeDto> Nodes { get; set; } = new();
        public List<NodeConnectionDto> Connections { get; set; } = new();
        public Dictionary<string, object> Variables { get; set; } = new();
    }

    #endregion
}
