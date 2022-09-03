using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Vorn.Haas.Hubs.EntityHub;
using Vorn.Hub;

public interface IEntityHubClient<T> where T : class
{
    EventHandler<List<EntityTrace<T>>>? NotifyChanges { get; set; }
}
public class EntityHubClient<T> : HubClient<EntityHubConfiguration>, IEntityHubClient<T> where T : class
{
    public EventHandler<List<EntityTrace<T>>>? NotifyChanges { get; set; }
    public EntityHubClient(IOptions<EntityHubConfiguration> options) : base(options.Value)
    {
        HubConnection.On<List<EntityTrace<T>>>(nameof(IEntityHubClientEvents.NotifyChanges), a => NotifyChanges?.Invoke(this, a));
        HubConnection.InvokeAsync(nameof(IEntityHubServer.SubscribeTo), typeof(T).Name);
    }
}
