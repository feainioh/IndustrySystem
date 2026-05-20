using Castle.DynamicProxy;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Presentation.Wpf.Services;
using Prism.Ioc;

namespace IndustrySystem.Presentation.Wpf.Interceptors;

/// <summary>
/// Helper to register a service interface with an audit proxy wrapping the concrete implementation.
/// </summary>
public static class AuditRegistrationExtensions
{
    private static readonly ProxyGenerator ProxyGenerator = new();

    /// <summary>
    /// Registers TImpl as itself and TInterface with an AuditInterceptor proxy
    /// that logs to both NLog and the database via IOperationLogService.
    /// </summary>
    public static void RegisterWithAudit<TInterface, TImpl>(this IContainerRegistry registry)
        where TInterface : class
        where TImpl : class, TInterface
    {
        registry.Register<TImpl>();
        registry.Register<TInterface>(sp =>
        {
            var target = sp.Resolve<TImpl>();
            var authState = sp.Resolve<IAuthState>();
            var operationLogService = sp.Resolve<IOperationLogService>();
            var interceptor = new AuditInterceptor(authState, operationLogService);
            return ProxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(target, interceptor);
        });
    }
}
