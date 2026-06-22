using AutoMapper;
using Castle.DynamicProxy;
using IndustrySystem.Application.AbpModules;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Application.Profiles;
using IndustrySystem.Application.Services;
using IndustrySystem.Domain.Repositories;
using IndustrySystem.Infrastructure.Communication.Abstractions;
using IndustrySystem.Infrastructure.Communication.Implementations;
using IndustrySystem.Infrastructure.SqlSugar;
using IndustrySystem.Infrastructure.SqlSugar.Abstractions;
using IndustrySystem.Infrastructure.SqlSugar.Implementations;
using IndustrySystem.Infrastructure.SqlSugar.Repositories;
using IndustrySystem.Presentation.Wpf.Interceptors;
using IndustrySystem.Presentation.Wpf.Resources;
using IndustrySystem.Presentation.Wpf.Services;
using IndustrySystem.Presentation.Wpf.ViewModels;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using IndustrySystem.Presentation.Wpf.Views;
using Microsoft.Extensions.Configuration;
using ModernWpf;
using NLog;
using Prism.Dialogs;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using SqlSugar;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Volo.Abp;

namespace IndustrySystem.Presentation.Wpf;

public partial class App : PrismApplication
{
    private bool _shellInitialized;
    private Mutex? _singleInstanceMutex;
    private bool _ownsSingleInstanceMutex;
    private const string SingleInstanceMutexName = @"Global\IndustrySystem.Presentation.Wpf.SingleInstance";

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

