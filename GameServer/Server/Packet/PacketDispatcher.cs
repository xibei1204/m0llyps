using MollyPS.Util;

namespace MollyPS.GameServer.Server.Packet;

public static class PacketDispatcher
{
    private static readonly Logger Logger = new("Packets");

    public static void Dispatch(EndfieldConnection connection, int opcode, byte[] body)
    {
        if (!HandlerManager.TryGetHandler(opcode, out var handler))
        {
            Logger.Warn($"No handler for CS msgid {opcode} ({body.Length} bytes)");
            return;
        }
        try
        {
            handler.OnHandle(connection, body);
        }
        catch (Exception exception)
        {
            Logger.Error($"Handler {handler.GetType().Name} failed: opcode={opcode}, uid={connection.Player?.Uid}", exception);
        }
    }
}
