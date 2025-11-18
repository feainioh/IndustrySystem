using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities.Users;
using IndustrySystem.Domain.Repositories;

namespace IndustrySystem.Application.Services;

public class UserAppService : IUserAppService
{
    private readonly IRepository<User> _repo;
    private readonly IUserRoleRepository _userRoleRepo;
    private readonly IRepository<Domain.Entities.Roles.Role> _roleRepo;
    private readonly IMapper _mapper;
    public UserAppService(IRepository<User> repo, IUserRoleRepository userRoleRepo, IRepository<Domain.Entities.Roles.Role> roleRepo, IMapper mapper)
    {
        _repo = repo; _userRoleRepo = userRoleRepo; _roleRepo = roleRepo; _mapper = mapper;
    }

    public async Task<UserDto?> GetAsync(Guid id)
    {
        var entity = await _repo.GetAsync(id);
        return entity is null ? null : _mapper.Map<UserDto>(entity);
    }

    public async Task<List<UserDto>> GetListAsync()
    {
        var list = await _repo.GetListAsync();
        return list.Select(_mapper.Map<UserDto>).ToList();
    }

    public async Task<UserDto> CreateAsync(UserDto input)
    {
        var entity = _mapper.Map<User>(input);
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        var saved = await _repo.InsertAsync(entity);
        return _mapper.Map<UserDto>(saved);
    }

    public async Task<UserDto> UpdateAsync(UserDto input)
    {
        var entity = _mapper.Map<User>(input);
        var saved = await _repo.UpdateAsync(entity);
        return _mapper.Map<UserDto>(saved);
    }

    public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);

    public async Task<Guid[]> GetRoleIdsAsync(Guid userId)
    {
        var ids = await _userRoleRepo.GetRoleIdsByUserIdAsync(userId);
        return ids.ToArray();
    }

    public async Task SetRolesAsync(Guid userId, Guid[] roleIds)
    {
        await _userRoleRepo.SetUserRolesAsync(userId, roleIds);
    }
}
