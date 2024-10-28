namespace BlazeJump.Common.Services.Connections.Factories;

public class ClientWebSocketFactory : IClientWebSocketFactory
{
    public IClientWebSocketWrapper Create()
    {
        return new ClientWebSocketWrapper();
    }
}