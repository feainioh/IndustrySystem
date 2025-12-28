using IndustrySystem.Infrastructure.MotionProgram.Abstractions;
using IndustrySystem.Infrastructure.MotionProgram.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace IndustrySystem.Infrastructure.MotionProgram.AbpModules;

/// <summary>
/// MotionProgram Infrastructure 模块
/// </summary>
public class IndustrySystemInfrastructureMotionProgramModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 JSON 解析器
        context.Services.AddTransient<IMotionProgramJsonParser, MotionProgramJsonParser>();
        
        // 注册 JSON 执行器
        context.Services.AddScoped<IMotionProgramJsonExecutor, MotionProgramJsonExecutor>();
    }
}
