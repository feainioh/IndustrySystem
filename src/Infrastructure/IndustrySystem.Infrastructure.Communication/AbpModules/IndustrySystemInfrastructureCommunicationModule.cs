using IndustrySystem.Infrastructure.Communication.Abstractions;
using IndustrySystem.Infrastructure.Communication.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace IndustrySystem.Infrastructure.Communication.AbpModules;

public class IndustrySystemInfrastructureCommunicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ICanClient, CanClient>();
        context.Services.AddTransient<IEthercatClient, EthercatClient>();
        context.Services.AddTransient<IModbusTcpClient, ModbusTcpClient>();
        context.Services.AddTransient<IHttpClient, SimpleHttpClient>();
        context.Services.AddTransient<ITcpClient, SimpleTcpClient>();
    }
}
