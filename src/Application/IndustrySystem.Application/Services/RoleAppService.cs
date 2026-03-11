using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities.Roles;
using IndustrySystem.Domain.Repositories;

namespace IndustrySystem.Application.Services;

public class RoleAppService : IRoleAppService
{
    private readonly IRepository<Role> _repo;
    private readonly IRolePermissionRepository _rolePermRepo;
    private readonly IMapper _mapper;

    public RoleAppService(IRepository<Role> repo, IRolePermissionRepository rolePermRepo, IMapper mapper)
    {
        _repo = repo;
        _rolePermRepo = rolePermRepo;
        _mapper = mapper;
    }

    /// <summary>
    /// 根据Id查询单个角色。
    /// </summary>
    public async Task<RoleDto?> GetAsync(Guid id)
    {
        var entity = await _repo.GetAsync(id);
        return entity is null ? null : _mapper.Map<RoleDto>(entity);
    }

    /// <summary>
    /// 查询全部角色列表。
    /// </summary>
    public async Task<List<RoleDto>> GetListAsync()
    {
        var list = await _repo.GetListAsync();
        return list.Select(_mapper.Map<RoleDto>).ToList();
    }

    /// <summary>
    /// 创建角色，若输入Id为空则自动生成新Id。
    /// </summary>
    public async Task<RoleDto> CreateAsync(RoleDto input)
    {
        var entity = _mapper.Map<Role>(input);
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        var saved = await _repo.InsertAsync(entity);
        return _mapper.Map<RoleDto>(saved);
    }

    /// <summary>
    /// 更新角色。
    /// </summary>
    public async Task<RoleDto> UpdateAsync(RoleDto input)
    {
        var entity = _mapper.Map<Role>(input);
        var saved = await _repo.UpdateAsync(entity);
        return _mapper.Map<RoleDto>(saved);
    }

    /// <summary>
    /// 删除角色。
    /// </summary>
    public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);

    /// <summary>
    /// 查询角色已分配权限Id列表。
    /// </summary>
    public async Task<Guid[]> GetPermissionIdsAsync(Guid roleId)
    {
        var ids = await _rolePermRepo.GetPermissionIdsByRoleIdAsync(roleId);
        return ids.ToArray();
    }

    /// <summary>
    /// 设置角色权限关系（覆盖式）。
    /// </summary>
    public async Task SetPermissionsAsync(Guid roleId, Guid[] permissionIds)
    {
        await _rolePermRepo.SetRolePermissionsAsync(roleId, permissionIds);
    }
}
