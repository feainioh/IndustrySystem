using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Dialogs;
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
using IndustrySystem.Presentation.Wpf.Views;
using IndustrySystem.Presentation.Wpf.Services;

namespace IndustrySystem.Presentation.Wpf;

public partial class App : PrismApplication
{
    private bool _shellInitialized;

    protected override Window CreateShell()
    {
        // Don't create Shell yet - show login first
        return null!;
    }

    protected override void InitializeShell(Window shell)
    {
        // Will be called after login success
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
                    var t = (entityProp as PropertyInfo)?.DeclaringType;
                    if (column.PropertyName == "Id") { column.IsPrimarykey = true; column.IsIdentity = false; }
                    if (t == typeof(Domain.Entities.Users.UserRole))
                    {
                        if (column.PropertyName == nameof(Domain.Entities.Users.UserRole.UserId) ||
                            column.PropertyName == nameof(Domain.Entities.Users.UserRole.RoleId))
                        { column.IsPrimarykey = true; column.IsIdentity = false; }
                    }
                    if (t == typeof(Domain.Entities.Roles.RolePermission))
                    {
                        if (column.PropertyName == nameof(Domain.Entities.Roles.RolePermission.RoleId) ||
                            column.PropertyName == nameof(Domain.Entities.Roles.RolePermission.PermissionId))
                        { column.IsPrimarykey = true; column.IsIdentity = false; }
                    }
                    if (column.PropertyInfo.PropertyType == typeof(DateTime?)) { column.IsNullable = true; }
                }
            }
        };
        containerRegistry.RegisterInstance<ISqlSugarClient>(new SqlSugarClient(conn));

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

        containerRegistry.RegisterSingleton<IDatabaseInitializer, SqlSugarDatabaseInitializer>();

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
        containerRegistry.RegisterInstance<IMapper>(mapperConfig.CreateMapper());

        // Register auth services before dialog (LoginViewModel depends on these)
        containerRegistry.RegisterSingleton<IAuthService, AuthService>();
        containerRegistry.RegisterSingleton<IAuthState, AuthState>();

        // Register ViewModels
        ViewModelLocationProvider.Register<Views.RoleManageView, RoleManageViewModel>();
        ViewModelLocationProvider.Register<Views.ExperimentTemplateView, ExperimentTemplateViewModel>();
        ViewModelLocationProvider.Register<Views.PermissionsView, PermissionsViewModel>();
        ViewModelLocationProvider.Register<Views.UsersView, UsersViewModel>();
        ViewModelLocationProvider.Register<Views.AlarmView, AlarmViewModel>();
        ViewModelLocationProvider.Register<Views.RunExperimentView, RunExperimentViewModel>();
        ViewModelLocationProvider.Register<Views.ExperimentsView, ExperimentsViewModel>();
        ViewModelLocationProvider.Register<Views.ExperimentHistoryView, ExperimentHistoryViewModel>();
        ViewModelLocationProvider.Register<Views.InventoryView, InventoryViewModel>();
        ViewModelLocationProvider.Register<Views.HardwareDebugView, HardwareDebugViewModel>();
        ViewModelLocationProvider.Register<Views.ExperimentGroupsView, ExperimentGroupsViewModel>();
        ViewModelLocationProvider.Register<Views.ExperimentConfigView, ExperimentConfigViewModel>();
        ViewModelLocationProvider.Register<Views.MaterialInfoView, MaterialInfoViewModel>();
        ViewModelLocationProvider.Register<Views.ShelfInfoView, ShelfInfoViewModel>();
        ViewModelLocationProvider.Register<Views.OperationLogsView, OperationLogsViewModel>();
        ViewModelLocationProvider.Register<Views.RealtimeDataView, RealtimeDataViewModel>();
        ViewModelLocationProvider.Register<Views.PeripheralDebugView, PeripheralDebugViewModel>();
        ViewModelLocationProvider.Register<Views.DeviceParamsView, DeviceParamsViewModel>();

        // Register LoginView as dialog
        containerRegistry.RegisterDialog<LoginView, LoginViewModel>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var logger = LogManager.GetCurrentClassLogger();
        ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;

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

        // Initialize database first, then show login
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

            // Show login dialog
            ShowLoginDialog();
        }), DispatcherPriority.ApplicationIdle);
    }

    public void ShowLoginDialog()
    {
        var dialogService = Container.Resolve<IDialogService>();
        dialogService.ShowDialog(nameof(LoginView), result =>
        {
            if (result.Result == ButtonResult.OK)
            {
                // Login successful - show shell
                ShowShellWindow();
            }
            else
            {
                // User cancelled login - shutdown application
                Shutdown();
            }
        });
    }

    private void ShowShellWindow()
    {
        var shell = new Shell(Container);
        InitializeShell(shell);
        shell.Show();
    }
}

