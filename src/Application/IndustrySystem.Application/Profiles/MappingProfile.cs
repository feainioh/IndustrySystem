using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Domain.Entities.Inventory;
using IndustrySystem.Domain.Entities.Materials;
using IndustrySystem.Domain.Entities.Roles;
using IndustrySystem.Domain.Entities.Experiments;
using IndustrySystem.Domain.Entities.Permissions;
using IndustrySystem.Domain.Entities.Shelves;
using IndustrySystem.Domain.Entities.Users;
using System.Linq;

namespace IndustrySystem.Application.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Role, RoleDto>().ReverseMap();

        CreateMap<Experiment, ExperimentTemplateDto>()
            .ForCtorParam(nameof(ExperimentTemplateDto.Description), opt => opt.MapFrom(src => (string?)null));
        CreateMap<ExperimentTemplateDto, Experiment>()
            .ForMember(dest => dest.GroupId, opt => opt.Ignore());

        CreateMap<ExperimentGroup, ExperimentGroupDto>()
            .ForCtorParam(nameof(ExperimentGroupDto.StepExperimentIds), opt => opt.MapFrom(src => src.StepExperimentIdList))
            .ForCtorParam(nameof(ExperimentGroupDto.StepDisplay), opt => opt.MapFrom(_ => string.Empty))
            .ReverseMap()
            .ForMember(dest => dest.StepExperimentIds, opt => opt.Ignore())
            .AfterMap((src, dest) => dest.StepExperimentIdList = src.StepExperimentIds.ToList());

        CreateMap<Permission, PermissionDto>().ReverseMap();
        CreateMap<User, UserDto>().ReverseMap();
        CreateMap<Material, MaterialDto>().ReverseMap();
        CreateMap<InventoryRecord, InventoryRecordDto>().ReverseMap();
        CreateMap<ContainerInfo, ContainerInfoDto>().ReverseMap();
        CreateMap<ShelfConfig, ShelfConfigDto>().ReverseMap();
    }
}

