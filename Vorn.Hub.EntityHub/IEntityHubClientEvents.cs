namespace Vorn.Haas.Hubs.EntityHub;
public interface IEntityHubClientEvents
{
    Task NotifyChanges(List<EntityTrace<object>> entityChanges);
}
