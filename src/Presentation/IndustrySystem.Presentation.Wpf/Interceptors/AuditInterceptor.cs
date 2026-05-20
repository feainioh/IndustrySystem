using System.Diagnostics;
using System.Reflection;
using Castle.DynamicProxy;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Presentation.Wpf.Services;
using Newtonsoft.Json;
using NLog;

namespace IndustrySystem.Presentation.Wpf.Interceptors;

/// <summary>
/// Castle IInterceptor that records audit logs (who, what, when, result)
/// to both NLog and the database via IOperationLogService.
/// </summary>
public sealed class AuditInterceptor : IInterceptor
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        MaxDepth = 3,
        NullValueHandling = NullValueHandling.Ignore
    };

    private readonly IAuthState _authState;
    private readonly IOperationLogService? _operationLogService;

    public AuditInterceptor(IAuthState authState, IOperationLogService? operationLogService = null)
    {
        _authState = authState ?? throw new ArgumentNullException(nameof(authState));
        _operationLogService = operationLogService;
    }

    public void Intercept(IInvocation invocation)
    {
        var sw = Stopwatch.StartNew();
        var userName = _authState.IsAuthenticated ? _authState.UserName ?? "(anonymous)" : "(not authenticated)";
        var serviceName = invocation.TargetType?.Name ?? invocation.Method.DeclaringType?.Name ?? "Unknown";
        var methodName = invocation.Method.Name;
        var operationType = InferOperationType(serviceName);
        var parameters = SerializeParameters(invocation.Method, invocation.Arguments);

        Logger.Info($"[Audit] User={userName} | Service={serviceName} | Method={methodName} | Params={parameters}");

        try
        {
            invocation.Proceed();

            var returnType = invocation.Method.ReturnType;
            if (typeof(Task).IsAssignableFrom(returnType))
            {
                var task = invocation.ReturnValue as Task;
                if (task is not null)
                {
                    if (returnType.IsGenericType)
                    {
                        var resultType = returnType.GetGenericArguments()[0];
                        var method = typeof(AuditInterceptor)
                            .GetMethod(nameof(HandleAsyncWithResult), BindingFlags.Instance | BindingFlags.NonPublic)!
                            .MakeGenericMethod(resultType);
                        invocation.ReturnValue = method.Invoke(this, [task, sw, userName, serviceName, methodName, operationType]);
                    }
                    else
                    {
                        invocation.ReturnValue = HandleAsync(task, sw, userName, serviceName, methodName, operationType);
                    }
                }
            }
            else
            {
                LogAndPersist(sw, userName, serviceName, methodName, operationType, true, null);
            }
        }
        catch (Exception ex)
        {
            LogAndPersist(sw, userName, serviceName, methodName, operationType, false, ex);
            throw;
        }
    }

    private async Task HandleAsync(Task continuation, Stopwatch sw, string userName, string serviceName, string methodName, string operationType)
    {
        try
        {
            await continuation.ConfigureAwait(false);
            LogAndPersist(sw, userName, serviceName, methodName, operationType, true, null);
        }
        catch (Exception ex)
        {
            LogAndPersist(sw, userName, serviceName, methodName, operationType, false, ex);
            throw;
        }
    }

    private async Task<T> HandleAsyncWithResult<T>(Task continuation, Stopwatch sw, string userName, string serviceName, string methodName, string operationType)
    {
        try
        {
            await continuation.ConfigureAwait(false);
            var typedTask = (Task<T>)continuation;
            var result = typedTask.Result;
            var resultJson = result is not null
                ? JsonConvert.SerializeObject(result, JsonSettings)
                : "(void)";
            LogAndPersist(sw, userName, serviceName, methodName, operationType, true, null, resultJson);
            return result;
        }
        catch (Exception ex)
        {
            LogAndPersist(sw, userName, serviceName, methodName, operationType, false, ex);
            throw;
        }
    }

    private void LogAndPersist(Stopwatch sw, string userName, string serviceName, string methodName,
        string operationType, bool isSuccess, Exception? ex, string? resultPreview = null)
    {
        sw.Stop();
        var description = $"{methodName}";
        if (!isSuccess && ex is not null)
        {
            description += $" | Error={Truncate(ex.Message, 400)}";
        }
        if (resultPreview is not null)
        {
            description += $" | Result={Truncate(resultPreview, 500)}";
        }

        if (isSuccess)
        {
            var resultPart = resultPreview is not null
                ? $" | Result={Truncate(resultPreview, 500)}"
                : string.Empty;
            Logger.Info($"[Audit] OK | User={userName} | Service={serviceName} | Method={methodName} | Elapsed={sw.ElapsedMilliseconds}ms{resultPart}");
        }
        else
        {
            Logger.Error(ex, $"[Audit] FAIL | User={userName} | Service={serviceName} | Method={methodName} | Elapsed={sw.ElapsedMilliseconds}ms");
        }

        // Persist to database (fire-and-forget via Task.Run to avoid blocking the caller)
        if (_operationLogService is not null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _operationLogService.LogAsync(new CreateOperationLogDto(
                        isSuccess ? "Info" : "Error",
                        operationType,
                        userName,
                        description,
                        "-",
                        serviceName,
                        sw.ElapsedMilliseconds,
                        isSuccess,
                        ex?.Message
                    ));
                }
                catch (Exception logEx)
                {
                    Logger.Warn(logEx, $"[Audit] Failed to persist audit record to DB for {serviceName}.{methodName}");
                }
            });
        }
    }

    private static string InferOperationType(string serviceName) => serviceName switch
    {
        var s when s.Contains("ExperimentTemplate") => "ExperimentTemplate",
        var s when s.Contains("ExperimentParameter") => "ExperimentParameter",
        var s when s.Contains("ExperimentGroup") => "ExperimentGroup",
        var s when s.Contains("Experiment") => "Experiment",
        var s when s.Contains("Material") => "Material",
        var s when s.Contains("Inventory") => "Inventory",
        var s when s.Contains("Shelf") => "Shelf",
        var s when s.Contains("User") => "User",
        var s when s.Contains("Role") => "Role",
        var s when s.Contains("Permission") => "Permission",
        _ => "System"
    };

    private static string SerializeParameters(MethodInfo method, object?[] args)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 0 || args.Length == 0) return "(none)";

        var dict = new Dictionary<string, object?>();
        for (var i = 0; i < Math.Min(parameters.Length, args.Length); i++)
        {
            var p = parameters[i];
            if (p.ParameterType == typeof(CancellationToken)) continue;
            var name = p.Name ?? $"arg{i}";
            var val = args[i];
            if (val is not null && val.GetType().IsClass && val.GetType() != typeof(string))
            {
                dict[name] = new { Type = val.GetType().Name, Summary = val.ToString() };
            }
            else
            {
                dict[name] = val;
            }
        }

        return Truncate(JsonConvert.SerializeObject(dict, JsonSettings), 800);
    }

    private static string Truncate(string value, int maxLen)
        => value.Length <= maxLen ? value : value[..maxLen] + "...";
}
