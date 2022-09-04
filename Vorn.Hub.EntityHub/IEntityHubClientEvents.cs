namespace Vorn.Hub.EntityHub;
public interface IEntityHubClientEvents
{
    Task NotifyChanges(List<EntityTrace<object>> entityChanges);
}
