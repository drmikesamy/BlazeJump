using BlazeJump.Common.Services.Connections.Factories;

namespace BlazeJump.Common.Services.Connections.Providers;

public class RelayConnectionProvider : IRelayConnectionProvider
{
    public IRelayConnection CreateRelayConnection(string uri)
    {
        return new RelayConnection(new ClientWebSocketFactory(), uri);
    }
}