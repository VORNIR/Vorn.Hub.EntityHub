using Microsoft.EntityFrameworkCore;

namespace Vorn.Haas.Hubs.EntityHub;

public class EntityTrace<T>
{
    public T Entity { get; init; }
    public EntityState State { get; init; }
}

public class EntityTraces
{
    public List<EntityTrace<object>> EntityTraceList { get; init; }
    public Type Type { get; init; }
}