using IndustrySystem.Domain.Shared.AbpModules;
using Volo.Abp.Modularity;

namespace IndustrySystem.Domain.AbpModules;

[DependsOn(typeof(IndustrySystemDomainSharedModule))]
/// <summary>
/// Domain 层 ABP 模块。
/// 预留领域服务与策略注册入口。
/// </summary>
public class IndustrySystemDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Domain services registrations (if any)
    }
}
