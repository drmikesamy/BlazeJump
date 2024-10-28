namespace BlazeJump.Common.Services.Connections.Factories;

public interface IClientWebSocketFactory
{
    IClientWebSocketWrapper Create();
}