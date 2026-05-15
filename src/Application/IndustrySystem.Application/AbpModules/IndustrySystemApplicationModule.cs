using IndustrySystem.Application.Contracts.AbpModules;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Application.Services;
using IndustrySystem.Domain.AbpModules;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace IndustrySystem.Application.AbpModules;

[DependsOn(typeof(IndustrySystemDomainModule), typeof(IndustrySystemApplicationContractsModule), typeof(AbpAutoMapperModule))]
/// <summary>
/// Application 层 ABP 模块。
/// 负责应用服务与对象映射注册。
/// </summary>
public class IndustrySystemApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterAutoMapper(context);
        RegisterApplicationServices(context.Services);
    }

    private void RegisterAutoMapper(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<IndustrySystemApplicationModule>();
        Configure<AbpAutoMapperOptions>(options => { options.AddMaps<IndustrySystemApplicationModule>(); });
    }

    private static void RegisterApplicationServices(IServiceCollection services)
    {
        // Core application services
        services.AddScoped<IRoleAppService, RoleAppService>();
        services.AddScoped<IExperimentTemplateAppService, ExperimentTemplateAppService>();
        services.AddScoped<IPermissionAppService, PermissionAppService>();
        services.AddScoped<IUserAppService, UserAppService>();

        // Motion program related services
        services.AddScoped<IMotionProgramAppService, MotionProgramAppService>();
        services.AddScoped<IMotionProgramExecutor, MotionProgramExecutor>();
        services.AddScoped<IHardwareController, SimulatedHardwareController>();
    }
}
