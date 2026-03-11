using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities.Permissions;
using IndustrySystem.Domain.Repositories;

namespace IndustrySystem.Application.Services;

public class PermissionAppService : IPermissionAppService
{
    private readonly IRepository<Permission> _repo;
    private readonly IMapper _mapper;

    public PermissionAppService(IRepository<Permission> repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    /// <summary>
    /// 根据Id查询单个权限。
    /// </summary>
    public async Task<PermissionDto?> GetAsync(Guid id)
    {
        var entity = await _repo.GetAsync(id);
        return entity is null ? null : _mapper.Map<PermissionDto>(entity);
    }

    /// <summary>
    /// 查询全部权限列表。
    /// </summary>
    public async Task<List<PermissionDto>> GetListAsync()
    {
        var list = await _repo.GetListAsync();
        return list.Select(_mapper.Map<PermissionDto>).ToList();
    }

    /// <summary>
    /// 创建权限，若输入Id为空则自动生成新Id。
    /// </summary>
    public async Task<PermissionDto> CreateAsync(PermissionDto input)
    {
        var entity = _mapper.Map<Permission>(input);
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        var saved = await _repo.InsertAsync(entity);
        return _mapper.Map<PermissionDto>(saved);
    }

    /// <summary>
    /// 更新权限。
    /// </summary>
    public async Task<PermissionDto> UpdateAsync(PermissionDto input)
    {
        var entity = _mapper.Map<Permission>(input);
        var saved = await _repo.UpdateAsync(entity);
        return _mapper.Map<PermissionDto>(saved);
    }

    /// <summary>
    /// 删除权限。
    /// </summary>
    public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);
}
