using IndustrySystem.Infrastructure.Communication.Abstractions;

namespace IndustrySystem.Infrastructure.Communication.Implementations;

public sealed class ExternalSyncChannelFactory : IExternalSyncChannelFactory
{
    private readonly Func<IHttpClient> _httpClientFactory;

    public ExternalSyncChannelFactory(Func<IHttpClient> httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public IExternalSyncChannel Create(ExternalSyncEndpointOptions endpoint)
    {
        return endpoint.Protocol switch
        {
            ExternalSyncProtocol.WebApi => new WebApiExternalSyncChannel(_httpClientFactory(), endpoint),
            ExternalSyncProtocol.Socket => new SocketExternalSyncChannel(endpoint),
            ExternalSyncProtocol.SignalR => new SignalRExternalSyncChannel(endpoint),
            _ => throw new NotSupportedException($"Unsupported sync protocol: {endpoint.Protocol}")
        };
    }
}
