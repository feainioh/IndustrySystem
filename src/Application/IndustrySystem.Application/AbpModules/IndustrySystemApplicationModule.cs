using IndustrySystem.Application.Contracts.AbpModules;
using IndustrySystem.Domain.AbpModules;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace IndustrySystem.Application.AbpModules;

[DependsOn(typeof(IndustrySystemDomainModule), typeof(IndustrySystemApplicationContractsModule), typeof(AbpAutoMapperModule))]
public class IndustrySystemApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAutoMapperObjectMapper<IndustrySystemApplicationModule>();
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<IndustrySystemApplicationModule>();
        });

        // Register application services
        context.Services.AddScoped<IndustrySystem.Application.Contracts.Services.IRoleAppService, IndustrySystem.Application.Services.RoleAppService>();
        context.Services.AddScoped<IndustrySystem.Application.Contracts.Services.IExperimentTemplateAppService, IndustrySystem.Application.Services.ExperimentTemplateAppService>();
        context.Services.AddScoped<IndustrySystem.Application.Contracts.Services.IPermissionAppService, IndustrySystem.Application.Services.PermissionAppService>();
        context.Services.AddScoped<IndustrySystem.Application.Contracts.Services.IUserAppService, IndustrySystem.Application.Services.UserAppService>();
        
        // Register motion program services
        context.Services.AddScoped<IndustrySystem.Application.Contracts.Services.IMotionProgramAppService, IndustrySystem.Application.Services.MotionProgramAppService>();
        context.Services.AddScoped<IndustrySystem.Application.Contracts.Services.IMotionProgramExecutor, IndustrySystem.Application.Services.MotionProgramExecutor>();
        context.Services.AddScoped<IndustrySystem.Application.Contracts.Services.IHardwareController, IndustrySystem.Application.Services.SimulatedHardwareController>();
    }
}
