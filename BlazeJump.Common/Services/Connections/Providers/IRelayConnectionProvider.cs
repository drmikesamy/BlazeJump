namespace BlazeJump.Common.Services.Connections.Providers;

public interface IRelayConnectionProvider
{
    IRelayConnection CreateRelayConnection(string uri);
}