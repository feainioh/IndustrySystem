using IndustrySystem.Domain.Shared.AbpModules;
using Volo.Abp.Modularity;

namespace IndustrySystem.Application.Contracts.AbpModules;

[DependsOn(typeof(IndustrySystemDomainSharedModule))]
public class IndustrySystemApplicationContractsModule : AbpModule
{
}
