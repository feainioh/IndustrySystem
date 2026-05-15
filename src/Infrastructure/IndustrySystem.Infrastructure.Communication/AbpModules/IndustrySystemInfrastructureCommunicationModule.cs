using IndustrySystem.Infrastructure.Communication.Abstractions;
using IndustrySystem.Infrastructure.Communication.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace IndustrySystem.Infrastructure.Communication.AbpModules;

/// <summary>
/// 通信基础设施模块。
/// 统一注册 TCP/HTTP/Modbus/CAN/EtherCAT 客户端实现。
/// </summary>
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
