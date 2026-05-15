using IndustrySystem.Domain.Shared.AbpModules;
using Volo.Abp.Modularity;

namespace IndustrySystem.Application.Contracts.AbpModules;

[DependsOn(typeof(IndustrySystemDomainSharedModule))]
/// <summary>
/// Application.Contracts 层 ABP 模块。
/// 用于承载 DTO 与服务契约。
/// </summary>
public class IndustrySystemApplicationContractsModule : AbpModule
{
}
