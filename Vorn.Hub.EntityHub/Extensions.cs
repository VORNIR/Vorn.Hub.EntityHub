using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Vorn.Haas.Hubs.EntityHub;
using Vorn.Hub.EntityHub;

public static class Extensions
{
    public static object? InvokeAsGeneric(this Type? constructor, string methodName, Type T, object? initiator, params object?[]? parameters)
    {
        MethodInfo? method = constructor?.GetMethod(methodName);
        MethodInfo? generic = method?.MakeGenericMethod(T);
        return generic?.Invoke(initiator, parameters);
    }
    public static void AddEntityHubClients(this WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Services.Configure<EntityHubConfiguration>(webApplicationBuilder.Configuration.GetSection(EntityHubConfiguration.Section));
        HashSet<Type> types = GetEntityTracableTypes();
        foreach(Type type in types)
        {
            InvokeAsGeneric(typeof(Extensions), nameof(AddEntityHubClient), type, null, webApplicationBuilder);
        }
    }
    public static void AddEntityHubClient<T>(this WebApplicationBuilder webApplicationBuilder) where T : IEntityHubTracable
    {
        webApplicationBuilder.Services.AddScoped<IEntityHubClient<T>, EntityHubClient<T>>();
    }
    public static void MapEntityHub(this WebApplication app)
    {
        app.MapHub<EntityHubServer>(app.Configuration.GetSection(EntityHubConfiguration.Section).Get<EntityHubConfiguration>().Endpoint);
    }
    public static List<EntityTraces> GetEntityChangeLists(this ChangeTracker changeTracker)
    {
        HashSet<Type> types = GetEntityTracableTypes();
        List<EntityTraces> traces = new List<EntityTraces>();
        foreach(Type type in types)
        {
            object? entityTraces = InvokeAsGeneric(typeof(Extensions), nameof(GetEntityChangeList), type, null, changeTracker);
            traces.Add(new EntityTraces { EntityTraceList = entityTraces as List<EntityTrace<object>> ?? new(), Type = type });
        }
        return traces;
    }
    public static List<EntityTrace<object>> GetEntityChangeList<T>(this ChangeTracker changeTracker) where T : class
    {
        if(changeTracker is null)
        {
            throw new ArgumentNullException(nameof(changeTracker));
        }
        List<EntityTrace<object>> changeList = new();
        IEnumerable<EntityEntry> changedObjects = changeTracker.Entries();
        if(changedObjects.Any())
        {
            List<EntityTrace<object>> changedData = changedObjects
                .Where(x => x.Entity.GetType() == typeof(T))
                .Select(x => new EntityTrace<object>
                {
                    Entity = x.Entity as object,
                    State = x.State,
                })
                .ToList();
            changeList.AddRange(changedData);
        }
        return changeList;
    }
    public static HashSet<Type> GetEntityTracableTypes()
    {
        HashSet<Type> types = GetAssemblies()
            .Where(a => a.FullName?.StartsWith("Vorn") ?? false)
            .SelectMany(a => a.GetTypes())
            .Where(t => t.GetCustomAttribute(typeof(EntityHubTracable), false) is not null).ToHashSet();
        return types;
    }
    public static IEnumerable<Assembly> GetAssemblies()
    {
        HashSet<string> list = new HashSet<string>();
        Stack<Assembly> stack = new Stack<Assembly>();

        stack.Push(Assembly.GetEntryAssembly());

        do
        {
            Assembly asm = stack.Pop();

            yield return asm;

            foreach(AssemblyName reference in asm.GetReferencedAssemblies())
                if(!list.Contains(reference.FullName))
                {
                    stack.Push(Assembly.Load(reference));
                    list.Add(reference.FullName);
                }

        }
        while(stack.Count > 0);
    }
}
