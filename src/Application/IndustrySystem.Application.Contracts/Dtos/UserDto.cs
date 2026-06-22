namespace IndustrySystem.Application.Contracts.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    /// <summary>新建或修改密码时传入。留空/null 表示不修改现有密码。</summary>
    public string? Password { get; set; }
}
