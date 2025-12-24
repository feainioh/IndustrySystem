$filePath = "src\Presentation\IndustrySystem.Presentation.Wpf\Resources\Strings.resx"
$content = Get-Content $filePath -Raw -Encoding UTF8

$newEntries = @"
  <!-- Status labels -->
  <data name="Status_Active" xml:space="preserve"><value>启用</value></data>
  <data name="Status_Inactive" xml:space="preserve"><value>禁用</value></data>
  <data name="Status_Yes" xml:space="preserve"><value>是</value></data>
  <data name="Status_No" xml:space="preserve"><value>否</value></data>

  <!-- New Buttons -->
  <data name="Btn_Refresh" xml:space="preserve"><value>刷新</value></data>
  <data name="Btn_Acknowledge" xml:space="preserve"><value>确认</value></data>

  <!-- New Navigation -->
  <data name="Nav_Alarm" xml:space="preserve"><value>告警管理</value></data>

  <!-- Hints -->
  <data name="Hint_User_UserName" xml:space="preserve"><value>请输入用户名</value></data>
  <data name="Hint_User_DisplayName" xml:space="preserve"><value>请输入显示名</value></data>
  <data name="Hint_Permission_Name" xml:space="preserve"><value>请输入权限名称</value></data>
  <data name="Hint_Permission_DisplayName" xml:space="preserve"><value>请输入权限显示名</value></data>
  <data name="Hint_Template_Name" xml:space="preserve"><value>请输入模板名称</value></data>
  <data name="Hint_Template_Description" xml:space="preserve"><value>请输入模板描述</value></data>

  <!-- Alarm related -->
  <data name="Alarm_NoAlarms" xml:space="preserve"><value>暂无告警</value></data>
  <data name="Alarm_Status_Acknowledged" xml:space="preserve"><value>已确认</value></data>
  <data name="Alarm_Status_Unacknowledged" xml:space="preserve"><value>未确认</value></data>

  <!-- Log messages - AlarmViewModel -->
  <data name="Log_AlarmViewModel_Initialized" xml:space="preserve"><value>告警视图模型已初始化</value></data>
  <data name="Log_Alarm_LoadStart" xml:space="preserve"><value>开始加载告警数据</value></data>
  <data name="Log_Alarm_LoadComplete" xml:space="preserve"><value>告警数据加载完成</value></data>
  <data name="Log_Alarm_Acknowledge" xml:space="preserve"><value>确认告警</value></data>

  <!-- Log messages - ExperimentHistoryViewModel -->
  <data name="Log_ExperimentHistoryViewModel_Initialized" xml:space="preserve"><value>实验历史视图模型已初始化</value></data>
  <data name="Log_ExperimentHistory_LoadStart" xml:space="preserve"><value>开始加载实验历史数据</value></data>
  <data name="Log_ExperimentHistory_LoadComplete" xml:space="preserve"><value>实验历史数据加载完成</value></data>

  <!-- Log messages - ExperimentsViewModel -->
  <data name="Log_ExperimentsViewModel_Initialized" xml:space="preserve"><value>实验视图模型已初始化</value></data>
  <data name="Log_Experiments_LoadStart" xml:space="preserve"><value>开始加载实验数据</value></data>
  <data name="Log_Experiments_LoadComplete" xml:space="preserve"><value>实验数据加载完成</value></data>
  <data name="Log_Experiments_Delete" xml:space="preserve"><value>删除实验</value></data>

  <!-- Log messages - ExperimentTemplateViewModel -->
  <data name="Log_ExperimentTemplateViewModel_Initialized" xml:space="preserve"><value>实验模板视图模型已初始化</value></data>
  <data name="Log_ExperimentTemplate_LoadStart" xml:space="preserve"><value>开始加载实验模板数据</value></data>
  <data name="Log_ExperimentTemplate_LoadComplete" xml:space="preserve"><value>实验模板数据加载完成</value></data>
  <data name="Log_ExperimentTemplate_Add" xml:space="preserve"><value>新增实验模板</value></data>
  <data name="Log_ExperimentTemplate_Delete" xml:space="preserve"><value>删除实验模板</value></data>

  <!-- Log messages - HardwareDebugViewModel -->
  <data name="Log_HardwareDebugViewModel_Initialized" xml:space="preserve"><value>硬件调试视图模型已初始化</value></data>
  <data name="Log_HardwareDebug_Connecting" xml:space="preserve"><value>正在连接硬件设备</value></data>
  <data name="Log_HardwareDebug_Connected" xml:space="preserve"><value>硬件设备连接成功</value></data>
  <data name="Log_HardwareDebug_ConnectFailed" xml:space="preserve"><value>硬件设备连接失败</value></data>
  <data name="Log_HardwareDebug_Disconnecting" xml:space="preserve"><value>正在断开硬件设备连接</value></data>
  <data name="Log_HardwareDebug_Disconnected" xml:space="preserve"><value>硬件设备已断开连接</value></data>
  <data name="Log_HardwareDebug_Reading" xml:space="preserve"><value>正在读取寄存器数据</value></data>
  <data name="Log_HardwareDebug_ReadSuccess" xml:space="preserve"><value>寄存器数据读取成功</value></data>
  <data name="Log_HardwareDebug_ReadFailed" xml:space="preserve"><value>寄存器数据读取失败</value></data>
  <data name="Log_HardwareDebug_WriteEmpty" xml:space="preserve"><value>写入数据为空</value></data>
  <data name="Log_HardwareDebug_WriteFormatError" xml:space="preserve"><value>写入数据格式错误</value></data>
  <data name="Log_HardwareDebug_WriteValueFormatError" xml:space="preserve"><value>写入值格式错误</value></data>
  <data name="Log_HardwareDebug_WriteSingleSuccess" xml:space="preserve"><value>单个寄存器写入成功</value></data>
  <data name="Log_HardwareDebug_WriteMultiSuccess" xml:space="preserve"><value>多个寄存器写入成功</value></data>
  <data name="Log_HardwareDebug_WriteFailed" xml:space="preserve"><value>寄存器写入失败</value></data>

  <!-- Log messages - InventoryViewModel -->
  <data name="Log_InventoryViewModel_Initialized" xml:space="preserve"><value>库存视图模型已初始化</value></data>
  <data name="Log_Inventory_LoadStart" xml:space="preserve"><value>开始加载库存数据</value></data>
  <data name="Log_Inventory_LoadComplete" xml:space="preserve"><value>库存数据加载完成</value></data>
  <data name="Log_Inventory_In" xml:space="preserve"><value>入库操作</value></data>
  <data name="Log_Inventory_Out" xml:space="preserve"><value>出库操作</value></data>

  <!-- Log messages - PermissionsViewModel -->
  <data name="Log_PermissionsViewModel_Initialized" xml:space="preserve"><value>权限视图模型已初始化</value></data>
  <data name="Log_Permissions_LoadStart" xml:space="preserve"><value>开始加载权限数据</value></data>
  <data name="Log_Permissions_LoadComplete" xml:space="preserve"><value>权限数据加载完成</value></data>
  <data name="Log_Permissions_Add" xml:space="preserve"><value>新增权限</value></data>
  <data name="Log_Permissions_Delete" xml:space="preserve"><value>删除权限</value></data>

  <!-- Log messages - RoleManageViewModel -->
  <data name="Log_RoleManageViewModel_Initialized" xml:space="preserve"><value>角色管理视图模型已初始化</value></data>
  <data name="Log_Role_LoadStart" xml:space="preserve"><value>开始加载角色数据</value></data>
  <data name="Log_Role_LoadComplete" xml:space="preserve"><value>角色数据加载完成</value></data>
  <data name="Log_Role_Add" xml:space="preserve"><value>新增角色</value></data>
  <data name="Log_Role_Delete" xml:space="preserve"><value>删除角色</value></data>

  <!-- Log messages - RunExperimentViewModel -->
  <data name="Log_RunExperimentViewModel_Initialized" xml:space="preserve"><value>运行实验视图模型已初始化</value></data>
  <data name="Log_RunExperiment_LoadExperiments" xml:space="preserve"><value>加载实验列表</value></data>
  <data name="Log_RunExperiment_Start" xml:space="preserve"><value>开始实验</value></data>
  <data name="Log_RunExperiment_Pause" xml:space="preserve"><value>暂停实验</value></data>
  <data name="Log_RunExperiment_Resume" xml:space="preserve"><value>恢复实验</value></data>
  <data name="Log_RunExperiment_Stop" xml:space="preserve"><value>停止实验</value></data>

  <!-- Log messages - UsersViewModel -->
  <data name="Log_UsersViewModel_Initialized" xml:space="preserve"><value>用户视图模型已初始化</value></data>
  <data name="Log_Users_LoadStart" xml:space="preserve"><value>开始加载用户数据</value></data>
  <data name="Log_Users_LoadComplete" xml:space="preserve"><value>用户数据加载完成</value></data>
  <data name="Log_Users_Add" xml:space="preserve"><value>新增用户</value></data>
  <data name="Log_Users_Delete" xml:space="preserve"><value>删除用户</value></data>

"@

$content = $content -replace '</root>', "$newEntries</root>"
$content | Set-Content $filePath -Encoding UTF8
Write-Host "Done updating Strings.resx"
