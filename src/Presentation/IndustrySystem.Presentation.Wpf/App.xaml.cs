using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;
using Volo.Abp;
using IndustrySystem.Application.AbpModules;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using IndustrySystem.Infrastructure.SqlSugar;
using IndustrySystem.Domain.Repositories;
using IndustrySystem.Infrastructure.SqlSugar.Repositories;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Application.Services;
using AutoMapper;
using IndustrySystem.Application.Profiles;
using Prism.Mvvm;
using IndustrySystem.Presentation.Wpf.ViewModels;
using IndustrySystem.Infrastructure.SqlSugar.Abstractions;
using IndustrySystem.Infrastructure.SqlSugar.Implementations;
using IndustrySystem.Infrastructure.Communication.Abstractions;
using IndustrySystem.Infrastructure.Communication.Implementations;
using NLog;
using System.Threading.Tasks;
using System.Windows.Threading;
using ModernWpf;
using System.Globalization;
using System;
using System.Reflection;
using IndustrySystem.Presentation.Wpf.Resources;
using IndustrySystem.Presentation.Wpf.Services;
using Prism.Dialogs;
using WpfDialogService = IndustrySystem.Presentation.Wpf.Services.IDialogService;
using WpfDialogServiceImpl = IndustrySystem.Presentation.Wpf.Services.DialogService;

namespace IndustrySystem.Presentation.Wpf;

public partial class App : PrismApplication
{
    private bool _shellInitialized;

    protected override Window CreateShell() => new Shell(Container);

