namespace IndustrySystem.Infrastructure.Communication.Abstractions;

public interface IExternalSyncChannelFactory
{
    IExternalSyncChannel Create(ExternalSyncEndpointOptions endpoint);
}
