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
        _repo = repo; _rolePermRepo = rolePermRepo; _mapper = mapper;
    }

    public async Task<RoleDto?> GetAsync(Guid id)
    {
        var entity = await _repo.GetAsync(id);
        return entity is null ? null : _mapper.Map<RoleDto>(entity);
    }

    public async Task<List<RoleDto>> GetListAsync()
    {
        var list = await _repo.GetListAsync();
        return list.Select(_mapper.Map<RoleDto>).ToList();
    }

    public async Task<RoleDto> CreateAsync(RoleDto input)
    {
        var entity = _mapper.Map<Role>(input);
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        var saved = await _repo.InsertAsync(entity);
        return _mapper.Map<RoleDto>(saved);
    }

    public async Task<RoleDto> UpdateAsync(RoleDto input)
    {
        var entity = _mapper.Map<Role>(input);
        var saved = await _repo.UpdateAsync(entity);
        return _mapper.Map<RoleDto>(saved);
    }

    public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);

    public async Task<Guid[]> GetPermissionIdsAsync(Guid roleId)
    {
        var ids = await _rolePermRepo.GetPermissionIdsByRoleIdAsync(roleId);
        return ids.ToArray();
    }

    public async Task SetPermissionsAsync(Guid roleId, Guid[] permissionIds)
    {
        await _rolePermRepo.SetRolePermissionsAsync(roleId, permissionIds);
    }
}
