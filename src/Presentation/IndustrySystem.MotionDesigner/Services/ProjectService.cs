using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using IndustrySystem.Application.Contracts.Dtos.MotionProgram;
using IndustrySystem.MotionDesigner.Models;
using IndustrySystem.MotionDesigner.ViewModels;
using NLog;

namespace IndustrySystem.MotionDesigner.Services;

/// <summary>
/// 项目服务接口 - 管理项目的创建、保存、加载
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// 当前项目
    /// </summary>
    MotionProject? CurrentProject { get; }

    /// <summary>
    /// 创建新项目
    /// </summary>
    MotionProject CreateProject(string name);

    /// <summary>
    /// 打开项目
    /// </summary>
    Task<MotionProject> OpenProjectAsync(string filePath);

    /// <summary>
    /// 保存项目
    /// </summary>
    Task SaveProjectAsync(MotionProject project, string? filePath = null);

    /// <summary>
    /// 关闭项目
    /// </summary>
    void CloseProject();

    /// <summary>
    /// 添加子项目
    /// </summary>
    SubProject AddSubProject(MotionProject project, string name);

    /// <summary>
    /// 删除子项目
    /// </summary>
    void RemoveSubProject(MotionProject project, SubProject subProject);

    /// <summary>
    /// 添加子程序
    /// </summary>
    SubProgram AddSubProgram(SubProject subProject, string name, SubProgramType type = SubProgramType.Normal);

    /// <summary>
    /// 删除子程序
    /// </summary>
    void RemoveSubProgram(SubProject subProject, SubProgram subProgram);

    /// <summary>
    /// 复制子程序
    /// </summary>
    SubProgram DuplicateSubProgram(SubProject subProject, SubProgram source);

    /// <summary>
    /// 导出子程序为独立文件
    /// </summary>
    Task ExportSubProgramAsync(SubProgram subProgram, string filePath);

    /// <summary>
    /// 导入子程序
    /// </summary>
    Task<SubProgram> ImportSubProgramAsync(string filePath);
}

