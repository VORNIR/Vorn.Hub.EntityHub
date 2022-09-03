using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Vorn.Haas.Hubs.EntityHub;
public class EntityHubTracableDbContext : DbContext
{
    private readonly IHubContext<EntityHubServer, IEntityHubClientEvents> hubContext;
    public EntityHubTracableDbContext(DbContextOptions options, IHubContext<EntityHubServer, IEntityHubClientEvents> hubContext) : base(options)
    {
        this.hubContext = hubContext;
    }
    public override int SaveChanges()
    {
        List<EntityTraces> changedData = ChangeTracker.GetEntityChangeLists();
        int result = base.SaveChanges();
        foreach(EntityTraces data in changedData)
        {
            hubContext.Clients.Group(data.Type.Name).NotifyChanges(data.EntityTraceList).Wait();
        }
        return result;
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        List<EntityTraces> changedData = ChangeTracker.GetEntityChangeLists();
        int result = await base.SaveChangesAsync(cancellationToken);
        foreach(EntityTraces data in changedData)
        {
            await hubContext.Clients.Group(data.Type.Name).NotifyChanges(data.EntityTraceList);
        }
        return result;
    }
}
