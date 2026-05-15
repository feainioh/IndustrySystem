using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Application.Services;

/// <summary>
/// 实验历史应用服务（内存版示例实现）。
/// </summary>
public class ExperimentHistoryAppService : IExperimentHistoryAppService
{
    private static readonly object SyncRoot = new();
    private static readonly List<ExperimentHistoryDto> HistoryRecords = BuildSeedData();

    public Task<IReadOnlyList<ExperimentHistoryDto>> GetListAsync()
    {
        lock (SyncRoot)
        {
            return Task.FromResult<IReadOnlyList<ExperimentHistoryDto>>(HistoryRecords
                .OrderByDescending(x => x.StartTime)
                .ToList());
        }
    }

    public async Task<IReadOnlyList<ExperimentHistoryDto>> GetRecentAsync(int take = 50)
    {
        var normalizedTake = take <= 0 ? 50 : take;
        var all = await GetListAsync();
        return all.Take(normalizedTake).ToList();
    }

    public Task DeleteAsync(Guid id)
    {
        lock (SyncRoot)
        {
            HistoryRecords.RemoveAll(x => x.Id == id);
        }

        return Task.CompletedTask;
    }

    private static List<ExperimentHistoryDto> BuildSeedData()
    {
        var random = new Random(20260513);
        var now = DateTime.Now;

        var experimentNames = new[]
        {
            "过滤工艺验证",
            "发酵过程监控",
            "细胞培养",
            "PCR 扩增",
            "旋蒸回收",
            "离心纯化",
            "淬灭安全测试",
            "在线取样分析"
        };

        var operators = new[]
        {
            "admin",
            "operator01",
            "operator02",
            "lab.user",
            "shift.leader"
        };

        var result = new List<ExperimentHistoryDto>();
        for (var i = 0; i < 120; i++)
        {
            var status = PickStatus(random);
            var startTime = now
                .AddHours(-(i * 3 + random.Next(0, 3)))
                .AddMinutes(-random.Next(0, 59));

            var duration = status switch
            {
                RunState.Completed => TimeSpan.FromMinutes(random.Next(20, 140)),
                RunState.Stopped => TimeSpan.FromMinutes(random.Next(2, 45)),
                RunState.Running => TimeSpan.FromMinutes(random.Next(1, 80)),
                RunState.Paused => TimeSpan.FromMinutes(random.Next(5, 90)),
                _ => (TimeSpan?)null
            };

            DateTime? endTime = status is RunState.Completed or RunState.Stopped && duration.HasValue
                ? startTime + duration.Value
                : null;

            var resultText = GetResultText(status, random);
            result.Add(new ExperimentHistoryDto(
                Guid.NewGuid(),
                $"EXP-{20260000 + i:0000}",
                experimentNames[i % experimentNames.Length],
                status,
                resultText,
                startTime,
                endTime,
                duration,
                operators[i % operators.Length],
                i % 2 == 0,
                resultText == "失败" ? "过程超时，温度未达到目标值" : null));
        }

        return result
            .OrderByDescending(x => x.StartTime)
            .ToList();
    }

    private static RunState PickStatus(Random random)
    {
        var value = random.Next(100);
        if (value < 62) return RunState.Completed;
        if (value < 80) return RunState.Stopped;
        if (value < 90) return RunState.Running;
        if (value < 96) return RunState.Paused;
        return RunState.Idle;
    }

    private static string GetResultText(RunState status, Random random)
    {
        return status switch
        {
            RunState.Completed => random.NextDouble() < 0.82 ? "成功" : "失败",
            RunState.Stopped => random.NextDouble() < 0.50 ? "已取消" : "失败",
            RunState.Running => "执行中",
            RunState.Paused => "已暂停",
            _ => "待执行"
        };
    }
}
