using Microsoft.AspNetCore.SignalR;
namespace Vorn.Hub.EntityHub;
public interface IEntityHubServer
{
    Task SubscribeTo(string typeName);
}
public class EntityHubServer : Hub<IEntityHubClientEvents>, IEntityHubServer
{
    public Task SubscribeTo(string typeName) => Groups.AddToGroupAsync(Context.ConnectionId, typeName);
}
