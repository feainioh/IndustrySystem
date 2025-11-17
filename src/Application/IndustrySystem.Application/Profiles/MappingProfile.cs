using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Domain.Entities.Roles;
using IndustrySystem.Domain.Entities.Experiments;
using IndustrySystem.Domain.Entities.Permissions;
using IndustrySystem.Domain.Entities.Users;

namespace IndustrySystem.Application.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Role, RoleDto>().ReverseMap();
        CreateMap<ExperimentTemplate, ExperimentTemplateDto>().ReverseMap();
        CreateMap<Permission, PermissionDto>().ReverseMap();
        CreateMap<User, UserDto>().ReverseMap();
    }
}