    protected override void InitializeShell(Window shell)
    {
        if (_shellInitialized || shell is null) return;
        _shellInitialized = true;
        MainWindow = shell;
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
        containerRegistry.RegisterInstance<IConfiguration>(config);

        var options = new SqlSugarOptions();
        config.GetSection("SqlSugar").Bind(options);
        containerRegistry.RegisterInstance(options);
        var conn = new ConnectionConfig
        {
            ConnectionString = options.ConnectionString,
            DbType = DbType.MySql,
            IsAutoCloseConnection = true,
            ConfigureExternalServices = new ConfigureExternalServices
            {
                EntityService = (entityProp, column) =>
                {
                    // entityProp is PropertyInfo, get declaring type
                    var t = (entityProp as PropertyInfo)?.DeclaringType;

                    // Primary Keys
                    if (column.PropertyName == "Id")
                    {
                        column.IsPrimarykey = true;
                        column.IsIdentity = false;
                    }
                    if (t == typeof(Domain.Entities.Users.UserRole))
                    {
                        if (column.PropertyName == nameof(Domain.Entities.Users.UserRole.UserId) ||
                            column.PropertyName == nameof(Domain.Entities.Users.UserRole.RoleId))
                        {
                            column.IsPrimarykey = true; // composite PK
                            column.IsIdentity = false;
                        }
                    }
                    if (t == typeof(Domain.Entities.Roles.RolePermission))
                    {
                        if (column.PropertyName == nameof(Domain.Entities.Roles.RolePermission.RoleId) ||
                            column.PropertyName == nameof(Domain.Entities.Roles.RolePermission.PermissionId))
                        {
                            column.IsPrimarykey = true; // composite PK
                            column.IsIdentity = false;
                        }
                    }

                    // Timestamps defaults / nullability
                    if (column.PropertyInfo.PropertyType == typeof(DateTime))
                    {
                        if (column.PropertyName == nameof(Domain.Entities.Users.User.CreatedAt) ||
                            column.PropertyName == nameof(Domain.Entities.Roles.Role.CreatedAt) ||
                            column.PropertyName == nameof(Domain.Entities.Permissions.Permission.CreatedAt) ||
                            column.PropertyName == nameof(Domain.Entities.Experiments.Experiment.CreatedAt) ||
                            column.PropertyName == nameof(Domain.Entities.Experiments.ExperimentTemplate.CreatedAt) ||
                            column.PropertyName == nameof(Domain.Entities.Experiments.ExperimentGroup.CreatedAt) ||
                            (t == typeof(Domain.Entities.Users.UserRole) && column.PropertyName == nameof(Domain.Entities.Users.UserRole.CreatedAt)) ||
                            (t == typeof(Domain.Entities.Roles.RolePermission) && column.PropertyName == nameof(Domain.Entities.Roles.RolePermission.CreatedAt)))
                        {
                            column.IsNullable = false;
                            column.DefaultValue = "CURRENT_TIMESTAMP";
                        }
                        if (column.PropertyName == nameof(Domain.Entities.Users.User.UpdatedAt) ||
                            column.PropertyName == nameof(Domain.Entities.Roles.Role.UpdatedAt) ||
                            column.PropertyName == nameof(Domain.Entities.Permissions.Permission.UpdatedAt) ||
                            column.PropertyName == nameof(Domain.Entities.Experiments.Experiment.UpdatedAt) ||
                            column.PropertyName == nameof(Domain.Entities.Experiments.ExperimentTemplate.UpdatedAt) ||
                            column.PropertyName == nameof(Domain.Entities.Experiments.ExperimentGroup.UpdatedAt) ||
                            (t == typeof(Domain.Entities.Users.UserRole) && column.PropertyName == nameof(Domain.Entities.Users.UserRole.UpdatedAt)) ||
                            (t == typeof(Domain.Entities.Roles.RolePermission) && column.PropertyName == nameof(Domain.Entities.Roles.RolePermission.UpdatedAt)))
                        {
                            column.IsNullable = true;
                        }
                    }
                    if (column.PropertyInfo.PropertyType == typeof(DateTime?))
                    {
                        column.IsNullable = true;
                    }
                }
            }
        };
        containerRegistry.RegisterInstance<ISqlSugarClient>(new SqlSugarClient(conn));

        // register repositories
        containerRegistry.Register(typeof(IRepository<>), typeof(SqlSugarRepository<>));
        containerRegistry.Register<IUserRoleRepository, UserRoleRepository>();
        containerRegistry.Register<IRolePermissionRepository, RolePermissionRepository>();

        containerRegistry.Register<IRoleAppService, RoleAppService>();
        containerRegistry.Register<IExperimentTemplateAppService, ExperimentTemplateAppService>();
        containerRegistry.Register<IPermissionAppService, PermissionAppService>();
        containerRegistry.Register<IUserAppService, UserAppService>();
        containerRegistry.Register<IExperimentAppService, ExperimentAppService>();
        containerRegistry.Register<IExperimentHistoryAppService, ExperimentHistoryAppService>();
        containerRegistry.Register<IAlarmAppService, AlarmAppService>();
        containerRegistry.Register<IInventoryAppService, InventoryAppService>();
        containerRegistry.Register<IRunExperimentAppService, RunExperimentAppService>();
        containerRegistry.Register<ICommunicationAppService, CommunicationAppService>();
        containerRegistry.Register<IModbusTcpClient, ModbusTcpClient>();

        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.RoleManageView, RoleManageViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.ExperimentTemplateView, ExperimentTemplateViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.PermissionsView, PermissionsViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.UsersView, UsersViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.ExperimentsView, ExperimentsViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.RunExperimentView, RunExperimentViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.ExperimentHistoryView, ExperimentHistoryViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.AlarmView, AlarmViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.HardwareDebugView, HardwareDebugViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.InventoryView, InventoryViewModel>();
        // Added registrations for remaining Navigation targets
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.RealtimeDataView, RealtimeDataViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.ExperimentConfigView, ExperimentConfigViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.ExperimentGroupsView, ExperimentGroupsViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.OperationLogsView, OperationLogsViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.MaterialInfoView, MaterialInfoViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.ShelfInfoView, ShelfInfoViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.DeviceParamsView, DeviceParamsViewModel>();
        ViewModelLocationProvider.Register<IndustrySystem.Presentation.Wpf.Views.PeripheralDebugView, PeripheralDebugViewModel>();

        containerRegistry.RegisterSingleton<IDatabaseInitializer, SqlSugarDatabaseInitializer>();

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
        containerRegistry.RegisterInstance<IMapper>(mapperConfig.CreateMapper());

        containerRegistry.RegisterSingleton<WpfDialogService, WpfDialogServiceImpl>();

        containerRegistry.RegisterSingleton<IAuthService, AuthService>();
        containerRegistry.RegisterSingleton<IAuthState, AuthState>();
        ViewModelLocationProvider.Register<Views.LoginView, LoginViewModel>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var logger = LogManager.GetCurrentClassLogger();
        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;

        // Force default to Chinese
        var zh = CultureInfo.GetCultureInfo("zh-CN");
        CultureInfo.DefaultThreadCurrentCulture = zh;
        CultureInfo.DefaultThreadCurrentUICulture = zh;
        Strings.Culture = zh;

        DispatcherUnhandledException += (s, ex) =>
        {
            logger.Error(ex.Exception, "[UI Thread] Unhandled exception");
            MessageBox.Show($"发生未处理异常: {ex.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            ex.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
        {
            if (ex.ExceptionObject is Exception ex2)
                logger.Fatal(ex2, "[AppDomain] Unhandled exception");
        };
        TaskScheduler.UnobservedTaskException += (s, ex) =>
        {
            logger.Error(ex.Exception, "[TaskScheduler] Unobserved exception");
            ex.SetObserved();
        };

        Initialize();

        Dispatcher.BeginInvoke(new Action(() =>
        {
            MainWindow?.Show();
        }), DispatcherPriority.ApplicationIdle);

        Dispatcher.BeginInvoke(new Action(async () =>
        {
            using var abpApp = AbpApplicationFactory.Create<IndustrySystemApplicationModule>();
            abpApp.Initialize();
            try
            {
                var initializer = Container.Resolve<IDatabaseInitializer>();
                await initializer.InitializeAsync();
            }
            catch (Exception ex2)
            {
                logger.Error(ex2, "[DB Init] Failed during application startup.");
            }
        }), DispatcherPriority.ApplicationIdle);
    }
}

