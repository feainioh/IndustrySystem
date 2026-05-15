namespace IndustrySystem.Infrastructure.Communication.Abstractions;

public interface IExternalSyncChannel : ICommunicationClient
{
    string Name { get; }
    ExternalSyncProtocol Protocol { get; }
    int PollingIntervalSeconds { get; }
    bool IsConnected { get; }

    Task<IReadOnlyList<ExternalSyncMessage>> ReceiveBatchAsync(CancellationToken ct = default);
    Task AcknowledgeAsync(IReadOnlyList<ExternalSyncMessage> messages, CancellationToken ct = default);
}