        var externalSyncOptions = new ExternalSyncOptions();
        config.GetSection("ExternalSync").Bind(externalSyncOptions);
        containerRegistry.RegisterInstance(externalSyncOptions);

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
                    if (Nullable.GetUnderlyingType(column.PropertyInfo.PropertyType) != null) { column.IsNullable = true; }
                }
            }
        };
        containerRegistry.RegisterInstance<ISqlSugarClient>(new SqlSugarScope(conn));

        containerRegistry.Register(typeof(IRepository<>), typeof(SqlSugarRepository<>));
        containerRegistry.Register<IUserRoleRepository, UserRoleRepository>();
        containerRegistry.Register<IRolePermissionRepository, RolePermissionRepository>();

        containerRegistry.RegisterWithAudit<IRoleAppService, RoleAppService>();
        containerRegistry.RegisterWithAudit<IExperimentTemplateAppService, ExperimentTemplateAppService>();
        containerRegistry.RegisterWithAudit<IExperimentParameterAppService, ExperimentParameterAppService>();
        containerRegistry.RegisterWithAudit<IPermissionAppService, PermissionAppService>();
        containerRegistry.RegisterWithAudit<IUserAppService, UserAppService>();
        containerRegistry.RegisterWithAudit<IExperimentAppService, ExperimentAppService>();
        containerRegistry.RegisterWithAudit<IExperimentGroupAppService, ExperimentGroupAppService>();
        containerRegistry.Register<IExperimentHistoryAppService, ExperimentHistoryAppService>();
        containerRegistry.Register<IAlarmAppService, AlarmAppService>();
        containerRegistry.RegisterWithAudit<IInventoryAppService, InventoryAppService>();
        containerRegistry.RegisterWithAudit<IMaterialAppService, MaterialAppService>();
        containerRegistry.RegisterWithAudit<IShelfAppService, ShelfAppService>();
        containerRegistry.Register<IRunExperimentAppService, RunExperimentAppService>();
        containerRegistry.Register<IOperationLogService, OperationLogService>();
        containerRegistry.RegisterSingleton<IHardwareController, SimulatedHardwareController>();
        // 注册实验组执行模拟服务
        containerRegistry.Register<IExperimentExecutionService, MockExperimentExecutionService>();
        containerRegistry.Register<ICommunicationAppService, CommunicationAppService>();
        containerRegistry.RegisterSingleton<IExternalDataSyncAppService, ExternalDataSyncAppService>();
        containerRegistry.Register<IHttpClient, SimpleHttpClient>();
        containerRegistry.Register<IModbusTcpClient, ModbusTcpClient>();
        containerRegistry.Register<IExternalSyncChannelFactory, ExternalSyncChannelFactory>();

        containerRegistry.RegisterSingleton<IDatabaseInitializer, SqlSugarDatabaseInitializer>();

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));
        containerRegistry.RegisterInstance<IMapper>(mapperConfig.CreateMapper());

        // Register auth services before dialog (LoginViewModel depends on these)
        containerRegistry.RegisterSingleton<IAuthService, AuthService>();
        containerRegistry.RegisterSingleton<IAuthState, AuthState>();
        containerRegistry.RegisterSingleton<IAppSessionService, AppSessionService>();
        containerRegistry.RegisterSingleton<IMainWindowService, MainWindowService>();
        containerRegistry.Register<ShellViewModel>();

        // Register ViewModels
        containerRegistry.RegisterForNavigation<RoleManageView, RoleManageViewModel>(nameof(RoleManageView));
        containerRegistry.RegisterForNavigation<Views.ExperimentTemplateView, ExperimentTemplateViewModel>(nameof(Views.ExperimentTemplateView));
        containerRegistry.RegisterForNavigation<Views.PermissionsView, PermissionsViewModel>(nameof(Views.PermissionsView));
        containerRegistry.RegisterForNavigation<Views.UsersView, UsersViewModel>(nameof(Views.UsersView));
        containerRegistry.RegisterForNavigation<Views.AlarmView, AlarmViewModel>(nameof(Views.AlarmView));
        containerRegistry.RegisterForNavigation<Views.RunExperimentView, RunExperimentViewModel>(nameof(Views.RunExperimentView));
        containerRegistry.RegisterForNavigation<Views.ExperimentsView, ExperimentsViewModel>(nameof(Views.ExperimentsView));
        containerRegistry.RegisterForNavigation<Views.ExperimentHistoryView, ExperimentHistoryViewModel>(nameof(Views.ExperimentHistoryView));
        containerRegistry.RegisterForNavigation<Views.InventoryView, InventoryViewModel>(nameof(Views.InventoryView));
        containerRegistry.RegisterForNavigation<Views.HardwareDebugView, HardwareDebugViewModel>(nameof(Views.HardwareDebugView));
        containerRegistry.RegisterForNavigation<Views.ExperimentGroupsView, ExperimentGroupsViewModel>(nameof(Views.ExperimentGroupsView));
        containerRegistry.RegisterForNavigation<Views.ExperimentConfigView, ExperimentConfigViewModel>(nameof(Views.ExperimentConfigView));
        containerRegistry.RegisterForNavigation<Views.MaterialInfoView, MaterialInfoViewModel>(nameof(Views.MaterialInfoView));
        containerRegistry.RegisterForNavigation<Views.ShelfInfoView, ShelfInfoViewModel>(nameof(Views.ShelfInfoView));
        containerRegistry.RegisterForNavigation<Views.OperationLogsView, OperationLogsViewModel>(nameof(Views.OperationLogsView));
        containerRegistry.RegisterForNavigation<Views.RealtimeDataView, RealtimeDataViewModel>(nameof(Views.RealtimeDataView));
        containerRegistry.RegisterForNavigation<Views.PeripheralDebugView, PeripheralDebugViewModel>(nameof(Views.PeripheralDebugView));
        containerRegistry.RegisterForNavigation<Views.DeviceParamsView, DeviceParamsViewModel>(nameof(Views.DeviceParamsView));
        containerRegistry.RegisterForNavigation<Views.MotionProgramRunView, MotionProgramRunViewModel>(nameof(Views.MotionProgramRunView));
        //ViewModelLocationProvider.Register<Views.RoleManageView, RoleManageViewModel>();
        //ViewModelLocationProvider.Register<Views.ExperimentTemplateView, ExperimentTemplateViewModel>();
        //ViewModelLocationProvider.Register<Views.PermissionsView, PermissionsViewModel>();
        //ViewModelLocationProvider.Register<Views.UsersView, UsersViewModel>();
        //ViewModelLocationProvider.Register<Views.AlarmView, AlarmViewModel>();
        //ViewModelLocationProvider.Register<Views.RunExperimentView, RunExperimentViewModel>();
        //ViewModelLocationProvider.Register<Views.ExperimentsView, ExperimentsViewModel>();
        //ViewModelLocationProvider.Register<Views.ExperimentHistoryView, ExperimentHistoryViewModel>();
        //ViewModelLocationProvider.Register<Views.InventoryView, InventoryViewModel>();
        //ViewModelLocationProvider.Register<Views.HardwareDebugView, HardwareDebugViewModel>();
        //ViewModelLocationProvider.Register<Views.ExperimentGroupsView, ExperimentGroupsViewModel>();
        //ViewModelLocationProvider.Register<Views.ExperimentConfigView, ExperimentConfigViewModel>();
        //ViewModelLocationProvider.Register<Views.MaterialInfoView, MaterialInfoViewModel>();
        //ViewModelLocationProvider.Register<Views.ShelfInfoView, ShelfInfoViewModel>();
        //ViewModelLocationProvider.Register<Views.OperationLogsView, OperationLogsViewModel>();
        //ViewModelLocationProvider.Register<Views.RealtimeDataView, RealtimeDataViewModel>();
        //ViewModelLocationProvider.Register<Views.PeripheralDebugView, PeripheralDebugViewModel>();
        //ViewModelLocationProvider.Register<Views.DeviceParamsView, DeviceParamsViewModel>();

        // Register LoginView as dialog
        containerRegistry.RegisterDialog<LoginView, LoginViewModel>();

        // Register edit dialogs for IDialogService
        containerRegistry.RegisterDialog<Views.Dialogs.UserEditDialog, ViewModels.Dialogs.UserEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.PermissionEditDialog, ViewModels.Dialogs.PermissionEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.RoleEditDialog, ViewModels.Dialogs.RoleEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.MaterialEditDialog, ViewModels.Dialogs.MaterialEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ShelfListDialog, ViewModels.Dialogs.ShelfListDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ShelfEditDialog, ViewModels.Dialogs.ShelfEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.SlotConfigDialog, ViewModels.Dialogs.SlotConfigDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.InventoryEditDialog, ViewModels.Dialogs.InventoryEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.InventoryOutboundDialog, ViewModels.Dialogs.InventoryOutboundDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ContainerListDialog, ViewModels.Dialogs.ContainerListDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ContainerEditDialog, ViewModels.Dialogs.ContainerEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ExperimentTemplateEditDialog, ViewModels.Dialogs.ExperimentTemplateEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ExperimentGroupEditDialog, ViewModels.Dialogs.ExperimentGroupEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ExperimentEditDialog, ViewModels.Dialogs.ExperimentEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ConfirmDialog, ViewModels.Dialogs.ConfirmDialogViewModel>();

        // Register type-specific parameter edit views for region navigation.
        // Keep one dedicated ViewModel per View to avoid shared editor state across types.
        containerRegistry.RegisterForNavigation<Views.Dialogs.ReactionParameterEditDialog, ViewModels.ExperimentParameters.ReactionParameterEditDialogViewModel>(nameof(Views.Dialogs.ReactionParameterEditDialog));
        containerRegistry.RegisterForNavigation<Views.Dialogs.RotaryEvaporationParameterEditDialog, ViewModels.ExperimentParameters.RotaryEvaporationParameterEditDialogViewModel>(nameof(Views.Dialogs.RotaryEvaporationParameterEditDialog));
        containerRegistry.RegisterForNavigation<Views.Dialogs.DetectionParameterEditDialog, ViewModels.ExperimentParameters.DetectionParameterEditDialogViewModel>(nameof(Views.Dialogs.DetectionParameterEditDialog));
        containerRegistry.RegisterForNavigation<Views.Dialogs.FiltrationParameterEditDialog, ViewModels.ExperimentParameters.FiltrationParameterEditDialogViewModel>(nameof(Views.Dialogs.FiltrationParameterEditDialog));
        containerRegistry.RegisterForNavigation<Views.Dialogs.DryingParameterEditDialog, ViewModels.ExperimentParameters.DryingParameterEditDialogViewModel>(nameof(Views.Dialogs.DryingParameterEditDialog));
        containerRegistry.RegisterForNavigation<Views.Dialogs.QuenchingParameterEditDialog, ViewModels.ExperimentParameters.QuenchingParameterEditDialogViewModel>(nameof(Views.Dialogs.QuenchingParameterEditDialog));
        containerRegistry.RegisterForNavigation<Views.Dialogs.ExtractionParameterEditDialog, ViewModels.ExperimentParameters.ExtractionParameterEditDialogViewModel>(nameof(Views.Dialogs.ExtractionParameterEditDialog));
        containerRegistry.RegisterForNavigation<Views.Dialogs.SamplingParameterEditDialog, ViewModels.ExperimentParameters.SamplingParameterEditDialogViewModel>(nameof(Views.Dialogs.SamplingParameterEditDialog));
        containerRegistry.RegisterForNavigation<Views.Dialogs.CentrifugationParameterEditDialog, ViewModels.ExperimentParameters.CentrifugationParameterEditDialogViewModel>(nameof(Views.Dialogs.CentrifugationParameterEditDialog));
        containerRegistry.RegisterForNavigation<Views.Dialogs.CustomDetectionParameterEditDialog, ViewModels.ExperimentParameters.CustomDetectionParameterEditDialogViewModel>(nameof(Views.Dialogs.CustomDetectionParameterEditDialog));
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var logger = LogManager.GetCurrentClassLogger();
        AppVisualThemeService.Apply(AppVisualTheme.Classic);

        var zh = CultureInfo.GetCultureInfo("zh-CN");
        CultureInfo.DefaultThreadCurrentCulture = zh;
        CultureInfo.DefaultThreadCurrentUICulture = zh;
        Strings.Culture = zh;

        if (!TryAcquireSingleInstanceMutex())
        {
            MessageBox.Show(LocalizationProvider.Instance["Msg_AppAlreadyRunning"], Strings.Msg_WarningTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        DispatcherUnhandledException += (s, ex) =>
        {
            logger.Error(ex.Exception, "[UI Thread] Unhandled exception");
            MessageBox.Show(string.Format(LocalizationProvider.Instance["Msg_UnhandledExceptionFormat"], ex.Exception.Message), Strings.Msg_ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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
        // 确保 ViewModelLocator 使用容器来创建 ViewModel 实例
        ViewModelLocationProvider.SetDefaultViewModelFactory((viewModelType) =>
        {
            return Container.Resolve(viewModelType);
        });
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

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_singleInstanceMutex is not null)
            {
                if (_ownsSingleInstanceMutex)
                {
                    try
                    {
                        _singleInstanceMutex.ReleaseMutex();
                    }
                    catch (ApplicationException)
                    {
                    }
                }

                _singleInstanceMutex.Dispose();
                _singleInstanceMutex = null;
            }
        }
        finally
        {
            base.OnExit(e);
        }
    }

    private bool TryAcquireSingleInstanceMutex()
    {
        _singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out var createdNew);
        _ownsSingleInstanceMutex = createdNew;
        return createdNew;
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
        var shell = new Shell()
        {
            DataContext = Container.Resolve<ShellViewModel>(),
            WindowState = WindowState.Maximized
        };

        var regionManager = Container.Resolve<IRegionManager>();

        // User logout/login can reuse the same RegionManager instance.
        // Ensure the previous Shell main region is removed before creating a new one.
        RemoveShellMainRegionIfExists(regionManager);

        RegionManager.SetRegionManager(shell, regionManager);
        InitializeShell(shell);
        shell.Show();
    }

    private static void RemoveShellMainRegionIfExists(IRegionManager regionManager)
    {
        const string regionName = ShellViewModel.MainRegionName;
        if (!regionManager.Regions.ContainsRegionWithName(regionName))
        {
            return;
        }

        var region = regionManager.Regions[regionName];
        var views = region.Views.Cast<object>().ToList();
        foreach (var view in views)
        {
            region.Remove(view);
        }

        regionManager.Regions.Remove(regionName);
    }
}

