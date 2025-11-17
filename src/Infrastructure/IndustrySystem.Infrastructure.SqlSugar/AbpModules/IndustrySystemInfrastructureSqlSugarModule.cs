using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using Volo.Abp.Modularity;
using IndustrySystem.Infrastructure.SqlSugar.Abstractions;
using IndustrySystem.Infrastructure.SqlSugar.Implementations;

namespace IndustrySystem.Infrastructure.SqlSugar.AbpModules;

public class IndustrySystemInfrastructureSqlSugarModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var options = new SqlSugarOptions();
        configuration.GetSection("SqlSugar").Bind(options);

        // bind options for other services
        context.Services.AddSingleton(options);

        context.Services.AddSingleton<ISqlSugarClient>(_ =>
        {
            var db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = options.ConnectionString,
                DbType = DbType.MySql,
                IsAutoCloseConnection = true
            });
            return db;
        });

        // DB initializer
        context.Services.AddSingleton<IDatabaseInitializer, SqlSugarDatabaseInitializer>();

    // register generic repository
    context.Services.AddScoped(typeof(IndustrySystem.Domain.Repositories.IRepository<>), typeof(IndustrySystem.Infrastructure.SqlSugar.Repositories.SqlSugarRepository<>));
    }
}
