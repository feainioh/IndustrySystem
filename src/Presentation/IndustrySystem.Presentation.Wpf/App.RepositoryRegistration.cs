using IndustrySystem.Domain.Repositories;
using IndustrySystem.Infrastructure.SqlSugar.Repositories;
using Microsoft.Extensions.Configuration;
using Prism.Ioc;

namespace IndustrySystem.Presentation.Wpf;

public partial class App
{
 private void RegisterRepositories(IContainerRegistry containerRegistry)
 {
 containerRegistry.Register(typeof(IRepository<>), typeof(SqlSugarRepository<>));
 containerRegistry.Register<IUserRoleRepository, UserRoleRepository>();
 containerRegistry.Register<IRolePermissionRepository, RolePermissionRepository>();
 }
}
