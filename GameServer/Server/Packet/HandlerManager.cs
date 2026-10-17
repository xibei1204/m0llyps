using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MollyPS.Util;

namespace MollyPS.GameServer.Server.Packet;

public static class HandlerManager
{
    private static readonly Logger Logger = new("HandlerManager");
    private static FrozenDictionary<int, Handler> _handlers = FrozenDictionary<int, Handler>.Empty;

    public static int Count => Volatile.Read(ref _handlers).Count;

    public static void Init()
    {
        var handlers = new Dictionary<int, Handler>();
        foreach (var type in typeof(Handler).Assembly.GetTypes().OrderBy(type => type.FullName, StringComparer.Ordinal))
        {
            if (type.IsAbstract || !type.IsSubclassOf(typeof(Handler))) continue;
            var attr = type.GetCustomAttribute<PacketOpcode>();
            if (attr == null)
            {
                Logger.Warn($"Handler {type.Name} has no PacketOpcode attribute, skipped");
                continue;
            }
            if (attr.CmdId <= 0)
                throw new InvalidOperationException($"Handler {type.Name} has invalid opcode {attr.CmdId}.");
            if (type.GetConstructor(Type.EmptyTypes) == null)
                throw new InvalidOperationException($"Handler {type.Name} requires a public parameterless constructor.");
            if (!handlers.TryAdd(attr.CmdId, (Handler)Activator.CreateInstance(type)!))
                throw new InvalidOperationException($"Duplicate opcode {attr.CmdId}: {type.Name}.");
        }
        Volatile.Write(ref _handlers, handlers.ToFrozenDictionary());
        Logger.Info($"Registered {handlers.Count} packet handlers");
    }

    public static bool TryGetHandler(int cmdId, [NotNullWhen(true)] out Handler? handler)
    {
        return Volatile.Read(ref _handlers).TryGetValue(cmdId, out handler);
    }
}
