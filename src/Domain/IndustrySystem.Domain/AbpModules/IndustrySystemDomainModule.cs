using IndustrySystem.Domain.Shared.AbpModules;
using Volo.Abp.Modularity;

namespace IndustrySystem.Domain.AbpModules;

[DependsOn(typeof(IndustrySystemDomainSharedModule))]
public class IndustrySystemDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Domain services registrations (if any)
    }
}
