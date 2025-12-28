using System.Windows;
using AutoMapper;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.DryIoc;
using Prism.Ioc;

namespace IndustrySystem.MotionDesigner;

public partial class App : PrismApplication
{
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // Register logging
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Debug));
        
        // Register AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MotionProgramAppService).Assembly);
        });
        var mapper = mapperConfig.CreateMapper();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        containerRegistry.RegisterInstance(serviceProvider.GetRequiredService<ILoggerFactory>());
        containerRegistry.Register(typeof(ILogger<>), typeof(Logger<>));
        containerRegistry.RegisterInstance<IMapper>(mapper);
        
        // Register services
        containerRegistry.RegisterSingleton<IHardwareController, SimulatedHardwareController>();
        containerRegistry.RegisterSingleton<IMotionProgramAppService, MotionProgramAppService>();
        containerRegistry.RegisterSingleton<IMotionProgramExecutor, MotionProgramExecutor>();
        
        // Register services
        containerRegistry.RegisterSingleton<Services.IDeviceConfigService, Services.DeviceConfigService>();
        
        // Register ViewModels
        containerRegistry.Register<ViewModels.DesignerViewModel>();
        containerRegistry.Register<ViewModels.DeviceDebugViewModel>();
        
        // Register views
        containerRegistry.RegisterForNavigation<Views.DesignerView>("DesignerView");
        containerRegistry.RegisterForNavigation<Views.DeviceDebugView>("DeviceDebugView");

        containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
    }
}