/// <summary>
/// 项目服务实现
/// </summary>
public class ProjectService : IProjectService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IDeviceConfigService _configService;
    private readonly JsonSerializerOptions _jsonOptions;

    public MotionProject? CurrentProject { get; private set; }

    public ProjectService(IDeviceConfigService configService)
    {
        _configService = configService;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public MotionProject CreateProject(string name)
    {
        var project = new MotionProject
        {
            Name = name,
            CreatedTime = DateTime.Now,
            ModifiedTime = DateTime.Now
        };

        // 创建默认主子项目
        var mainSubProject = AddSubProject(project, "主流程");

        // 创建默认主程序
        AddSubProgram(mainSubProject, "Main", SubProgramType.Main);

        CurrentProject = project;
        _logger.Info($"Created new project: {name}");

        return project;
    }

    public async Task<MotionProject> OpenProjectAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var projectDto = JsonSerializer.Deserialize<ProjectFileDto>(json, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize project file");

            var project = ConvertFromDto(projectDto);
            project.FilePath = filePath;
            project.IsModified = false;

            // 加载设备配置
            var projectDir = Path.GetDirectoryName(filePath) ?? string.Empty;
            var configPath = Path.Combine(projectDir, project.DeviceConfigPath);
            if (File.Exists(configPath))
            {
                project.DeviceConfig = await _configService.ImportFromFileAsync(configPath);
            }

            CurrentProject = project;
            _logger.Info($"Opened project: {project.Name} from {filePath}");

            return project;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to open project from {filePath}");
            throw;
        }
    }

    public async Task SaveProjectAsync(MotionProject project, string? filePath = null)
    {
        try
        {
            filePath ??= project.FilePath;
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path is required for new projects");

            var projectDir = Path.GetDirectoryName(filePath) ?? string.Empty;
            if (!Directory.Exists(projectDir))
                Directory.CreateDirectory(projectDir);

            // 保存设备配置
            if (project.DeviceConfig != null)
            {
                var configPath = Path.Combine(projectDir, project.DeviceConfigPath);
                await _configService.ExportToFileAsync(project.DeviceConfig, configPath);
            }

            // 保存项目文件
            var projectDto = ConvertToDto(project);
            var json = JsonSerializer.Serialize(projectDto, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);

            project.FilePath = filePath;
            project.IsModified = false;

            _logger.Info($"Saved project: {project.Name} to {filePath}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to save project to {filePath}");
            throw;
        }
    }

    public void CloseProject()
    {
        CurrentProject = null;
        _logger.Info("Project closed");
    }

    public SubProject AddSubProject(MotionProject project, string name)
    {
        var subProject = new SubProject
        {
            Name = name,
            Order = project.SubProjects.Count
        };

        project.SubProjects.Add(subProject);
        project.IsModified = true;

        _logger.Info($"Added sub-project: {name}");
        return subProject;
    }

    public void RemoveSubProject(MotionProject project, SubProject subProject)
    {
        project.SubProjects.Remove(subProject);
        project.IsModified = true;
        _logger.Info($"Removed sub-project: {subProject.Name}");
    }

    public SubProgram AddSubProgram(SubProject subProject, string name, SubProgramType type = SubProgramType.Normal)
    {
        var subProgram = new SubProgram
        {
            Name = name,
            ProgramType = type,
            Order = subProject.SubPrograms.Count
        };

        subProject.SubPrograms.Add(subProgram);
        _logger.Info($"Added sub-program: {name} ({type})");

        return subProgram;
    }

    public void RemoveSubProgram(SubProject subProject, SubProgram subProgram)
    {
        subProject.SubPrograms.Remove(subProgram);
        _logger.Info($"Removed sub-program: {subProgram.Name}");
    }

    public SubProgram DuplicateSubProgram(SubProject subProject, SubProgram source)
    {
        var copy = new SubProgram
        {
            Name = $"{source.Name} - 副本",
            Description = source.Description,
            ProgramType = source.ProgramType,
            Order = subProject.SubPrograms.Count,
            LocalVariables = new Dictionary<string, object>(source.LocalVariables),
            InputParameters = source.InputParameters.ToList(),
            OutputParameters = source.OutputParameters.ToList()
        };

        // 复制节点
        var nodeIdMap = new Dictionary<Guid, Guid>();
        foreach (var node in source.Nodes)
        {
            var newNode = new ActionNodeViewModel
            {
                Id = Guid.NewGuid(),
                Name = node.Name,
                ActionType = node.ActionType,
                X = node.X + 50,
                Y = node.Y + 50,
                Width = node.Width,
                Height = node.Height,
                Description = node.Description,
                IsEnabled = node.IsEnabled,
                TimeoutMs = node.TimeoutMs,
                RetryCount = node.RetryCount,
                ErrorHandling = node.ErrorHandling,
                Parameters = new Dictionary<string, object>(node.Parameters)
            };
            nodeIdMap[node.Id] = newNode.Id;
            copy.Nodes.Add(newNode);
        }

        // 复制连接
        foreach (var conn in source.Connections)
        {
            if (nodeIdMap.TryGetValue(conn.SourceNodeId, out var newSourceId) &&
                nodeIdMap.TryGetValue(conn.TargetNodeId, out var newTargetId))
            {
                copy.Connections.Add(new ConnectionViewModel
                {
                    Id = Guid.NewGuid(),
                    SourceNodeId = newSourceId,
                    TargetNodeId = newTargetId,
                    SourcePortDirection = conn.SourcePortDirection,
                    TargetPortDirection = conn.TargetPortDirection
                });
            }
        }

        subProject.SubPrograms.Add(copy);
        _logger.Info($"Duplicated sub-program: {source.Name} -> {copy.Name}");

        return copy;
    }

    public async Task ExportSubProgramAsync(SubProgram subProgram, string filePath)
    {
        var dto = ConvertSubProgramToDto(subProgram);
        var json = JsonSerializer.Serialize(dto, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
        _logger.Info($"Exported sub-program: {subProgram.Name} to {filePath}");
    }

    public async Task<SubProgram> ImportSubProgramAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var dto = JsonSerializer.Deserialize<SubProgramDto>(json, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize sub-program file");

        var subProgram = ConvertSubProgramFromDto(dto);
        _logger.Info($"Imported sub-program: {subProgram.Name} from {filePath}");

        return subProgram;
    }

    #region DTO Conversion

    private ProjectFileDto ConvertToDto(MotionProject project)
    {
        return new ProjectFileDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Version = project.Version,
            Author = project.Author,
            CreatedTime = project.CreatedTime,
            ModifiedTime = project.ModifiedTime,
            DeviceConfigPath = project.DeviceConfigPath,
            GlobalVariables = project.GlobalVariables,
            SubProjects = project.SubProjects.Select(ConvertSubProjectToDto).ToList()
        };
    }

    private MotionProject ConvertFromDto(ProjectFileDto dto)
    {
        var project = new MotionProject
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Version = dto.Version,
            Author = dto.Author,
            CreatedTime = dto.CreatedTime,
            ModifiedTime = dto.ModifiedTime,
            DeviceConfigPath = dto.DeviceConfigPath,
            GlobalVariables = dto.GlobalVariables ?? []
        };

        foreach (var subProjectDto in dto.SubProjects ?? [])
        {
            project.SubProjects.Add(ConvertSubProjectFromDto(subProjectDto));
        }

        return project;
    }

    private SubProjectDto ConvertSubProjectToDto(SubProject subProject)
    {
        return new SubProjectDto
        {
            Id = subProject.Id,
            Name = subProject.Name,
            Description = subProject.Description,
            Order = subProject.Order,
            Variables = subProject.Variables,
            SubPrograms = subProject.SubPrograms.Select(ConvertSubProgramToDto).ToList()
        };
    }

    private SubProject ConvertSubProjectFromDto(SubProjectDto dto)
    {
        var subProject = new SubProject
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Order = dto.Order,
            Variables = dto.Variables ?? []
        };

        foreach (var subProgramDto in dto.SubPrograms ?? [])
        {
            subProject.SubPrograms.Add(ConvertSubProgramFromDto(subProgramDto));
        }

        return subProject;
    }

    private SubProgramDto ConvertSubProgramToDto(SubProgram subProgram)
    {
        return new SubProgramDto
        {
            Id = subProgram.Id,
            Name = subProgram.Name,
            Description = subProgram.Description,
            Order = subProgram.Order,
            ProgramType = subProgram.ProgramType,
            LocalVariables = subProgram.LocalVariables,
            InputParameters = subProgram.InputParameters,
            OutputParameters = subProgram.OutputParameters,
            Nodes = subProgram.Nodes.Select(n => new NodeDto
            {
                Id = n.Id,
                Name = n.Name,
                ActionType = n.ActionType,
                X = n.X,
                Y = n.Y,
                Width = n.Width,
                Height = n.Height,
                Description = n.Description,
                IsEnabled = n.IsEnabled,
                TimeoutMs = n.TimeoutMs,
                RetryCount = n.RetryCount,
                ErrorHandling = n.ErrorHandling,
                Parameters = n.Parameters
            }).ToList(),
            Connections = subProgram.Connections.Select(c => new ConnectionDto
            {
                Id = c.Id,
                SourceNodeId = c.SourceNodeId,
                TargetNodeId = c.TargetNodeId,
                SourcePortDirection = c.SourcePortDirection,
                TargetPortDirection = c.TargetPortDirection
            }).ToList()
        };
    }

    private SubProgram ConvertSubProgramFromDto(SubProgramDto dto)
    {
        var subProgram = new SubProgram
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            Order = dto.Order,
            ProgramType = dto.ProgramType,
            LocalVariables = dto.LocalVariables ?? [],
            InputParameters = dto.InputParameters ?? [],
            OutputParameters = dto.OutputParameters ?? []
        };

        foreach (var nodeDto in dto.Nodes ?? [])
        {
            subProgram.Nodes.Add(new ActionNodeViewModel
            {
                Id = nodeDto.Id,
                Name = nodeDto.Name,
                ActionType = nodeDto.ActionType,
                X = nodeDto.X,
                Y = nodeDto.Y,
                Width = nodeDto.Width,
                Height = nodeDto.Height,
                Description = nodeDto.Description,
                IsEnabled = nodeDto.IsEnabled,
                TimeoutMs = nodeDto.TimeoutMs,
                RetryCount = nodeDto.RetryCount,
                ErrorHandling = nodeDto.ErrorHandling,
                Parameters = nodeDto.Parameters ?? []
            });
        }

        foreach (var connDto in dto.Connections ?? [])
        {
            subProgram.Connections.Add(new ConnectionViewModel
            {
                Id = connDto.Id,
                SourceNodeId = connDto.SourceNodeId,
                TargetNodeId = connDto.TargetNodeId,
                SourcePortDirection = connDto.SourcePortDirection,
                TargetPortDirection = connDto.TargetPortDirection
            });
        }

        return subProgram;
    }

    #endregion
}

#region DTOs

internal class ProjectFileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public string DeviceConfigPath { get; set; } = "deviceconfig.json";
    public Dictionary<string, object>? GlobalVariables { get; set; }
    public List<SubProjectDto>? SubProjects { get; set; }
}

internal class SubProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public Dictionary<string, object>? Variables { get; set; }
    public List<SubProgramDto>? SubPrograms { get; set; }
}

internal class SubProgramDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public SubProgramType ProgramType { get; set; }
    public Dictionary<string, object>? LocalVariables { get; set; }
    public List<ProgramParameter>? InputParameters { get; set; }
    public List<ProgramParameter>? OutputParameters { get; set; }
    public List<NodeDto>? Nodes { get; set; }
    public List<ConnectionDto>? Connections { get; set; }
}

internal class NodeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ActionType ActionType { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int TimeoutMs { get; set; }
    public int RetryCount { get; set; }
    public ErrorHandling ErrorHandling { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

internal class ConnectionDto
{
    public Guid Id { get; set; }
    public Guid SourceNodeId { get; set; }
    public Guid TargetNodeId { get; set; }
    public int SourcePortDirection { get; set; }
    public int TargetPortDirection { get; set; }
}

#endregion
