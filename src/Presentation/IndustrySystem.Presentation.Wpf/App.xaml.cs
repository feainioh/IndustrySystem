using AutoMapper;
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
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Volo.Abp;

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
                    if (Nullable.GetUnderlyingType(column.PropertyInfo.PropertyType) != null) { column.IsNullable = true; }
                }
            }
        };
        containerRegistry.RegisterInstance<ISqlSugarClient>(new SqlSugarScope(conn));

        containerRegistry.Register(typeof(IRepository<>), typeof(SqlSugarRepository<>));
        containerRegistry.Register<IUserRoleRepository, UserRoleRepository>();
        containerRegistry.Register<IRolePermissionRepository, RolePermissionRepository>();

        containerRegistry.Register<IRoleAppService, RoleAppService>();
        containerRegistry.Register<IExperimentTemplateAppService, ExperimentTemplateAppService>();
        containerRegistry.Register<IExperimentParameterAppService, ExperimentParameterAppService>();
        containerRegistry.Register<IPermissionAppService, PermissionAppService>();
        containerRegistry.Register<IUserAppService, UserAppService>();
        containerRegistry.Register<IExperimentAppService, ExperimentAppService>();
        containerRegistry.Register<IExperimentGroupAppService, ExperimentGroupAppService>();
        containerRegistry.Register<IExperimentHistoryAppService, ExperimentHistoryAppService>();
        containerRegistry.Register<IAlarmAppService, AlarmAppService>();
        containerRegistry.Register<IInventoryAppService, InventoryAppService>();
        containerRegistry.Register<IMaterialAppService, MaterialAppService>();
        containerRegistry.Register<IShelfAppService, ShelfAppService>();
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
        containerRegistry.RegisterForNavigation<RoleManageView, RoleManageViewModel>();
        containerRegistry.RegisterForNavigation<Views.ExperimentTemplateView, ExperimentTemplateViewModel>();
        containerRegistry.RegisterForNavigation<Views.PermissionsView, PermissionsViewModel>();
        containerRegistry.RegisterForNavigation<Views.UsersView, UsersViewModel>();
        containerRegistry.RegisterForNavigation<Views.AlarmView, AlarmViewModel>();
        containerRegistry.RegisterForNavigation<Views.RunExperimentView, RunExperimentViewModel>();
        containerRegistry.RegisterForNavigation<Views.ExperimentsView, ExperimentsViewModel>();
        containerRegistry.RegisterForNavigation<Views.ExperimentHistoryView, ExperimentHistoryViewModel>();
        containerRegistry.RegisterForNavigation<Views.InventoryView, InventoryViewModel>();
        containerRegistry.RegisterForNavigation<Views.HardwareDebugView, HardwareDebugViewModel>();
        containerRegistry.RegisterForNavigation<Views.ExperimentGroupsView, ExperimentGroupsViewModel>();
        containerRegistry.RegisterForNavigation<Views.ExperimentConfigView, ExperimentConfigViewModel>();
        containerRegistry.RegisterForNavigation<Views.MaterialInfoView, MaterialInfoViewModel>();
        containerRegistry.RegisterForNavigation<Views.ShelfInfoView, ShelfInfoViewModel>();
        containerRegistry.RegisterForNavigation<Views.OperationLogsView, OperationLogsViewModel>();
        containerRegistry.RegisterForNavigation<Views.RealtimeDataView, RealtimeDataViewModel>();
        containerRegistry.RegisterForNavigation<Views.PeripheralDebugView, PeripheralDebugViewModel>();
        containerRegistry.RegisterForNavigation<Views.DeviceParamsView, DeviceParamsViewModel>();
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
        containerRegistry.RegisterDialog<Views.Dialogs.ShelfEditDialog, ViewModels.Dialogs.ShelfEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.SlotConfigDialog, ViewModels.Dialogs.SlotConfigDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.InventoryEditDialog, ViewModels.Dialogs.InventoryEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.InventoryOutboundDialog, ViewModels.Dialogs.InventoryOutboundDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ContainerEditDialog, ViewModels.Dialogs.ContainerEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ExperimentTemplateEditDialog, ViewModels.Dialogs.ExperimentTemplateEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ExperimentGroupEditDialog, ViewModels.Dialogs.ExperimentGroupEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ExperimentEditDialog, ViewModels.Dialogs.ExperimentEditDialogViewModel>();
        containerRegistry.RegisterDialog<Views.Dialogs.ConfirmDialog, ViewModels.Dialogs.ConfirmDialogViewModel>();

        // Register type-specific parameter edit views for region navigation
        containerRegistry.RegisterForNavigation<Views.Dialogs.ReactionParameterEditDialog, ViewModels.Dialogs.ExperimentParameterEditorViewModel>();
        containerRegistry.RegisterForNavigation<Views.Dialogs.RotaryEvaporationParameterEditDialog, ViewModels.Dialogs.ExperimentParameterEditorViewModel>();
        containerRegistry.RegisterForNavigation<Views.Dialogs.DetectionParameterEditDialog, ViewModels.Dialogs.ExperimentParameterEditorViewModel>();
        containerRegistry.RegisterForNavigation<Views.Dialogs.FiltrationParameterEditDialog, ViewModels.Dialogs.ExperimentParameterEditorViewModel>();
        containerRegistry.RegisterForNavigation<Views.Dialogs.DryingParameterEditDialog, ViewModels.Dialogs.ExperimentParameterEditorViewModel>();
        containerRegistry.RegisterForNavigation<Views.Dialogs.QuenchingParameterEditDialog, ViewModels.Dialogs.ExperimentParameterEditorViewModel>();
        containerRegistry.RegisterForNavigation<Views.Dialogs.ExtractionParameterEditDialog, ViewModels.Dialogs.ExperimentParameterEditorViewModel>();
        containerRegistry.RegisterForNavigation<Views.Dialogs.SamplingParameterEditDialog, ViewModels.Dialogs.ExperimentParameterEditorViewModel>();
        containerRegistry.RegisterForNavigation<Views.Dialogs.CentrifugationParameterEditDialog, ViewModels.Dialogs.ExperimentParameterEditorViewModel>();
        containerRegistry.RegisterForNavigation<Views.Dialogs.CustomDetectionParameterEditDialog, ViewModels.Dialogs.ExperimentParameterEditorViewModel>();
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

