using System.Text.Json;
using IndustrySystem.Application.Contracts.Dtos.MotionProgram;
using Microsoft.Extensions.Logging;

namespace IndustrySystem.Infrastructure.MotionProgram.Implementations;

/// <summary>
/// MotionProgram JSON 解析器实现
/// </summary>
public class MotionProgramJsonParser : IMotionProgramJsonParser
{
    private readonly ILogger<MotionProgramJsonParser> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public MotionProgramJsonParser(ILogger<MotionProgramJsonParser> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
    }

    public async Task<MotionProgramDto> ParseFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"文件不存在: {filePath}", filePath);
        }

        _logger.LogInformation("正在解析 JSON 文件: {FilePath}", filePath);

        try
        {
            var json = await File.ReadAllTextAsync(filePath, cancellationToken);
            return await ParseFromJsonAsync(json, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析 JSON 文件失败: {FilePath}", filePath);
            throw new InvalidOperationException($"解析 JSON 文件失败: {ex.Message}", ex);
        }
    }

    public Task<MotionProgramDto> ParseFromJsonAsync(string json, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON 字符串不能为空", nameof(json));
        }

        _logger.LogDebug("正在解析 JSON 字符串");

        try
        {
            var program = JsonSerializer.Deserialize<MotionProgramDto>(json, _jsonOptions);
            
            if (program == null)
            {
                throw new InvalidOperationException("JSON 反序列化结果为 null");
            }

            // 验证必要字段
            ValidateProgram(program);

            _logger.LogInformation("成功解析 MotionProgram: {Name} (ID: {Id})", program.Name, program.Id);
            
            return Task.FromResult(program);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON 反序列化失败");
            throw new InvalidOperationException($"JSON 格式错误: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析 JSON 失败");
            throw;
        }
    }

    public Task<bool> ValidateJsonAsync(string json, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Task.FromResult(false);
        }

        try
        {
            var program = JsonSerializer.Deserialize<MotionProgramDto>(json, _jsonOptions);
            
            if (program == null)
            {
                return Task.FromResult(false);
            }

            ValidateProgram(program);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private void ValidateProgram(MotionProgramDto program)
    {
        if (string.IsNullOrWhiteSpace(program.Name))
        {
            throw new InvalidOperationException("程序名称不能为空");
        }

        if (program.Nodes == null)
        {
            throw new InvalidOperationException("节点列表不能为 null");
        }

        if (program.Connections == null)
        {
            throw new InvalidOperationException("连接列表不能为 null");
        }

        if (program.Variables == null)
        {
            throw new InvalidOperationException("变量字典不能为 null");
        }

        // 验证节点 ID 唯一性
        var nodeIds = program.Nodes.Select(n => n.Id).ToList();
        if (nodeIds.Count != nodeIds.Distinct().Count())
        {
            throw new InvalidOperationException("节点 ID 必须唯一");
        }

        // 验证连接引用的节点存在
        var allNodeIds = nodeIds.ToHashSet();
        foreach (var connection in program.Connections)
        {
            if (!allNodeIds.Contains(connection.SourceNodeId))
            {
                throw new InvalidOperationException($"连接引用的源节点不存在: {connection.SourceNodeId}");
            }

            if (!allNodeIds.Contains(connection.TargetNodeId))
            {
                throw new InvalidOperationException($"连接引用的目标节点不存在: {connection.TargetNodeId}");
            }
        }
    }
}
