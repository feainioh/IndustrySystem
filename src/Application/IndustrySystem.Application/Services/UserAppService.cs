using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities.Users;
using IndustrySystem.Domain.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace IndustrySystem.Application.Services;

public class UserAppService : IUserAppService
{
    private readonly IRepository<User> _repo;
    private readonly IUserRoleRepository _userRoleRepo;
    private readonly IRepository<Domain.Entities.Roles.Role> _roleRepo;
    private readonly IMapper _mapper;

    public UserAppService(
        IRepository<User> repo,
        IUserRoleRepository userRoleRepo,
        IRepository<Domain.Entities.Roles.Role> roleRepo,
        IMapper mapper)
    {
        _repo = repo;
        _userRoleRepo = userRoleRepo;
        _roleRepo = roleRepo;
        _mapper = mapper;
    }

    /// <summary>
    /// 根据Id查询单个用户。
    /// </summary>
    public async Task<UserDto?> GetAsync(Guid id)
    {
        var entity = await _repo.GetAsync(id);
        return entity is null ? null : _mapper.Map<UserDto>(entity);
    }

    /// <summary>
    /// 查询全部用户列表。
    /// </summary>
    public async Task<List<UserDto>> GetListAsync()
    {
        var list = await _repo.GetListAsync();
        return list.Select(_mapper.Map<UserDto>).ToList();
    }

    /// <summary>
    /// 创建用户，若输入Id为空则自动生成新Id。
    /// 默认密码为 123。
    /// </summary>
    public async Task<UserDto> CreateAsync(UserDto input)
    {
        var entity = _mapper.Map<User>(input);
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        entity.PasswordHash = HashPassword("123");
        var saved = await _repo.InsertAsync(entity);
        return _mapper.Map<UserDto>(saved);
    }

    /// <summary>
    /// 更新用户。
    /// </summary>
    public async Task<UserDto> UpdateAsync(UserDto input)
    {
        var entity = _mapper.Map<User>(input);
        var old = await _repo.GetAsync(entity.Id);
        if (old != null)
        {
            entity.PasswordHash = old.PasswordHash;
        }
        var saved = await _repo.UpdateAsync(entity);
        return _mapper.Map<UserDto>(saved);
    }

    /// <summary>
    /// 删除用户。
    /// </summary>
    public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);

    /// <summary>
    /// 查询用户已分配角色Id列表。
    /// </summary>
    public async Task<Guid[]> GetRoleIdsAsync(Guid userId)
    {
        var ids = await _userRoleRepo.GetRoleIdsByUserIdAsync(userId);
        return ids.ToArray();
    }

    /// <summary>
    /// 设置用户角色关系（覆盖式）。
    /// </summary>
    public async Task SetRolesAsync(Guid userId, Guid[] roleIds)
    {
        await _userRoleRepo.SetUserRolesAsync(userId, roleIds);
    }

    /// <summary>
    /// 校验用户名密码。
    /// </summary>
    public async Task<bool> ValidateCredentialsAsync(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) return false;
        var users = await _repo.GetListAsync();
        var user = users.FirstOrDefault(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        if (user == null || !user.IsActive) return false;
        return string.Equals(user.PasswordHash, HashPassword(password), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 修改用户密码（需校验旧密码）。
    /// </summary>
    public async Task ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(oldPassword))
        {
            throw new ArgumentException("旧密码不能为空", nameof(oldPassword));
        }
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            throw new ArgumentException("新密码不能为空", nameof(newPassword));
        }

        var user = await _repo.GetAsync(userId) ?? throw new InvalidOperationException("用户不存在");
        var oldHash = HashPassword(oldPassword);
        if (!string.Equals(user.PasswordHash, oldHash, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("旧密码错误");
        }

        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(user);
    }

    private static string HashPassword(string plain)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(plain);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
