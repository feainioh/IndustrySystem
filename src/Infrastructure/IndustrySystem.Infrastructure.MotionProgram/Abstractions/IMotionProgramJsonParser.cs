using IndustrySystem.Application.Contracts.Dtos.MotionProgram;

namespace IndustrySystem.Infrastructure.MotionProgram.Abstractions;

/// <summary>
/// MotionProgram JSON 解析器接口
/// </summary>
public interface IMotionProgramJsonParser
{
    /// <summary>
    /// 从文件路径解析 JSON 文件
    /// </summary>
    Task<MotionProgramDto> ParseFromFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 从 JSON 字符串解析
    /// </summary>
    Task<MotionProgramDto> ParseFromJsonAsync(string json, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 验证 JSON 文件格式
    /// </summary>
    Task<bool> ValidateJsonAsync(string json, CancellationToken cancellationToken = default);
}
